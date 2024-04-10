using System;
using System.Collections;
using System.Collections.Generic;
using Player;
using UnityEngine;

public class GoldPiece : MonoBehaviour, IEntity, ICollectable
{
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
