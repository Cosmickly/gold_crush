using Interfaces;
using Tiles;
using UnityEngine;

namespace Players
{
    public abstract class BasePlayer : MonoBehaviour, IEntity
    {
        protected Rigidbody Rb;
        protected Collider Collider;
        protected MeshRenderer MeshRenderer;

        [Header("Parameters")]
        [SerializeField] protected float MoveSpeed;
        [SerializeField] protected float JumpForce;
        [SerializeField] private float _smoothTime;

        protected Vector3 DesiredDirection;
        private Vector3 _velocityRef = Vector3.zero;
    

        public TilemapManager TilemapManager { private get; set; }
        [SerializeField] protected LayerMask TileMask;
        [SerializeField] protected bool AboveTile;
        [SerializeField] protected bool Grounded;
        [SerializeField] private Vector3Int _currentCell;
        [SerializeField] protected bool Fell;

        // [SerializeField] private Collider _tileCheckCollider;
    
        public int ID { get; set; }

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
    
        /*
     * CHECKS
     */

        protected virtual void GroundCheck()
        {
            Grounded = Physics.BoxCast(transform.position, new Vector3(0.2f, 0f, 0.2f), Vector3.down, out var hit,
                Quaternion.identity,  Collider.bounds.extents.y + 0.05f, TileMask);
        
            if (Grounded && hit.transform.TryGetComponent(out GroundTile tile))
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
            AboveTile = Physics.BoxCast(transform.position, new Vector3(0.2f, 0, 0.2f), Vector3.down, out var hit,
                Quaternion.identity, 10f, TileMask);

            if (!Grounded || !AboveTile) return;
        
            var cell = TilemapManager.GetCell(hit.transform.position);
        
            if (_currentCell == cell) return;
        
            _currentCell = cell;
            TilemapManager.UpdatePlayerLocation(ID, _currentCell);
        
            // var flatPos = new Vector3(pos.x, 0, pos.z);
            // if (Grounded && AboveTile) TilemapManager.UpdatePlayerLocation(ID, TilemapManager.GetCell(flatPos));
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
    
        protected virtual void Rotate()
        {
            if (DesiredDirection == Vector3.zero) return;
            transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(DesiredDirection, Vector3.up), 1f);
        }
    
        /*
     * ABILITIES
     */

        protected virtual void SwingPickaxe()
        {
            Physics.BoxCast(transform.position, new Vector3(0.2f, 0f, 0.2f), Vector3.forward, out var hit);
        
        }
    
        /*
     * EVENTS
     */
    
        public delegate void OnUpdateUI();
        public OnUpdateUI onUpdateUI;
    
        public void AddGold()
        {
            NumOfGold++;
            onUpdateUI.Invoke();
        }
    
        /*
     * MISC
     */

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

        protected Vector3 GetRotatedVector(Vector3 vector)
        {
            return Quaternion.Euler(0f, 45f, 0f) * vector;
        }
    }
}
