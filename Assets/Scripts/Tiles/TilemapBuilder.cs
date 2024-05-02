using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.AI.Navigation;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Tilemaps;
using Random = System.Random;

public class TilemapBuilder : MonoBehaviour
{
    private Random _random = new();
    private TilemapManager _tilemapManager;
    
    [SerializeField] private Vector2Int _tilemapSize;
    private NavMeshSurface _navMeshSurface;

    private readonly Vector3Int _topLayerOffset = new(0, 1, 0);
    
    [SerializeField] private Boundary _boundary;
    
    [SerializeField] [Range(0, 100)] private int _iceTileRate;
    [SerializeField] [Range(0, 10)] private int _iceSmoothRate;
    
    [Header("Obstacle Parameters")]
    [SerializeField] [Range(0, 100)] private int _obstacleFillRate;
    [SerializeField] [Range(0, 100)] private int _layoutFillRate;
    [SerializeField] [Range(0, 10)] private int _obstacleSmoothRate;
    // [SerializeField] [Range(0, 1)] private float _obstacleRemoveRate;
    // [SerializeField] [Range(0, 9)] private int _neighbourThreshold;
    
    [Header("Prefabs")] 
    [SerializeField] private GroundTile _rockTile;
    [SerializeField] private GroundTile _iceTile;
    [SerializeField] private GoldPiece _goldPiecePrefab;
    [SerializeField] private RockObstacle _rockObstaclePrefab;
    // [SerializeField] private List<GameObject> _layouts = new();
    // [SerializeField] private List<GameObject> _obstacleLayouts = new();
    // [SerializeField] private Vector2Int _layoutSize;
    // [SerializeField] private Vector2Int _numOfLayouts;

    private void Awake()
    {
        _tilemapManager = GetComponent<TilemapManager>();
        _navMeshSurface = GetComponent<NavMeshSurface>();
        _boundary.TilemapManager = _tilemapManager;
    }

    public void Build()
    {
        CellularAutomataGround();
        CellularAutomataObstacles();
        
        // FindEmptyTiles();
        
        _navMeshSurface.BuildNavMesh();
        _boundary.BuildBoundary(_tilemapSize);
    }
    

    private void CellularAutomataGround()
    {
        int[][] groundTileMap = new int[_tilemapSize.x][];
        for (int i = 0; i < _tilemapSize.x; i++) groundTileMap[i] = new int[_tilemapSize.y];
        
        for (int i = 0; i < _tilemapSize.x; i++)
        {
            for (int j = 0; j < _tilemapSize.y; j++)
            {
                if (i is < 5 or > 19 && j is < 5 or > 19)
                {
                    groundTileMap[i][j] = 0;
                    continue;
                }
                groundTileMap[i][j] = _random.Next(100) < _iceTileRate ? 1 : 0;
            }
        }

        for (int i = 0; i < _iceSmoothRate; i++) groundTileMap = SmoothMap(groundTileMap);

        for (int i = 0; i < _tilemapSize.x; i++)
        {
            for (int j = 0; j < _tilemapSize.y; j++)
            {
                var pos = new Vector3Int(i, 0, j);
                GroundTile tile;
                tile = Instantiate(groundTileMap[i][j] == 1 ? _iceTile : _rockTile, pos, Quaternion.identity, transform);

                tile.Cell = _tilemapManager.GetCell(pos);
                tile.TilemapManager = _tilemapManager;
                _tilemapManager.ActiveTiles.Add(tile.Cell, tile);
            }
        }
    }
    
    private void CellularAutomataObstacles()
    {
        int[][] obstacleMap = new int[_tilemapSize.x][];
        for (int i = 0; i < _tilemapSize.x; i++) obstacleMap[i] = new int[_tilemapSize.y];
        int[][] removeMap = new int[_tilemapSize.x][];
        for (int i = 0; i < _tilemapSize.x; i++) removeMap[i] = new int[_tilemapSize.y];

        for (int i = 0; i < _tilemapSize.x; i++)
        {
            for (int j = 0; j < _tilemapSize.y; j++)
            {
                if (i is < 5 or > 19 && j is < 5 or > 19) continue;

                if (i is 0 or 5 or 10 or 14 or 19 or 24 || j is 0 or 5 or 10 or 14 or 19 or 24)
                {
                    obstacleMap[i][j] = _random.Next(100) < _layoutFillRate ? 1 : 0;
                }
                
                if (i is 1 or 4 or 9 or 15 or 20 or 23 || j is 1 or 4 or 9 or 15 or 20 or 23)
                {
                    removeMap[i][j] = _random.Next(100) < _layoutFillRate ? 1 : 0;
                }
                obstacleMap[i][j] = _random.Next(100) < _obstacleFillRate ? 1 : obstacleMap[i][j];
            }
        }
        
        for (int i = 0; i < _obstacleSmoothRate; i++) obstacleMap = SmoothMap(obstacleMap, true);

        for (int i = 0; i < _tilemapSize.x; i++)
        {
            for (int j = 0; j < _tilemapSize.y; j++)
            {
                if (obstacleMap[i][j] == 1 && removeMap[i][j] == 0)
                {
                    var pos = new Vector3Int(i, 0, j) + _topLayerOffset;
                    var obstacle = Instantiate(_rockObstaclePrefab, pos, Quaternion.identity, transform);
                    obstacle.Cell = _tilemapManager.GetCell(pos);
                    obstacle.TilemapManager = _tilemapManager;
                    _tilemapManager.Obstacles.Add(obstacle.Cell, obstacle);
                }
            }
        }
    }

