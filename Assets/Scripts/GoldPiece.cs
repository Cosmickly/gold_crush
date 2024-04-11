using System;
using System.Collections;
using System.Collections.Generic;
using Player;
using UnityEngine;

public class GoldPiece : MonoBehaviour, IEntity, ICollectable
{
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
        Destroy(gameObject);
    }

    public void Collect(BasePlayerController player)
    {
        player.AddGold();
        Destroy(gameObject);
    }

    private void OnCollisionEnter(Collision other)
    {
        if (other.gameObject.TryGetComponent(out BasePlayerController player))
        {
            Collect(player);
        }
    }
}
