using Cinemachine.Utility;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UIElements;
using static UnityEngine.UI.Image;

public class ogdummyscript_mario : MonoBehaviour
{
    //----------------------------MOVEMENT SETTINGS--------------------------------
    [SerializeField] private float Speed_value = 50f;
    [SerializeField] private float speed;
    [SerializeField] private float rotate_speed = 8.0f;
    [SerializeField] private float coyoteTime = 0.5f;
    [SerializeField] private float doubleJumpTime = 1f;
    [SerializeField] private float grav_value = 30f;
    private float coyoteTimeCounter;
    private float doubleJumpTimer;
    private bool canDoubleJump = false;
    // private int planetcount = 0;
    [SerializeField] private Vector3 last_position;
    [SerializeField] private Vector3 current_position;

    //----------------------------CAMERA VARIABLES--------------------------------
    [SerializeField] private control_camera camOrbit;

    //----------------------------CLASS INHERITANCE--------------------------------
    private Char_control playerInput;
    private CharacterController char_controller;

    //----------------------------MOVEMENT VARIABLES--------------------------------
    private Vector2 input;
    private Vector3 movement;

    //----------------------------COROUTINE VARIABLES--------------------------------
    private Coroutine RotateRoutine;

    //----------------------------FAUX GRAVITY SYSTEM VARIABLES--------------------------------
    [SerializeField] private Transform currentplanet;
    [SerializeField] private Transform previousPlanet;
    [SerializeField] private Transform checkCurrentPlanet;
    [SerializeField] private float gravityStrength;
    private Vector3 gravityDirection;
    private Vector3 velocity;
    [SerializeField] private float jumpForce = 10f;
    private Quaternion default_rotation;
    [SerializeField] private float momentum;
    [SerializeField] private float planetDetectionradius;
    [SerializeField] private Vector3 current_velocity;
    [SerializeField] private LayerMask planetMask;
    [SerializeField] private bool Ground_State;
    [SerializeField] private Vector3 origin;
    [SerializeField] private float radius = 0.5f;
    [SerializeField] private float rayDistance = 1.4f;
    [SerializeField] private Vector3 rayDirection;
    private Vector3 start_pos;
    private Vector3 limit_pos;
    private Vector3 pos;

    // Start is called before the first frame update
    //awake aclled before start 
    void Awake()
    {
        default_rotation = transform.rotation;

        //------------------------movement based declaration with lambda functioning ------------------------
        playerInput = new Char_control();
        char_controller = GetComponent<CharacterController>();
        char_controller.slopeLimit = 45f;
        char_controller.stepOffset = 0.3f;
        speed = Speed_value;
        gravityStrength = grav_value;
        start_pos = new Vector3(-1301f, -55f, -1083f);
        limit_pos = new Vector3(560f, 1351f, 1694.3f);
        //movementinput oriented:
        playerInput.player_movement.Move.started += (context) =>
        {
            input = context.ReadValue<Vector2>();
            movement = new Vector3(input.x, 0, input.y);
            //to move the gameobject along it's own axis not the world axis
            movement = transform.TransformDirection(movement);
            movement.y = Mathf.Clamp(movement.y, 0, 1000);
            movement = movement * speed;

        };
        playerInput.player_movement.Move.performed += (context) =>
        {
            input = context.ReadValue<Vector2>();
            movement = new Vector3(input.x, 0, input.y);
            movement = transform.TransformDirection(movement);
            movement.y = Mathf.Clamp(movement.y, 0, 1000);
            speed = Mathf.Clamp(speed, 0, 100);
            speed = speed + 0.5f;
            movement = movement * speed;

        };
        playerInput.player_movement.Move.canceled += (context) =>
        {
            input = Vector2.zero;
            movement = Vector3.zero;
            speed = Speed_value;

        };
        // JUMP INPUT
        playerInput.player_movement.JUMP.started += (context) =>
        {
            Jump();

        };
        // DROP DOWN INPUT 
        playerInput.player_movement.DOWN.started += OnDrop;
        playerInput.player_movement.DOWN.canceled += OnDrop;

    }

    void Start()
    {
        last_position = transform.position;
    }

