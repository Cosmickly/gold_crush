using System.Collections;
using Interfaces;
using Tiles;
using UnityEngine;

namespace Entities
{
    public class Bomb : MonoBehaviour, IEntity
    {
        [SerializeField] private float _radius;
        [SerializeField] private float _bombTime;
        private bool _armed;
        private float _bombTimer;
        private Collider _collider;
        private GameObject _meshObject;
        private ParticleSystem _particleSystem;
        private Rigidbody _rigidbody;

        private int TileMask => 1 << LayerMask.NameToLayer("Tile");

        private void Awake()
        {
            _collider = GetComponent<Collider>();
            _rigidbody = GetComponent<Rigidbody>();
            _particleSystem = GetComponent<ParticleSystem>();
            _meshObject = GetComponentInChildren<MeshRenderer>().gameObject;
        }

        private void Start()
        {
            _bombTimer = _bombTime;
            ArmBomb();
        }

        private void Update()
        {
            if (_armed) _bombTimer -= Time.deltaTime;

            if (_bombTimer <= 0) Explode();
        }

        public void Fall()
        {
            Destroy(gameObject);
        }

        private void ArmBomb()
        {
            _armed = true;
        }

        private void Explode()
        {
            _armed = false;
            _bombTimer = _bombTime;
            var hits = new Collider[64];
            int numFound = Physics.OverlapSphereNonAlloc(transform.position, _radius, hits, TileMask);

            for (var i = 0; i < numFound; i++)
                if (hits[i].TryGetComponent(out GroundTile tile))
                    tile.InstantBreak();

            _particleSystem.Play();
            StartCoroutine(DestroyBomb());
        }

        private IEnumerator DestroyBomb()
        {
            _rigidbody.useGravity = false;
            _rigidbody.velocity = Vector3.zero;
            _rigidbody.angularVelocity = Vector3.zero;
            _meshObject.SetActive(false);
            _collider.enabled = false;
            yield return new WaitForSeconds(2);
            Destroy(gameObject);
        }

        public void Push(Vector3 direction)
        {
            _rigidbody.AddForce(direction, ForceMode.Impulse);
        }
    }
}