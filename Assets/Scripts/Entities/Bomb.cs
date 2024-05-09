using System;
using System.Collections;
using System.Collections.Generic;
using Interfaces;
using Tiles;
using UnityEngine;

public class Bomb : MonoBehaviour, IEntity
{
    private Rigidbody _rigidbody;
    [SerializeField] private float _radius;
    [SerializeField] private float _bombTime;
    private float _bombTimer;
    private bool _armed;
    
    private int TileMask => 1 << LayerMask.NameToLayer("Tile");

    private void Awake()
    {
        _rigidbody = GetComponent<Rigidbody>();
    }

    private void Start()
    {
        _bombTimer = _bombTime;
        ArmBomb();
    }

    private void Update()
    {
        if (_armed)
        {
            _bombTimer -= Time.deltaTime;
        }

        if (_bombTimer <= 0)
        {
            Explode();
        }
    }

    public void Fall()
    {
        Destroy(gameObject);
    }

    private void ArmBomb()
    {
        _armed = true;
    }

    private void Explode()
    {
        Collider[] hits = new Collider[64];
        int numFound = Physics.OverlapSphereNonAlloc(transform.position, _radius, hits, TileMask);

        for(int i = 0; i < numFound; i++)
        {
            if (hits[i].TryGetComponent(out GroundTile tile))
            {
                tile.InstantBreak();
            }
        }
        
        Destroy(gameObject);
    }
    
    public void Push(Vector3 direction)
    {
        _rigidbody.AddForce(direction, ForceMode.Impulse);
    }
}
