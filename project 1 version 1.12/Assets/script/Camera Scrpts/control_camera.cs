using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class control_camera : MonoBehaviour
{
    [SerializeField] private Transform target;
    [SerializeField] private float distance = 8f;
    [SerializeField] private float minDistance = -50f;
    [SerializeField] private float maxDistance = 50f;

    [SerializeField] private float sensitivityX = 120f;
    [SerializeField] private float sensitivityY = 120f;

    [SerializeField] private float minY = -30f;
    [SerializeField] private float minX = 60f;

    [SerializeField] private float yaw;
    [SerializeField] private float pitch;
    [SerializeField] private Vector2 mouse_move;

    [SerializeField] private float default_x = 0f;
    [SerializeField] private float default_y = 0f;
    

    private Char_control input;
    Vector2 scroll;


    private void Awake()
    {
        //
        // transform.position = new Vector3(target.position.x + default_x, target.position.y + default_y, target.position.z + distance);
        input = new Char_control();
        input.player_movement.SCROLL.started += ctx => scroll = ctx.ReadValue<Vector2>();
        input.player_movement.SCROLL.performed += ctx => scroll = ctx.ReadValue<Vector2>();
        input.player_movement.SCROLL.canceled += ctx => scroll = ctx.ReadValue<Vector2>();
      
    }

    private void OnDisable()
    {
        input.player_movement.Disable();
    }

    private void OnEnable()
    {
        input.player_movement.Enable();
    }

    private void LateUpdate()
    {
        HandleLook();
        HandleZoom();
        UpdateCameraPosition();
    }

    //------------------------------------------------------GET THE MOUSE INPUTS AND OBTAIN THE X AND Y FOR ROTATION------------------------------------------------------

    private void HandleLook()
    {
        
        mouse_move = input.player_movement.camera.ReadValue<Vector2>();
        //YAW = X AXIS (LEFT,RIGHT)
        if (mouse_move == null)
        {
            mouse_move = Vector2.zero;

        }
        //if the mouse is not moving the move value is set to 0;
        yaw += mouse_move.x * sensitivityX * Time.deltaTime;
        //PITCH = Y AXIS(UP/DOWN)
        pitch -= mouse_move.y * sensitivityY * Time.deltaTime;
        pitch = Mathf.Clamp(pitch, minY, minX);
        
        

    }
    //-------------------------------------------------------GET THE SCROLL INPUT AND SET THE ZOOM LIMITS-------------------------------------------------------
    private void HandleZoom()
    {
        //------------------------ONLY INSIDE A FUNCTION NOT OUTSIDE IT------------------------
        // Vector2 Scroll = input.player_movement.SCROLL.ReadValue<Vector2>();
        float ZoomAmount = scroll.y;
        distance -= ZoomAmount * 2f;
        distance = Mathf.Clamp(distance, minDistance, maxDistance);
    }

    //-------------------------------------------------------MOVE AND ROTATE  THE CAMERA POSITION BASED ON THE CALCULATED VALUES-------------------------------------------------------
    private void UpdateCameraPosition()
    { //---------------------------------------------------WAY ONE -------------------------------------------------------------------------

        /* 
         GETTING X AND Z VALUE USING ROATATION FORMULA:
           // X = X.COS(ANGLE) + Z.SIN(ANGLE)
           // Z = -X.SIN(ANGLE) + Z.COS(ANGLE)
         EXAMPLE:
           // X = 0, Z = -5 ANGLE = 90 DEDGREE
           // X = 0.COS(90) + -5.SIN(90) = 0+-5 = -5
           // Z = -0.SIN(90) + -5.COS(90) = 0+0 = 0;
          THEN THE PLAYER POSITION IS ADDED TO ORBIT AROUND THE PLAYER
           // OFFSET = (0,0,-5)
           // PLAYER POSITION = (10,15,20)
           // OFFSET + PLAYER POSITION = (10,15,15)
        */
        
        Quaternion rotation = Quaternion.Euler(pitch, yaw, 0);
        //
        Vector3 offset = rotation * new Vector3(default_x, default_y, -distance);
        Vector3 move_cam = target.position + offset;
        transform.position = Vector3.Lerp(transform.position, move_cam, 1f);
        transform.rotation = Quaternion.Slerp(transform.rotation, rotation, 1f);

        
        //---------------------------------------------------WAY TWO -------------------------------------------------------------------------

        /* 
            GETTING X AND Z VALUE USING ROATATION FORMULA:
                // X = X.COS(ANGLE) + Z.SIN(ANGLE)
                // Z = -X.SIN(ANGLE) + Z.COS(ANGLE)
            EXAMPLE: X = 0, Z = -5 ANGLE = 90 DEDGREE
               // X = 0.COS(90) + -5.SIN(90) = 0+-5 = -5
               // Z = -0.SIN(90) + -5.COS(90) = 0+0 = 0;
            THEN THE PLAYER POSITION IS ADDED TO ORBIT AROUND THE PLAYER
               // OFFSET = (0,0,-5)
               // PLAYER POSITION = (10,15,20)
               // OFFSET + PLAYER POSITION = (10,15,15)
        */

        /*
        Quaternion rotation = Quaternion.Euler(pitch, yaw, 0);
        Vector3 OFFSET = rotation * new Vector3(0, 0, -distance);
        // ALWAYS FIRST SET THE POSITION THEN ROTATION TO AVOID JITTERING
        transform.position = target.position + OFFSET;
        // LOOK AT TARGET IS FOR ROATAION TOWARDS THE TARGET
        transform.LookAt(target);
        */




    }

    public float GetYaw()
    {
       return yaw;
    }
}
