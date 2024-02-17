using FishNet.Connection;
using FishNet.Object;
using UnityEngine;

namespace FishNet.Example.Scened
{
    public class PlayerController : NetworkBehaviour
    {
        [SerializeField] private GameObject _camera;
        [SerializeField] private float _moveRate = 4f;
        [SerializeField] private Animator _animator;

        private Rigidbody _rigidbody;
        private CapsuleCollider _collider;
        private bool _jumping;
        private bool _tempBehaviourLocked;

        public float walkSpeed = 0.15f;
        public float runSpeed = 1.0f;
        public float sprintSpeed = 2.0f;
        public float speedDampTime = 0.1f;
        public string jumpButton = "Jump";
        public float jumpHeight = 1.5f;
        public float jumpInertialForce = 10f;
        private float speed, speedSeeker;
        private int jumpBool;
        private int groundedBool;
        private bool isColliding;

        public override void OnStartServer()
        {
            base.OnStartServer();
            _rigidbody = GetComponent<Rigidbody>();
            _collider = GetComponent<CapsuleCollider>();
            speedSeeker = runSpeed;
            jumpBool = Animator.StringToHash("Jump");
            groundedBool = Animator.StringToHash("Grounded");
            _animator.SetBool(groundedBool, true);
        }

        private void Update()
        {
            if (!base.IsOwner)
                return;

            float hor = Input.GetAxisRaw("Horizontal");
            float ver = Input.GetAxisRaw("Vertical");

            if (Input.GetButtonDown("Jump"))
            {
                _jumping = true;
                ServerJump();
            }

            ServerMove(hor, ver);

            UpdateAnimator(hor, ver);

            // Call the basic movement manager.
            MovementManagement(hor, ver);

            // Call the jump manager.
            JumpManagement();
        }

        [ServerRpc]
        private void ServerMove(float hor, float ver)
        {
            Vector3 direction = new Vector3(hor, 0f, ver).normalized * _moveRate * Time.deltaTime;
            _rigidbody.MovePosition(_rigidbody.position + transform.TransformDirection(direction));

            // Call function that deals with player orientation.
            Rotating(hor, ver);
        }

        [ServerRpc]
        private void ServerJump()
        {
            if (IsGrounded())
            {
                _jumping = true;
            }
        }

        private bool IsGrounded()
        {
            Ray ray = new Ray(transform.position + new Vector3(0f, 0.1f, 0f), -Vector3.up);
            return Physics.Raycast(ray, 0.2f);
        }

        private void UpdateAnimator(float hor, float ver)
        {
            // Calcula a velocidade do jogador baseada nos inputs de movimento
            float speed = Mathf.Abs(hor) + Mathf.Abs(ver);
            _animator.SetFloat("Speed", speed);
        }

        // Rotate the player to match correct orientation, according to camera and key pressed.
        Vector3 Rotating(float horizontal, float vertical)
        {
            // Get camera forward direction, without vertical component.
            Vector3 forward = _camera.transform.TransformDirection(Vector3.forward);

            // Player is moving on ground, Y component of camera facing is not relevant.
            forward.y = 0.0f;
            forward = forward.normalized;

            // Calculate target direction based on camera forward and direction key.
            Vector3 right = new Vector3(forward.z, 0, -forward.x);
            Vector3 targetDirection = forward * vertical + right * horizontal;

            // Lerp current direction to calculated target direction.
            if ((Mathf.Abs(horizontal) > 0.1 || Mathf.Abs(vertical) > 0.1))
            {
                Quaternion targetRotation = Quaternion.LookRotation(targetDirection);
                Quaternion newRotation = Quaternion.Slerp(_rigidbody.rotation, targetRotation, Time.deltaTime * speedDampTime);
                _rigidbody.MoveRotation(newRotation);
            }
            // If idle, Ignore current camera facing and consider last moving direction.
            else
            {
                Repositioning();
            }

            return targetDirection;
        }

        private void Repositioning()
        {
            // Implement your repositioning logic here
        }

        // Deal with the basic player movement
        void MovementManagement(float horizontal, float vertical)
        {
            // On ground, obey gravity.
            if (IsGrounded())
                _rigidbody.useGravity = true;

            // Avoid takeoff when reached a slope end.
            else if (!_jumping && _rigidbody.velocity.y > 0)
            {
                RemoveVerticalVelocity();
            }

            // Set proper speed.
            Vector2 dir = new Vector2(horizontal, vertical);
            speed = Vector2.ClampMagnitude(dir, 1f).magnitude;
            // This is for PC only, gamepads control speed via analog stick.
            speedSeeker += Input.GetAxis("Mouse ScrollWheel");
            speedSeeker = Mathf.Clamp(speedSeeker, walkSpeed, runSpeed);
            speed *= speedSeeker;
            if (_jumping)
            {
                speed = sprintSpeed;
            }

            _animator.SetFloat("Speed", speed, speedDampTime, Time.deltaTime);
        }

        // Remove vertical rigidbody velocity.
        private void RemoveVerticalVelocity()
        {
            Vector3 horizontalVelocity = _rigidbody.velocity;
            horizontalVelocity.y = 0;
            _rigidbody.velocity = horizontalVelocity;
        }

        // Execute the idle and walk/run jump movements.
        void JumpManagement()
        {
            // Start a new jump.
            if (_jumping && !_animator.GetBool(jumpBool) && IsGrounded())
            {
                // Set jump related parameters.
                _tempBehaviourLocked = true;
                _animator.SetBool(jumpBool, true);
                // Is a locomotion jump?
                if (_animator.GetFloat("Speed") > 0.1)
                {
                    // Temporarily change player friction to pass through obstacles.
                    _collider.material.dynamicFriction = 0f;
                    _collider.material.staticFriction = 0f;
                    // Remove vertical velocity to avoid "super jumps" on slope ends.
                    RemoveVerticalVelocity();
                    // Set jump vertical impulse velocity.
                    float velocity = 2f * Mathf.Abs(Physics.gravity.y) * jumpHeight;
                    velocity = Mathf.Sqrt(velocity);
                    _rigidbody.AddForce(Vector3.up * velocity, ForceMode.VelocityChange);
                }
            }
            // Is already jumping?
            else if (_animator.GetBool(jumpBool))
            {
                // Keep forward movement while in the air.
                if (!IsGrounded() && !isColliding && _tempBehaviourLocked)
                {
                    _rigidbody.AddForce(transform.forward * (jumpInertialForce * Physics.gravity.magnitude * sprintSpeed), ForceMode.Acceleration);
                }
                // Has landed?
                if ((_rigidbody.velocity.y < 0) && IsGrounded())
                {
                    _animator.SetBool(groundedBool, true);
                    // Change back player friction to default.
                    _collider.material.dynamicFriction = 0.6f;
                    _collider.material.staticFriction = 0.6f;
                    // Set jump related parameters.
                    _jumping = false;
                    _animator.SetBool(jumpBool, false);
                    _tempBehaviourLocked = false;
                }
            }
        }

        // Collision detection.
        private void OnCollisionStay(Collision collision)
        {
            isColliding = true;
            // Slide on vertical obstacles
            if (IsGrounded() && collision.GetContact(0).normal.y <= 0.1f)
            {
                _collider.material.dynamicFriction = 0f;
                _collider.material.staticFriction = 0f;
            }
        }

        private void OnCollisionExit(Collision collision)
        {
            isColliding = false;
            _collider.material.dynamicFriction = 0.6f;
            _collider.material.staticFriction = 0.6f;
        }
    }
}
