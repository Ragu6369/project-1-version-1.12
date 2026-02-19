using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.InputSystem;

public class DummyScrip_mario : MonoBehaviour
{
    [Header("RayLengths")]
    [SerializeField] private float downRayLength = 30f;
    [SerializeField] private float forwardRayLength = 30f;
    [SerializeField] private float backRayLength = 30f;
    [SerializeField] private float rightRayLength = 30f;
    [SerializeField] private float leftRayLength = 30f;
    [SerializeField] private float rayCastLength = 30f;

    [SerializeField] private float rotationSpeed;
    private float tmpRotationSpeed;
    [SerializeField] private float speed;

    [SerializeField] private float gravity;
    private float tmpGravity;

    public float jumpForce;
    private float tmpJumpForce;
    public float ExtraJumpForce;
    

    private Rigidbody rb;
    public Transform currentPlanet;
    public Transform previousParent;
    public Transform playerVisual;
    public Transform CurrentParent;
    [SerializeField] private Vector3 default_rotation;
    [SerializeField] private float downgravity = 2f;

    [SerializeField] private float planetDetectionRadius = 10f;
    [SerializeField] private LayerMask planetmask;

    private float tmp_downgravity;
    [SerializeField] private float speedIncreaser = 1.5f;
    private float tmpspeed;

    RaycastHit[] hits;
    Vector3 planetDir;
    Vector3 normalVector;
    Vector3 input;

    public bool isTouchingPlanetSurface = false;
    private Transform MainCameraTransform;
    public Transform CameraArmTransform;

    private Char_control playerInput;
    private CharacterController char_controller;
    Animator anim;

    public bool PlatformParented;
    public float OnceParented = 0f;
    bool CanJump = true;
    bool slowDown = false;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        MainCameraTransform = Camera.main.transform;
         anim = GetComponent<Animator>();
        tmpGravity = gravity;
        tmpspeed = speed;
        previousParent = null;
        tmp_downgravity = downgravity;
        tmpRotationSpeed = rotationSpeed;
        tmpJumpForce = jumpForce;
        default_rotation = new Vector3(0, 0, 0);

