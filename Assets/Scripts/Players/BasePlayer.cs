using System.Collections;
using Entities;
using Interfaces;
using Tiles;
using UnityEngine;

namespace Players
{
    public abstract class BasePlayer : MonoBehaviour, IEntity
    {
        /*
         * EVENTS
         */

        public delegate void OnUpdateUI();

        public Transform ModelHolder;
        [SerializeField] private AudioClip CollectSound;
        [SerializeField] private AudioClip FallSound;

        [Header("Parameters")]
        [SerializeField] protected float MoveSpeed;
        [SerializeField] protected float JumpForce;
        [SerializeField] private float _smoothTime;
        [SerializeField] protected float PickaxeCooldown = 1f;

        [SerializeField] protected Vector3 DesiredDirection;
        [SerializeField] protected bool AboveTile;
        [SerializeField] protected bool Grounded;
        [SerializeField] public Vector3Int CurrentCell;
        [SerializeField] public bool Fell;

        [Header("Bombs")]
        [SerializeField] private Bomb _bombPrefab;
        [SerializeField] private int _maxNumOfBombs = 10;
        [SerializeField] private float _bombThrowCooldown = 0.5f;

        [Header("Animation")]
        private Animator _animator;

        [Header("Audio")]
        private AudioSource _audioSource;

        private float _bombThrowTimer;
        private Collider _collider;
        private int _desiredDirectionAnimationID;
        private int _numOfBombs;
        private int _pickaxeAnimationID;
        private float _pickaxeTimer;
        private Vector3 _velocityRef = Vector3.zero;
        public OnUpdateUI OnUpdateScore;
        protected Rigidbody Rb;


        public TilemapManager TilemapManager { get; set; }
        private int TileMask => 1 << LayerMask.NameToLayer("Tile");

        public int PlayerId { get; set; }

        public int NumOfGold { get; private set; }

        protected virtual void Awake()
        {
            Rb = GetComponent<Rigidbody>();
            _collider = GetComponent<CapsuleCollider>();
            ModelHolder = transform.GetChild(0);
            _animator = GetComponent<Animator>();
            _pickaxeAnimationID = Animator.StringToHash("Pickaxe");
            _desiredDirectionAnimationID = Animator.StringToHash("DesiredDirection");
            _audioSource = GetComponent<AudioSource>();
        }

        protected virtual void Update()
        {
            GroundCheck();
            TileCheck();

            if (_pickaxeTimer > 0) _pickaxeTimer -= Time.deltaTime;
            if (_bombThrowTimer > 0) _bombThrowTimer -= Time.deltaTime;

            _animator.SetBool(_desiredDirectionAnimationID, DesiredDirection != Vector3.zero);
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.green;
            var center = transform.position;
            center += transform.forward * 0.5f;

            var rotationMatrix = Matrix4x4.TRS(center, transform.rotation, Vector3.one);
            Gizmos.matrix = rotationMatrix;
            Gizmos.DrawWireCube(Vector3.zero, new Vector3(0.5f, 0.5f, 0.5f));
        }

        public void Fall()
        {
            Fell = true;
            _numOfBombs = _maxNumOfBombs;
            _audioSource.PlayOneShot(FallSound);
        }

        /*
         * CHECKS
         */

        protected virtual void GroundCheck()
        {
            Grounded = Physics.BoxCast(transform.position, new Vector3(0.2f, 0f, 0.2f), Vector3.down, out var hit,
                Quaternion.identity, _collider.bounds.extents.y + 0.05f, TileMask);

            if (Grounded && hit.transform.TryGetComponent(out GroundTile tile)) _smoothTime = tile.Slipperiness;
        }


        protected virtual void TileCheck()
        {
            AboveTile = Physics.BoxCast(transform.position, new Vector3(0.2f, 0, 0.2f), Vector3.down, out var hit,
                Quaternion.identity, 10f, TileMask);

            if (!Grounded || !AboveTile) return;

            var cell = TilemapManager.GetCell(hit.transform.position);

            if (CurrentCell == cell) return;

            CurrentCell = cell;
            TilemapManager.UpdatePlayerLocation(PlayerId, CurrentCell);
        }

        private IEnumerator UpdateOnLand()
        {
            yield return new WaitUntil(() => Grounded);
            yield return new WaitForSeconds(2f);
            TilemapManager.UpdatePlayerLocation(PlayerId, CurrentCell);
            // TilemapManager.CrackTile(CurrentCell);
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
            var hits = new RaycastHit[10];
            int numFound = Physics.BoxCastNonAlloc(transform.position, new Vector3(0.25f, 0.25f, 0.25f),
                transform.forward, hits, transform.rotation, 0.5f);
            for (var i = 0; i < numFound; i++)
                if (hits[i].transform.TryGetComponent(out IHittable hittable))
                    hittable.Hit();
            _animator.SetTrigger(_pickaxeAnimationID);
        }

        protected void ThrowBomb()
        {
            if (_numOfBombs <= 0 || _bombThrowTimer > 0) return;
            _bombThrowTimer = _bombThrowCooldown;
            var bomb = Instantiate(_bombPrefab, transform.position + new Vector3(0, 2.5f, 0), Quaternion.identity);
            bomb.Push(DesiredDirection * 3f + new Vector3(0, 8f, 0));
            _numOfBombs--;
        }

        public virtual void AddGold()
        {
            NumOfGold++;
            OnUpdateScore.Invoke();
            _audioSource.PlayOneShot(CollectSound);
        }

        /*
         * MISC
         */

        public void SetMaterial(Material material)
        {
            // MeshRenderer.material = material;
            // PlayerColour = material.color;
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