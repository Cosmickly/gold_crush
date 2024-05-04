using Interfaces;
using Players;
using Tiles;
using UnityEngine;

namespace Entities
{
    public class GoldPiece : MonoBehaviour, IEntity, ICollectable
    {
        public Vector3Int Cell { get; set; }
        public TilemapManager TilemapManager { private get; set; }
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
            var newY = _centerPos + Mathf.Sin(Time.fixedTime * Mathf.PI * _frequency) * _amplitude;
            _meshObject.localPosition = new Vector3(pos.x, newY, pos.z);
        }

        public void Fall()
        {
            Destroy(gameObject);
        }

        public void Collect(BasePlayer player)
        {
            // if (TilemapManager.RemoveGoldPiece(this))
            {
                player.AddGold();
                Destroy(gameObject);
            }
        }

        private void OnTriggerEnter(Collider other)
        {
            if (other.gameObject.TryGetComponent(out BasePlayer player))
            {
                Collect(player);
            }
        }

        public void Push(Vector3 direction)
        {
            _rigidbody.AddForce(direction, ForceMode.Impulse);
        }
    }
}