        playerInput = new Char_control();
        char_controller = GetComponent<CharacterController>();
        playerInput.player_movement.JUMP.performed += Jump;
    }

    private void FixedUpdate()
    {

        Movement();
        FindNearestPlanet();
        ApplyGravity();
        ApplyPlanetRotation();
        CurrentParent = transform.parent;

        // Adjust JumpForce based on Planet State
        if (currentPlanet != null)
        {
            jumpForce = ExtraJumpForce;
        }
        else
        {
            jumpForce = tmpJumpForce;
        }
    }

    void Jump(InputAction.CallbackContext context)
    {
        if (!CanJump) return;
        rb.velocity *= 0;
        rb.AddForce(normalVector * jumpForce, ForceMode.Impulse);
        gravity = tmpGravity / 2f;
        speed = speed * speedIncreaser;
        Invoke(nameof(RestoreGravity), 0.2f);
        CanJump = false;
        // can jump = true after 0.2 seconds 
        rotationSpeed = tmpRotationSpeed / 2f;
    }

    void RestoreGravity()
    {
        gravity = tmpGravity;
        speed = tmpspeed;
        CanJump = true;
        slowDown = false;
    }

    //-----------------------------------Find Near Planet-------------------------------------//
    private void FindNearestPlanet()
    {
        if(currentPlanet != null)
        { 
            previousParent = currentPlanet;
        }
        if(previousParent != null && OnceParented > 0 && PlatformParented == false)
        {
            currentPlanet = previousParent;
            return;
        }
       
        Collider[] hits = Physics.OverlapSphere(transform.position, planetDetectionRadius, planetmask);

        if (hits.Length == 0)
        {
            currentPlanet = null;
            return;
        }

        float closestDist = Mathf.Infinity;
        Transform nearest = null;

        foreach (Collider col in hits)
        {
            float dist = Vector3.Distance(transform.position, col.transform.position);
            if (dist < closestDist)
            {
                closestDist = dist;
                nearest = col.transform.root;
            }
        }

        if (currentPlanet == nearest)
        {
            return;
        }

        if (nearest != currentPlanet && nearest != null)
        {
            currentPlanet = nearest;
            EnterNewGravityField();
        }
    }



    public void EnterNewGravityField()
    {
        gravity = tmpGravity / 4f;
        rb.velocity *= .5f;
        rotationSpeed = tmpRotationSpeed / 10f;
        slowDown = true;
        CanJump = false;
        Invoke(nameof(RestoreGravity), .5f);
    }

    void Movement()
    {
        input = new Vector3(Input.GetAxisRaw("Horizontal"), 0, Input.GetAxisRaw("Vertical"));
        float yaw = CameraArmTransform.GetComponent<CameraArmController>().GetYaw();
        Vector3 cameraRotation = new Vector3(0, yaw, 0);
        Vector3 Dir = Quaternion.Euler(cameraRotation) * input;
        Vector3 movement_dir = (transform.forward * Dir.z + transform.right * Dir.x);
        Vector3 currentNormalVelocity = Vector3.Project(rb.velocity, normalVector.normalized);
        rb.velocity = currentNormalVelocity + (movement_dir * speed);

        if (movement_dir != Vector3.zero)
        {
            anim.SetBool("IsWalking", true);
            playerVisual.localRotation = Quaternion.LookRotation(Dir);
        }
        else
        {
            anim.SetBool("IsWalking", false);
        }
        if (slowDown)
            rb.velocity *= .5f;
    }

    void ApplyGravity()
    {

        // Sweep Rays  
        hits = Physics.RaycastAll(transform.position, -transform.up, downRayLength);


        if (hits.Length == 0)
        {
            hits = Physics.RaycastAll(transform.position, transform.forward,forwardRayLength);
        }

        if (hits.Length == 0)
        {
            hits = Physics.RaycastAll(transform.position, -transform.forward, backRayLength);
        }

        if (hits.Length == 0)
        {
            hits = Physics.RaycastAll(transform.position, transform.right, rightRayLength);
        }

        if (hits.Length == 0)
        {
            hits = Physics.RaycastAll(transform.position, -transform.right, leftRayLength);
        }

        // If no hits and no planet -> apply downward gravity
        if (hits.Length == 0 && currentPlanet == null)
        {

            rb.AddForce(Vector3.down * -gravity * downgravity, ForceMode.Acceleration);
            // downgravity += 0.5f;  // keep increasing until ground 
            return;

        }

        // final check of raycast when there is a planet 
        if (hits.Length == 0 && currentPlanet != null)
        {
            planetDir = currentPlanet.position - transform.position;
            hits = Physics.RaycastAll(transform.position, planetDir, rayCastLength);
        }


        GetPlanetNormal();
        rb.AddForce(normalVector.normalized * gravity, ForceMode.Acceleration);
        hits = new RaycastHit[0];
    }

    void ApplyPlanetRotation()
    {

        if (currentPlanet == null)
        {
            Quaternion dtargetRotation = Quaternion.FromToRotation(transform.up, Vector3.up) * transform.rotation; ;
            transform.rotation = Quaternion.Slerp(transform.rotation, dtargetRotation, rotationSpeed * Time.deltaTime);

        }
        else if (currentPlanet != null)
        {
            Quaternion targetRotation = Quaternion.FromToRotation(transform.up, normalVector) * transform.rotation;
            transform.rotation = Quaternion.Lerp(transform.rotation, targetRotation, rotationSpeed * Time.fixedDeltaTime);
        }

        if (isTouchingPlanetSurface && CanJump)
            rotationSpeed = tmpRotationSpeed;
    }

    void GetPlanetNormal()
    {
        if (currentPlanet == null)
        {
            // Default to world up when no planet 
            normalVector = Vector3.up;
            return;

        }
        normalVector = (transform.position - currentPlanet.position).normalized;
        for (int i = 0; i < hits.Length; i++)
        {
            if (hits[i].transform == currentPlanet)
            {
                normalVector = hits[i].normal.normalized;
                break;
            }
        }
        return;
    }

    private void OnTriggerEnter(Collider other)
    {

        if (other.transform == currentPlanet)
        {
            isTouchingPlanetSurface = true;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.transform == currentPlanet)
        {
            isTouchingPlanetSurface = false;
            currentPlanet = null; // reset to world gravity 
        }
    }

    private void OnEnable()
    {
        playerInput.Enable();
    }

    private void OnDisable()
    {
        playerInput.Disable();
    }

    //-----------------------------------Gizmos Visualization-------------------------------------//

    private void OnDrawGizmos()
    {
        if (!Application.isPlaying) return;

        if (currentPlanet != null)
        {
            Vector3 gravityDirection = (transform.position - currentPlanet.position).normalized;
            // Draw gravity direction ray
            Gizmos.color = Color.yellow;
            Gizmos.DrawLine(transform.position, transform.position + gravityDirection * rayCastLength);
        }

        // Draw detection sphere
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(transform.position, planetDetectionRadius);



        // HighLight current Planet
        if (currentPlanet != null)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawLine(transform.position, currentPlanet.position);
            Gizmos.DrawWireSphere(currentPlanet.position, 1f);
        }
    }
}

