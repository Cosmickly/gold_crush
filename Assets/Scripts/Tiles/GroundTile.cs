using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.AI.Navigation;
using UnityEngine;
using UnityEngine.AI;

public class GroundTile : MonoBehaviour
{
    public TilemapManager TilemapManager { private get; set; }
    public Vector3Int Cell { get; set; }

    private BoxCollider _collider;
    private MeshRenderer _mesh;
    private Color _initialColor;
    private NavMeshObstacle _navMeshObstacle;
    
    public bool Cracking { get; set; }
    private float _crackTimer;
    [SerializeField] private float _crackTime;
    [SerializeField] private float _crackMultiplier;
    
    [SerializeField] private float _slipperiness;

    public bool PlayerOnMe
    {
        get => _playerOnMe;
        set => _playerOnMe = value;
    }
    [SerializeField] private bool _playerOnMe;

    public float Slipperiness
    {
        get => _slipperiness;
        private set => _slipperiness = value;
    }

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
            if (TilemapManager.RemoveTile(Cell))
            {
                Break();
                return;
            }
        }

        _crackTimer += PlayerOnMe ? Time.deltaTime * _crackMultiplier : Time.deltaTime;
        _mesh.material.color = Color.Lerp(_initialColor, Color.black, _crackTimer / _crackTime);
    }

    private void Break()
    {
        Cracking = false;
        _mesh.enabled = false;
        _collider.enabled = false;
        _navMeshObstacle.enabled = true;
    }
}
