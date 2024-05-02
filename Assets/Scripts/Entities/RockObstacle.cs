using Interfaces;
using Tiles;
using UnityEngine;

namespace Entities
{
    public class RockObstacle : MonoBehaviour, IEntity
    {
        public TilemapManager TilemapManager { private get; set; }
        public Vector3Int Cell { get; set; }

        public void Fall()
        {
            Destroy(gameObject);
        }
    }
}
