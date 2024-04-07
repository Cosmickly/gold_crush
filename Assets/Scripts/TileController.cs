using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TileController : MonoBehaviour
{
    private BoxCollider _collider;
    private MeshRenderer _mesh;
    private Color _initialColor;
    public bool Cracking { get; set; }
    private TilemapManager _tilemapManager;

    [SerializeField] private float _crackTime;
    private float _crackTimer;

    private void Awake()
    {
        _collider = GetComponent<BoxCollider>();
        _mesh = GetComponent<MeshRenderer>();
        _initialColor = _mesh.material.color;
    }

    private void FixedUpdate()
    {
        if (!Cracking) return;
        
        if (_crackTimer >= _crackTime)
        {
            Cracking = false;
            _tilemapManager.BreakTile(transform.position);
        }

        _crackTimer += Time.deltaTime;
        _mesh.material.color = Color.Lerp(_initialColor, Color.black, _crackTimer / _crackTime);
    }

    public void SetTilemapManager(TilemapManager manager)
    {
        _tilemapManager = manager;
    }

    private void SetColliderTrigger(bool toggle)
    {
        _collider.isTrigger = toggle;
    }
}
