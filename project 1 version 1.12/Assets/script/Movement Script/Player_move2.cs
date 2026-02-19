 using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
public class Player_move2 : MonoBehaviour
{
    Vector2 currentMovementInput;
    Vector3 currentMovement;
    Vector3 runmovement;
    Vector3 lookat;

    public float speed = 5.0f;
    public float rotate_speed = 3.0f;
    public float runspeed = 2.0f;
    public float default_gravity = -.05f;
    bool IsMovePressed;

    Char_control playerInput;
    CharacterController char_controller;
    Animator animator;
    int IsWalkingHash;
    int IsRunningHash;
    bool IsWalking;
    bool IsRunning;
    bool IsRunPressed;
    float gravity_y;

    // Start is called before the first frame update
    void Awake()
    {   //declaration:
        playerInput = new Char_control();
        animator = GetComponent<Animator>();
        char_controller = GetComponent<CharacterController>();

        IsRunningHash = Animator.StringToHash("IsRunning");
        IsWalkingHash = Animator.StringToHash("IsWalking");

        //movementinput oriented:
        playerInput.player_movement.Move.started += OnMovementInput;
        playerInput.player_movement.Move.performed+= OnMovementInput;
        playerInput.player_movement.Move.canceled += OnMovementInput;
        playerInput.player_movement.Run.started += OnRun;
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
        //so runmovement will be 0 if shift only pressed without(W,A,S,D)
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
        rotater();
    }

    void OnRun(InputAction.CallbackContext context)
    {
        IsRunPressed = context.ReadValueAsButton();
    }
    void animate()
    { 
        //declartion and initialize
        IsWalking = animator.GetBool(IsWalkingHash);
        IsRunning = animator.GetBool(IsRunningHash);
        //value updating
        if(IsMovePressed && !IsWalking)
        {
            animator.SetBool(IsWalkingHash, true);
        }
        else if(!IsMovePressed && IsWalking)
        {
            animator.SetBool(IsWalkingHash, false);
        }
        if(IsMovePressed && IsRunPressed && !IsRunning)
        {
            animator.SetBool(IsRunningHash, true);
        }
        else if(!IsRunPressed && IsRunning)
        {
            animator.SetBool(IsRunningHash, false);
        }

    }
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
    }
    void rotater()
    {
        
        if (IsMovePressed)
        {
            lookat = new Vector3(currentMovement.x, 0, currentMovement.z);
            Quaternion Trotation = transform.rotation;
            Quaternion Nrotation = Quaternion.LookRotation(lookat);
            transform.rotation = Quaternion.Slerp(Trotation, Nrotation, rotate_speed * Time.deltaTime);
        }
        // extra addition by me TO RESET TO ROATATION 0,0,0
      /*  else
        {
            Vector3 ZR = new Vector3(0, 0, 0);//here i craete a vector value of x=0,y=0,z=0
            Quaternion zrotation = Quaternion.LookRotation(ZR);//here i convert that vector3 value into quaternion 
            transform.rotation = Quaternion.Slerp(transform.rotation, zrotation, 5 * Time.deltaTime);//here i perform the roatation of the object when not moventpressd to 0,0,0
        }
        */
    }

    void OnMovementInput(InputAction.CallbackContext context)
    {
        currentMovementInput = context.ReadValue<Vector2>();
        currentMovement.x = currentMovementInput.x;
        currentMovement.z = currentMovementInput.y;
        runmovement.x = currentMovement.x * runspeed;
        runmovement.z = currentMovement.z * runspeed;
        IsMovePressed = currentMovement.x != 0 || currentMovement.z != 0;
    }
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

}
