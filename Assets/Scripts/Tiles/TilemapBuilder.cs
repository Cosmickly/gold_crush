using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.AI.Navigation;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Tilemaps;
using Random = System.Random;

// using Random = UnityEngine.Random;

public class TilemapBuilder : MonoBehaviour
{
    private Random _random = new();
    private TilemapManager _tilemapManager;
    private Tilemap _tilemap;
    [SerializeField] private Vector2Int _tilemapSize;
    private NavMeshSurface _navMeshSurface;

    private Dictionary<Vector3Int, RockObstacle> _obstacles = new();
    private readonly Vector3Int _topLayerOffset = new(0, 1, 0);
    
    [SerializeField] private Boundary _boundary;
    
    private int[,] _groundTileMap;
    [SerializeField] [Range(0, 100)] private int _iceTileRate;
    
    [Header("Obstacle Parameters")]
    private int[,] _obstacleMap;
    [SerializeField] [Range(0, 100)] private int _obstacleFillRate;
    [SerializeField] [Range(0, 100)] private int _layoutFillRate;
    [SerializeField] [Range(0, 10)] private int _smoothMapRate;
    // [SerializeField] [Range(0, 1)] private float _obstacleRemoveRate;
    // [SerializeField] [Range(0, 9)] private int _neighbourThreshold;
    
    [Header("Prefabs")] 
    [SerializeField] private GroundTile _rockTile;
    [SerializeField] private GroundTile _iceTile;
    [SerializeField] private GoldPiece _goldPiecePrefab;
    [SerializeField] private RockObstacle _rockObstaclePrefab;
    [SerializeField] private List<GameObject> _layouts = new();
    [SerializeField] private List<GameObject> _obstacleLayouts = new();
    [SerializeField] private Vector2Int _layoutSize;
    [SerializeField] private Vector2Int _numOfLayouts;

