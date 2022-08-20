using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    public float moveSpeed = 0.5f;
    public float boostMult = 2;

    public float speedH = 2;
    public float speedV = 2;

    private float yaw = 0;
    private float pitch = 0;

    private bool active = true;

    ScreenShot sc;

    // Start is called before the first frame update
    void Start()
    {
        sc = GetComponent<ScreenShot>();

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

    }

    // Update is called once per frame
    void Update()
    {
        if (active) {
            float h = Input.GetAxisRaw("Horizontal");
            float f = Input.GetAxisRaw("Vertical");
            float v;

            float trueSpeed = moveSpeed;
            if (Input.GetKey(KeyCode.LeftShift)) trueSpeed *= boostMult;

            if(Input.GetKey(KeyCode.Space)) {
                v = 1;
            }
            else if (Input.GetKey(KeyCode.LeftControl)) {
                v = -1;
            }
            else v = 0;

            yaw += speedH * Input.GetAxis("Mouse X");
            pitch += speedV * (-Input.GetAxis("Mouse Y"));

            if (pitch < -90) pitch = -90;
            if (pitch > 90) pitch = 90;

            transform.eulerAngles = new Vector3(pitch, yaw, 0);
            transform.position += Vector3.Normalize(transform.TransformDirection(Vector3.forward)*f + 
                                  transform.TransformDirection(Vector3.right)*h +
                                  transform.TransformDirection(Vector3.up)*v) * trueSpeed * Time.deltaTime;

            if (Input.GetKeyDown(KeyCode.F2)) {
                sc.takeScreenshot();
                Debug.Log("Took Screenshot");
            }
        }
        
        if(Input.GetKeyDown(KeyCode.Escape)) {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
            active = false;
        }

        if (Input.GetMouseButtonDown(0)) {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
            active = true;
        }
    }
}
