using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RockObstacle : MonoBehaviour, IEntity
{
    public void Fall()
    {
        Destroy(gameObject);
    }
}
