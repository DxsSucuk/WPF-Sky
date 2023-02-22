using Photon.Pun;
using UnityEngine;

public class Shoot : MonoBehaviourPun
{
    public PhotonView projectilePrefab;

    private Rigidbody rb;
    public ParticleSystem particle;
    public AudioSource audioSource;
    
    private void Awake()
    {
        rb = GetComponent<Rigidbody>();

        if (particle is not null)
        {
            particle.Play();
        }

        if (audioSource is not null)
        {
            audioSource.Play();
        }
        
        rb.AddForce(Vector3.forward, ForceMode.VelocityChange);
    }
    
}