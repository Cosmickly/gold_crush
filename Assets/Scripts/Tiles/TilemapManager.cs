using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Entities;
using Players;
using Unity.AI.Navigation;
using UnityEngine;
using UnityEngine.Tilemaps;
using Random = UnityEngine.Random;

namespace Tiles
{
    public class TilemapManager : MonoBehaviour
    {
        [SerializeField] private GameManager _gameManager;
        private TilemapBuilder _tilemapBuilder;
        private Tilemap _tilemap;

        public Dictionary<Vector3Int, GroundTile> AllTiles { get; set; } = new();
        private Dictionary<Vector3Int, GroundTile> _crackingTiles = new();
        public Dictionary<Vector3Int, RockObstacle> Obstacles { get; } = new();

        private Dictionary<NavMeshLink, Tuple<Vector3Int, Vector3Int>> _navMeshLinks = new();

        private Dictionary<int, Vector3Int> _playerLocations = new();
    
        [Header("Prefabs")] 
        [SerializeField] public GoldPiece GoldPiecePrefab;
        [SerializeField] private Vector2 _goldSpawnRadius;
    
        [Header("Parameters")] 
        [SerializeField] private bool _goldEnabled;
        [SerializeField] private float _goldSpawnTimeMax;
        [SerializeField] private float _goldSpawnTimeMin;
        private float _goldSpawnTime;
        private float _goldSpawnTimer;
        
        [SerializeField] private bool _tileCrackEnabled;
        [SerializeField] private float _tileCrackTimeMax;
        [SerializeField] private float _tileCrackTimeMin;
        private float _tileCrackTime;
        private float _tileCrackTimer;
        
        [SerializeField] private int _maxIntensity;
        [SerializeField] private float _currentIntensity;

        public bool Active { get; private set; } = true;

        private Vector2Int _centerPos;
        public Vector2Int TilemapSize
        {
            get => _tilemapBuilder.TilemapSize;
            private set
            {
                _tilemapBuilder.TilemapSize = value;
                _centerPos = new Vector2Int(TilemapSize.x / 2, TilemapSize.y / 2);
            }
        }
        
        private void Awake()
        {
            _tilemapBuilder = GetComponent<TilemapBuilder>();
            TilemapSize = _tilemapBuilder.TilemapSize;
            _tilemap = GetComponent<Tilemap>();
        }

        private void Start()
        {
            StartLevel();
        }

        private void Update()
        {
            if (!Active) return;
            
            if (_currentIntensity < _maxIntensity) _currentIntensity += Time.deltaTime;
            var intensityRatio = _currentIntensity / _maxIntensity;
            
            _goldSpawnRadius = Vector2.Lerp(new Vector2(5, 5), TilemapSize, intensityRatio);

            if (_goldEnabled)
            {
                _goldSpawnTime = Mathf.Lerp(_goldSpawnTimeMax, _goldSpawnTimeMin, intensityRatio);
                _goldSpawnTimer -= Time.deltaTime;
                if (_goldSpawnTimer <= 0)
                {
                    SpawnRandomCoin();
                    _goldSpawnTimer = _goldSpawnTime;
                }
            }

            if (_tileCrackEnabled)
            {
                _tileCrackTime = Mathf.Lerp(_goldSpawnTimeMax, _goldSpawnTimeMin, intensityRatio);
                _tileCrackTimer -= Time.deltaTime;
                if (_tileCrackTimer <= 0)
                {
                    CrackRandomTile();
                    _tileCrackTimer = _tileCrackTime;
                }
            }
            
            if (Input.GetKeyDown(KeyCode.R) && Active) RebuildTilesOnly();
        }

        /*
         * LEVEL
         */

        private void StartLevel()
        {
            _currentIntensity = 0;
            _tilemapBuilder.Build();
            Active = true;
        }

        public IEnumerator ResetLevel()
        {
            ClearAllTiles();
            ClearAllLinks();
            Active = false;
            yield return new WaitForSeconds(2f);
            StartLevel();
            _gameManager.ResetPlayers();
        }

        private void RebuildTilesOnly()
        {
            ClearObstacles();
            ClearAllTiles();
            ClearAllLinks();
            _tilemapBuilder.Build();
            // _allCellPositions = new HashSet<Vector3Int>(AllTiles.Keys);
        }

