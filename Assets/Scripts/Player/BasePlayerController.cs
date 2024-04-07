using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class BasePlayerController : MonoBehaviour
{
    protected Rigidbody Rb;
    protected Collider Collider;
    protected MeshRenderer MeshRenderer;

    [SerializeField] protected float MoveSpeed;
    [SerializeField] protected float JumpForce;
    [SerializeField] protected float AirSpeedMultiplier;

    [SerializeField] protected LayerMask GroundMask;
    [SerializeField] protected float GroundDrag;
    [SerializeField] protected bool Grounded;

    [SerializeField] protected TilemapManager TilemapManager;
    [SerializeField] protected LayerMask TileMask;
    [SerializeField] protected bool AboveTile;
    
    
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
        FallCheck();
    }
    
    protected virtual void FixedUpdate()
    {
        SpeedControl();
    }

    protected virtual void GroundCheck()
    {
        Grounded = Physics.Raycast(transform.position, Vector3.down, Collider.bounds.extents.y + 10f, GroundMask);
        Rb.drag = Grounded ? GroundDrag : 0f;
    }

    protected virtual void TileCheck()
    {
        var pos = transform.position;
        AboveTile = Physics.Raycast(pos, Vector3.down, Collider.bounds.extents.y + 10f, TileMask);
        var flatPos = new Vector3(pos.x, 0, pos.z);
        if (Grounded) TilemapManager.CrackTile(flatPos);
    }

    protected virtual void FallCheck()
    {
        if (!Grounded || AboveTile) return;
        Collider.excludeLayers = GroundMask;
        Collider.includeLayers = TileMask;
    }

    protected virtual void SpeedControl()
    {
        var velocity = Rb.velocity;
        Vector3 flatVel = new Vector3(velocity.x, 0f, velocity.z);
        
        if (flatVel.magnitude <= MoveSpeed) return;
        Vector3 limitedVel = flatVel.normalized * MoveSpeed;
        if (!Grounded) limitedVel *= AirSpeedMultiplier;
        Rb.velocity = new Vector3(limitedVel.x, velocity.y, limitedVel.z);
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

    public void SetColour(Material material)
    {
        MeshRenderer.material = material;
    }
}
