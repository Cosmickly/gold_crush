using System;
using Interfaces;
using Tiles;
using UnityEngine;

namespace Entities
{
    public class RockObstacle : MonoBehaviour, IEntity, IHittable
    {
        private ParticleSystem _particleSystem;
        private Collider _collider;
        private MeshRenderer _meshRenderer;
        
        public TilemapManager TilemapManager { protected get; set; }
        public Vector3Int Cell { get; set; }
        public int HitsToBreak;

        private void Awake()
        {
            _particleSystem = GetComponent<ParticleSystem>();
            _collider = GetComponent<Collider>();
            _meshRenderer = GetComponent<MeshRenderer>();
            var main = _particleSystem.main;
            main.startColor = _meshRenderer.material.color;
        }

        public void Fall()
        {
            BreakObstacle();
        }

        public virtual void Hit()
        {
            HitsToBreak--;
            PlayParticles();
            if (HitsToBreak <= 0)
                BreakObstacle();
        }

        private void PlayParticles()
        {
            if (HitsToBreak <= 0)
            {
                var module = _particleSystem.emission;
                module.burstCount = 6;
                var main = _particleSystem.main;
                main.startSize = 0.4f;
            }
            _particleSystem.Play();
        }

        private void BreakObstacle()
        {
            if (TilemapManager.RemoveObstacle(Cell))
            {
                // gameObject.SetActive(false);
                _collider.enabled = false;
                _meshRenderer.enabled = false;
                // _emissionModule.burstCount = _initialBurstCount;
                // _mainModule.startSize = _initialParticleSize;
            }
        }
    }
}
