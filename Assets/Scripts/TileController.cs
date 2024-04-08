using System;
using System.Collections;
using System.Collections.Generic;
using Unity.AI.Navigation;
using UnityEngine;
using UnityEngine.AI;

public class TileController : MonoBehaviour
{
    private TilemapManager _tilemapManager;
    
    private BoxCollider _collider;
    private MeshRenderer _mesh;
    private Color _initialColor;
    private NavMeshObstacle _navMeshObstacle;
    
    public bool Cracking { get; set; }
    [SerializeField] private float _crackTime;
    private float _crackTimer;


    private void Awake()
    {
        _collider = GetComponent<BoxCollider>();
        _mesh = GetComponent<MeshRenderer>();
        _navMeshObstacle = GetComponentInChildren<NavMeshObstacle>();
        _initialColor = _mesh.material.color;
    }

    private void FixedUpdate()
    {
        if (!Cracking) return;
        
        if (_crackTimer >= _crackTime)
        {
            _tilemapManager.BreakTile(transform.position);
        }

        _crackTimer += Time.deltaTime;
        _mesh.material.color = Color.Lerp(_initialColor, Color.black, _crackTimer / _crackTime);
    }

    public void Break()
    {
        Cracking = false;
        _mesh.enabled = false;
        gameObject.layer = LayerMask.NameToLayer("Ground");
        _navMeshObstacle.enabled = true;
    }

    public void SetTilemapManager(TilemapManager manager)
    {
        _tilemapManager = manager;
    }
}