    private int GetNeighbourCount(int centerX, int centerY, int[][] map)
    {
        int count = 0;
        for (int i = centerX - 1; i <= centerX + 1; i++)
        {
            for (int j = centerY - 1; j <= centerY + 1; j++)
            {
                // if (i < 0 || j < 0 || i >= _tilemapSize.x || j >= _tilemapSize.y)
                // {
                //     count++;
                //     continue;
                // }
                if (i == centerX && j == centerY) continue;
                count += map[i][j];
            }
        }
        return count;
    }

    private int GetAdjacentObstacleCount(int centerX, int centerY, int[][] map)
    {
        int count = 0;

        count += map[centerX - 1][centerY];
        count += map[centerX + 1][centerY];
        count += map[centerX][centerY - 1];
        count += map[centerX][centerY + 1];

        return count;
    }
    
    private int[][] SmoothMap(int[][] map, bool useAdjacent = false)
    {
        int neighbourLimit = useAdjacent ? 2 : 4;
        for (int i = 1; i < _tilemapSize.x-1; i++)
        {
            for (int j = 1; j < _tilemapSize.y-1; j++)
            {
                int neighbourCount = useAdjacent ? GetAdjacentObstacleCount(i, j, map) : GetNeighbourCount(i, j, map);
                if (neighbourCount > neighbourLimit) map[i][j] = 1;
                if (neighbourCount < neighbourLimit) map[i][j] = 0;
            }
        }
        return map;
    }
    
    // Only needed if empty tiles exist at start
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
    
    // private void GetObstaclesFromChildren()
    // {
    //     foreach (var rockObstacle in GetComponentsInChildren<RockObstacle>())
    //     {
    //         rockObstacle.Cell = _tilemap.WorldToCell(rockObstacle.transform.position);
    //         _obstacles.Add(rockObstacle.Cell, rockObstacle);
    //     }
    // }
    
    // private void SpawnObstacleLayout(Vector3Int pos, GameObject layout)
    // {
    //     var tiles = layout.GetComponentsInChildren<GroundTile>();
    //     var tilePositions = (from tile in tiles select tile.transform.localPosition).ToHashSet();
    //     var obstacleObject = _obstacleLayouts[_random.Next(_obstacleLayouts.Count)];
    //     var obstacles = obstacleObject.GetComponentsInChildren<RockObstacle>();
    //     var obstaclePositions = (from obstacle in obstacles select obstacle.transform.localPosition).ToHashSet();
    //
    //     if (obstaclePositions.IsSubsetOf(tilePositions))
    //     {
    //         Instantiate(obstacleObject, pos + _topLayerOffset, Quaternion.identity, transform);
    //     }
    // }

    // private void SpawnIceTiles()
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
    //             if (perlin > _iceTileRate) continue;
    //             var cell = new Vector3Int(i, j, 0);
    //             
    //             if (!_tilemapManager.ActiveTiles.TryGetValue(cell, out var tile)) continue;
    //             Destroy(tile.gameObject);
    //             var iceTile = Instantiate(_iceTile, new Vector3(cell.x, 0, cell.y), Quaternion.identity, transform);
    //             iceTile.Cell = cell;
    //             iceTile.TilemapManager = _tilemapManager;
    //             _tilemapManager.ActiveTiles[cell] = iceTile;
    //         }
    //     }
    // }
    
    // private void SpawnLayouts()
    // {
    //     // Instantiate(_layouts[0], new Vector3(0, 0, 0), Quaternion.identity, transform);
    //     // Instantiate(_layouts[0], new Vector3(0, 0, 14), Quaternion.identity, transform);
    //     // Instantiate(_layouts[0], new Vector3(14, 0, 0), Quaternion.identity, transform);
    //     // Instantiate(_layouts[0], new Vector3(14, 0, 14), Quaternion.identity, transform);
    //     
    //     for (int i = 0; i < _numOfLayouts.x; i++)
    //     {
    //         for (int j = 0; j < _numOfLayouts.y; j++)
    //         {
    //             var pos = new Vector3Int(i * _layoutSize.x, 0, j * _layoutSize.y);
    //             GameObject layout = _layouts[0];
    //             if (i is not 0 and not 4 || j is not 0 and not 4)
    //             {
    //                 // layout = _layouts[Random.Range(0, _layouts.Count)];
    //                 // SpawnObstacleLayout(pos, layout); 
    //             }
    //             Instantiate(layout, pos, Quaternion.identity, transform);
    //         }
    //     }
    // }
    
    // private void GetTilesFromChildren()
    // {
    //     foreach (GroundTile tile in GetComponentsInChildren<GroundTile>())
    //     {
    //         var pos = tile.transform.position;
    //         tile.Cell = _tilemap.WorldToCell(pos);
    //         tile.TilemapManager = _tilemapManager;
    //         _tilemapManager.ActiveTiles.Add(tile.Cell, tile);
    //
    //         if (tile.Cell.x > _tilemapSize.x) _tilemapSize.x = tile.Cell.x;
    //         if (tile.Cell.y > _tilemapSize.y) _tilemapSize.y = tile.Cell.y;
    //     }
    //     _tilemapSize += new Vector2Int(1, 1);
    // }

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
