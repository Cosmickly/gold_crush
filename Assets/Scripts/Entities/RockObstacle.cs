using System;
using Interfaces;
using Tiles;
using UnityEngine;

namespace Entities
{
    public class RockObstacle : MonoBehaviour, IEntity, IHittable
    {
        private ParticleSystem _particleSystem;
        public TilemapManager TilemapManager { protected get; set; }
        public Vector3Int Cell { get; set; }
        public int HitsToBreak;

        private void Awake()
        {
            _particleSystem = GetComponent<ParticleSystem>();
        }

        public void Fall()
        {
            BreakObstacle();
        }

        public virtual void Hit()
        {
            _particleSystem.Play();
            HitsToBreak--;
            if (HitsToBreak >= 0)
                BreakObstacle();
        }

        private void BreakObstacle()
        {
            if (TilemapManager.RemoveObstacle(Cell))
                gameObject.SetActive(false);
        }
    }
}
