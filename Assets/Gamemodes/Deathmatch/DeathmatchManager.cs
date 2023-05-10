﻿using System.Collections.Generic;
using System.Linq;
using ExitGames.Client.Photon;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine;

public class DeathmatchManager : MonoBehaviour
{
    public int neededKills = 10;
    private static Dictionary<int, int> playerKills = new();

    public GameObject winScreen;
    public GameObject loseScreen;

    private void Awake()
    {
        Hashtable hash = PhotonNetwork.LocalPlayer.CustomProperties;
        hash.Add("ping", PhotonNetwork.GetPing());
        hash.Add("deaths", 0);
        hash.Add("kills", 0);
        PhotonNetwork.LocalPlayer.SetCustomProperties(hash);
    }

    private void OnEnable()
    {
        PhotonNetwork.NetworkingClient.EventReceived += OnEvent;
    }

    private void OnDisable()
    {
        PhotonNetwork.NetworkingClient.EventReceived -= OnEvent;
    }

    private void LateUpdate()
    {
        Hashtable hash = PhotonNetwork.LocalPlayer.CustomProperties;
        hash["ping"] = PhotonNetwork.GetPing();
        PhotonNetwork.LocalPlayer.SetCustomProperties(hash);
    }

    public void OnEvent(EventData photonEvent)
    {
        byte eventCode = photonEvent.Code;

        if (eventCode == EventList.DEATH_EVENT)
        {
            if (photonEvent.CustomData is object[] content)
            {
                if (!PhotonNetwork.IsMasterClient) return;

                int actorNr = (int)content[0];
                int viewId = (int)content[1];
                int damagerId = (int)content[2];

                int kills = 0;
                
                if (playerKills.TryGetValue(damagerId, out kills))
                {
                    playerKills.Remove(damagerId);
                }

                kills += 1;
                
                playerKills.Add(damagerId, kills);

                Debug.Log(damagerId + " - > " + kills);
                
                object[] contentNew = new object[] { damagerId, actorNr, kills};

                PhotonNetwork.RaiseEvent(EventList.KILL_EVENT, contentNew, new RaiseEventOptions
                {
                    Receivers = ReceiverGroup.All
                }, SendOptions.SendReliable);

                if (kills >= neededKills)
                {
                    object[] contentGameOver = new object[] { damagerId, kills};

                    PhotonNetwork.RaiseEvent(EventList.GAME_OVER_EVENT, contentNew, new RaiseEventOptions
                    {
                        Receivers = ReceiverGroup.All
                    }, SendOptions.SendReliable); 
                }
            }
        }
        else if (eventCode == EventList.KILL_EVENT)
        {
            if (photonEvent.CustomData is object[] content)
            {
                int localActorId = PhotonNetwork.LocalPlayer.ActorNumber;
                
                int killerActorId = (int)content[0];
                int victimActorId = (int)content[1];
                int kills = (int)content[2];

                Player[] listOfPlayers = PhotonNetwork.PlayerList;
                Player killerPlayer = listOfPlayers.First(c => c.ActorNumber == killerActorId);
                Player victimPlayer = listOfPlayers.First(c => c.ActorNumber == victimActorId);

                Debug.Log(killerPlayer.NickName + " killed " + victimPlayer.NickName);

                Hashtable hash = PhotonNetwork.LocalPlayer.CustomProperties;

                if (victimActorId == localActorId)
                {
                    int newDeath = (int)hash["deaths"] + 1;
                    hash["deaths"] = newDeath;
                    Debug.Log("Current Local Deaths -> " + hash["deaths"] + ", it should be " + newDeath);
                }

                if (killerActorId == localActorId)
                {
                    hash["kills"] = kills;
                    Debug.Log("Current Local Kills -> " + hash["kills"] + ", it should be " + kills);
                }
                
                PhotonNetwork.LocalPlayer.SetCustomProperties(hash);
            }
        }
        else if (eventCode == EventList.GAME_OVER_EVENT)
        {
            if (photonEvent.CustomData is object[] content)
            {
                int winnerActorId = (int)content[0];
                int kills = (int)content[1];

                Player[] listOfPlayers = PhotonNetwork.PlayerList;
                Player winnerPlayer = listOfPlayers.First(c => c.ActorNumber == winnerActorId);

                Debug.Log(winnerPlayer.NickName + " won the Game with " + kills + "!");

                if (PhotonNetwork.GetPhotonView(PhotonNetwork.SyncViewId)
                    .TryGetComponent(out ShipMovement shipMovement))
                {
                    shipMovement.canShoot = false;
                    shipMovement.canMove = false;
                }
                
                if (winnerPlayer.ActorNumber == PhotonNetwork.LocalPlayer.ActorNumber)
                {
                    winScreen.SetActive(true);
                }
                else
                {
                    loseScreen.SetActive(true);
                }
            }
        }
    }
}