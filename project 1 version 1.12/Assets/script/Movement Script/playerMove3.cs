 using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
public class Player_move_3: MonoBehaviour
{
    Vector2 currentMovementInput;
    Vector3 currentMovement;
    Vector3 runmovement;
    Vector3 lookat;

    [Header("movement settings")]
    [SerializeField] private float speed = 5.0f;
    [SerializeField] private float rotate_speed = 3.0f;
    [SerializeField] private float runspeed = 2.0f;
    [SerializeField] public float default_gravity = -.05f;
    [SerializeField] private float gravity_y;
    
    [Header("class inheritance")]
    Char_control playerInput;
    CharacterController char_controller;
    Animator animator;

    [Header("Animation Hashes")]
    private int IsWalkingHash;
    private int IsRunningHash;
    private int IsSprintHash;
    // below count is used in the OnRun() 
    // private int count = 0;

    [Header("state Bools")]
    private bool IsWalking;
    private bool IsRunning;
    private bool IsSprint;
    // private bool sprintcheck;
    private bool runcheck;
    private bool IsRunPressed = false;
    private bool IsMovePressed;
    private bool IsSprintPressed;


    // Start is called before the first frame update
    void Awake()
    {   //declaration:
        playerInput = new Char_control();
        animator = GetComponent<Animator>();
        char_controller = GetComponent<CharacterController>();

        IsRunningHash = Animator.StringToHash("IsRunning");
        IsWalkingHash = Animator.StringToHash("IsWalking");
        IsSprintHash = Animator.StringToHash("IsSprint");

        //movementinput oriented:
        playerInput.player_movement.Move.started += OnMovementInput;
        playerInput.player_movement.Move.performed+= OnMovementInput; 
        playerInput.player_movement.Move.canceled += OnMovementInput;
        playerInput.player_movement.Run.started += OnRun;
        //here the below program to check the not press of run is not used but it just exists 
        playerInput.player_movement.Run.canceled += OnRun;

    }

    // Update is called once per frame
    void Update()
    {  
        if(IsRunPressed)
        {
            char_controller.Move(runmovement * speed * Time.deltaTime);
        }       
        else
        {
            char_controller.Move(currentMovement * speed  * Time.deltaTime);
        }

        //here it is optional to check both (IsMovePressed && IsRunPressed) the runmovement value can be get only through the OnMovementInput()
        //so runmovement will be 0 if only the shift  pressed without(W,A,S,D)
        /* if (IsMovePressed && IsRunPressed) 
         {
             char_controller.Move(runmovement * speed * Time.deltaTime);
         }
         else
         {
             char_controller.Move(currentMovement * speed * Time.deltaTime);
         }
        */
        gravity_control();
        animate();
        rotated();
    }

    //---------------------MOVEMENT INPUT FUNCTION-----------------------

    void OnMovementInput(InputAction.CallbackContext context)
    {
        currentMovementInput = context.ReadValue<Vector2>();
        currentMovement.x = currentMovementInput.x;
        currentMovement.z = currentMovementInput.y;
        runmovement.x = currentMovement.x * runspeed;
        runmovement.z = currentMovement.z * runspeed;
        IsMovePressed = currentMovement.x != 0 || currentMovement.z != 0;
        if (!IsMovePressed)
        {
            IsRunPressed = false;
            
        }
       
    }



    //---------------------RUN INPUT FUNCTION-----------------------

    void OnRun(InputAction.CallbackContext context)
    {
        //------------------------------way one--------------------------------
        //way one: simple way using toggle  from inputAction
        runcheck = context.ReadValueAsButton();
        if (context.started && IsMovePressed)
        {
           
                IsRunPressed = !IsRunPressed;
           
        }
       


        //------------------------------way two--------------------------------
        //way two :quick toggle method for run on and off on each press
        /*
        if(context.started && IsMovePressed)
          {
              IsRunPressed = !IsRunPressed;
          }
        */

        //------------------------------way three--------------------------------
        // way three : used if a button is to be toggled on and off on each press
        /*
          runcheck = context.ReadValueAsButton();
          if (runcheck && IsMovePressed)
          {
              if (count == 0)
              {
                  IsRunPressed = true;
                  count++;
              }
              else if (count == 1)
              {
                  IsRunPressed = false;
                  count = 0;
              }

          }
        */

    }

