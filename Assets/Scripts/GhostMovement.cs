using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GhostMovement : MonoBehaviour
{
    [Header("Ghost")]
    [SerializeField] private GameObject ghostGameObject;
    [SerializeField] private float maxDistance = 8;
    [SerializeField] private float ghostSmoothingSpeed = 15.0f;
    [SerializeField] private LayerMask groundLayer;
    private bool isInactive = true;


    [Space]
    [Header("Dash")]
    [SerializeField] private float dashCooldown = 1f;
    [SerializeField] private float dashSpeed = 50f;
    [SerializeField][Range(0.2f, 1f)] private float wallColissionOffset = 0.2f;
    private float dashTimer = 0f;
    private bool canDash = true;

    private bool dashing = false;
    private int dashFrame = 0;
    private int dashLength = 0;
    private Vector3 dashTarget;
    private ArrayList points = new ArrayList();

    private Transform playerTransform;
    private Rigidbody2D playerRb;
    private PlayerMovement playerMovement;

    // Start is called before the first frame update
    void Start()
    {
        playerTransform = GetComponent<Transform>();
        playerRb = GetComponent<Rigidbody2D>();
        playerMovement = GetComponent<PlayerMovement>();
        ghostGameObject.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {
        HandleDash();
        if (!canDash){
            dashTimer+=Time.deltaTime;
            if(dashTimer>=dashCooldown){
                canDash=true;
                dashTimer=0f;
            }
            ghostGameObject.SetActive(false);
            return;
        }
        if(Input.GetButton("Fire2")){
            if(isInactive) ghostGameObject.transform.position = playerTransform.position;
            isInactive = false;
            ghostGameObject.SetActive(true);
        } 
        else{
            isInactive = true;
            ghostGameObject.SetActive(false);
        } 
        if(!ghostGameObject.activeSelf) return;

        Vector3 targetPosition = GetGhostPosition(ghostGameObject.transform.position.z, maxDistance);
        ghostGameObject.transform.position = Vector3.Lerp(ghostGameObject.transform.position, targetPosition, Time.deltaTime * ghostSmoothingSpeed);

        if(Input.GetButtonDown("Dash")){
            isInactive = true;
            points.Clear();
            dashing = true;
            dashTimer = 0f;
            canDash = false;
            dashFrame = 0;
            dashLength = 0;
            dashTarget = ghostGameObject.transform.position;
            GetDashPoints(points, playerTransform.position, dashTarget, dashSpeed);

            if(dashTarget.x>=playerTransform.position.x && !playerMovement.IsFacingRight()){
                playerMovement.Flip();
            }
            if(dashTarget.x<playerTransform.position.x && playerMovement.IsFacingRight()){
                playerMovement.Flip();
            }
            
            ghostGameObject.SetActive(false);
        }
    }

    void GetDashPoints(ArrayList points, Vector3 start, Vector3 end, float speed)
    {
        float distance = Vector3.Distance(start, end);
        float totalDashTime = distance / speed; // Total time needed to complete the dash
        float elapsedTime = 0f; // Time elapsed since the start of the dash

        while (elapsedTime < totalDashTime)
        {
            float t = Mathf.Clamp01(elapsedTime / totalDashTime); // Clamping t between 0 and 1
            Vector3 point = Vector3.Lerp(start, end, t);
            points.Add(point);

            // Increment the time elapsed based on Time.deltaTime
            elapsedTime += Time.deltaTime;

            dashLength++;
        }
    }

    void HandleDash(){
        if(dashing && dashLength>dashFrame){
            Vector3 pointPosition = (Vector3) points[dashFrame];
            if(!InsideCollider(pointPosition) || !InsideCollider(dashTarget)) playerTransform.position = pointPosition;
            playerRb.velocity = new Vector2(0, 0);
            dashFrame++;
        } else if (dashing && dashLength==dashFrame){
            //last position swap
            if(!InsideCollider(dashTarget)) playerTransform.position = dashTarget;
            playerRb.velocity = new Vector2(playerRb.velocity.x, 10f);
            dashing = false;
        }
    }

    bool InsideCollider(Vector3 pointPosition){
        Collider2D[] colliders = Physics2D.OverlapCircleAll(pointPosition, 0.5f, groundLayer);
        for (int i = 0; i < colliders.Length; i++)
        {
            if (colliders[i].gameObject != gameObject)
            {
                return true;
            }
        }
        return false;
    }

    Vector3 GetGhostPosition(float z, float maxDistanceFromPlayer){
        Vector3 mouseWorldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        mouseWorldPos = new Vector3(mouseWorldPos.x, mouseWorldPos.y, z);
        Vector3 playerPosition = transform.position;
        Vector3 targetPosition = ClampPos(mouseWorldPos, playerPosition, maxDistanceFromPlayer);

        RaycastHit2D hit = Physics2D.Raycast(playerPosition, targetPosition - playerPosition, Mathf.Min(Vector3.Distance(playerPosition, targetPosition)+0.2f, maxDistanceFromPlayer), groundLayer);
        Debug.DrawRay(playerPosition, (targetPosition - playerPosition), Color.blue);
        if (hit.collider != null){
            targetPosition = hit.point + hit.normal * wallColissionOffset;
            Debug.DrawLine(playerPosition, targetPosition, Color.red);
        }

        return targetPosition;
    }

    Vector3 ClampPos(Vector3 targetPosition, Vector3 playerPosition, float maxDistance){
        float distance = Vector3.Distance(targetPosition, playerPosition);
        if(distance > maxDistance){
            Vector3 direction = (targetPosition - playerPosition).normalized;
            return playerPosition + direction * maxDistance;
        }
        else return targetPosition;
    }
}
