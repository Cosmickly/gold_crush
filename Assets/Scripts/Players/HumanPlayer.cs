using System;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

namespace Players
{
    public class HumanPlayer : BasePlayer
    {
        private bool _desiredJump;
        private bool _desiredPickaxe;

        [Header("Input")]
        private InputActionAsset _actionAsset;
        private InputAction _moveAction;
        private InputAction _jumpAction;
        private InputAction _pickaxeAction;
        private InputAction _pauseAction;

        private void OnEnable()
        {
            if (_pauseAction != null) _pauseAction.started += PausePressed;
            // _actionAsset.Enable();
        }
        
        private void OnDisable()
        {
            if (_pauseAction != null) _pauseAction.started -= PausePressed;
            // _actionAsset.Disable();
        }

        protected override void Awake()
        {
            base.Awake();
            _actionAsset = GetComponent<PlayerInput>().actions;
            _moveAction = _actionAsset.FindActionMap("Gameplay").FindAction("Move");
            _jumpAction = _actionAsset.FindActionMap("Gameplay").FindAction("Jump");
            _pickaxeAction = _actionAsset.FindActionMap("Gameplay").FindAction("Pickaxe");
            _pauseAction = _actionAsset.FindActionMap("Gameplay").FindAction("Pause");
        }

        protected override void Update()
        {
            base.Update();
            Input();
        }
        

        private void Input()
        {
            var input = _moveAction.ReadValue<Vector2>();
            DesiredDirection = new Vector3(input.x, 0, input.y);
            DesiredDirection = GetRotatedVector(DesiredDirection).normalized;
        
            _desiredJump = _jumpAction.IsPressed();
            _desiredPickaxe = _pickaxeAction.IsPressed();
        }

        protected void FixedUpdate()
        {
            if (!TilemapManager.Active) return;
            Move();
            if(_desiredJump) Jump();
            Rotate(DesiredDirection);

            if (_desiredPickaxe)
            {
                if (!Fell)
                    SwingPickaxe();
                else
                    ThrowBomb();
            }
        }

        private void PausePressed(InputAction.CallbackContext context)
        {
            TilemapManager.Pause();
        }

        // protected void SpeedControl()
        // {
        //     var vel = Rb.velocity;
        //     var flatVel = new Vector3(vel.x, 0, vel.z);
        //
        //     if (vel.magnitude > MoveSpeed)
        //     {
        //         var limitedVel = flatVel.normalized * MoveSpeed;
        //         Rb.velocity = new Vector3(limitedVel.x, vel.y, limitedVel.z);
        //     }
        // }
    
        // private void Reset()
        // {
        //     SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        // }
    }
}