        /*
         * TILES
         */
        
        private void ClearAllTiles()
        {
            for (int i = 0; i < AllTiles.Count; i++)
            {
                var pair = AllTiles.ElementAt(i);
                pair.Value.Break();
            }
            for (int i = 0; i < _crackingTiles.Count; i++)
            {
                var pair = _crackingTiles.ElementAt(i);
                pair.Value.Break();
            }
            
            _crackingTiles.Clear();
            ClearLinksToCell(Vector3Int.zero);
        }
    
        private void CrackTile(Vector3Int cell)
        {
            var availableCells = AllTiles.Keys.Except(_crackingTiles.Keys).ToList();
            if (_tileCrackEnabled && availableCells.Contains(cell))
            {
                if (AllTiles.TryGetValue(cell, out var tile))
                {
                    _crackingTiles.Add(cell, tile);
                    tile.Cracking = true;
                }
            }
        }
    
        public bool RemoveTile(Vector3Int cell)
        {
            if (_crackingTiles.TryGetValue(cell, out var crackTile))
            {
                ClearLinksToCell(cell);
                GenerateNewLinks(cell);
                _crackingTiles.Remove(cell);
                return true;
            }

            if (AllTiles.TryGetValue(cell, out var activeTile))
            {
                // activeTile.Break();
                ClearLinksToCell(cell);
                GenerateNewLinks(cell);
                // AllTiles.Remove(cell);
                return true;
            }
        
            return false;
        }
    
        private Vector3Int RandomTile()
        {
            var availableCells = AllTiles.Keys.Except(_crackingTiles.Keys).ToList();
            // var keys = ActiveTiles.Keys.ToList();
            // var randomInt = Random.Range(0, keys.Count);
            // var key = keys[randomInt];
            var key = availableCells.ElementAt(Random.Range(0, availableCells.Count));
            return key;
        }
    
        private void CrackRandomTile()
        {
            if (_crackingTiles.Count < AllTiles.Count)
            {
                CancelInvoke(nameof(CrackRandomTile));
                return;
            }
            CrackTile(RandomTile());
        }
        
        /*
         * OBSTACLES
         */
        
        public bool RemoveObstacle(Vector3Int cell)
        {
            return Obstacles.Remove(cell, out RockObstacle obstacle);
        }
        
        private void ClearObstacles()
        {
            foreach (var obstacle in Obstacles) Destroy(obstacle.Value.gameObject);
            Obstacles.Clear();
        }
        
    
        /*
         * GOLD PIECES
         */

        private void SpawnRandomCoin()
        {
            SpawnCoin(CellToWorld(GetRandomFreeCell()));
        }
    
        private void SpawnCoin(Vector3 pos)
        {
            if (_goldEnabled)
            {
                var goldPiece = Instantiate(GoldPiecePrefab, pos + new Vector3Int(0, 1, 0), Quaternion.identity, transform);
                goldPiece.TilemapManager = this;
                // _goldPieces.Add(goldPiece);
            }
        }

        // public bool RemoveGoldPiece(GoldPiece goldPiece)
        // {
        //     return _goldPieces.Remove(goldPiece);
        // }
        //
        // private void ClearAllGoldPieces()
        // {
        //     for (int i = 0; i < _goldPieces.Count; i++)
        //     {
        //         Destroy(_goldPieces[i]);
        //     }
        //     
        //     _goldPieces.Clear();
        // }
    
        /*
         * PLAYER LOCATIONS
         */

        // TODO: find a better way to handle tile checks
        public void UpdatePlayerLocation(int id, Vector3Int newCell)
        {
            if (_playerLocations.TryGetValue(id, out var oldCell))
            {
                ExitedTile(oldCell);
            }
        
            EnteredTile(newCell);
            _playerLocations[id] = newCell;
        }

        private void EnteredTile(Vector3Int cell)
        {
            CrackTile(cell);
            if (AllTiles.TryGetValue(cell, out var tile))
                tile.PlayerOnMe = true;
        }

        private void ExitedTile(Vector3Int cell)
        {
            // CrackTile(pos);
            if (AllTiles.TryGetValue(cell, out var tile))
                tile.PlayerOnMe = false;
        }

