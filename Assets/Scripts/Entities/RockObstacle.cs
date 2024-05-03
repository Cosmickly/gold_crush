using Interfaces;
using Tiles;
using UnityEngine;

namespace Entities
{
    public class RockObstacle : MonoBehaviour, IEntity, IHittable
    {
        public TilemapManager TilemapManager { protected get; set; }
        public Vector3Int Cell { get; set; }
        public int HitsToBreak;

        public void Fall()
        {
            if (TilemapManager.RemoveObstacle(Cell)) 
                Destroy(gameObject); 
        }
        
        public virtual void Hit()
        {
            if (HitsToBreak >= 0 && TilemapManager.RemoveObstacle(Cell))
                Destroy(gameObject);

            HitsToBreak--;
        }
    }
}
