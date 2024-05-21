using System;
using System.Collections.Generic;
using Entities;
using Unity.AI.Navigation;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Tiles
{
    public class TilemapBuilder : MonoBehaviour
    {
        [SerializeField] private GameManager _gameManager;
        private TilemapManager _tilemapManager;
        private NavMeshSurface _navMeshSurface;
        [SerializeField] private Boundary _boundary;
    
        [SerializeField] public Vector2Int TilemapSize;

        private readonly Vector3Int _topLayerOffset = new(0, 1, 0);
    
        [SerializeField] [Range(0, 1)] private float _iceTileRateMax;
        private const float Scale = 0.1f;

        [Header("Obstacle Parameters")]
        [SerializeField] [Range(0, 100)] private int _obstacleFillRate;
        [SerializeField] [Range(0, 100)] private int _layoutFillRate;
        [SerializeField] [Range(0, 10)] private int _obstacleSmoothRate;
        [SerializeField] [Range(0, 100)] private int _goldChunkRateMax;

    
        [Header("Prefabs")] 
        [SerializeField] private GroundTile _groundTile;
        [SerializeField] private RockObstacle _rockObstaclePrefab;
        [SerializeField] private GoldOre _goldOrePrefab;


        private int[][] _obstacleMap;

        private float _intensityRatio;
        
        private void Awake()
        {
            _tilemapManager = GetComponent<TilemapManager>();
            _navMeshSurface = GetComponent<NavMeshSurface>();
            
            InstantiateTiles();
            _navMeshSurface.BuildNavMesh();
        }

        private void Start()
        {
            _boundary.BuildBoundary(TilemapSize);
        }

        private void InstantiateTiles()
        {
            for (int i = 0; i < TilemapSize.x; i++)
            {
                for (int j = 0; j < TilemapSize.y; j++)
                {
                    var pos = new Vector3Int(i, 0, j);
                    GroundTile tile = Instantiate(_groundTile, pos, Quaternion.identity, transform);
                    tile.Cell = _tilemapManager.GetCell(pos);
                    tile.TilemapManager = _tilemapManager;
                    _tilemapManager.AllTiles.Add(tile.Cell, tile);
                }
            }
        }
        
        public void Build()
        {
            _intensityRatio = (float) _gameManager.CurrentLevel / _gameManager.MaxLevel;
            CellularAutomataGround();
            CellularAutomataObstacles();
            InstantiateObstacles();
        }

        private void CellularAutomataGround()
        {
            int[][] groundTileMap = new int[TilemapSize.x][];
            for (int i = 0; i < TilemapSize.x; i++) groundTileMap[i] = new int[TilemapSize.y];
            var offsetX = Random.Range(0,100);
            var offsetY = Random.Range(0,100);
            
        
            for (int i = 0; i < TilemapSize.x; i++)
            {
                for (int j = 0; j < TilemapSize.y; j++)
                {
                    if (i is < 5 or > 19 && j is < 5 or > 19)
                    {
                        groundTileMap[i][j] = 0;
                        continue;
                    }

                    var perlin = Mathf.PerlinNoise(Scale * i + offsetX, Scale * j + offsetY);
                    groundTileMap[i][j] = perlin < (_intensityRatio * _iceTileRateMax) ? 1 : 0;
                }
            }
            
            for (int i = 0; i < TilemapSize.x; i++)
            {
                for (int j = 0; j < TilemapSize.y; j++)
                {
                    var pos = new Vector3Int(i, 0, j);
                    var cell = _tilemapManager.GetCell(pos);
                    var tile = _tilemapManager.AllTiles[cell];
                    tile.Rebuild();
                    tile.ToggleIce(groundTileMap[i][j] == 1);
                }
            }

        }
    
        private void CellularAutomataObstacles()
        {
            _obstacleMap = new int[TilemapSize.x][];
            for (int i = 0; i < TilemapSize.x; i++) _obstacleMap[i] = new int[TilemapSize.y];
            int[][] removeMap = new int[TilemapSize.x][];
            for (int i = 0; i < TilemapSize.x; i++) removeMap[i] = new int[TilemapSize.y];

            for (int i = 0; i < TilemapSize.x; i++)
            {
                for (int j = 0; j < TilemapSize.y; j++)
                {
                    if (i is < 5 or > 19 && j is < 5 or > 19) continue;

                    if (i is 0 or 5 or 10 or 14 or 19 or 24 || j is 0 or 5 or 10 or 14 or 19 or 24)
                    {
                        _obstacleMap[i][j] = Random.Range(0, 100) < _layoutFillRate ? 1 : 0;
                    }
                
                    _obstacleMap[i][j] = Random.Range(0, 100) < _obstacleFillRate ? 1 : _obstacleMap[i][j];
                }
            }
        
            for (int i = 0; i < _obstacleSmoothRate; i++) _obstacleMap = SmoothMap(_obstacleMap, true);
        }

        private void InstantiateObstacles()
        {
            for (int i = 0; i < TilemapSize.x; i++)
            {
                for (int j = 0; j < TilemapSize.y; j++)
                {
                    if (i is 1 or 4 or 9 or 15 or 20 or 23 || j is 1 or 4 or 9 or 15 or 20 or 23)
                    {
                        if (Random.Range(0, 100) < _layoutFillRate) continue;
                    }

                    if (_obstacleMap[i][j] != 1) continue;
                    
                    var pos = new Vector3Int(i, 0, j) + _topLayerOffset;
                    RockObstacle obstacle;
                    if (i is > 5 and < 20 && j is > 5 and < 20 
                                          && Random.Range(0, 100) < _intensityRatio * _goldChunkRateMax)
                    {
                        obstacle = Instantiate(_goldOrePrefab, pos, Quaternion.identity, transform);
                            
                    }
                    else
                    {
                        obstacle = Instantiate(_rockObstaclePrefab, pos, Quaternion.identity, transform);
                    }
                    obstacle.Cell = _tilemapManager.GetCell(pos);
                    obstacle.TilemapManager = _tilemapManager;
                    _tilemapManager.Obstacles.Add(obstacle.Cell, obstacle);
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
            for (int i = 1; i < TilemapSize.x-1; i++)
            {
                for (int j = 1; j < TilemapSize.y-1; j++)
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
            for (int i = 0; i < TilemapSize.x; i++)
            {
                for (int j = 0; j < TilemapSize.y; j++)
                {
                    var pos = new Vector3Int(i, j, 0);
                    if (!_tilemapManager.AllTiles.ContainsKey(pos))
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
}
