using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Tilemaps;

public class TilemapManager : MonoBehaviour
{
    private Tilemap _tilemap;
    private BoxCollider _collider;
    private Dictionary<Vector3Int, TileController> _tiles = new();

    private void Awake()
    {
        _tilemap = GetComponent<Tilemap>();
        _collider = GetComponent<BoxCollider>();
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
        
        var size = new Vector3(max.x - min.x + 1, 1, max.y - min.y + 1);
        _collider.size = size;
        _collider.center = new Vector3(min.x + size.x/2, 0, min.y + size.z/2);
        
        CrackTile(min);
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
        if (_tiles.TryGetValue(_tilemap.WorldToCell(pos), out TileController tile) && !tile.GetCracking())
        {
            tile.SetCracking(true);
        }
    }
    
    public void BreakTile(Vector3 pos)
    {
        if (_tiles.Remove(_tilemap.WorldToCell(pos), out TileController tile))
        {
            Destroy(tile.gameObject);
        }
    }
}
