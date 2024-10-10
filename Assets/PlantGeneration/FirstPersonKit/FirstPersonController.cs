using UnityEngine;
using System.Collections;

[RequireComponent (typeof(CharacterController))]
public class FirstPersonController : MonoBehaviour {

    [Tooltip("Move speed of the character in m/s")]
    public float movementSpeed = 5.0f;

    [Tooltip("Sprint speed")]
    public float sprintSpeed = 10.0f;

    [Tooltip("Look/rotation sensitivity via mouse")]
    public float lookSensitivity = 5.0f;

    [Tooltip("The height the player can jump")]
    public float jumpHeight = 1.2f;
    [Tooltip("The character uses its own gravity value. The engine default is -9.81f")]
    public float gravity = -15.0f;

    float verticalRotation = 0;
	public float upDownRange = 60.0f;
	
	float verticalVelocity = 0;

    public string jumpInput = "Jump";
    public string sprintInput = "Fire3";


    CharacterController characterController;
    
	// Use this for initialization
	void Start () {
		Cursor.lockState = CursorLockMode.Locked;
		characterController = GetComponent<CharacterController>();

	}
	
	// Update is called once per frame
	void Update () {

        // Rotation
        float rotLeftRight = Input.GetAxis("Mouse X") * lookSensitivity;
        verticalRotation -= Input.GetAxis("Mouse Y") * lookSensitivity;

        /*
        Right stick controller support. 
        1- Go to project settings > Input manager
        2- duplicate the Horizontal input by right clicking > duplicate
        3- set it to 4th axis 
        4- Rename it HorizontalLook  
        5- Do the same with VerticalLook on the 5th axis
         */

        //rotLeftRight = Input.GetAxis("HorizontalLook") * lookSensitivity/10;
        //verticalRotation = Map(Input.GetAxis("VerticalLook"), -1, 1, -upDownRange, upDownRange);
        
        transform.Rotate(0, rotLeftRight, 0);

        verticalRotation = Mathf.Clamp(verticalRotation, -upDownRange, upDownRange);
		Camera.main.transform.localRotation = Quaternion.Euler(verticalRotation, 0, 0);

        float targetSpeed = movementSpeed;

        if (Input.GetButton(sprintInput))
        {
            targetSpeed = sprintSpeed;
        }
		// Movement
		
		float forwardSpeed = Input.GetAxis("Vertical") * targetSpeed;
		float sideSpeed = Input.GetAxis("Horizontal") * targetSpeed;
        

        verticalVelocity += gravity * Time.deltaTime;
		
		if( characterController.isGrounded && Input.GetButton(jumpInput) ) {
			verticalVelocity = Mathf.Sqrt(jumpHeight * -2f * gravity); ;
		}
		
		Vector3 speed = new Vector3( sideSpeed, verticalVelocity, forwardSpeed );
		
		speed = transform.rotation * speed;
		
		
		characterController.Move( speed * Time.deltaTime );

        

    }
    
    public void FreezeControllerFor(float time)
    {
        FreezeController();
        Invoke("UnfreezeController", time);
    }

    public void FreezeController()
    {
        characterController.enabled = false;
    }

    public void UnfreezeController()
    {
        characterController.enabled = true;
    }
    

    private float Map(float OldValue, float OldMin, float OldMax, float NewMin, float NewMax)
    {

        float OldRange = (OldMax - OldMin);
        float NewRange = (NewMax - NewMin);
        float NewValue = (((OldValue - OldMin) * NewRange) / OldRange) + NewMin;

        return (NewValue);
    }

}
