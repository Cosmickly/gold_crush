using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Serialization;

public class PlayerController : MonoBehaviour
{
    private Rigidbody _rb;
    private Collider _collider;
    
    [SerializeField] private float moveSpeed;
    private Vector3 _desiredDirection;
    
    public float jumpForce;
    public float airSpeedMultiplier;
    private bool _desiredJump;
    
    public LayerMask groundMask;
    public float groundDrag;
    [SerializeField] private bool grounded;
    
    public TilemapManager tilemapManager;
    public LayerMask tileMask;
    [SerializeField] private bool aboveTile;

    public TextMeshProUGUI tmp;


    private void Awake()
    {
        _rb = GetComponent<Rigidbody>();
        _collider = GetComponent<CapsuleCollider>();
    }

    private void Update()
    {
        GroundCheck();
        TileCheck();
        FallCheck();
        _desiredDirection = new Vector3(Input.GetAxisRaw("Horizontal"), 0, Input.GetAxisRaw("Vertical")).normalized;
        _desiredDirection = Quaternion.Euler(0f, 45f, 0f) * _desiredDirection;
        
        _desiredJump = Input.GetKey(KeyCode.Space);

        if (Input.GetKeyDown(KeyCode.Escape)) SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    private void FixedUpdate()
    {
        if (grounded)
            _rb.AddForce(moveSpeed * 10f * _desiredDirection, ForceMode.Force);
        else
            _rb.AddForce(moveSpeed * 10f * airSpeedMultiplier * _desiredDirection, ForceMode.Force);
        
        
        if (_desiredJump && grounded) Jump();
        
        SpeedControl();
        tmp.text = "velocity " + _rb.velocity;
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
        var velocity = _rb.velocity;
        velocity = new Vector3(velocity.x, 0, velocity.z);
        _rb.velocity = velocity;
        
        _rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
    }
    
    public Vector3 GetRotatedPos()
    {
        return Quaternion.Euler(0f, 45f, 0f) * transform.position;
    }
}
