using System;
using Photon.Pun;
using UnityEngine;

public class ShipMovement : MonoBehaviourPun
{
    public float baseSpeed;
    public float maxPositiveSpeed;
    public float maxNegativeSpeed;
    public float turnSpeed;
    public float currentSpeed;

    public Camera viewCamera;

    public bool invertCamera;

    public float mouseSensitivity;
    public float maxLookAngle;
    
    private Rigidbody rb;
    
    private float horizontalInput;
    private float forwardInput;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        if (!photonView.IsMine)
        {
            GetComponentInChildren<Camera>().gameObject.SetActive(false);
            GetComponentInChildren<AudioListener>().gameObject.SetActive(false);
        }
        else
        {
            Cursor.visible = false;
            Cursor.lockState = CursorLockMode.Locked;
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    private float yaw, pitch;
    private void Update()
    {
        yaw = transform.localEulerAngles.y + Input.GetAxis("Mouse X") * mouseSensitivity;

        if (!invertCamera)
        {
            pitch -= mouseSensitivity * Input.GetAxis("Mouse Y");
        }
        else
        {
            // Inverted Y
            pitch += mouseSensitivity * Input.GetAxis("Mouse Y");
        }

        // Clamp pitch between lookAngle
        pitch = Mathf.Clamp(pitch, -maxLookAngle, maxLookAngle);

        transform.localEulerAngles = new Vector3(pitch, yaw, 0);
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        if (!photonView.IsMine) return;
        
        horizontalInput = Input.GetAxis("Horizontal");
        forwardInput = Input.GetAxis("Vertical");

        float previousCurrentSpeed = currentSpeed;
        
        currentSpeed += forwardInput * baseSpeed;

        if (currentSpeed > maxPositiveSpeed)
        {
            currentSpeed = previousCurrentSpeed;
        } else if (currentSpeed < -maxNegativeSpeed)
        {
            currentSpeed = previousCurrentSpeed;
        }

        Vector3 velocity = transform.forward * currentSpeed * Time.fixedDeltaTime;
        velocity.y = rb.velocity.y;
        rb.velocity = velocity;
        transform.Rotate(transform.up * horizontalInput * turnSpeed * Time.fixedDeltaTime);
    }
}