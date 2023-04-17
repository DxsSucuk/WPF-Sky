using Photon.Pun;
using Unity.VisualScripting;
using UnityEngine;

public class ShipMovement : MonoBehaviourPun
{
    public float baseSpeed;
    public float maxPositiveSpeed;
    public float maxNegativeSpeed;
    public float turnSpeed;
    public float currentSpeed;

    public Transform firePoint;
    public GameObject LaserPrefab;
    public GameObject LaserInstance;
    public EGA_Laser LaserScript;
    
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
            Camera cameraObject = GetComponentInChildren<Camera>();
            AudioListener audioListenerObject = GetComponentInChildren<AudioListener>();
            
            if (audioListenerObject is not null) audioListenerObject.gameObject.SetActive(false);
            
            if (cameraObject is not null) cameraObject.gameObject.SetActive(false);
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
        if (!photonView.IsMine) return;
        
        handleWeapon();
        
        yaw = transform.eulerAngles.y + Input.GetAxis("Mouse X") * mouseSensitivity;

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

        transform.eulerAngles = new Vector3(pitch, yaw, 0);
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        if (!photonView.IsMine) return;
        
        handleMovement();
    }

    private void handleMovement()
    {
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
        rb.velocity = velocity;
        //// transform.Rotate(transform.up * horizontalInput * turnSpeed * Time.fixedDeltaTime);
    }
    
    private void handleWeapon()
    {
        if (Input.GetMouseButtonDown(0))
        {
            if (LaserInstance is not null && !LaserInstance.IsUnityNull())
                PhotonNetwork.Destroy(LaserInstance);
            
            LaserInstance = PhotonNetwork.Instantiate("Prefab/Laser/" + LaserPrefab.name, firePoint.position, firePoint.rotation);
            LaserInstance.transform.parent = transform;
            LaserScript = LaserInstance.GetComponent<EGA_Laser>();
        }
        
        if (Input.GetMouseButtonUp(0))
        {
            LaserScript.DisablePrepare();
            PhotonNetwork.Destroy(LaserInstance);
        }
    }
}