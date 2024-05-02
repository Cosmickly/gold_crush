using Interfaces;
using Tiles;
using UnityEngine;

namespace Entities
{
    public class RockObstacle : MonoBehaviour, IEntity, IHittable
    {
        public TilemapManager TilemapManager { private get; set; }
        public Vector3Int Cell { get; set; }

        public void Fall()
        {
            if (TilemapManager.RemoveObstacle(Cell)) 
                Destroy(gameObject); 
        }
        
        public void Hit()
        {
            if (TilemapManager.RemoveObstacle(Cell))
                Destroy(gameObject);
        }
    }
}
