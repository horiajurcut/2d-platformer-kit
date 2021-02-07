using UnityEngine;

[RequireComponent(typeof(CharacterController2D))]
public class Player : MonoBehaviour
{
    public float moveSpeed = 6f;
    
    public float jumpHeight = 4f;
    public float timeToJumpApex = .4f;
    
    public float accelerationTimeAirborne = .2f;
    public float accelerationTimeGrounded = .1f;
    
    private float _gravity;
    private float _jumpVelocity;
    private float _velocityXSmoothing;
    
    private Vector3 _velocity;

    private CharacterController2D _characterController2D;

    private void Start()
    {
        _characterController2D = GetComponent<CharacterController2D>();
        
        ComputeEquationsOfMotion();
    }
    
    /// <summary>
    /// Compute <c>_gravity</c> and <c>_jumpVelocity</c> by substituting
    /// <c>jumpHeight</c> and <c>timeToApex</c> in the equations of motion.
    /// </summary>
    /// <remarks>
    /// <code>
    /// deltaMovement = initialVelocity * time + (acceleration * time * time) / 2
    /// initialVelocity = 0; acceleration = gravity;
    /// jumpHeight = 0 + (gravity * timeToJumpApex * timeToJumpApex) / 2
    /// finalVelocity = initialVelocity + (acceleration * time)
    /// </code>
    /// </remarks>
    private void ComputeEquationsOfMotion()
    {
        _gravity = -(2 * jumpHeight) / (timeToJumpApex * timeToJumpApex);
        _jumpVelocity = Mathf.Abs(_gravity) * timeToJumpApex;
    }

    private void Update()
    {
        if (_characterController2D.Collisions.Above || _characterController2D.Collisions.Below)
        {
            _velocity.y = 0f;
        }
        
        // TODO: Replace with new Input System
        var input = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));
        if (Input.GetKeyDown(KeyCode.Space) && _characterController2D.Collisions.Below)
        {
            _velocity.y = _jumpVelocity;
        }

        var targetVelocityX = input.x * moveSpeed;
        var smoothTime = _characterController2D.Collisions.Below ? accelerationTimeGrounded : accelerationTimeAirborne;
        
        _velocity.x = Mathf.SmoothDamp(_velocity.x, targetVelocityX, ref _velocityXSmoothing, smoothTime);
        _velocity.y += _gravity * Time.deltaTime;
        
        _characterController2D.Move(_velocity * Time.deltaTime);
    }
}