    private void Awake()
    {
        _tilemapManager = GetComponent<TilemapManager>();
        _tilemap = GetComponent<Tilemap>();
        _navMeshSurface = GetComponent<NavMeshSurface>();
        _boundary.TilemapManager = _tilemapManager;
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.R))
        {
            ClearObstacles();
            CellularAutomataObstacles();
        }
    }

    public void Build()
    {
        // BuildTiles();
        // SpawnLayouts();
        // GetTilesFromChildren();
        // GetObstaclesFromChildren();
        // SpawnIceTiles();
        
        // EditObstacles();
        // FindEmptyTiles();
        
        CellularAutomataGround();
        // CellularAutomataObstacles();
        
        _navMeshSurface.BuildNavMesh();
        _boundary.BuildBoundary(_tilemapSize);
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

    private void GetObstaclesFromChildren()
    {
        foreach (var rockObstacle in GetComponentsInChildren<RockObstacle>())
        {
            rockObstacle.Cell = _tilemap.WorldToCell(rockObstacle.transform.position);
            _obstacles.Add(rockObstacle.Cell, rockObstacle);
        }
    }
    
    private void SpawnLayouts()
    {
        // Instantiate(_layouts[0], new Vector3(0, 0, 0), Quaternion.identity, transform);
        // Instantiate(_layouts[0], new Vector3(0, 0, 14), Quaternion.identity, transform);
        // Instantiate(_layouts[0], new Vector3(14, 0, 0), Quaternion.identity, transform);
        // Instantiate(_layouts[0], new Vector3(14, 0, 14), Quaternion.identity, transform);
        
        for (int i = 0; i < _numOfLayouts.x; i++)
        {
            for (int j = 0; j < _numOfLayouts.y; j++)
            {
                var pos = new Vector3Int(i * _layoutSize.x, 0, j * _layoutSize.y);
                GameObject layout = _layouts[0];
                if (i is not 0 and not 4 || j is not 0 and not 4)
                {
                    // layout = _layouts[Random.Range(0, _layouts.Count)];
                    // SpawnObstacleLayout(pos, layout); 
                }
                Instantiate(layout, pos, Quaternion.identity, transform);
            }
        }
    }
    
    private void ClearObstacles()
    {
        foreach (var obstacle in _obstacles) Destroy(obstacle.Value.gameObject);
        _obstacles.Clear();
    }

    private void CellularAutomataGround()
    {
        _groundTileMap = new int[_tilemapSize.x, _tilemapSize.y];
        for (int i = 0; i < _tilemapSize.x; i++)
        {
            for (int j = 0; j < _tilemapSize.y; j++)
            {
                if (i is < 5 or > 19 && j is < 5 or > 19)
                {
                    _groundTileMap[i, j] = 0;
                    continue;
                }
                
                _groundTileMap[i, j] = _random.Next(100) < _iceTileRate ? 1 : 0;

            }
        }

        for (int i = 0; i < _smoothMapRate; i++) _groundTileMap = SmoothMap(_groundTileMap);

        for (int i = 0; i < _tilemapSize.x; i++)
        {
            for (int j = 0; j < _tilemapSize.y; j++)
            {
                var pos = new Vector3Int(i, 0, j);
                GroundTile tile;
                if (_groundTileMap[i, j] == 1)
                {
                    tile = Instantiate(_iceTile, pos, Quaternion.identity, transform);
                }
                else
                {
                    tile = Instantiate(_rockTile, pos, Quaternion.identity, transform);
                }
                tile.Cell = _tilemap.WorldToCell(pos);
                tile.TilemapManager = _tilemapManager;
                _tilemapManager.ActiveTiles.Add(tile.Cell, tile);
            }
        }
    }
    
    private void CellularAutomataObstacles()
    {
        _obstacleMap = new int[_tilemapSize.x, _tilemapSize.y];
        for (int i = 0; i < _tilemapSize.x; i++)
        {
            for (int j = 0; j < _tilemapSize.y; j++)
            {
                if (i is < 5 or > 19 && j is < 5 or > 19) continue;

                if (i == 0 || i == _tilemapSize.x - 1 || j == 0 || j == _tilemapSize.y - 1 || i % 5 == 0 || j % 5 == 0)
                {
                    _obstacleMap[i, j] = _random.Next(100) < _layoutFillRate ? 1 : 0;
                }
                _obstacleMap[i, j] = _random.Next(100) < _obstacleFillRate ? 1 : _obstacleMap[i, j];
            }
        }
        
        for (int i = 0; i < _smoothMapRate; i++) _obstacleMap = SmoothMap(_obstacleMap);

        for (int i = 0; i < _tilemapSize.x; i++)
        {
            for (int j = 0; j < _tilemapSize.y; j++)
            {
                if (_obstacleMap[i, j] == 1)
                {
                    var pos = new Vector3Int(i, 0, j) + _topLayerOffset;
                    var obstacle = Instantiate(_rockObstaclePrefab, pos, Quaternion.identity, transform);
                    obstacle.Cell = _tilemap.WorldToCell(pos);
                    obstacle.TilemapManager = _tilemapManager;
                    _obstacles.Add(obstacle.Cell, obstacle);
                }
            }
        }
    }

    private int GetNeighbouringObstacleCount(int centerX, int centerY, int[,] map)
    {
        int count = 0;

        count += map[centerX - 1, centerY];
        count += map[centerX + 1, centerY];
        count += map[centerX, centerY - 1];
        count += map[centerX, centerY + 1];

        return count;
    }
    
    private int[,] SmoothMap(int[,] map)
    {
        for (int i = 1; i < _tilemapSize.x-1; i++)
        {
            for (int j = 1; j < _tilemapSize.y-1; j++)
            {
                var neighbourCount = GetNeighbouringObstacleCount(i, j, map);
                if (neighbourCount > 2) map[i, j] = 1;
                if (neighbourCount < 2) map[i, j] = 0;
            }
        }

        return map;
    }

    private void SpawnObstacleLayout(Vector3Int pos, GameObject layout)
    {
        var tiles = layout.GetComponentsInChildren<GroundTile>();
        var tilePositions = (from tile in tiles select tile.transform.localPosition).ToHashSet();
        var obstacleObject = _obstacleLayouts[_random.Next(_obstacleLayouts.Count)];
        var obstacles = obstacleObject.GetComponentsInChildren<RockObstacle>();
        var obstaclePositions = (from obstacle in obstacles select obstacle.transform.localPosition).ToHashSet();

        if (obstaclePositions.IsSubsetOf(tilePositions))
        {
            Instantiate(obstacleObject, pos + _topLayerOffset, Quaternion.identity, transform);
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

    private void SpawnIceTiles()
    {
        var offsetX = _random.Next(0,100);
        var offsetY = _random.Next(0,100);
        for (int i = 0; i < _tilemapSize.x; i++)
        {
            for (int j = 0; j < _tilemapSize.y; j++)
            {
                if (i is < 5 or > 19 && j is < 5 or > 19) continue;

                var perlin = Mathf.PerlinNoise(i * 0.1f + offsetX, j * 0.1f + offsetY);
                
                if (perlin > _iceTileRate) continue;
                var cell = new Vector3Int(i, j, 0);
                
                if (!_tilemapManager.ActiveTiles.TryGetValue(cell, out var tile)) continue;
                Destroy(tile.gameObject);
                var iceTile = Instantiate(_iceTile, new Vector3(cell.x, 0, cell.y), Quaternion.identity, transform);
                iceTile.Cell = cell;
                iceTile.TilemapManager = _tilemapManager;
                _tilemapManager.ActiveTiles[cell] = iceTile;
            }
        }
    }

    // private void EditObstacles()
    // {
    //     var offsetX = _random.Next(0,100);
    //     var offsetY = _random.Next(0,100);
    //     for (int i = 0; i < _tilemapSize.x; i++)
    //     {
    //         for (int j = 0; j < _tilemapSize.y; j++)
    //         {
    //             if (i is < 5 or > 19 && j is < 5 or > 19) continue;
    //
    //             var perlin = Mathf.PerlinNoise(i * 0.1f + offsetX, j * 0.1f + offsetY);
    //             
    //             if (perlin > _obstacleRemoveRate) continue;
    //             var cell = new Vector3Int(i, j, 0);
    //
    //             if (!_obstacles.TryGetValue(cell, out var obstacle)) continue;
    //             Destroy(obstacle.gameObject);
    //             _obstacles.Remove(cell);
    //         }
    //     }
    // }
    
    // private void BuildTiles()
    // {
    //     for (int i = 0; i < _tilemapSize.x; i++)
    //     {
    //         for (int j = 0; j < _tilemapSize.y; j++)
    //         {
    //             var pos = new Vector3Int(i, 0, j);
    //             var tilePrefab = Random.value <= 0.9 ? _rockTile : _iceTile;
    //             var tile = Instantiate(tilePrefab, pos, Quaternion.identity, transform);
    //             tile.Cell = _tilemap.WorldToCell(pos);
    //             tile.TilemapManager = _tilemapManager;
    //             _tilemapManager.ActiveTiles.Add(tile.Cell, tile);
    //             
    //             SpawnTopLayerObject(pos + _topLayerOffset);
    //         }
    //     }
    // }

    // private void SpawnTopLayerObject(Vector3Int pos)
    // {
    //     var rand = Random.value;
    //     if (_tilemapManager.GoldEnabled && rand <= 0.1f)
    //     {
    //         var goldPiece = Instantiate(_goldPiecePrefab, pos, Quaternion.identity, transform);
    //         goldPiece.Cell = _tilemap.WorldToCell(pos);
    //         goldPiece.TilemapManager = _tilemapManager;
    //     }
    //     
    //     if (rand is > 0.1f and <= 0.2f)
    //     {
    //         var obstacle = Instantiate(_rockObstaclePrefab, pos, Quaternion.identity, transform);
    //         obstacle.Cell = _tilemap.WorldToCell(pos);
    //         obstacle.TilemapManager = _tilemapManager;
    //     }
    // }
}
