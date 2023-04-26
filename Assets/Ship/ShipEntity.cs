using ExitGames.Client.Photon;
using Photon.Pun;
using Photon.Realtime;
using Photon.Voice.PUN;
using UnityEngine;
using UnityEngine.UI;

public class ShipEntity : MonoBehaviourPun
{
    public string shipName;
    public ShipTyp shipTyp;
    public float REAL_HP = 0;
    public float MAX_HP = 500;
    public int respawnDelay = 2;
    public Transform Center;
    public GameObject CorpseExplosion;
    public MeshRenderer ShipModel;
    public ShipMovement ShipMovement;
    public Slider HPSlider;

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

    public float HP
    {
        get => REAL_HP;

        set
        {
            REAL_HP = value;
            if (HPSlider is not null && photonView.IsMine)
            {
                float sliderValue = (REAL_HP / MAX_HP);
                HPSlider.value = sliderValue;
            }
        }
    }

    public void Ship_DamageNonRPC(float damage, int damagerId)
    {
        photonView.RPC(nameof(Ship_Damage), RpcTarget.All, damage, damagerId);
    }

    [PunRPC]
    public void Ship_Damage(float damage, int damagerId)
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
                photonView.RPC(nameof(Ship_Death), RpcTarget.All, damagerId);
            }
        }
    }

    [PunRPC]
    public void Ship_Heal(float heal)
    {
        HP = HP + heal > MAX_HP ? MAX_HP : HP + heal;
    }

    [PunRPC]
    public void Ship_Death(int damagerId)
    {
        Debug.Log(photonView.Owner.NickName + "s Ship died!");
        if (photonView.IsMine)
        {
            if (HP < -1) return;

            HP = -2;

            if (ShipModel is not null)
            {
                photonView.RPC(nameof(Ship_Hide), RpcTarget.All);
                if (ShipMovement.inShip)
                {
                    ShipMovement.canShoot = false;
                    ShipMovement.canMove = false;
                }

                // Respawn in x seconds.
                Invoke(nameof(Ship_Respawn), respawnDelay);
            }

            PhotonNetwork.Instantiate("Prefab/Effect/" + CorpseExplosion.name, Center.position, Center.rotation);
            
            object[] content = new object[] { PhotonNetwork.LocalPlayer.ActorNumber, photonView.ViewID, damagerId };

            PhotonNetwork.RaiseEvent(EventList.DEATH_EVENT, content, new RaiseEventOptions
            {
                Receivers = ReceiverGroup.MasterClient
            }, SendOptions.SendReliable);
        }
    }

    public void Ship_Respawn()
    {
        photonView.RPC(nameof(Ship_Heal), RpcTarget.All, 99999f);

        if (ShipMovement.inShip)
        {
            GameObject[] Spawnpoints = GameObject.FindGameObjectsWithTag("Respawn");
            transform.position = Spawnpoints[new System.Random().Next(Spawnpoints.Length)].transform.position;
            ShipMovement.canShoot = true;
            ShipMovement.canMove = true;
        }
        else
        {
            ShipMovement.transform.position = ShipMovement.playerObject.transform.position + (Vector3.up * 2);
        }

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