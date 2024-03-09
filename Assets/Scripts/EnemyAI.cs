using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Pathfinding;

public class EnemyAI : MonoBehaviour
{
    [Header("Pathfinding")]
    public Transform target;
    public float activateDistance = 30f;
    public float pathUpdateSeconds = 0.5f;

    [Header("Physics")]
    public float speed = 200f;
    public float jumpForce = 100f;
    public float nextWaypointDistance = 3f;
    public float jumpNodeHeightRequirement = 0.8f;
    public float jumpModifier = 0.3f;
    public float jumpCheckOffset = 0.1f;

    [Header("Custom Behavior")]
    public bool followEnabled = true;
    public bool jumpEnabled = true;
    private bool isJumping, isInAir;
    public bool directionLookEnabled = true;
    private bool isOnCoolDown;

    [Header("Ground Collision")]
    [SerializeField] private float _baseGroundRaycastLength = 1.5f;
    [SerializeField] private LayerMask _groundLayer;
    [SerializeField] private Vector3 _groundRaycastOffset;
    private float _groundRaycastLength;
    bool isGrounded;


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


    private Path path;
    private int currentWaypoint = 0;
    private float horizontalDirection;

    
    Seeker seeker;
    Rigidbody2D rb;
    CapsuleCollider2D _cc;
    private GameObject player;
    Animator anim;

    public void Start()
    {      
        player = GameObject.FindGameObjectWithTag("Player");
        _cc = GetComponent<CapsuleCollider2D>();
        _colliderSize = _cc.size;
        seeker = GetComponent<Seeker>();
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
        _groundRaycastLength = _baseGroundRaycastLength;
        InvokeRepeating("UpdatePath", 0f, pathUpdateSeconds);
    }

    private void Update() {
        // Debug.Log(horizontalDirection);
        // Debug.Log(_isOnSlope);
    }
    private void FixedUpdate()
    {   

        // SlopesCheck();
        if(TargetInDistance() && followEnabled)
        {
            PathFollow();
        }
        if(TargetInDistance()) horizontalDirection = (player.transform.position.x - transform.position.x) > 0 ? 1 : -1;

    }

    private void UpdatePath()
    {
        if(followEnabled && TargetInDistance() && seeker.IsDone())
        {
            seeker.StartPath(rb.position, target.position, OnPathComplete);
        }
    }
    private void PathFollow(){
        if(path == null)
        {
            return;
        }

        if(currentWaypoint >= path.vectorPath.Count)
        {
            return;
        }

        Vector3 startOffset = transform.position - new Vector3(0f, GetComponent<Collider2D>().bounds.extents.y + jumpCheckOffset, 0f);
        isGrounded = Physics2D.Raycast(transform.position + _groundRaycastOffset, Vector2.down, _groundRaycastLength, _groundLayer) ||
                    Physics2D.Raycast(transform.position - _groundRaycastOffset, Vector2.down, _groundRaycastLength, _groundLayer);

        Vector2 direction = ((Vector2)path.vectorPath[currentWaypoint] - rb.position).normalized;
        Vector2 force = direction * speed * Time.deltaTime;
        // Debug.Log(direction.y);

        if (jumpEnabled && isGrounded && !isInAir && !isOnCoolDown)
        {
            if (direction.y > jumpNodeHeightRequirement)
            {
                if (isInAir) return;
                Debug.Log("Jump");
                isJumping = true;
                rb.velocity = new Vector2(rb.velocity.x, jumpForce);
                StartCoroutine(JumpCoolDown());

            }
        }
        if (isGrounded)
        {
            isJumping = false;
            isInAir = false; 
        }
        else
        {
            isInAir = true;

        }

        // rb.AddForce(force);
        rb.velocity = new Vector2(horizontalDirection *  speed, rb.velocity.y);
        if(_isOnSlope)
        {
            isInAir = true;
        }
        else
        {
            
        }
        
        
        if(rb.velocity !=  Vector2.zero){
            anim.SetFloat("Speed", 1);
        }else{
            anim.SetFloat("Speed", 0);
        }

        float distance = Vector2.Distance(rb.position, path.vectorPath[currentWaypoint]);
        if(distance < nextWaypointDistance)
        {
            currentWaypoint++;
        }

        if(directionLookEnabled)
        {
            if(rb.velocity.x >= 0.01f)
            {
                transform.localScale = new Vector3(-1f, 1f, 1f);
            }
            else if(rb.velocity.x <= -0.01f)
            {
                transform.localScale = new Vector3(1f, 1f, 1f);
            }
        }
    }
    private bool TargetInDistance()
    {
        return Vector2.Distance(transform.position, target.transform.position) < activateDistance;
    }

    private void OnPathComplete(Path p)
    {
        if(!p.error)
        {
            path = p;
            currentWaypoint = 0;
        }
    }
    
    IEnumerator JumpCoolDown()
    {
        isOnCoolDown = true; 
        yield return new WaitForSeconds(1f);
        isOnCoolDown = false;
    }
    // private void SlopesCheck()
    // {
    //     Vector2 checkPos = transform.position - new Vector3(0.0f,  _cc.size.y / 2);

    //     SlopesCheckHorizontal(checkPos);
    //     SlopesCheckVertical(checkPos);
    // }
    // private void SlopesCheckHorizontal(Vector2 checkPos)
    // {
    //     RaycastHit2D slopeHitFront = Physics2D.Raycast(checkPos, transform.right, _slopeCheckDistance, _groundLayer);
    //     RaycastHit2D slopeHitBack = Physics2D.Raycast(checkPos, -transform.right, _slopeCheckDistance, _groundLayer);

    //     if(slopeHitFront)
    //     {
    //         _isOnSlope = true;
    //         _slopeSideAngle = Vector2.Angle(slopeHitFront.normal, Vector2.up);
    //     }
    //     else if(slopeHitBack)
    //     {
    //         _isOnSlope = true;
    //         _slopeSideAngle = Vector2.Angle(slopeHitBack.normal, Vector2.up);
    //     }
    //     else
    //     {
    //         _slopeSideAngle = 0;
    //         _isOnSlope = false;
    //     }
    // }
    // private void SlopesCheckVertical(Vector2 checkPos)
    // {
    //     RaycastHit2D hit = Physics2D.Raycast(checkPos, Vector2.down, _slopeCheckDistance, _groundLayer);

    //     if(hit)
    //     {
    //         _slopeNormalPerp = Vector2.Perpendicular(hit.normal).normalized;

    //         _slopeDownAngle = Vector2.Angle(hit.normal, Vector2.up);

    //         if(_slopeDownAngle != _slopeDownAngleOld)
    //         {
    //             _isOnSlope = true;
    //         }

    //         _slopeDownAngleOld = _slopeDownAngle;

    //         Debug.DrawRay(hit.point, _slopeNormalPerp, Color.red);
    //         Debug.DrawRay(hit.point, hit.normal, Color.green);
    //     }
    //     if(_isOnSlope)
    //     //  && _horizontalDirection == 0
    //     {
    //         rb.sharedMaterial = _fullFriction;
    //     }
    //     else
    //     {
    //         rb.sharedMaterial = _noFriction;
    //     }
    // }

}
