using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraRotation : MonoBehaviour
{
    public float speed = 0f;

    void Update()
    {
        transform.Rotate(0, speed * Time.deltaTime,0);
    }
}
