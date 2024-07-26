using Interfaces;
using Players;
using UnityEngine;

namespace Entities
{
    public class GoldPiece : MonoBehaviour, IEntity, ICollectable
    {
        [SerializeField] private float _centerPos;
        [SerializeField] private float _amplitude;
        [SerializeField] private float _frequency;

        private Transform _meshObject;
        private Rigidbody _rigidbody;

        private void Awake()
        {
            _rigidbody = GetComponent<Rigidbody>();
        }

        private void Start()
        {
            _meshObject = GetComponentInChildren<MeshRenderer>().transform;
        }

        private void Update()
        {
            var pos = _meshObject.localPosition;
            float newY = _centerPos + Mathf.Sin(Time.fixedTime * Mathf.PI * _frequency) * _amplitude;
            _meshObject.localPosition = new Vector3(pos.x, newY, pos.z);
        }

        private void OnTriggerEnter(Collider other)
        {
            if (other.gameObject.TryGetComponent(out BasePlayer player)) Collect(player);
        }

        public void Collect(BasePlayer player)
        {
            player.AddGold();
            Destroy(gameObject);
        }

        public void Fall()
        {
            Destroy(gameObject);
        }

        public void Push(Vector3 direction)
        {
            _rigidbody.AddForce(direction, ForceMode.Impulse);
        }
    }
}