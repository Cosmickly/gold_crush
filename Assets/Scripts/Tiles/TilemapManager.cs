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
    private TilemapBuilder _tilemapBuilder;
    private Tilemap _tilemap;

    private Dictionary<Vector3Int, GroundTile> _activeTiles = new();
    private Dictionary<Vector3Int, GroundTile> _crackingTiles = new();
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
        return _crackingTiles.Remove(pos);
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
}
