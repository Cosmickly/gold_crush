using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Serialization;

namespace Tiles
{
    public class GroundTile : MonoBehaviour
    {
        public TilemapManager TilemapManager { private get; set; }
        public Vector3Int Cell { get; set; }
        
        private BoxCollider _collider;
        private MeshRenderer _meshRenderer;
        private NavMeshObstacle _navMeshObstacle;
    
        public bool Cracking { get; set; }
        private float _crackTimer;
        [Header("Crack Parameters")]
        [SerializeField] private float _crackTime;
        [SerializeField] private float _crackMultiplier;
        
        public bool Ice;
        [SerializeField] private Material _defaultMaterial;
        [SerializeField] private Material _iceMaterial;
        [SerializeField] private float _defaultSlipperiness;
        [SerializeField] private float _iceSlipperiness;
        public float Slipperiness => Ice ? _iceSlipperiness : _defaultSlipperiness;

        public bool PlayerOnMe
        {
            get => _playerOnMe;
            set => _playerOnMe = value;
        }
        [SerializeField] private bool _playerOnMe;
        
        private void Awake()
        {
            _collider = GetComponent<BoxCollider>();
            _meshRenderer = GetComponentInChildren<MeshRenderer>();
            _navMeshObstacle = GetComponentInChildren<NavMeshObstacle>();
        }

        private void FixedUpdate()
        {
            if (!Cracking) return;
        
            if (_crackTimer >= _crackTime)
            {
                if (TilemapManager.RemoveTile(Cell))
                {
                    Break();
                    return;
                }
            }

            _crackTimer += PlayerOnMe ? Time.deltaTime * _crackMultiplier : Time.deltaTime;
            var currentColor = _meshRenderer.material.color;
            _meshRenderer.material.color = Color.Lerp(currentColor, Color.black, _crackTimer / _crackTime);
        }

        private void Break()
        {
            Cracking = false;
            _meshRenderer.enabled = false;
            _collider.enabled = false;
            _navMeshObstacle.enabled = true;
        }

        public void ToggleIce(bool togglIce)
        {
            Ice = togglIce;
            _meshRenderer.material = Ice ? _iceMaterial : _defaultMaterial;
        }
    }
}
