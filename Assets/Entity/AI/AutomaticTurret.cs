using System;
using Photon.Pun;
using Unity.VisualScripting;
using UnityEngine;

public class AutomaticTurret : MonoBehaviourPun
{
    public int whatIsPlayer;
    public float sightRange;
    public GameObject targetPlayer;

    public Transform firePoint;

    public GameObject LaserPrefab;

    public bool canShoot = true;

    public float shootDelay = 0.1f;
    public int velocityBoost = 3;

    GameObject GetClosestEnemy(ShipEntity[] enemies)
    {
        GameObject bestTarget = null;
        float closestDistanceSqr = Mathf.Infinity;
        Vector3 currentPosition = transform.position;
        foreach (ShipEntity potentialTarget in enemies)
        {
            Vector3 directionToTarget = potentialTarget.transform.position - currentPosition;
            float dSqrToTarget = directionToTarget.sqrMagnitude;
            float distance = directionToTarget.magnitude;

            if (distance > sightRange) continue;

            if (dSqrToTarget < closestDistanceSqr)
            {
                if (checkBetween(potentialTarget.transform.position))
                {
                    continue;
                }
                
                closestDistanceSqr = dSqrToTarget;
                bestTarget = potentialTarget.gameObject;
            }
        }


        return bestTarget;
    }

    private void Update()
    {
        if (photonView.Controller is not null && photonView.Controller.IsMasterClient && photonView.IsMine)
        {
            ChasePlayer();
        }
    }

    private void ChasePlayer()
    {
        if (targetPlayer is null || targetPlayer.IsUnityNull())
        {
            targetPlayer = GetClosestEnemy(FindObjectsOfType<ShipEntity>());
        }
        else
        {
            Vector3 directionToTarget = targetPlayer.transform.position - transform.position;
            float distance = directionToTarget.magnitude;
            if (distance > sightRange)
            {
                targetPlayer = GetClosestEnemy(FindObjectsOfType<ShipEntity>());
            }
            else if (checkBetween(targetPlayer.transform.position))
            {
                targetPlayer = GetClosestEnemy(FindObjectsOfType<ShipEntity>());
            }
        }

        if (targetPlayer is not null)
        {
            transform.LookAt(targetPlayer.transform);
            Shoot();
        }
    }

    private bool checkBetween(Vector3 targetPosition)
    {
        Debug.DrawLine(transform.position, targetPosition);
        if (Physics.Linecast(transform.position, targetPosition, out RaycastHit raycastHit))
        {
            if (raycastHit.collider.gameObject.layer != whatIsPlayer)
            {
                return true;
            }
        }

        return false;
    }

    public void Shoot()
    {
        if (!canShoot) return;
        
        GameObject LaserObject =
            PhotonNetwork.Instantiate("Prefab/Laser/" + LaserPrefab.name, firePoint.position, transform.rotation);

        Rigidbody LaserRigidbody = LaserObject.GetComponent<Rigidbody>();
        LaserRigidbody.AddForce(transform.TransformVector(Vector3.forward) * velocityBoost, ForceMode.Impulse);
        canShoot = false;
        Invoke(nameof(ResetShoot), shootDelay);
    }

    public void ResetShoot()
    {
        canShoot = true;
    }
    
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, sightRange);
    }
}