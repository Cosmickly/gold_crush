using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Boundary : MonoBehaviour
{
    public TilemapManager TilemapManager { private get; set; }
    private BoxCollider[] _boundaryColliders;

    private void Awake()
    {
        _boundaryColliders = GetComponents<BoxCollider>();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.gameObject.TryGetComponent(out IEntity entity)) return;
        entity.Fall();
        if (other.gameObject.TryGetComponent(out BasePlayer player))
        {
            TilemapManager.PlayerFell(player); //TODO should message game manager -> tilemap manager & scoreboard
        }
    }

    public void BuildBoundary(Vector2Int tilemapSize)
    {
        if (_boundaryColliders.Length < 1) return;
        
        var deathBoundary = _boundaryColliders[0];
        var size = new Vector3(tilemapSize.x, 1, tilemapSize.y);
        deathBoundary.size = size;
        deathBoundary.center = new Vector3(size.x / 2, -0.5f, size.z / 2);
        
        if (_boundaryColliders.Length < 5) return;
        
        var hSize = new Vector3(size.x, 10f, 1f);
        var vSize = new Vector3(1f, 10f, size.z);
        
        // s
        _boundaryColliders[1].size = hSize;
        _boundaryColliders[1].center = new Vector3(size.x / 2, 0f, -0.5f);

        // w
        _boundaryColliders[2].size = vSize;
        _boundaryColliders[2].center = new Vector3(-0.5f, 0f, size.z / 2);
        
        // n
        _boundaryColliders[3].size = hSize;
        _boundaryColliders[3].center = new Vector3(size.x / 2, 0f, size.z + 0.5f);

        // e
        _boundaryColliders[4].size = vSize;
        _boundaryColliders[4].center = new Vector3(size.x + 0.5f, 0f, size.z / 2);
    }
}
