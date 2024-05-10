using System;
using System.Collections;
using System.Collections.Generic;
using Interfaces;
using Players;
using Tiles;
using UnityEngine;

public class Bomb : MonoBehaviour, IEntity
{
    private Collider _collider;
    private Rigidbody _rigidbody;
    private ParticleSystem _particleSystem;
    private GameObject _meshObject;
    [SerializeField] private float _radius;
    [SerializeField] private float _bombTime;
    private float _bombTimer;
    private bool _armed;
    
    private int TileMask => 1 << LayerMask.NameToLayer("Tile");

    private void Awake()
    {
        _collider = GetComponent<Collider>();
        _rigidbody = GetComponent<Rigidbody>();
        _particleSystem = GetComponent<ParticleSystem>();
        _meshObject = GetComponentInChildren<MeshRenderer>().gameObject;
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
        _armed = false;
        _bombTimer = _bombTime;
        Collider[] hits = new Collider[64];
        int numFound = Physics.OverlapSphereNonAlloc(transform.position, _radius, hits, TileMask);

        for(int i = 0; i < numFound; i++)
        {
            if (hits[i].TryGetComponent(out GroundTile tile))
            {
                tile.InstantBreak();
            }
        }

        _particleSystem.Play();
        StartCoroutine(DestroyBomb());
    }

    private IEnumerator DestroyBomb()
    {
        _rigidbody.useGravity = false;
        _rigidbody.velocity = Vector3.zero;
        _rigidbody.angularVelocity = Vector3.zero;
        _meshObject.SetActive(false);
        _collider.enabled = false;
        yield return new WaitForSeconds(2);
        Destroy(gameObject);
    }
    
    public void Push(Vector3 direction)
    {
        _rigidbody.AddForce(direction, ForceMode.Impulse);
    }
}
