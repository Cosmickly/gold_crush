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
        // [SerializeField] protected float AirSpeedMultiplier;

        [SerializeField] protected float GroundDrag;
        [SerializeField] protected bool Grounded;

        [SerializeField] protected LayerMask TileMask;
        [SerializeField] protected TilemapManager TilemapManager;
        [SerializeField] protected bool AboveTile;
    
        [SerializeField] protected bool Fell;

        [SerializeField] protected int NumOfGold;
    
        protected virtual void Awake()
        {
            // TileMask = LayerMask.NameToLayer("Tile");
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
            Grounded = Physics.BoxCast(transform.position, new Vector3(0.2f, 0f, 0.2f), Vector3.down,
                Quaternion.identity, Collider.bounds.extents.y + 0.05f, TileMask);
            Rb.drag = Grounded ? GroundDrag : 0f;
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
            Rb.AddForce(Vector3.down, ForceMode.Impulse);
        }

        public void AddGold()
        {
            NumOfGold++;
        }
    }
}
