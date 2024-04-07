using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Serialization;
using UnityEngine.InputSystem;

public class PlayerController : BasePlayerController
{
    private Vector3 _desiredDirection;
    private bool _desiredJump;

    // [SerializeField] private TextMeshProUGUI tmp;

    private InputActionAsset _actionAsset;
    private InputAction _moveAction;
    private InputAction _jumpAction;
    
    private void OnEnable()
    {
        _actionAsset.Enable();
    }

    private void OnDisable()
    {
        _actionAsset.Disable();
    }

    protected override void Awake()
    {
        base.Awake();
        _actionAsset = GetComponent<PlayerInput>().actions;
        _moveAction = _actionAsset.FindActionMap("Gameplay").FindAction("Move");
        _jumpAction = _actionAsset.FindActionMap("Gameplay").FindAction("Jump");

        _actionAsset.FindActionMap("Gameplay").FindAction("Pause").performed += callbackContext => Reset();
    }

    protected override void Update()
    {
        base.Update();
        Input();
    }

    private void Input()
    {
        var input = _moveAction.ReadValue<Vector2>().normalized;
        _desiredDirection = new Vector3(input.x, 0, input.y);
        _desiredDirection = Quaternion.Euler(0f, 45f, 0f) * _desiredDirection;
        
        _desiredJump = _jumpAction.IsPressed();
    }

    protected override void FixedUpdate()
    {
        if (Grounded)
            Rb.AddForce(MoveSpeed * 10f * _desiredDirection, ForceMode.Force);
        else
            Rb.AddForce(MoveSpeed * 10f * AirSpeedMultiplier * _desiredDirection, ForceMode.Force);
        
        if(Grounded && _desiredJump) Jump();
        
        base.FixedUpdate();
        // tmp.text = "velocity " + _rb.velocity;
    }
    
    private void Reset()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public Vector3 GetRotatedPos()
    {
        return Quaternion.Euler(0f, 45f, 0f) * transform.position;
    }
}
