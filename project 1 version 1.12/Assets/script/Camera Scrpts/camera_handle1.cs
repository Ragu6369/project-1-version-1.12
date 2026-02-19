using UnityEngine;

public class CameraArmController : MonoBehaviour
{
    public float verticalClamp = 30f;
    public Vector2 sensitivity = Vector2.one;
    private Char_control playerInput;
    private CharacterController char_controller;
    private float yaw;
    private float pitch;
    private void Awake()
    {
        playerInput = new Char_control();
        char_controller = GetComponent<CharacterController>();
    }

    private void OnEnable()
    {
        playerInput.Enable();
    }

    private void OnDisable()
    {
        playerInput.Disable();
    }

    private void LateUpdate()
    {
        AdjustCamera();
    }

    
   void AdjustCamera()
   {
     Vector2 input = playerInput.player_movement.camera.ReadValue<Vector2>(); 
     input *= sensitivity; yaw += input.x; pitch -= input.y; 
     pitch = Mathf.Clamp(pitch, -verticalClamp, verticalClamp);
     transform.localRotation = Quaternion.Euler(pitch, yaw, 0); 
   } 
   public float GetYaw() => yaw;
    
    // WAY Two : have some bugs in orbiting 
    /*
    void AdjustCamera()
    {
        // Read mouse look input, not scroll
        Vector2 input = playerInput.player_movement.camera.ReadValue<Vector2>();
        input *= sensitivity;

        transform.localRotation = Quaternion.Euler(
            new Vector3(input.y, input.x * -1f, 0) + transform.localRotation.eulerAngles);

        float clamped_x = 0;

        if (transform.localRotation.eulerAngles.x < 180)
            clamped_x = Mathf.Clamp(transform.localRotation.eulerAngles.x, -verticalClamp, verticalClamp);
        else
            clamped_x = Mathf.Clamp(transform.localRotation.eulerAngles.x, 360f - verticalClamp, 360f + verticalClamp);

        transform.localRotation = Quaternion.Euler(
            new Vector3(
                clamped_x,
                transform.localRotation.eulerAngles.y,
                0));
    }
    */
    
}
