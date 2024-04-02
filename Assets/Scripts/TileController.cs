using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TileController : MonoBehaviour
{
    private MeshRenderer _mesh;
    private bool _cracking;
    private TilemapManager _tilemapManager;

    private void Awake()
    {
        _mesh = GetComponent<MeshRenderer>();
    }

    private void Start()
    {
        
    }

    private void Update()
    {
        
    }

    private void FixedUpdate()
    {
        if (!_cracking) return;
        
        Color materialColor = _mesh.material.color;
        if (materialColor.r <= 0)
        {
            _cracking = false;
            _tilemapManager.BreakTile(transform.position);
        }
            
        _mesh.material.color = materialColor - new Color(0.01f,0.01f,0.01f,0f);
    }

    public void SetCracking(bool toggle)
    {
        _cracking = toggle;
    }

    public bool GetCracking()
    {
        return _cracking;
    }

    public void SetTilemapManager(TilemapManager manager)
    {
        _tilemapManager = manager;
    }
}
