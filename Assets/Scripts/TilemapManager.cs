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

    [SerializeField] private bool _goldEnabled;
    [SerializeField] private bool _tileCrackEnabled;
    [SerializeField] private float _randomTileRate;
    
    [Header("Prefabs")] 
    [SerializeField] private TileController _groundTile;
    [SerializeField] private TileController _iceTile;
    [SerializeField] private GoldPiece _goldPiecePrefab;
    [SerializeField] private RockObstacle _rockObstaclePrefab;
    
    private readonly Vector3 _topLayerOffset = new(0, 1, 0);

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
            tile.SetTilemapManager(this);
            _activeTiles.Add(_tilemap.WorldToCell(pos), tile);
        
            SpawnTopLayerObject(pos);
        }
    }

    private void BuildTiles()
    {
        for (int i = 0; i < TilemapSize.x; i++)
        {
            for (int j = 0; j < TilemapSize.y; j++)
            {
                var pos = new Vector3(i, 0, j);
                var tilePrefab = Random.value <= 0.9 ? _groundTile : _iceTile;
                var tile = Instantiate(tilePrefab, pos, Quaternion.identity, transform);
                tile.SetTilemapManager(this);
                _activeTiles.Add(_tilemap.WorldToCell(pos), tile);
                
                SpawnTopLayerObject(pos);
            }
        }
    }

    private void SpawnTopLayerObject(Vector3 pos)
    {
        var rand = Random.value;
        if (_goldEnabled && rand <= 0.1f)
            Instantiate(_goldPiecePrefab, pos + _topLayerOffset, Quaternion.identity, transform);
        if (rand is > 0.1f and <= 0.2f)
            Instantiate(_rockObstaclePrefab, pos + _topLayerOffset, Quaternion.identity, transform);
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
        _boundary.center = new Vector3(min.x + size.x/2 - 0.5f, -0.5f, min.y + size.z/2 - 0.5f);
    }

    public Vector3Int GetCell(Vector3 pos)
    {
        return _tilemap.WorldToCell(pos);
    }

    public bool HasTile(Vector3 pos)
    {
        return _activeTiles.TryGetValue(_tilemap.WorldToCell(pos), out _);
    }

    public void CrackTile(Vector3Int pos)
    {
        if (_tileCrackEnabled && _activeTiles.Remove(pos, out TileController tile))
        {
            _crackingTiles.Add(pos, tile);
            tile.Cracking = true;
        }
    }
    
    public void BreakTile(Vector3 pos)
    {
        if (_crackingTiles.Remove(_tilemap.WorldToCell(pos), out TileController tile))
        {
            tile.Break();
        }
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
}
