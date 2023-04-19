using Photon.Pun;
using Unity.VisualScripting;
using UnityEngine;

public class ShipMovement : MonoBehaviourPun
{
    public float baseSpeed;
    public float acceleration;
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
    private float defaultFov;
    
    private float horizontalInput;
    private float forwardInput;

    public bool canShoot = true, canMove = true;

    private void Awake()
    {
        defaultFov = Camera.main.fieldOfView;
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
    
    private float yaw, pitch;
    private void Update()
    {
        if (!photonView.IsMine) return;

        handleWeapon();
        handleCamera();
        handleCameraZoom();
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        if (!photonView.IsMine) return;
        
        handleMovement();
    }

    private void handleCamera()
    {
        if (!canMove) return;
        
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

        transform.localEulerAngles = new Vector3(pitch, yaw, 0);
    }

    private void handleCameraZoom()
    {
        if (!canMove) return;
        
        var localVel = transform.InverseTransformDirection(rb.velocity);

        if (localVel.z > 0)
        {
            Camera.main.fieldOfView = defaultFov + 20;
        }
        else
        {
            Camera.main.fieldOfView = defaultFov;
        }
    }
    
    private void handleMovement()
    {
        if (!canMove) return;
        
        horizontalInput = Input.GetAxis("Horizontal");
        forwardInput = Input.GetAxis("Vertical");

        float previousCurrentSpeed = currentSpeed;
        
        currentSpeed += forwardInput * (baseSpeed * acceleration);

        if (currentSpeed > maxPositiveSpeed)
        {
            currentSpeed = previousCurrentSpeed;
        } else if (currentSpeed < -maxNegativeSpeed)
        {
            currentSpeed = previousCurrentSpeed;
        }

        Vector3 velocity = transform.forward * currentSpeed * Time.fixedDeltaTime;
        rb.velocity = velocity;
        transform.Rotate(transform.up * horizontalInput * turnSpeed * Time.fixedDeltaTime);
    }
    
    private void handleWeapon()
    {
        if (Input.GetMouseButtonDown(0) && canShoot)
        {
            if (LaserInstance is not null && !LaserInstance.IsUnityNull())
                PhotonNetwork.Destroy(LaserInstance);
            
            LaserInstance = PhotonNetwork.Instantiate("Prefab/Laser/" + LaserPrefab.name, firePoint.position, firePoint.rotation);
            //// LaserInstance.transform.parent = transform;
            LaserScript = LaserInstance.GetComponent<EGA_Laser>();
            photonView.RPC(nameof(Shoot), RpcTarget.All, LaserInstance.GetPhotonView().ViewID);
        }
        
        if (Input.GetMouseButtonUp(0))
        {
            if (LaserInstance is null || LaserInstance.IsUnityNull())
                return;
            
            LaserScript.DisablePrepare();
            PhotonNetwork.Destroy(LaserInstance);
        }
    }

    [PunRPC]
    private void Shoot(int viewId)
    {
        PhotonNetwork.GetPhotonView(viewId).gameObject.transform.parent = transform;
    }
}