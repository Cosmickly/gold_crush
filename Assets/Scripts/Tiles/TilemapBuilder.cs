using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.AI.Navigation;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Tilemaps;
using Random = UnityEngine.Random;

public class TilemapBuilder : MonoBehaviour
{
    private TilemapManager _tilemapManager;
    private Tilemap _tilemap;
    private NavMeshSurface _navMeshSurface;
    [SerializeField] private Boundary _boundary;
    
    private Vector2Int _tilemapSize;
    private readonly Vector3Int _topLayerOffset = new(0, 1, 0);

    [Header("Prefabs")] 
    [SerializeField] private GroundTile _rockTile;
    [SerializeField] private GroundTile _iceTile;
    [SerializeField] private GoldPiece _goldPiecePrefab;
    [SerializeField] private RockObstacle _rockObstaclePrefab;
    [SerializeField] private List<GameObject> _layouts = new();

    private void Awake()
    {
        _tilemapManager = GetComponent<TilemapManager>();
        _tilemap = GetComponent<Tilemap>();
        _navMeshSurface = GetComponent<NavMeshSurface>();
        _boundary.TilemapManager = _tilemapManager;
    }

    public void Build()
    {
        // BuildTiles();
        SpawnLayouts();
        GetTilesFromChildren();
        
        FindEmptyTiles();
        _navMeshSurface.BuildNavMesh();
        _boundary.BuildBoundary(_tilemapSize);
    }
    
    private void BuildTiles()
    {
        for (int i = 0; i < _tilemapSize.x; i++)
        {
            for (int j = 0; j < _tilemapSize.y; j++)
            {
                var pos = new Vector3Int(i, 0, j);
                var tilePrefab = Random.value <= 0.9 ? _rockTile : _iceTile;
                var tile = Instantiate(tilePrefab, pos, Quaternion.identity, transform);
                tile.Cell = _tilemap.WorldToCell(pos);
                tile.TilemapManager = _tilemapManager;
                _tilemapManager.ActiveTiles.Add(tile.Cell, tile);
                
                SpawnTopLayerObject(pos + _topLayerOffset);
            }
        }
    }

    private void SpawnTopLayerObject(Vector3Int pos)
    {
        var rand = Random.value;
        if (_tilemapManager.GoldEnabled && rand <= 0.1f)
        {
            var goldPiece = Instantiate(_goldPiecePrefab, pos, Quaternion.identity, transform);
            goldPiece.Cell = _tilemap.WorldToCell(pos);
            goldPiece.TilemapManager = _tilemapManager;
        }

        if (rand is > 0.1f and <= 0.2f)
        {
            var obstacle = Instantiate(_rockObstaclePrefab, pos, Quaternion.identity, transform);
            obstacle.Cell = _tilemap.WorldToCell(pos);
            obstacle.TilemapManager = _tilemapManager;
        }
    }
    
    private void GetTilesFromChildren()
    {
        foreach (GroundTile tile in GetComponentsInChildren<GroundTile>())
        {
            var pos = tile.transform.position;
            tile.Cell = _tilemap.WorldToCell(pos);
            tile.TilemapManager = _tilemapManager;
            _tilemapManager.ActiveTiles.Add(tile.Cell, tile);

            if (tile.Cell.x > _tilemapSize.x) _tilemapSize.x = tile.Cell.x;
            if (tile.Cell.y > _tilemapSize.y) _tilemapSize.y = tile.Cell.y;

            // SpawnTopLayerObject(pos);
        }

        _tilemapSize += new Vector2Int(1, 1);
    }

    private void SpawnLayouts()
    {
        // Instantiate(_layouts[0], new Vector3(0, 0, 0), Quaternion.identity, transform);
        // Instantiate(_layouts[0], new Vector3(0, 0, 14), Quaternion.identity, transform);
        // Instantiate(_layouts[0], new Vector3(14, 0, 0), Quaternion.identity, transform);
        // Instantiate(_layouts[0], new Vector3(14, 0, 14), Quaternion.identity, transform);

        
        for (int i = 0; i < 3; i++)
        {
            for (int j = 0; j < 3; j++)
            {
                if (i is 0 or 2 && j is 0 or 2)
                    Instantiate(_layouts[0], new Vector3(i*7, 0, j*7), Quaternion.identity, transform);
                else
                {
                    var layout = _layouts[Random.Range(1, _layouts.Count - 1)];
                    Instantiate(layout, new Vector3(i*7, 0, j*7), Quaternion.identity, transform);
                }
        
            }
        }
    }
    
    private void FindEmptyTiles()
    {
        for (int i = 0; i < _tilemapSize.x; i++)
        {
            for (int j = 0; j < _tilemapSize.y; j++)
            {
                var pos = new Vector3Int(i, j, 0);
                if (!_tilemapManager.ActiveTiles.ContainsKey(pos))
                {
                    _tilemapManager.GenerateNewLinks(pos);
                }
            }
        }
    }
}
