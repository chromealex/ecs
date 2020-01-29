using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class cameraMove : MonoBehaviour {

    private float rSpeed = 3.0f;
    private float mSpeed = 20.0f;
    private float X = 0.0f;
    private float Y = 0.0f;

    void Update()
    {

        X += Input.GetAxis("Mouse X") * rSpeed;
        Y += Input.GetAxis("Mouse Y") * rSpeed;
        transform.localRotation = Quaternion.AngleAxis(X, Vector3.up);
        transform.localRotation *= Quaternion.AngleAxis(Y, Vector3.left);
        transform.position += transform.forward * mSpeed * Input.GetAxis("Vertical") * Time.deltaTime;
        transform.position += transform.right * mSpeed * Input.GetAxis("Horizontal") * Time.deltaTime;
    }
}
