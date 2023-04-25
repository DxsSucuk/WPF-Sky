using System.Collections.Generic;
using System.Linq;
using ExitGames.Client.Photon;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine;

public class DeathmatchManager : MonoBehaviour
{
    public int neededKills = 25;
    private static Dictionary<int, int> playerKills = new();

    private void OnEnable()
    {
        PhotonNetwork.NetworkingClient.EventReceived += OnEvent;
    }

    private void OnDisable()
    {
        PhotonNetwork.NetworkingClient.EventReceived -= OnEvent;
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
                
                playerKills.Add(damagerId, ++kills);

                Debug.Log(playerKills);
                
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
                int killerActorId = (int)content[0];
                int victimActorId = (int)content[1];

                Player[] listOfPlayers = PhotonNetwork.PlayerList;
                Player killerPlayer = listOfPlayers.First(c => c.ActorNumber == killerActorId);
                Player victimPlayer = listOfPlayers.First(c => c.ActorNumber == victimActorId);

                Debug.Log(killerPlayer.NickName + " killed " + victimPlayer.NickName);
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
            }
        }
    }
}