        public void PlayerFell(BasePlayer player)
        {
            ExitedTile(_playerLocations[player.ID]);
        }
    
        /*
         * NAVMESH LINKS
         */

        private void ClearLinksToCell(Vector3Int cell)
        {
            var removeList = new List<NavMeshLink>();

            foreach (var (key, tiles) in _navMeshLinks)
            {
                if (tiles.Item1 == cell || tiles.Item2 == cell)
                {
                    removeList.Add(key);
                }
            }

            foreach (var link in removeList)
            {
                _navMeshLinks.Remove(link);
                Destroy(link);
            }
        }

        private void ClearAllLinks()
        {
            foreach (var (link, tiles) in _navMeshLinks)
            {
                Destroy(link);
            }
            
            _navMeshLinks.Clear();
        }

        public void GenerateNewLinks(Vector3Int cell)
        {
            var n = cell + new Vector3Int(1, 1, 0);
            var ne = cell + new Vector3Int(1, 0, 0);
            var e = cell + new Vector3Int(1, -1, 0);
            var se = cell + new Vector3Int(0, -1, 0);
            var s = cell + new Vector3Int(-1, -1, 0);
            var sw = cell + new Vector3Int(-1, 0, 0);
            var w = cell + new Vector3Int(-1, 1, 0);
            var nw = cell + new Vector3Int(0, 1, 0);

            // var allTiles = AllTiles(kvp => kvp.Key, kvp => kvp.Value);

            if (AllTiles.ContainsKey(n) && AllTiles.ContainsKey(s))
                CreateNavMeshLink(n, s, 0.2f);

            if (AllTiles.ContainsKey(e) && AllTiles.ContainsKey(w))
                CreateNavMeshLink(e, w, 0.2f);

            if (AllTiles.ContainsKey(ne) && AllTiles.ContainsKey(sw))
                CreateNavMeshLink(ne, sw, 0.8f);

            if (AllTiles.ContainsKey(se) && AllTiles.ContainsKey(nw))
                CreateNavMeshLink(se, nw, 0.8f);
        }
    
        private void CreateNavMeshLink(Vector3Int cellA, Vector3Int cellB, float width)
        {
            var link = gameObject.AddComponent<NavMeshLink>();

            var diff = (cellA - cellB) / 2;
            var offset = new Vector3(diff.x * 0.4f, 0, diff.y * 0.4f);
            link.startPoint = CellToWorld(cellA) - offset;
            link.endPoint = CellToWorld(cellB) + offset;
            link.bidirectional = true;
            link.width = width;
            // link.area = 2;
            _navMeshLinks.Add(link, new Tuple<Vector3Int, Vector3Int>(cellA, cellB));
        }
    
        /*
         * HELPER
         */

        private Vector3 CellToWorld(Vector3Int cell)
        {
            return new Vector3(cell.x + 0.5f, 0.5f, cell.y + 0.5f);
        }
    
        public Vector3Int GetCell(Vector3 pos)
        {
            return _tilemap.WorldToCell(pos);
        }

        public GroundTile GetTile(Vector3 pos)
        {
            var cell = _tilemap.WorldToCell(pos);
            return AllTiles.GetValueOrDefault(cell);
            // if (ActiveTiles.TryGetValue(cell, out var activeTile))
            //     return activeTile;
            // return _crackingTiles.GetValueOrDefault(cell);
        }

        private Vector3Int GetRandomFreeCell()
        {
            var possibleCells = AllTiles.Keys.Except(Obstacles.Keys).ToList();
            possibleCells = possibleCells.Where(
                cell => cell.x >= _centerPos.x - _goldSpawnRadius.x / 2 
                        && cell.x <= _centerPos.x + _goldSpawnRadius.x / 2 
                        && cell.y >= _centerPos.y - _goldSpawnRadius.y / 2 
                        && cell.y <= _centerPos.y + _goldSpawnRadius.y / 2 ).ToList();
            return possibleCells.ElementAt(Random.Range(0, possibleCells.Count-1));
        }
    
        // public bool HasTile(Vector3Int pos)
        // {
        //     return ActiveTiles.ContainsKey(pos);
        // }
    }
}
