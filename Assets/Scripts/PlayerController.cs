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
    
    [SerializeField] private float _moveSpeed;
    private Vector3 _desiredDirection;

    [SerializeField] private float _jumpForce;
    [SerializeField] private float _airSpeedMultiplier;
    private bool _desiredJump;

    [SerializeField] private LayerMask _groundMask;
    [SerializeField] private float _groundDrag;
    [SerializeField] private bool _grounded;

    [SerializeField] private TilemapManager _tilemapManager;
    [SerializeField] private LayerMask _tileMask;
    [SerializeField] private bool _aboveTile;

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
        // TileCheck();
        // FallCheck();
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
        if (_grounded)
            _rb.AddForce(_moveSpeed * 10f * _desiredDirection, ForceMode.Force);
        else
            _rb.AddForce(_moveSpeed * 10f * _airSpeedMultiplier * _desiredDirection, ForceMode.Force);
        
        if(_grounded && _desiredJump) Jump();
        
        SpeedControl();
        // tmp.text = "velocity " + _rb.velocity;
    }

    private void GroundCheck()
    {
        _grounded = Physics.Raycast(transform.position, Vector3.down, _collider.bounds.extents.y + 0.05f, _groundMask);
        _rb.drag = _grounded ? _groundDrag : 0f;
    }

    private void TileCheck()
    {
        var pos = transform.position;
        _aboveTile = Physics.Raycast(pos, Vector3.down, _collider.bounds.extents.y + 10f, _tileMask);
        var flatPos = new Vector3(pos.x, 0, pos.z);
        if (_grounded) _tilemapManager.CrackTile(flatPos);
    }

    private void FallCheck()
    {
        if (!_grounded || _aboveTile) return;
        _collider.excludeLayers = _groundMask;
        _collider.includeLayers = _tileMask;
    }
    

    private void SpeedControl()
    {
        var velocity = _rb.velocity;
        Vector3 flatVel = new Vector3(velocity.x, 0f, velocity.z);
        
        if (flatVel.magnitude <= _moveSpeed) return;
        Vector3 limitedVel = flatVel.normalized * _moveSpeed;
        if (!_grounded) limitedVel *= _airSpeedMultiplier;
        _rb.velocity = new Vector3(limitedVel.x, velocity.y, limitedVel.z);
    }

    private void Jump()
    {
        if (!_grounded) return;
        
        var velocity = _rb.velocity;
        velocity = new Vector3(velocity.x, 0, velocity.z);
        _rb.velocity = velocity;
            
        _rb.AddForce(Vector3.up * _jumpForce, ForceMode.Impulse);
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
        _tilemapManager = ground;
    }

    public void SetColour(Material material)
    {
        _meshRenderer.material = material;
    }
}
