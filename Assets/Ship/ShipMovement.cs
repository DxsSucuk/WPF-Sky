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

    public GameObject PlayerPrefab;
    
    public Camera viewCamera;
    public Canvas NameTagCanvas;

    public bool invertCamera;

    public float mouseSensitivity;
    public float maxLookAngle;

    private Rigidbody rb;
    private float defaultFov;
    
    private float horizontalInput;
    private float forwardInput;

    private GameObject playerObject;
    
    public bool canShoot = true, canMove = true, inShip = true;

    private void Awake()
    {
        defaultFov = viewCamera.fieldOfView;
        rb = GetComponent<Rigidbody>();
        if (!photonView.IsMine)
        {
            AudioListener audioListenerObject = GetComponentInChildren<AudioListener>();
            
            if (audioListenerObject is not null) audioListenerObject.gameObject.SetActive(false);
            
            if (viewCamera is not null) viewCamera.gameObject.SetActive(false);
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
        handleActions();
        if (!canMove) return;
        handleCamera();
        handleCameraZoom();
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        if (!photonView.IsMine) return;
        if (!canMove) return;
        handleMovement();
    }

    private void handleCamera()
    {

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
        float maxFov = defaultFov + 10;
        
        var localVel = transform.InverseTransformDirection(rb.velocity);

        if (localVel.z > 0)
        {
            viewCamera.fieldOfView = maxFov;
        }
        else
        {
            viewCamera.fieldOfView = defaultFov;
        }
    }
    
    private void handleMovement()
    {
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

    private void handleActions()
    {
        if (Input.GetKeyDown(KeyCode.F) && inShip)
        {
            photonView.RPC(nameof(ShipLeaving), RpcTarget.All);
        }
    }
    
    [PunRPC]
    private void ShipLeaving()
    {
        NameTagCanvas.enabled = false;
        inShip = false;
        if (photonView.IsMine)
        {
            viewCamera.gameObject.SetActive(false);
            canMove = false;
            canShoot = false;
            spawnPlayer();
        }
    }

    public void ShipEntering()
    {
        if (photonView.IsMine)
        {
            viewCamera.gameObject.SetActive(true);
            canMove = true;
            canShoot = true;
            despawnPlayer();
            photonView.RPC(nameof(ShipEntered), RpcTarget.All);
        }
    }

    [PunRPC]
    private void ShipEntered()
    {
        NameTagCanvas.enabled = false;
        inShip = false;
    }

    private void spawnPlayer()
    {
        playerObject = PhotonNetwork.Instantiate("Prefab/Player/" + PlayerPrefab.name, transform.position + (Vector3.up * 5), transform.rotation);
    }

    private void despawnPlayer()
    {
        PhotonNetwork.Destroy(playerObject);
    }
    
    [PunRPC]
    private void Shoot(int viewId)
    {
        PhotonNetwork.GetPhotonView(viewId).gameObject.transform.parent = transform;
    }
}