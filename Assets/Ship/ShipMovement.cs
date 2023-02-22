using Photon.Pun;
using UnityEngine;

public class ShipMovement : MonoBehaviourPun
{
    public float baseSpeed;
    public float turnSpeed;
    public float currentSpeed;

    private Rigidbody rb;
    
    private float horizontalInput;
    private float forwardInput;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        if (!photonView.IsMine)
        {
            GetComponentInChildren<Camera>().gameObject.SetActive(false);
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        if (!photonView.IsMine) return;
        
        horizontalInput = Input.GetAxis("Horizontal");
        forwardInput = Input.GetAxis("Vertical");

        float previousCurrentSpeed = currentSpeed;
        
        currentSpeed += forwardInput * baseSpeed;

        if (currentSpeed > 1500)
        {
            currentSpeed = previousCurrentSpeed;
        } else if (currentSpeed < -100)
        {
            currentSpeed = previousCurrentSpeed;
        }

        Vector3 velocity = transform.forward * currentSpeed * Time.fixedDeltaTime;
        velocity.y = rb.velocity.y;
        rb.velocity = velocity;
        transform.Rotate(transform.up * horizontalInput * turnSpeed * Time.fixedDeltaTime);
    }
}