using System;
using Photon.Pun;
using UnityEngine;

public class ShipEntity : MonoBehaviourPun
{
    public float HP = 0;
    public float MAX_HP = 500;
    public Transform Center;
    public GameObject CorpseExplosion;

    private void Awake()
    {
        HP = MAX_HP;
    }

    public void Ship_DamageNonRPC(float damage)
    {
        photonView.RPC("Ship_Damage", RpcTarget.All, damage);
    }
    
    [PunRPC]
    public void Ship_Damage(float damage)
    {
        HP -= damage;
        if (HP <= 0)
        {
            HP = -1;
            
            if (photonView.IsMine)
            {
                photonView.RPC("Ship_Death", RpcTarget.All);
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
            
            GameObject[] Spawnpoints = GameObject.FindGameObjectsWithTag("Respawn");
            
            photonView.RPC("Ship_Heal", RpcTarget.All, 99999f);
            
            PhotonNetwork.Instantiate("Prefab/Effect/" + CorpseExplosion.name, Center.position, Center.rotation);

            transform.position = Spawnpoints[new System.Random().Next(Spawnpoints.Length)].transform.position;
        }
    }
}