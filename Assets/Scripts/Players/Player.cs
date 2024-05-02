using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

namespace Players
{
    public class Player : BasePlayer
    {
        private bool _desiredJump;
    
        [Header("Input")]
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

            // TODO replace with actual pause
            // _actionAsset.FindActionMap("Gameplay").FindAction("Pause").performed += callbackContext => Reset();
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
        }

        protected void FixedUpdate()
        {
            Move();
            if(Grounded && _desiredJump) Jump();
            Rotate();
        }

        protected void SpeedControl()
        {
            var vel = Rb.velocity;
            var flatVel = new Vector3(vel.x, 0, vel.z);

            if (vel.magnitude > MoveSpeed)
            {
                var limitedVel = flatVel.normalized * MoveSpeed;
                Rb.velocity = new Vector3(limitedVel.x, vel.y, limitedVel.z);
            }
        }
    
        private void Reset()
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }
    }
}
