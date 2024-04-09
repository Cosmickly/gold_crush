using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.AI.Navigation;
using UnityEngine;
using UnityEngine.Tilemaps;

public class TilemapManager : MonoBehaviour
{
    private Tilemap _tilemap;
    private BoxCollider _boundary;
    private Dictionary<Vector3Int, TileController> _tiles = new();
    [SerializeField] private bool _tileCrackEnabled;

    private NavMeshSurface _navMeshSurface;

    private void Awake()
    {
        _tilemap = GetComponent<Tilemap>();
        _boundary = GetComponent<BoxCollider>();
        _navMeshSurface = GetComponent<NavMeshSurface>();
    }

    private void Start()
    {
        foreach (TileController tile in GetComponentsInChildren<TileController>())
        {
            tile.SetTilemapManager(this);
            _tiles.Add(_tilemap.WorldToCell(tile.transform.position), tile);
        }

        var orderedKeys = _tiles.Keys.OrderBy(k => k.magnitude);
        var min = orderedKeys.First();
        var max = orderedKeys.Last();
        
        if (!_boundary) return;
        var size = new Vector3(max.x - min.x + 1, 1, max.y - min.y + 1);
        _boundary.size = size;
        _boundary.center = new Vector3(min.x + size.x/2, -0.5f, min.y + size.z/2);
        
        _navMeshSurface.BuildNavMesh();
    }

    public Vector3Int GetCell(Vector3 pos)
    {
        return _tilemap.WorldToCell(pos);
    }

    public bool HasTile(Vector3 pos)
    {
        return _tiles.TryGetValue(_tilemap.WorldToCell(pos), out _);
    }

    public void CrackTile(Vector3 pos)
    {
        if (_tileCrackEnabled && _tiles.TryGetValue(_tilemap.WorldToCell(pos), out TileController tile) && !tile.Cracking)
        {
            tile.Cracking = true;
        }
    }
    
    public void BreakTile(Vector3 pos)
    {
        if (_tiles.Remove(_tilemap.WorldToCell(pos), out TileController tile))
        {
            tile.Break();
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
