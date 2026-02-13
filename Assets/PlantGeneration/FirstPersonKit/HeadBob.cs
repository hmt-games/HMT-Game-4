using UnityEngine;

public class HeadBob : MonoBehaviour
{
    public Camera playerCamera;
    public float walkingBobbingSpeed = 14f;
    public float bobbingAmount = 0.05f;
    public CharacterController controller;

    float defaultPosY = 0;
    float timer = 0;

    // Start is called before the first frame update
    void Start()
    {
        if (playerCamera == null)
            playerCamera = Camera.main;
        if (controller == null)
            controller = gameObject.GetComponent<CharacterController>();

        defaultPosY = transform.localPosition.y;
    }

    // Update is called once per frame
    void Update()
    {
        if (controller != null)
        {
            if (Mathf.Abs(controller.velocity.x) > 0.1f || Mathf.Abs(controller.velocity.z) > 0.1f)
            {
                //Player is moving
                timer += Time.deltaTime * walkingBobbingSpeed;
                playerCamera.transform.localPosition = new Vector3(playerCamera.transform.localPosition.x, defaultPosY + Mathf.Sin(timer) * bobbingAmount, playerCamera.transform.localPosition.z);
            }
            else
            {
                //Idle
                timer = 0;
                playerCamera.transform.localPosition = new Vector3(playerCamera.transform.localPosition.x, Mathf.Lerp(playerCamera.transform.localPosition.y, defaultPosY, Time.deltaTime * walkingBobbingSpeed), playerCamera.transform.localPosition.z);
            }
        }
    }
}
