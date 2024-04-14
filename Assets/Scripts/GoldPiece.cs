using System;
using System.Collections;
using System.Collections.Generic;
using Player;
using UnityEngine;

public class GoldPiece : MonoBehaviour, IEntity, ICollectable
{
    public Vector3Int Cell { get; set; }
    public TilemapManager TilemapManager { private get; set; }
    [SerializeField] private float _centerPos;
    [SerializeField] private float _amplitude;
    [SerializeField] private float _frequency;

    private Transform _goldObject;

    private void Start()
    {
        _goldObject = GetComponentInChildren<MeshRenderer>().transform;
    }

    private void Update()
    {
        var pos = _goldObject.localPosition;
        var newY = _centerPos + Mathf.Sin(Time.fixedTime * Mathf.PI * _frequency) * _amplitude;
        _goldObject.localPosition = new Vector3(pos.x, newY, pos.z);
    }

    public void Fall()
    {
        if (TilemapManager.RemoveGoldPiece(Cell))
        {
            Destroy(gameObject);
        }
    }

    public void Collect(BasePlayerController player)
    {
        player.AddGold();
        Destroy(gameObject);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.TryGetComponent(out BasePlayerController player))
        {
            Collect(player);
        }
    }
}
