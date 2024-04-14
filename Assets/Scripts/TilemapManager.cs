using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.AI.Navigation;
using UnityEngine;
using UnityEngine.Tilemaps;
using Random = UnityEngine.Random;

public class TilemapManager : MonoBehaviour
{
    private Tilemap _tilemap;
    private BoxCollider _boundary;
    private NavMeshSurface _navMeshSurface;

    public Vector2Int TilemapSize;
    
    private Dictionary<Vector3Int, TileController> _activeTiles = new();
    private Dictionary<Vector3Int, TileController> _crackingTiles = new();
    private List<Vector3Int> _goldPieces = new();
    private List<Vector3Int> _obstacles = new();

    [SerializeField] private bool _goldEnabled;
    [SerializeField] private bool _tileCrackEnabled;
    [SerializeField] private float _randomTileRate;
    
    [Header("Prefabs")] 
    [SerializeField] private TileController _groundTile;
    [SerializeField] private TileController _iceTile;
    [SerializeField] private GoldPiece _goldPiecePrefab;
    [SerializeField] private RockObstacle _rockObstaclePrefab;

    private readonly Vector3Int _topLayerOffset = new(0, 1, 0);

    private void Awake()
    {
        _tilemap = GetComponent<Tilemap>();
        _boundary = GetComponent<BoxCollider>();
        _navMeshSurface = GetComponent<NavMeshSurface>();
    }

    private void Start()
    {
        BuildTiles();
        
        BuildBoundary();
        
        _navMeshSurface.BuildNavMesh();
        
        InvokeRepeating(nameof(CrackRandomTile), 0f, _randomTileRate);
    }

    private void GetTilesFromChildren()
    {
        foreach (TileController tile in GetComponentsInChildren<TileController>())
        {
            var pos = tile.transform.position;
            tile.TilemapManager = this;
            _activeTiles.Add(_tilemap.WorldToCell(pos), tile);
        
            // SpawnTopLayerObject(pos);
        }
    }

    private void BuildTiles()
    {
        for (int i = 0; i < TilemapSize.x; i++)
        {
            for (int j = 0; j < TilemapSize.y; j++)
            {
                var pos = new Vector3Int(i, 0, j);
                var tilePrefab = Random.value <= 0.9 ? _groundTile : _iceTile;
                var tile = Instantiate(tilePrefab, pos, Quaternion.identity, transform);
                tile.Cell = pos;
                tile.TilemapManager = this;
                _activeTiles.Add(_tilemap.WorldToCell(pos), tile);
                
                SpawnTopLayerObject(pos + _topLayerOffset);
            }
        }
    }

    private void SpawnTopLayerObject(Vector3Int pos)
    {
        var rand = Random.value;
        if (_goldEnabled && rand <= 0.1f)
        {
            var goldPiece = Instantiate(_goldPiecePrefab, pos, Quaternion.identity, transform);
            goldPiece.Cell = pos;
            goldPiece.TilemapManager = this;
            _goldPieces.Add(pos);
        }

        if (rand is > 0.1f and <= 0.2f)
        {
            var obstacle = Instantiate(_rockObstaclePrefab, pos, Quaternion.identity, transform);
            obstacle.Cell = pos;
            obstacle.TilemapManager = this;
            _obstacles.Add(pos);
            Debug.Log("obstacle is at " + pos);
        }
    }

    private void BuildBoundary()
    {
        if (_activeTiles.Count <=0) return;
        var orderedKeys = _activeTiles.Keys.OrderBy(k => k.magnitude).ToList();
        var min = orderedKeys.First();
        var max = orderedKeys.Last();
        
        if (!_boundary) return;
        var size = new Vector3(max.x - min.x + 1, 1, max.y - min.y + 1);
        _boundary.size = size;
        _boundary.center = new Vector3(min.x + size.x/2, -0.5f, min.y + size.z/2);
    }

    public Vector3Int GetCell(Vector3 pos)
    {
        return _tilemap.WorldToCell(pos);
    }

    public void CrackTile(Vector3Int pos)
    {
        if (_tileCrackEnabled && _activeTiles.Remove(pos, out TileController tile))
        {
            _crackingTiles.Add(pos, tile);
            tile.Cracking = true;
        }
    }
    
    public bool RemoveTile(Vector3Int pos)
    {
        return _crackingTiles.Remove(_tilemap.WorldToCell(pos));
    }
    
    private Vector3Int RandomTile()
    {
        var keys = _activeTiles.Keys.ToList();
        var randomInt = Random.Range(0, keys.Count);
        var key = keys[randomInt];
        return key;
    }
    
    private void CrackRandomTile()
    {
        if (_activeTiles.Count > 0)
        {
            CrackTile(RandomTile());
        }
        else
        {
            CancelInvoke(nameof(CrackRandomTile));
        }
    }
    
    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.TryGetComponent(out IEntity entity))
        {
            entity.Fall();
        }
    }

    public bool RemoveGoldPiece(Vector3Int pos)
    {
        return _goldPieces.Remove(pos);
    }

    public bool RemoveObstacle(Vector3Int pos)
    {
        return _obstacles.Remove(pos);
    }
}
