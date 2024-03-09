using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    [Header("Components")]
    private Rigidbody2D _rb;
    private CapsuleCollider2D _cc;
    [SerializeField] private Animator _anim;

    [SerializeField] private bool _facingRight = true;
    [Header("Layer Masks")]
    [SerializeField] private LayerMask _groundLayer;

    [Header("Running")]
    [SerializeField] private float _acceleration = 35f;
    [SerializeField] private float _maxSpeed = 12f;
    [SerializeField] private float _deceleration = 15f;
    private float _horizontalDirection;
    private bool _changingDirection => (_horizontalDirection > 0 && _rb.velocity.x < 0) || (_horizontalDirection < 0 && _rb.velocity.x > 0);

    [Header("Jumping")]
    [SerializeField] private float _jumpForce = 25f;
    [SerializeField] private float _airDeceleration = 2.5f;
    [SerializeField] private float _fallMultiplier = 8f;
    [SerializeField] private float _lowJumpFallMultiplier = 5f;
    [SerializeField] private bool _enableJumpBuffer = false;
    [SerializeField][Range(0f, 1f)] private float _jumpBufferTimeWindow = 0.2f;
    private bool _jumpBuffer = false;
    private float _jumpBufferTimer = 0f;
    private bool _canJump => handleJump();
    
    [Header("Ground Collision")]
    [SerializeField] private float _baseGroundRaycastLength = 1.5f;
    private float _groundRaycastLength;
    [SerializeField] private Vector3 _groundRaycastOffset;
    private bool _onGround;

    
    [Header("Slopes")]
    [SerializeField] private float _slopeCheckDistance = 1.2f;
    private float _slopeSideAngle;
    private float _slopeDownAngle;
    private Vector2 _colliderSize;
    private Vector2 _slopeNormalPerp;
    private bool _isOnSlope;
    private float _slopeDownAngleOld;
    [SerializeField] private PhysicsMaterial2D _noFriction;
    [SerializeField] private PhysicsMaterial2D _fullFriction;


    Vector3 jumpPosition;

    void Start()
    {
        _rb = GetComponent<Rigidbody2D>();
        _cc = GetComponent<CapsuleCollider2D>();
        _anim = GetComponent<Animator>();


        _colliderSize = _cc.size;
        _groundRaycastLength = _baseGroundRaycastLength;
    }

    void Update()
    {
        _anim.SetBool("isJumping", !_onGround);
        _horizontalDirection = GetInput().x;
        if(_isOnSlope){
            _groundRaycastLength = _baseGroundRaycastLength + 0.5f;
        } else _groundRaycastLength = _baseGroundRaycastLength;
        
        if(_canJump)
        {
            Jump();
            jumpPosition = transform.position;
            
        }
        if(_jumpBuffer){
            _jumpBufferTimer+=Time.deltaTime;
            if(_jumpBufferTimer>=_jumpBufferTimeWindow){
                _jumpBuffer = false;
                _jumpBufferTimer = 0f;
            }
        }
    }
    private void FixedUpdate()
    {   
        CheckCollisions();
        SlopesCheck();
        MoveCharacter();
        ApplyGroundDeceleration();
        _anim.SetFloat("speed", Mathf.Abs(_horizontalDirection));
        if(_onGround)
        {
            ApplyGroundDeceleration();
        }
        else
        {
            ApplyAirDeceleration();
            FallMultiplier();
        }
        if((_horizontalDirection > 0 && !_facingRight) || (_horizontalDirection < 0 && _facingRight))
        {
            Flip();
        }
    }

    private bool handleJump(){
        bool jump = Input.GetButtonDown("Jump");
        if(!_onGround && jump){
            _jumpBuffer = true;
            _jumpBufferTimer = 0f;
            
        
        }
        return (jump || (_jumpBuffer && _enableJumpBuffer)) &&  _onGround;
        
    }
    
    private Vector2 GetInput()
    {
        return new Vector2(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical"));
    }
    private void MoveCharacter()

    {
        _rb.AddForce(new Vector2(_horizontalDirection, 0f) * _acceleration);

        if (Mathf.Abs(_rb.velocity.x) > _maxSpeed)
        {
            _rb.velocity = new Vector2(Mathf.Sign(_rb.velocity.x) * _maxSpeed, _rb.velocity.y);
        }


        if(_isOnSlope)
        {
            _rb.velocity = new Vector2(_horizontalDirection  * _maxSpeed, _rb.velocity.y);
        }
        else
        {
            _rb.velocity = new Vector2(_horizontalDirection  * _maxSpeed, _rb.velocity.y);
        }
        
    }
    private void Jump()
    {
        _rb.velocity = new Vector2(_rb.velocity.x, 0);
        _rb.AddForce(Vector2.up * _jumpForce, ForceMode2D.Impulse);
        _jumpBuffer = false;
        
    }
    private void ApplyGroundDeceleration()
    {
        if (Mathf.Abs(_horizontalDirection) < 0.4f)
        {
            _rb.drag = _deceleration;
        }
        else
        {
            _rb.drag = 0;
        }
    }
    private void ApplyAirDeceleration()
    {
        _rb.drag = _airDeceleration;
    }


    private void FallMultiplier()
    {
        if (_rb.velocity.y < 0)
        {
            _rb.gravityScale = _fallMultiplier;
        }
        else if (_rb.velocity.y > 0 && !Input.GetButton("Jump"))
        {
            _rb.gravityScale = _lowJumpFallMultiplier;
        }
        else
        {
            _rb.gravityScale = 1f;
        }
    }
    private void CheckCollisions()
    {
        _onGround = Physics2D.Raycast(transform.position + _groundRaycastOffset, Vector2.down, _groundRaycastLength, _groundLayer) ||
                    Physics2D.Raycast(transform.position - _groundRaycastOffset, Vector2.down, _groundRaycastLength, _groundLayer);

        
    }
    private void SlopesCheck()
    {
        Vector2 checkPos = transform.position - new Vector3(0.0f,  _cc.size.y / 2);

        SlopesCheckHorizontal(checkPos);
        SlopesCheckVertical(checkPos);
    }
    private void SlopesCheckHorizontal(Vector2 checkPos)
    {
        RaycastHit2D slopeHitFront = Physics2D.Raycast(checkPos, transform.right, _slopeCheckDistance, _groundLayer);
        RaycastHit2D slopeHitBack = Physics2D.Raycast(checkPos, -transform.right, _slopeCheckDistance, _groundLayer);

        if(slopeHitFront)
        {
            _isOnSlope = true;
            _slopeSideAngle = Vector2.Angle(slopeHitFront.normal, Vector2.up);
        }
        else if(slopeHitBack)
        {
            _isOnSlope = true;
            _slopeSideAngle = Vector2.Angle(slopeHitBack.normal, Vector2.up);
        }
        else
        {
            _slopeSideAngle = 0;
            _isOnSlope = false;
        }
    }
    private void SlopesCheckVertical(Vector2 checkPos)
    {
        RaycastHit2D hit = Physics2D.Raycast(checkPos, Vector2.down, _slopeCheckDistance, _groundLayer);

        if(hit)
        {
            _slopeNormalPerp = Vector2.Perpendicular(hit.normal).normalized;

            _slopeDownAngle = Vector2.Angle(hit.normal, Vector2.up);

            if(_slopeDownAngle != _slopeDownAngleOld)
            {
                _isOnSlope = true;
            }

            _slopeDownAngleOld = _slopeDownAngle;

            // Debug.DrawRay(hit.point, _slopeNormalPerp, Color.red);
            // Debug.DrawRay(hit.point, hit.normal, Color.green);
        }
        if(_isOnSlope && _horizontalDirection == 0)
        {
            _rb.sharedMaterial = _fullFriction;
        }
        else
        {
            _rb.sharedMaterial = _noFriction;
        }
    }
    private void OnDrawGizmos()
    {
        // Gizmos.color = Color.red;
        // Gizmos.DrawLine(transform.position + _groundRaycastOffset, transform.position + _groundRaycastOffset + Vector3.down * _groundRaycastLength);
        // Gizmos.DrawLine(transform.position - _groundRaycastOffset, transform.position - _groundRaycastOffset + Vector3.down * _groundRaycastLength);

        if (Input.GetButton("Jump"))
        {
        Gizmos.color = Color.red;
        float height = _cc.size.y;
        float radius = _cc.size.x / 2;
        Vector3 center = jumpPosition + new Vector3(_cc.offset.x, _cc.offset.y, 0);

        // Draw the two half circles
        Gizmos.DrawWireSphere(center + Vector3.up * (height / 2 - radius), radius);
        Gizmos.DrawWireSphere(center - Vector3.up * (height / 2 - radius), radius);

        // Draw the rectangle in the middle
        Gizmos.DrawWireCube(center, new Vector3(_cc.size.x, height - 2 * radius, 0));
        }
    }
    
    public void Flip()
    {
        _facingRight = !_facingRight;
        Vector3 scale = transform.localScale;
        scale.x *= -1;
        transform.localScale = scale;
    }
    public bool IsFacingRight(){
        return _facingRight;
    }
}

