using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RockObstacle : MonoBehaviour, IEntity
{
    public TilemapManager TilemapManager { private get; set; }
    public Vector3Int Cell { get; set; }

    public void Fall()
    {
        if (TilemapManager.RemoveObstacle(Cell))
            Destroy(gameObject);
    }
}
