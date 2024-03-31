using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    private Rigidbody _rb;
    private Collider _collider;
    
    [SerializeField] private float moveSpeed;
    private Vector3 _desiredDirection;

    public TextMeshProUGUI tmp;

    public LayerMask groundMask;
    public bool grounded;
    public float groundDrag;
    public TilemapManager tilemapManager;
    public bool onTile;


    // Start is called before the first frame update
    void Start()
    {
        _rb = GetComponent<Rigidbody>();
        _collider = GetComponent<CapsuleCollider>();
    }

    // Update is called once per frame
    void Update()
    {
        _desiredDirection = new Vector3(Input.GetAxisRaw("Horizontal"), 0, Input.GetAxisRaw("Vertical")).normalized;
        _desiredDirection = Quaternion.Euler(0f, 45f, 0f) * _desiredDirection;
        tmp.text = "velocity " + _rb.velocity;

        grounded = Physics.Raycast(transform.position, Vector3.down, _collider.bounds.extents.y + 0.1f, groundMask);
        _rb.drag = grounded ? groundDrag : 0f;
    }

    private void FixedUpdate()
    {
        _rb.AddForce(_desiredDirection * moveSpeed, ForceMode.Acceleration);
    }
    
    public Vector3 GetRotatedPos()
    {
        return Quaternion.Euler(0f, 45f, 0f) * transform.position;
    }
}
