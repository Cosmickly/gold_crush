using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.AI.Navigation;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Tilemaps;
using Random = UnityEngine.Random;

public class TilemapManager : MonoBehaviour
{
    private TilemapBuilder _tilemapBuilder;
    private Tilemap _tilemap;

    private Dictionary<Vector3Int, GroundTile> _activeTiles = new();
    private Dictionary<Vector3Int, GroundTile> _crackingTiles = new();
    private Dictionary<OffMeshLink, Tuple<Vector3Int, Vector3Int>> _offMeshLinks = new();
    private Dictionary<int, Vector3Int> _playerLocations = new();
    
    public bool GoldEnabled
    {
        get => _goldEnabled;
        private set => _goldEnabled = value;
    }
    [Header("Parameters")] 
    [SerializeField] private bool _goldEnabled;
    [SerializeField] private bool _tileCrackEnabled;
    [SerializeField] private float _randomTileRate;
    
    private void Awake()
    {
        _tilemapBuilder = GetComponent<TilemapBuilder>();
        _tilemap = GetComponent<Tilemap>();
    }

    private void Start()
    {
        _tilemapBuilder.Build();
        InvokeRepeating(nameof(CrackRandomTile), 0f, _randomTileRate);
    }

    public Vector3Int GetCell(Vector3 pos)
    {
        return _tilemap.WorldToCell(pos);
    }

    private void CrackTile(Vector3Int pos)
    {
        if (_tileCrackEnabled && _activeTiles.Remove(pos, out GroundTile tile))
        {
            _crackingTiles.Add(pos, tile);
            tile.Cracking = true;
        }
    }
    
    public bool RemoveTile(Vector3Int pos)
    {
        if (_crackingTiles.ContainsKey(pos))
        {
            UpdateLinks(pos);
            _crackingTiles.Remove(pos);
            return true;
        }
        
        return false;
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
        if (_activeTiles.Count <= 0)
        {
            CancelInvoke(nameof(CrackRandomTile));
            return;
        }
        CrackTile(RandomTile());
    }
    
    private void OnTriggerEnter(Collider other)
    {
        if (!other.gameObject.TryGetComponent(out IEntity entity)) return;
        entity.Fall();
        if (other.gameObject.TryGetComponent(out BasePlayer player))
        {
            ExitedTile(_playerLocations[player.ID]);
        }
    }

    // TODO: find a better way to handle tile checks
    public void UpdatePlayerLocation(int id, Vector3Int pos)
    {
        if (_playerLocations.TryGetValue(id, out var cell))
        {
            ExitedTile(cell);
        }
        
        EnteredTile(pos);
        _playerLocations[id] = pos;
    }

    private void EnteredTile(Vector3Int pos)
    {
        CrackTile(pos);
        if(_crackingTiles.TryGetValue(pos, out var tile))
            tile.PlayerOnMe = true;
    }

    private void ExitedTile(Vector3Int pos)
    {
        CrackTile(pos);
        if (_crackingTiles.TryGetValue(pos, out var tile))
            tile.PlayerOnMe = false;
    }

    public void AddTile(GroundTile tile)
    {
        _activeTiles.Add(tile.Cell, tile);
    }

    private void UpdateLinks(Vector3Int pos)
    {
        var removeList = new List<OffMeshLink>();

        foreach (var (key, tiles) in _offMeshLinks)
        {
            if (tiles.Item1 == pos || tiles.Item2 == pos)
            {
                removeList.Add(key);
            }
        }

        foreach (var link in removeList)
        {
            _offMeshLinks.Remove(link);
            Destroy(link);
        }
        
        GenerateNewLinks(pos);
    }

    private void GenerateNewLinks(Vector3Int pos)
    {
        var n = pos + new Vector3Int(1, 1, 0);
        var ne = pos + new Vector3Int(1, 0, 0);
        var e = pos + new Vector3Int(1, -1, 0);
        var se = pos + new Vector3Int(0, -1, 0);
        var s = pos + new Vector3Int(-1, -1, 0);
        var sw = pos + new Vector3Int(-1, 0, 0);
        var w = pos + new Vector3Int(-1, 1, 0);
        var nw = pos + new Vector3Int(0, 1, 0);

        var allTiles = _activeTiles.Concat(_crackingTiles).ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
        
        if (allTiles.TryGetValue(n, out var tileN) && allTiles.TryGetValue(s, out var tileS))
            CreateLink(tileN, tileS);
        
        if (allTiles.TryGetValue(ne, out var tileNE) && allTiles.TryGetValue(sw, out var tileSW))
            CreateLink(tileNE, tileSW);
            
        if (allTiles.TryGetValue(e, out var tileE) && allTiles.TryGetValue(w, out var tileW))
            CreateLink(tileE, tileW);
            
        if (allTiles.TryGetValue(se, out var tileSE) && allTiles.TryGetValue(nw, out var tileNW))
            CreateLink(tileSE, tileNW);
            

        //TODO generate links at start to store them
    }

    private void CreateLink(GroundTile a, GroundTile b)
    {
        var link = gameObject.AddComponent<OffMeshLink>();
        link.startTransform = a.transform;
        link.endTransform = b.transform;
        link.biDirectional = true;
        _offMeshLinks.Add(link, new Tuple<Vector3Int, Vector3Int>(a.Cell, b.Cell));
        
        // link.UpdatePositions();
        // _offMeshLinks.Add(link);
        // link.startTransform = new Vector3(a.x, 0.5f, a.y) - _offset;
        // link.endPoint = new Vector3(b.x, 0.5f, b.y) - _offset;
        // link.bidirectional = true;
        // link.width = 1f;
    }
}
