using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Serialization;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    private Rigidbody _rb;
    private Collider _collider;
    
    [SerializeField] private float moveSpeed;
    private Vector3 _desiredDirection;

    [SerializeField] private float jumpForce;
    [SerializeField] private float airSpeedMultiplier;
    private bool _desiredJump;

    [SerializeField] private LayerMask groundMask;
    [SerializeField] private float groundDrag;
    [SerializeField] private bool grounded;

    [SerializeField] private TilemapManager tilemapManager;
    [SerializeField] private LayerMask tileMask;
    [SerializeField] private bool aboveTile;

    // [SerializeField] private TextMeshProUGUI tmp;

    private InputActionAsset _actionAsset;
    private InputAction _moveAction;
    private InputAction _jumpAction;

    private MeshRenderer _meshRenderer;

    private void OnEnable()
    {
        _actionAsset.Enable();
    }

    private void OnDisable()
    {
        _actionAsset.Disable();
    }

    private void Awake()
    {
        _rb = GetComponent<Rigidbody>();
        _collider = GetComponent<CapsuleCollider>();
        _meshRenderer = GetComponent<MeshRenderer>();
        _actionAsset = GetComponent<PlayerInput>().actions;
        _moveAction = _actionAsset.FindActionMap("Gameplay").FindAction("Move");
        _jumpAction = _actionAsset.FindActionMap("Gameplay").FindAction("Jump");

        _actionAsset.FindActionMap("Gameplay").FindAction("Pause").performed += callbackContext => Reset();
    }

    private void Update()
    {
        GroundCheck();
        TileCheck();
        FallCheck();
        Input();
    }

    private void Input()
    {
        var input = _moveAction.ReadValue<Vector2>().normalized;
        _desiredDirection = new Vector3(input.x, 0, input.y);
        _desiredDirection = Quaternion.Euler(0f, 45f, 0f) * _desiredDirection;
        
        _desiredJump = _jumpAction.IsPressed();
    }

    private void FixedUpdate()
    {
        if (grounded)
            _rb.AddForce(moveSpeed * 10f * _desiredDirection, ForceMode.Force);
        else
            _rb.AddForce(moveSpeed * 10f * airSpeedMultiplier * _desiredDirection, ForceMode.Force);
        
        if(grounded && _desiredJump) Jump();
        
        SpeedControl();
        // tmp.text = "velocity " + _rb.velocity;
    }

    private void GroundCheck()
    {
        grounded = Physics.Raycast(transform.position, Vector3.down, _collider.bounds.extents.y + 0.05f, groundMask);
        _rb.drag = grounded ? groundDrag : 0f;
    }

    private void TileCheck()
    {
        var pos = transform.position;
        aboveTile = Physics.Raycast(pos, Vector3.down, _collider.bounds.extents.y + 10f, tileMask);
        var flatPos = new Vector3(pos.x, 0, pos.z);
        if (grounded) tilemapManager.CrackTile(flatPos);
    }

    private void FallCheck()
    {
        if (!grounded || aboveTile) return;
        _collider.excludeLayers = groundMask;
        _collider.includeLayers = tileMask;
    }
    

    private void SpeedControl()
    {
        var velocity = _rb.velocity;
        Vector3 flatVel = new Vector3(velocity.x, 0f, velocity.z);
        
        if (flatVel.magnitude <= moveSpeed) return;
        Vector3 limitedVel = flatVel.normalized * moveSpeed;
        if (!grounded) limitedVel *= airSpeedMultiplier;
        _rb.velocity = new Vector3(limitedVel.x, velocity.y, limitedVel.z);
    }

    private void Jump()
    {
        if (!grounded) return;
        
        var velocity = _rb.velocity;
        velocity = new Vector3(velocity.x, 0, velocity.z);
        _rb.velocity = velocity;
            
        _rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
    }

    private void Reset()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public Vector3 GetRotatedPos()
    {
        return Quaternion.Euler(0f, 45f, 0f) * transform.position;
    }

    public void SetGround(TilemapManager ground)
    {
        tilemapManager = ground;
    }

    public void SetColour(Material material)
    {
        _meshRenderer.material = material;
    }
}
