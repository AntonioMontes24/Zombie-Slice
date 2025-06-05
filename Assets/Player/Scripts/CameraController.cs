using UnityEngine;

public class CameraController : MonoBehaviour
{
    public float sens = 100f;
    public float lockVertMin = -90f;
    public float lockVertMax = 90f;
    public bool invertY = false;
    public Transform playerBody;

    //rotate on X axis looks up and down on Y axis, weird thing but REMEMBER THIS!!!
    float rotX = 0f;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
    }

    // Update is called once per frame
    void Update()
    {
        // get input
        float mouseX = Input.GetAxis("Mouse X") * sens * Time.deltaTime;
        float mouseY = Input.GetAxis("Mouse Y") * sens * Time.deltaTime;

        // give option to invert mouse look up and down
        if (invertY)
            rotX += mouseY;
        else
            rotX -= mouseY;
        //clamp camera on the x-axis 
        rotX = Mathf.Clamp(rotX, lockVertMin, lockVertMax);

        //rotate camera on x-axis to look up and down
        transform.localRotation = Quaternion.Euler(rotX, 0, 0);

        // rotate player on y-axis to look left and right
        playerBody.Rotate(Vector3.up * mouseX);
    }
}