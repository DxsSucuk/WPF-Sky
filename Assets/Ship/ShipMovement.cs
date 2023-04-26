using Photon.Pun;
using Photon.Voice.PUN;
using Photon.Voice.Unity;
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

    public PlayerMovement playerObject;
    
    public bool canShoot = true, canMove = true, inShip = true;

    private RigidbodyConstraints defaultConstraints;

    public bool shootableLaser = false;
    
    public float shootDelay = 0.1f;
    public int velocityBoost = 3;

    public PhotonVoiceView photonVoiceView;
    public Speaker photonSpeaker;
    public AudioSource photonAudioSource;
    
    private void Awake()
    {
        defaultFov = viewCamera.fieldOfView;
        rb = GetComponent<Rigidbody>();
        defaultConstraints = rb.constraints;
        
        photonVoiceView = GetComponent<PhotonVoiceView>();
        photonSpeaker = GetComponent<Speaker>();
        photonAudioSource = GetComponent<AudioSource>();
        
        if (!photonView.IsMine)
        {
            AudioListener audioListenerObject = GetComponentInChildren<AudioListener>();
            
            if (audioListenerObject is not null) audioListenerObject.gameObject.SetActive(false);
            
            if (viewCamera is not null) viewCamera.gameObject.SetActive(false);

            if (!inShip)
            {
                rb.constraints = RigidbodyConstraints.FreezeAll;
            }
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
        
        handleUIActions();
        
        if (!inShip) return;

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
        if (!inShip) return;
        if (!canMove) return;
        handleMovement();
    }

    private void handleUIActions()
    {
        if (Input.GetKeyDown(KeyCode.Tab))
        {
            FindObjectOfType<ScoreboardManager>(true).gameObject.SetActive(true);
        }

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            GameObject pauseObject = FindObjectOfType<PauseMenu>(true).gameObject;
            if (pauseObject.activeSelf)
            {
                pauseObject.SetActive(false);
                if (inShip)
                {
                    canMove = true;
                    canShoot = true;
                }
                else
                {
                    playerObject.canMove = true;
                }
            }
            else
            {
                pauseObject.SetActive(true);
                if (inShip)
                {
                    canMove = false;
                    canShoot = false;
                }
                else
                {
                    playerObject.canMove = false;
                }
            }
        }
        
        if (Input.GetKeyUp(KeyCode.Tab))
        {
            FindObjectOfType<ScoreboardManager>(true).gameObject.SetActive(false);
        }
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
        if (!shootableLaser)
        {
            if (Input.GetMouseButtonDown(0) && canShoot)
            {
                if (LaserInstance is not null && !LaserInstance.IsUnityNull())
                    PhotonNetwork.Destroy(LaserInstance);

                LaserInstance = PhotonNetwork.Instantiate("Prefab/Laser/" + LaserPrefab.name, firePoint.position,
                    firePoint.rotation);
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
        else
        {
            if (Input.GetMouseButton(0) && canShoot)
            {
                GameObject LaserObject =
                    PhotonNetwork.Instantiate("Prefab/Laser/" + LaserPrefab.name, firePoint.position, transform.rotation);

                Rigidbody LaserRigidbody = LaserObject.GetComponent<Rigidbody>();
                LaserRigidbody.AddForce(transform.TransformVector(Vector3.forward) * velocityBoost, ForceMode.Impulse);
                canShoot = false;
                Invoke(nameof(ResetShoot), shootDelay);
            }
        }
    }

    public void ResetShoot()
    {
        if (inShip)
        {
            canShoot = true;
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
        rb.constraints = RigidbodyConstraints.FreezeAll;
        photonVoiceView.enabled = false;
        photonSpeaker.enabled = false;
        photonAudioSource.enabled = false;
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
        rb.constraints = defaultConstraints;
        photonVoiceView.enabled = true;
        photonSpeaker.enabled = true;
        photonAudioSource.enabled = true;
        NameTagCanvas.enabled = false;
        inShip = true;
    }

    private void spawnPlayer()
    {
        playerObject = PhotonNetwork.Instantiate("Prefab/Player/" + PlayerPrefab.name, transform.position + (Vector3.up * 5), transform.rotation).GetComponent<PlayerMovement>();
    }

    private void despawnPlayer()
    {
        PhotonNetwork.Destroy(playerObject.gameObject);
    }
    
    [PunRPC]
    private void Shoot(int viewId)
    {
        PhotonNetwork.GetPhotonView(viewId).gameObject.transform.parent = transform;
    }
}