    // Update is called once per frame
    void Update()
    {

        last_position = transform.position;
        pos = transform.position;
        pos.x = Mathf.Clamp(pos.x, start_pos.x, limit_pos.x);
        pos.y = Mathf.Clamp(pos.y, start_pos.y, limit_pos.y);
        pos.z = Mathf.Clamp(pos.z, start_pos.z, limit_pos.z);
        transform.position = pos;
        FindNearestPlanet();
        AlignToPlanet();
        Ground_State = IsGrounded();
        // AutoMove();
        //always perform these above functions  before moving to avoid jitter
        Vector3 totalMove = movement * Time.deltaTime + velocity * Time.deltaTime;
        char_controller.Move(totalMove);
        origin = transform.position;
        rayDirection = -gravityDirection;
        ApplyFauxGravity();

        current_velocity = (transform.position - last_position) / 2;
        momentum = movement.magnitude;
        current_position = transform.position;
        Debug.Log("momentum" + momentum);
        Debug.Log("current_velocity" + current_velocity);
        Debug.Log("Gravity_velocity" + velocity);
        Debug.Log("speed" + speed);
        Debug.Log("MOVEMENT" + movement);
        Debug.Log("IsGrounded" + Ground_State);
        Debug.Log("last_position" + last_position);
        Debug.Log("current_position" + current_position);

    }


    //---------------------------FIND NEAR BY PLANET---------------------------------

    /*
    private void FindNearestPlanet()
    {
        Collider[] hits = Physics.OverlapSphere(transform.position, planetDetectionradius, planetMask);

        if (hits.Length == 0)
        {
            if(checkCurrentPlanet != null)
            {
                previousPlanet = checkCurrentPlanet;
            }
            // (Addition: set currentplanet top null only when the earth gravity is on)
            currentplanet = null;
            // return;
        }

        float closestDist = Mathf.Infinity;
        Transform nearest = null;

        foreach (Collider col in hits)
        {
            float dist = Vector3.Distance(transform.position, col.transform.position);
            if (dist < closestDist)
            {
                closestDist = dist;
                nearest = col.transform;
            }
            
        }

        if(checkCurrentPlanet != null && checkCurrentPlanet != nearest )
        {
            previousPlanet = checkCurrentPlanet;
        }

        if(currentplanet == null && previousPlanet != null)
        {
            nearest = previousPlanet;
        }

        currentplanet = nearest;
        // a variable that  holds the currentplanet ,because currentplanet becomes null when leaving planet
        if( currentplanet != null )
        {
            checkCurrentPlanet = currentplanet;
        }

    }
    */

