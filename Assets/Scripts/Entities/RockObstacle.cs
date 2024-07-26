using Interfaces;
using Tiles;
using UnityEngine;

namespace Entities
{
    public class RockObstacle : MonoBehaviour, IEntity, IHittable
    {
        public AudioClip BreakSound;
        public AudioClip HitSound;
        public int HitsToBreak;
        private AudioSource _audioSource;
        private Collider _collider;
        private GameObject _meshObject;
        private ParticleSystem _particleSystem;

        public TilemapManager TilemapManager { protected get; set; }
        public Vector3Int Cell { get; set; }

        private void Awake()
        {
            _particleSystem = GetComponent<ParticleSystem>();
            _collider = GetComponent<Collider>();
            _meshObject = transform.GetChild(0).gameObject;
            _audioSource = GetComponent<AudioSource>();
        }

        public void Fall()
        {
            BreakObstacle();
        }

        public virtual void Hit()
        {
            HitsToBreak--;
            PlayParticles();
            _audioSource.PlayOneShot(HitSound);
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
                _collider.enabled = false;
                _meshObject.SetActive(false);
                _audioSource.PlayOneShot(BreakSound);
            }
        }
    }
}