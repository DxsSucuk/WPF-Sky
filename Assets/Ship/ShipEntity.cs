using System;
using Photon.Pun;
using Photon.Voice.PUN;
using UnityEngine;

public class ShipEntity : MonoBehaviourPun
{
    public float HP = 0;
    public float MAX_HP = 500;
    public int respawnDelay = 2;
    public Transform Center;
    public GameObject CorpseExplosion;
    public MeshRenderer ShipModel;
    public ShipMovement ShipMovement;

    private void Awake()
    {
        HP = MAX_HP;
        if (photonView.Owner is null)
        {
            PhotonVoiceView photonVoiceView = GetComponent<PhotonVoiceView>();
            if (photonVoiceView is not null)
            {
                photonVoiceView.enabled = false;
            }
        }
    }

    public void Ship_DamageNonRPC(float damage)
    {
        photonView.RPC(nameof(Ship_Damage), RpcTarget.All, damage);
    }
    
    [PunRPC]
    public void Ship_Damage(float damage)
    {
        if (HP > 0)
        {
            HP -= damage;
        }
        else
        {
            if (HP < -1) return;
            
            HP = -1;
            
            if (photonView.IsMine)
            {
                photonView.RPC(nameof(Ship_Death), RpcTarget.All);
            }
        }
    }
    
    [PunRPC]
    public void Ship_Heal(float heal)
    {
        HP = HP + heal > MAX_HP ? MAX_HP : HP + heal;
    }

    [PunRPC]
    public void Ship_Death()
    {
        Debug.Log(photonView.Owner.NickName + " died!");
        if (photonView.IsMine)
        {
            if (HP < -1) return;
            
            HP = -2;

            if (ShipModel is not null)
            {
                photonView.RPC(nameof(Ship_Hide), RpcTarget.All);
                ShipMovement.canShoot = false;
                ShipMovement.canMove = false;
                // Respawn in x seconds.
                Invoke(nameof(Ship_Respawn), respawnDelay);
            }
            
            PhotonNetwork.Instantiate("Prefab/Effect/" + CorpseExplosion.name, Center.position, Center.rotation);
        }
    }

    public void Ship_Respawn()
    {
        photonView.RPC(nameof(Ship_Heal), RpcTarget.All, 99999f);
        GameObject[] Spawnpoints = GameObject.FindGameObjectsWithTag("Respawn");
        transform.position = Spawnpoints[new System.Random().Next(Spawnpoints.Length)].transform.position;
        ShipMovement.canShoot = true;
        ShipMovement.canMove = true;
        photonView.RPC(nameof(Ship_Show), RpcTarget.All);
    }
    
    [PunRPC]
    public void Ship_Show()
    {
        ShipModel.enabled = true;
    }

    [PunRPC]
    public void Ship_Hide()
    {
        ShipModel.enabled = false;
    }
}