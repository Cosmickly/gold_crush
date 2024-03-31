using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Tilemaps;

public class TilemapManager : MonoBehaviour
{
    private Tilemap _tilemap;
    private BoxCollider _collider;
    private Dictionary<Vector3Int, Transform> _tiles = new();
    
    // Start is called before the first frame update
    void Start()
    {
        _tilemap = GetComponent<Tilemap>();
        _collider = GetComponent<BoxCollider>();
        
        foreach (Transform tile in GetComponentInChildren<Transform>())
        {
            _tiles.Add(_tilemap.WorldToCell(tile.position), tile);
        }
        
        // Debug.Log("Has tile 0,0,0 " + _tiles.ContainsKey(Vector3Int.zero));
        // Debug.Log("Has tile -100,0,0 " + _tiles.ContainsKey(new Vector3Int(-100,0,0)));

        var max = _tiles.Keys.OrderBy(k => k.magnitude).Last();
        var bounds = new Vector3(max.x + 1, 1, max.y + 1);
        _collider.size = bounds;
        _collider.center = new Vector3(bounds.x/2, 0, bounds.z/2);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public Vector3Int GetCell(Vector3 pos)
    {
        return _tilemap.WorldToCell(pos);
    }

    public bool HasTile(Vector3 pos)
    {
        return _tiles.ContainsKey(_tilemap.WorldToCell(pos));
    }

    public bool HasTile(Vector3Int pos)
    {
        return _tiles.ContainsKey(pos);
    }
}