    // ---------------------ANIMATION CONTROL FUNCTION-----------------------
    void animate()
    { 
        //declartion and initialize
        IsWalking = animator.GetBool(IsWalkingHash);
        IsRunning = animator.GetBool(IsRunningHash);
        
        //value updating

        //FOR WALK ANIMATION
        if (IsMovePressed && !IsWalking)
        {
            animator.SetBool(IsWalkingHash, true);
        }
        else if(!IsMovePressed && IsWalking)
        {
            animator.SetBool(IsWalkingHash, false);

        }
        //FOR RUN ANIMATION
        if (IsMovePressed && IsRunPressed && !IsRunning)
        {
            {
                animator.SetBool(IsRunningHash, true);
            }
       
        }
        //EXTRA ADDITION TO STOP RUNNING WHEN SHIFT IS PRESSED BUT MOVEMENT KEY IS NOT PRESSED
        
        if (IsRunPressed && IsRunning)
        {
            if(!IsMovePressed)
            {
                animator.SetBool(IsRunningHash, false);
            }
           
        }
        else if (!IsRunPressed && IsRunning)
        {
            animator.SetBool(IsRunningHash, false);

        }
        
    }

    //---------------------GRAVITY CONTROL FUNCTION-----------------------
    void gravity_control()
    {
        //in character controller the gravity is not applied automatically we have to apply it manually
        //even though it has isGrounded a downward force is applied to avoid floating,jittering
        // so we need to provide the small amount of downward force to keep the player grounded
        if ( char_controller.isGrounded)
        {
            gravity_y = default_gravity;
            currentMovement.y = gravity_y;
        }
        else
        {
            gravity_y = -9.81f;
            currentMovement.y += gravity_y;
        }
        //EXTRA ADDITION TO MAINTAIN THE RUN GRAVITY WHILE RUNNING ELSE THE CHARACTER WILL FLOAT UNTIL "SHIFT" IS RELEASED
        if (IsRunPressed)
        {
            runmovement.y = currentMovement.y;
        }
    }

    //----------------------ROTATION FUNCTION-----------------------------
    void rotated()
    {
        
        if (IsMovePressed)
        {
            // normalize prevents maginutes issues when turning like some janks if the input to lookat is (1,0,1) then it's magnitude is more than 1 that is sqrt(1^2+0^2+1^2) is sqrt(2) that is around  
            lookat = new Vector3(currentMovement.x, 0, currentMovement.z).normalized;
            Quaternion Trotation = transform.rotation;
            Quaternion Nrotation = Quaternion.LookRotation(lookat);
            transform.rotation = Quaternion.Slerp(Trotation,Nrotation, rotate_speed * Time.deltaTime);
        }
        /*
          else
        {
            Vector3 ZR = new Vector3(0, 0, 0);//here i craete a vector value of x=0,y=0,z=0
            Quaternion zrotation = Quaternion.LookRotation(ZR);//here i convert that vector3 value into quaternion 
            transform.rotation = Quaternion.Slerp(transform.rotation, zrotation, 5 * Time.deltaTime);//here i perform the roatation of the object when not moventpressd to 0,0,0
        }
        */
        
    }

   
    //---------------------ENABLE AND DISABLE INPUT_ACTIONFUNCTIONS-----------------------
    void OnEnable()
    {
        //IT SHOULD BE ENABLE FOR USAGE(TO GET INPUT)
        playerInput.player_movement.Enable();
    }

    void OnDisable()
    {
        //IT SHOULD BE DIABELD AFTER USAGE(TO AVOID GETTING INPUTS EVEN AFTER CLSOING THE SCRIPT)
        playerInput.player_movement.Disable();
    }

    //-----------------------------------GIZMO FUNCTION-----------------------
    //GIZMO TO SHOW THE LOOKAT DIRECTION
    //SYSTEM FUNCTION TO DRAW GIZMO
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawRay(transform.position,lookat);
    }
}
