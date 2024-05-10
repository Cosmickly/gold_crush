using System;
using System.Collections;
using System.Collections.Generic;
using Interfaces;
using Tiles;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Players
{
    public abstract class BasePlayer : MonoBehaviour, IEntity
    {
        protected Rigidbody Rb;
        private Collider _collider;
        public Transform ModelHolder;
        
        [Header("Animation")]
        private Animator _animator;
        private int _pickaxeAnimationID;
        private int _desiredDirectionAnimationID;
        
        [Header("Parameters")]
        [SerializeField] protected float MoveSpeed;
        [SerializeField] protected float JumpForce;
        [SerializeField] private float _smoothTime;
        [SerializeField] protected float PickaxeCooldown = 1f;
        private float _pickaxeTimer;

        [SerializeField] protected Vector3 DesiredDirection;
        private Vector3 _velocityRef = Vector3.zero;
    

        public TilemapManager TilemapManager { get; set; }
        protected int TileMask => 1 << LayerMask.NameToLayer("Tile");
        [SerializeField] protected bool AboveTile;
        [SerializeField] protected bool Grounded;
        [SerializeField] private Vector3Int _currentCell;
        [SerializeField] public bool Fell;

        [SerializeField] private Bomb _bombPrefab;
        
        public int ID { get; set; }

        public int NumOfGold { get; private set; }

        [SerializeField] private int _maxNumOfBombs = 10;
        private int _numOfBombs;
        [SerializeField] private float _bombThrowCooldown = 0.5f;
        private float _bombThrowTimer;

        protected virtual void Awake()
        {
            Rb = GetComponent<Rigidbody>();
            _collider = GetComponent<CapsuleCollider>();
            ModelHolder = transform.GetChild(0);
            _animator = GetComponent<Animator>();
            _pickaxeAnimationID = Animator.StringToHash("Pickaxe");
            _desiredDirectionAnimationID = Animator.StringToHash("DesiredDirection");
        }

        protected virtual void Update()
        {
            GroundCheck();
            TileCheck();
            
            if (_pickaxeTimer > 0) _pickaxeTimer -= Time.deltaTime;
            if (_bombThrowTimer > 0) _bombThrowTimer -= Time.deltaTime;

            _animator.SetBool(_desiredDirectionAnimationID, DesiredDirection != Vector3.zero);
        }

        /*
         * CHECKS
         */

        protected virtual void GroundCheck()
        {
            Grounded = Physics.BoxCast(transform.position, new Vector3(0.2f, 0f, 0.2f), Vector3.down, out var hit,
                Quaternion.identity,  _collider.bounds.extents.y + 0.05f, TileMask);
        
            if (Grounded && hit.transform.TryGetComponent(out GroundTile tile))
            {
                _smoothTime = tile.Slipperiness;
            }
        }


        protected virtual void TileCheck()
        {
            AboveTile = Physics.BoxCast(transform.position, new Vector3(0.2f, 0, 0.2f), Vector3.down, out var hit,
                Quaternion.identity, 10f, TileMask);

            if (!Grounded || !AboveTile) return;
        
            var cell = TilemapManager.GetCell(hit.transform.position);
        
            if (_currentCell == cell) return;
        
            _currentCell = cell;
            TilemapManager.UpdatePlayerLocation(ID, _currentCell);
        }

        private IEnumerator UpdateOnLand()
        {
            yield return new WaitUntil(() => Grounded);
            TilemapManager.UpdatePlayerLocation(ID, _currentCell);
        }
    
        /*
         * MOVEMENT
         */

        protected virtual void Move()
        {
            if (!Grounded) return;
            Rb.velocity = Vector3.SmoothDamp(Rb.velocity, MoveSpeed * DesiredDirection, ref _velocityRef, _smoothTime);
        }

        protected virtual void Jump()
        {
            if (!Grounded) return;
    
            var velocity = Rb.velocity;
            velocity = new Vector3(velocity.x, 0, velocity.z);
            Rb.velocity = velocity;
        
            Rb.AddForce(Vector3.up * JumpForce, ForceMode.Impulse);
        }

        protected virtual void Rotate(Vector3 forward)
        {
            if (forward == Vector3.zero) return;
            transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(forward, Vector3.up), 1f);
        }
    
        /*
         * ABILITIES
         */

        protected virtual void SwingPickaxe()
        {
            if (!Grounded || _pickaxeTimer > 0) return;
            _pickaxeTimer = PickaxeCooldown;
            RaycastHit[] hits = new RaycastHit[10];
            int numFound = Physics.BoxCastNonAlloc(transform.position, new Vector3(0.25f, 0.25f, 0.25f),
                transform.forward, hits, transform.rotation, 0.5f);
            for (int i=0; i<numFound; i++)
            {
                if (hits[i].transform.TryGetComponent(out IHittable hittable))
                {
                    hittable.Hit();
                }
            }
            _animator.SetTrigger(_pickaxeAnimationID);
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.green;
            var center = transform.position;
            center += (transform.forward * 0.5f);

            Matrix4x4 rotationMatrix = Matrix4x4.TRS(center, transform.rotation, Vector3.one);
            Gizmos.matrix = rotationMatrix;
            Gizmos.DrawWireCube(Vector3.zero, new Vector3(0.5f, 0.5f, 0.5f));
        }
        
        protected void ThrowBomb()
        {
            if (_numOfBombs <= 0 || _bombThrowTimer > 0) return;
            _bombThrowTimer = _bombThrowCooldown;
            var bomb = Instantiate(_bombPrefab, transform.position + new Vector3(0, 2.5f, 0), Quaternion.identity);
            bomb.Push(DesiredDirection * 3f + new Vector3(0, 8f, 0));
            _numOfBombs--;
        }
    
        /*
         * EVENTS
         */
    
        public delegate void OnUpdateUI();
        public OnUpdateUI onUpdateUI;

        public virtual void AddGold()
        {
            NumOfGold++;
            onUpdateUI.Invoke();
        }
    
        /*
         * MISC
         */

        public void SetMaterial(Material material)
        {
            // MeshRenderer.material = material;
            // PlayerColour = material.color;
        }

        public void Fall()
        {
            Fell = true;
            _numOfBombs = _maxNumOfBombs;
        }

        protected Vector3 GetRotatedVector(Vector3 vector)
        {
            return Quaternion.Euler(0f, 45f, 0f) * vector;
        }

        public virtual void TogglePlayerEnabled(bool enable)
        {
            // ModelObject.SetActive(enable);
            _collider.enabled = enable;
            Rb.useGravity = enable;   
            Rb.velocity = Vector3.zero;
            if (enable)
            {
                _numOfBombs = 0;
                StartCoroutine(UpdateOnLand());
            }
        }
    }
}