    //way 2:
    // HERE IT'S SIMPLE AS WELL AS EFFICIENT :
    private void FindNearestPlanet()
    {
        Collider[] hits = Physics.OverlapSphere(transform.position, planetDetectionradius, planetMask);

        if (hits.Length == 0 && transform.parent == null)
        {
            // (Addition: set currentplanet top null only when the earth gravity INPUT  is on)
            currentplanet = null;
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
                nearest = col.transform;
            }

        }
        currentplanet = nearest;

    }



    //----------------------------FAUX GRAVITY SYSTEM--------------------------------

    private void ApplyFauxGravity()
    {
        if (currentplanet != null)
        {
            gravityDirection = (currentplanet.position - transform.position).normalized;
        }
        else
        {
            gravityDirection = Vector3.down * 1.5f; // fallback gravity 
        }

        // if(Ground_State)
        if (char_controller.isGrounded || Ground_State)
        {
            //small inward push to keep the player grounded

            // way 1:
            velocity = -gravityDirection * 0.5f;


            /*
            // way 2:(Works)
            // Dot product gives whether the given value are in same direction 
            // it returns for same direction >0
            // for opposite direction < 0
            // if they are perpendicular =0
            if(Vector3.Dot(velocity,gravityDirection) > 0)
            {
                velocity = -gravityDirection * 0.05f;
            }
            */


        }
        else if (!Ground_State || !char_controller.isGrounded)
        //else if(!Ground_state)
        {
            velocity += gravityDirection * gravityStrength * Time.deltaTime;
        }
        //To Set Jump Timers
        if (!char_controller.isGrounded)
        {
            coyoteTimeCounter -= Time.deltaTime;
            doubleJumpTimer += Time.deltaTime;

        }
        else
        {
            coyoteTimeCounter = coyoteTime;
            doubleJumpTimer = doubleJumpTime;
            canDoubleJump = false;
        }

        //
        // char_controller.Move(velocity * Time.deltaTime);
    }

    //----------------------------IsGrounded---------------------------------------------

    private bool IsGrounded()
    {

        if (currentplanet == null)
        {
            return char_controller.isGrounded;
        }


        // cast towards the planet surface only 
        return Physics.SphereCast(origin, radius, -gravityDirection, out RaycastHit hit, rayDistance, planetMask, QueryTriggerInteraction.Ignore);


    }
    private void OnDrawGizmos()
    {


        if (Physics.SphereCast(origin, radius, -gravityDirection, out RaycastHit hit, rayDistance, planetMask, QueryTriggerInteraction.Ignore))
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(hit.point, radius);
        }
        // Here we add the playerposition with gravityDirection and multiply it by the rayDistance for the point needed to hit 
        Gizmos.color = Color.yellow;
        Gizmos.DrawLine(transform.position + rayDirection, transform.position + rayDirection * rayDistance);
        Gizmos.DrawWireSphere(transform.position, radius);
        Gizmos.DrawWireSphere(transform.position + rayDirection * rayDistance, radius);
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(transform.position, planetDetectionradius);

    }


    //----------------------------AUTO MOVE PREVENTION ----------------------------------

    private void AutoMove()
    {


        if (char_controller.isGrounded && movement == Vector3.zero)
        {
            velocity = Vector3.zero;
        }

    }

    //----------------------------JUMP SYSTEM--------------------------------------------

    private void Jump()
    {

        // if(Ground_State || coyoteTimeCounter > 0)
        if (char_controller.isGrounded || coyoteTimeCounter > 0f)
        {

            velocity = -gravityDirection * jumpForce;
            canDoubleJump = true;
        }
        else if (canDoubleJump && doubleJumpTimer >= doubleJumpTime) // can perform double jump before getting grounded
        {

            velocity = -gravityDirection * jumpForce * 1.5f;
            doubleJumpTimer = 0f;
        }

        /*
        else if (canDoubleJump) // can perform double jump once  before getting grounded
        {

            velocity = -gravityDirection * jumpForce;
            canDoubleJump = false;

        }
        
        else if (canDoubleJump && doubleJumpTimer > 0f) // can perform double jump within  n second after jump before getting grounded 
        {

            velocity = -gravityDirection * jumpForce;
            canDoubleJump = false;

        }
        */



    }

    //-----------------------------DROP DROWN----------------------------------------------------
    private void OnDrop(InputAction.CallbackContext context)
    {
        if (context.started)
        {
            gravityStrength = 100f;
        }
        if (context.canceled)
        {
            gravityStrength = grav_value;
        }

    }


    //----------------------------ROTATION/ALIGN TO PLANET SYSTEM--------------------------------
    private void AlignToPlanet()
    {
        if (currentplanet == null)
        {
            //return to default_rotation
            transform.rotation = Quaternion.Slerp(transform.rotation, default_rotation, rotate_speed * Time.deltaTime);
            return;
        }
        Quaternion targetRotation = Quaternion.FromToRotation(transform.up, -gravityDirection) * transform.rotation;
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotate_speed * Time.deltaTime);
    }

    //----------------------------Gravity Zone With Trigger --------------------------------

    /*
    private void OnTriggerEnter(Collider other)
    {
        if(other.CompareTag("Planet"))
        {
            planetcount++;
            currentplanet = other.transform;
        }
    }

    
    
    private void OnTriggerExit(Collider other)
    {
        if(other.CompareTag("Planet"))
        {
            planetcount--;
            if(planetcount <= 0)
            {
                currentplanet = null;//no planet in range and normal gravity 
            }
        }
    }
    */

    //---------------------------- ROTATE PLAYER BASED ON CAMERA  --------------------------------

    /*
    private void rotate_cam()
    {
        if (currentplanet == null)
        {
            if (!CompareTag("Player"))
            {
                return;
            }
            float yaw = camOrbit.GetYaw();
            Quaternion targetRot = Quaternion.Euler(0, yaw, 0);

            transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, rotate_speed * Time.deltaTime);
        }

    }
    */
    //-----------------------------COROUTINE FUNCTION--------------------------------

    /*
    private IEnumerator RotateLoop()
    {
        yield return new WaitForSeconds(1f);

        while (true)
        {
            rotate_cam();
            yield return new WaitForSeconds(1f);
        }
    }
    */
    //----------------------------ENABLE DISABLE inputAction--------------------------------
    private void OnEnable()
    {
        playerInput.player_movement.Enable();
        //-----------------------------START COROUTINE ROBOST  WAY--------------------------------

        // RotateRoutine = StartCoroutine(RotateLoop());

        //-----------------------------START COROUTINE SIMPLE WAY--------------------------------
        //InvokeRepeating(nameof(rotate_cam), 0.3f, 0.3f);
    }

    private void OnDisable()
    {
        playerInput.player_movement.Disable();

        //-----------------------------STOP COROUTINE ROBOST  WAY--------------------------------
        /*
        if (RotateRoutine != null)
        {
            StopCoroutine(RotateLoop());
        }
        */
        //-----------------------------STOP COROUTINE SIMPLE WAY--------------------------------
        //  CancelInvoke(nameof(rotate_cam));
    }

}
