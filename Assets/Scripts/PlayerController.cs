using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    private Rigidbody _rb;
    [SerializeField] private float moveSpeed;
    private Vector3 _desiredDirection;

    public TextMeshProUGUI tmp;

    // Start is called before the first frame update
    void Start()
    {
        _rb = GetComponent<Rigidbody>();
    }

    // Update is called once per frame
    void Update()
    {
        _desiredDirection = new Vector3(Input.GetAxisRaw("Horizontal"),0,Input.GetAxisRaw("Vertical")).normalized;
        _desiredDirection = Quaternion.Euler(0f, 45f, 0f) * _desiredDirection;
        tmp.text = GetRotatedPos().ToString();
    }

    private void FixedUpdate()
    {
        _rb.AddForce(_desiredDirection * moveSpeed, ForceMode.Force);
    }

    private void OnCollisionStay(Collision other)
    {
        
    }

    public Vector3 GetRotatedPos()
    {
        return Quaternion.Euler(0f, 45f, 0f) * transform.position;
    }
}
