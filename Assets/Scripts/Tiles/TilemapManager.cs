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

    public Dictionary<Vector3Int, GroundTile> ActiveTiles { private get; set; } = new();
    private Dictionary<Vector3Int, GroundTile> _crackingTiles = new();
    // private Dictionary<OffMeshLink, Tuple<Vector3Int, Vector3Int>> OffMeshLinks { get; set; } = new();
    private Dictionary<NavMeshLink, Tuple<Vector3Int, Vector3Int>> NavMeshLinks { get; set; } = new();
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
        if (_tileCrackEnabled && ActiveTiles.Remove(pos, out GroundTile tile))
        {
            _crackingTiles.Add(pos, tile);
            tile.Cracking = true;
        }
    }
    
    public bool RemoveTile(Vector3Int pos)
    {
        if (_crackingTiles.ContainsKey(pos))
        {
            ClearLinks(pos);
            GenerateNewLinks(pos);
            _crackingTiles.Remove(pos);
            return true;
        }
        
        return false;
    }
    
    private Vector3Int RandomTile()
    {
        var keys = ActiveTiles.Keys.ToList();
        var randomInt = Random.Range(0, keys.Count);
        var key = keys[randomInt];
        return key;
    }
    
    private void CrackRandomTile()
    {
        if (ActiveTiles.Count <= 0)
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

    // public bool HasTile(Vector3Int pos)
    // {
    //     return ActiveTiles.ContainsKey(pos);
    // }

    private void ClearLinks(Vector3Int pos)
    {
        var removeList = new List<NavMeshLink>();

        foreach (var (key, tiles) in NavMeshLinks)
        {
            if (tiles.Item1 == pos || tiles.Item2 == pos)
            {
                removeList.Add(key);
            }
        }

        foreach (var link in removeList)
        {
            NavMeshLinks.Remove(link);
            Destroy(link);
        }
    }

    public void GenerateNewLinks(Vector3Int pos)
    {
        var n = pos + new Vector3Int(1, 1, 0);
        var ne = pos + new Vector3Int(1, 0, 0);
        var e = pos + new Vector3Int(1, -1, 0);
        var se = pos + new Vector3Int(0, -1, 0);
        var s = pos + new Vector3Int(-1, -1, 0);
        var sw = pos + new Vector3Int(-1, 0, 0);
        var w = pos + new Vector3Int(-1, 1, 0);
        var nw = pos + new Vector3Int(0, 1, 0);

        var allTiles = ActiveTiles.Concat(_crackingTiles).ToDictionary(kvp => kvp.Key, kvp => kvp.Value);

        if (allTiles.ContainsKey(n) && allTiles.ContainsKey(s))
            CreateNavMeshLink(n, s, 0.2f);
        
        if (allTiles.ContainsKey(e) && allTiles.ContainsKey(w))
            CreateNavMeshLink(e, w, 0.2f);
        
        if (allTiles.ContainsKey(ne) && allTiles.ContainsKey(sw))
            CreateNavMeshLink(ne, sw, 0.8f);
            
        if (allTiles.ContainsKey(se) && allTiles.ContainsKey(nw))
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
        NavMeshLinks.Add(link, new Tuple<Vector3Int, Vector3Int>(cellA, cellB));
    }

    private Vector3 CellToWorld(Vector3Int cell)
    {
        return new Vector3(cell.x + 0.5f, 0.5f, cell.y + 0.5f);
    }
}
