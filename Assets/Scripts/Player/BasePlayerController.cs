using System;
using UnityEngine;

namespace Player
{
    public abstract class BasePlayerController : MonoBehaviour, IEntity
    {
        protected Rigidbody Rb;
        protected Collider Collider;
        protected MeshRenderer MeshRenderer;

        [SerializeField] protected float MoveSpeed;
        [SerializeField] protected float JumpForce;

        protected Vector3 DesiredDirection;
        [SerializeField] private float _smoothTime;
        private Vector3 _velocityRef = Vector3.zero;
        
        [SerializeField] protected bool Grounded;

        [SerializeField] protected LayerMask TileMask;
        [SerializeField] protected TilemapManager TilemapManager;
        [SerializeField] protected bool AboveTile;
    
        [SerializeField] protected bool Fell;

        public int NumOfGold { get; private set; }
    
        protected virtual void Awake()
        {
            Rb = GetComponent<Rigidbody>();
            Collider = GetComponent<CapsuleCollider>();
            MeshRenderer = GetComponent<MeshRenderer>();
        }

        protected virtual void Update()
        {
            GroundCheck();
            TileCheck();
        }

        protected virtual void GroundCheck()
        {
            Grounded = Physics.BoxCast(transform.position, new Vector3(0.2f, 0f, 0.2f), Vector3.down, out var hit,
                Quaternion.identity,  Collider.bounds.extents.y + 0.05f, TileMask);
            
            if (Grounded && hit.transform.TryGetComponent(out TileController tile))
            {
                _smoothTime = tile.Slipperiness;
            }
        }

        // private void OnDrawGizmosSelected()
        // {
        //     Gizmos.color = Color.green;
        //     var pos = transform.position;
        //     pos.y -= 5f;
        //     Gizmos.DrawWireCube(pos, new Vector3(0.4f, 10f, 0.4f));
        // }

        protected virtual void TileCheck()
        {
            var pos = transform.position;
            AboveTile = Physics.BoxCast(pos, new Vector3(0.2f, 0, 0.2f), Vector3.down, Quaternion.identity, 10f,
                TileMask);
        
            var flatPos = new Vector3(pos.x, 0, pos.z);
            if (Grounded && AboveTile) TilemapManager.CrackTile(TilemapManager.GetCell(flatPos));
        }

        protected virtual void Move()
        {
            if (Grounded)
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

        public void SetGround(TilemapManager ground)
        {
            TilemapManager = ground;
        }

        public void SetMaterial(Material material)
        {
            MeshRenderer.material = material;
        }

        public void Fall()
        {
            Fell = true;
            Destroy(gameObject);
            // Rb.AddForce(Vector3.down, ForceMode.Impulse);
        }

        
        public delegate void OnUpdateUI();
        public OnUpdateUI onUpdateUI;
        
        public void AddGold()
        {
            NumOfGold++;
            onUpdateUI.Invoke();
        }
    }
}
