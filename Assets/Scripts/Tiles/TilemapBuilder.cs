using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.AI.Navigation;
using UnityEngine;
using UnityEngine.Tilemaps;
using Random = UnityEngine.Random;

public class TilemapBuilder : MonoBehaviour
{
    private TilemapManager _tilemapManager;
    private Tilemap _tilemap;
    private NavMeshSurface _navMeshSurface;
    private BoxCollider _boundary;

    
    public Vector2Int TilemapSize;
    private readonly Vector3Int _topLayerOffset = new(0, 1, 0);

    [Header("Prefabs")] 
    [SerializeField] private GroundTile _rockTile;
    [SerializeField] private GroundTile _iceTile;
    [SerializeField] private GoldPiece _goldPiecePrefab;
    [SerializeField] private RockObstacle _rockObstaclePrefab;


    private void Awake()
    {
        _tilemapManager = GetComponent<TilemapManager>();
        _tilemap = GetComponent<Tilemap>();
        _navMeshSurface = GetComponent<NavMeshSurface>();
        _boundary = GetComponent<BoxCollider>();
    }

    public void Build()
    {
        BuildTiles();
        _navMeshSurface.BuildNavMesh();
        BuildBoundary();
    }
    
    private void BuildTiles()
    {
        for (int i = 0; i < TilemapSize.x; i++)
        {
            for (int j = 0; j < TilemapSize.y; j++)
            {
                var pos = new Vector3Int(i, 0, j);
                var tilePrefab = Random.value <= 0.9 ? _rockTile : _iceTile;
                var tile = Instantiate(tilePrefab, pos, Quaternion.identity, transform);
                tile.Cell = _tilemap.WorldToCell(pos);
                tile.TilemapManager = _tilemapManager;
                _tilemapManager.AddTile(tile);
                
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
            goldPiece.Cell = pos;
            goldPiece.TilemapManager = _tilemapManager;
        }

        if (rand is > 0.1f and <= 0.2f)
        {
            var obstacle = Instantiate(_rockObstaclePrefab, pos, Quaternion.identity, transform);
            obstacle.Cell = pos;
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
            _tilemapManager.AddTile(tile);
        
            // SpawnTopLayerObject(pos);
        }
    }
    
    private void BuildBoundary()
    {
        // if (_tilemapManager._activeTiles.Count <=0) return;
        // var orderedKeys = _tilemapManager._activeTiles.Keys.OrderBy(k => k.magnitude).ToList();
        // var min = orderedKeys.First();
        // var max = orderedKeys.Last();
        
        if (!_boundary) return;
        var size = new Vector3(TilemapSize.x, 1, TilemapSize.y);
        _boundary.size = size;
        _boundary.center = new Vector3(size.x/2, -0.5f, size.z/2);
    }
}
