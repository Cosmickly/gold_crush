using System;
using Unity.AI.Navigation;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Serialization;
using Random = UnityEngine.Random;

namespace Tiles
{
    public class GroundTile : MonoBehaviour
    {
        public TilemapManager TilemapManager { private get; set; }
        public Vector3Int Cell { get; set; }
        
        private BoxCollider _collider;
        private MeshRenderer _meshRenderer;
        private GameObject _meshObject;
        private NavMeshObstacle _navMeshObstacle;
        // private NavMeshModifier _navMeshModifier;
        
        public bool Cracking { get; set; }
        private float _crackTimer;
        [Header("Crack Parameters")]
        [SerializeField] private float _crackTime;
        [SerializeField] private float _crackMultiplier;
        
        [Header("Shake Parameters")]
        [SerializeField] private float _amplitude;
        [SerializeField] private float _frequency;
        
        [Header("Ice Parameters")]
        public bool Ice;
        [SerializeField] private Material _defaultMaterial;
        [SerializeField] private Material _iceMaterial;
        [SerializeField] private float _defaultSlipperiness;
        [SerializeField] private float _iceSlipperiness;
        public float Slipperiness => Ice ? _iceSlipperiness : _defaultSlipperiness;

        private Color _initialColor;

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
            _meshObject = _meshRenderer.gameObject;
            _initialColor = _meshRenderer.material.color;
            _navMeshObstacle = GetComponentInChildren<NavMeshObstacle>();
            // _navMeshModifier = GetComponentInChildren<NavMeshModifier>();

            var newY = transform.position.y + Random.Range(-_amplitude, _amplitude);
            _meshObject.transform.localPosition += new Vector3(0, newY, 0);
        }
        
        private void Start()
        {
        }

        private void FixedUpdate()
        {
            if (!Cracking) return;
        
            if (_crackTimer >= _crackTime)
            {
                TilemapManager.RemoveTile(Cell);
            }

            _crackTimer += PlayerOnMe ? Time.deltaTime * _crackMultiplier : Time.deltaTime;
            var crackRatio = _crackTimer / _crackTime;
            _meshRenderer.material.color = Color.Lerp(_initialColor, Color.black, crackRatio);
            
            var newY = Mathf.Sin(Time.fixedTime * Mathf.PI * _frequency * crackRatio) * (_amplitude * crackRatio);
            _meshObject.transform.localPosition = new Vector3(0, newY, 0);
        }

        public void Break()
        {
            Cracking = false;
            _meshRenderer.enabled = false;
            _collider.enabled = false;
            _navMeshObstacle.enabled = true;
            // _navMeshModifier.overrideArea = true;
        }

        public void Rebuild()
        {
            _meshRenderer.enabled = true;
            _collider.enabled = true;
            _navMeshObstacle.enabled = false;
            _crackTimer = 0f;
        }

        private void OnDestroy()
        {
            _navMeshObstacle.enabled = false;
        }

        public void ToggleIce(bool togglIce)
        {
            Ice = togglIce;
            _meshRenderer.material = Ice ? _iceMaterial : _defaultMaterial;
            _initialColor = _meshRenderer.material.color;
        }
    }
}
