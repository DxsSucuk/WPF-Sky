using System.Linq;
using Photon.Pun;
using Photon.Realtime;
using Photon.Voice.PUN;
using Photon.Voice.Unity;
using UnityEngine;


public class NetworkManager : MonoBehaviourPunCallbacks
{
    public PhotonView playerPrefab;

    public GameObject[] spawnpoints;
    
    private int connectionRetries = 0;

    private bool rejoinCalled;

    private bool reconnectCalled;

    private bool inRoom;

    private DisconnectCause previousDisconnectCause;

    public PunVoiceClient PunVoiceClient;

    public void Start()
    {
        if (!PhotonNetwork.IsConnected)
            PhotonNetwork.ConnectUsingSettings();

        if (!PhotonNetwork.InRoom && PhotonNetwork.IsConnectedAndReady)
            PhotonNetwork.JoinRandomOrCreateRoom(null, 0, MatchmakingMode.FillRoom, null, null,
                RandomString(5));
    }

    public override void OnDisconnected(DisconnectCause cause)
    {
        Debug.LogFormat("OnDisconnected(cause={0}) ClientState={1} PeerState={2}",
            cause,
            PhotonNetwork.NetworkingClient.State,
            PhotonNetwork.NetworkingClient.LoadBalancingPeer.PeerState);
        if (SceneManagerHelper.ActiveSceneBuildIndex == 1)
        {
            if (this.rejoinCalled)
            {
                Debug.LogErrorFormat("Rejoin failed, client disconnected, causes; prev.:{0} current:{1}",
                    this.previousDisconnectCause, cause);
                this.rejoinCalled = false;
            }
            else if (this.reconnectCalled)
            {
                Debug.LogErrorFormat("Reconnect failed, client disconnected, causes; prev.:{0} current:{1}",
                    this.previousDisconnectCause, cause);
                this.reconnectCalled = false;
            }

            if (connectionRetries <= 3)
            {
                ++connectionRetries;
                this.HandleDisconnect(cause); // add attempts counter? to avoid infinite retries?
            }

            this.inRoom = false;
            this.previousDisconnectCause = cause;
        }
    }

    private void HandleDisconnect(DisconnectCause cause)
    {
        switch (cause)
        {
            // cases that we can recover from
            case DisconnectCause.ServerTimeout:
            case DisconnectCause.Exception:
            case DisconnectCause.ClientTimeout:
            case DisconnectCause.DisconnectByServerLogic:
            case DisconnectCause.AuthenticationTicketExpired:
            case DisconnectCause.DisconnectByServerReasonUnknown:
                if (this.inRoom)
                {
                    Debug.Log("calling PhotonNetwork.ReconnectAndRejoin()");
                    this.rejoinCalled = PhotonNetwork.ReconnectAndRejoin();
                    if (!this.rejoinCalled)
                    {
                        Debug.LogWarning(
                            "PhotonNetwork.ReconnectAndRejoin returned false, PhotonNetwork.Reconnect is called instead.");
                        this.reconnectCalled = PhotonNetwork.Reconnect();
                    }
                }
                else
                {
                    Debug.Log("calling PhotonNetwork.Reconnect()");
                    this.reconnectCalled = PhotonNetwork.Reconnect();
                }

                if (!this.rejoinCalled && !this.reconnectCalled)
                {
                    Debug.LogError(
                        "PhotonNetwork.ReconnectAndRejoin() or PhotonNetwork.Reconnect() returned false, client stays disconnected.");
                }

                break;
            case DisconnectCause.None:
            case DisconnectCause.OperationNotAllowedInCurrentState:
            case DisconnectCause.CustomAuthenticationFailed:
            case DisconnectCause.DisconnectByClientLogic:
            case DisconnectCause.InvalidAuthentication:
            case DisconnectCause.ExceptionOnConnect:
            case DisconnectCause.MaxCcuReached:
            case DisconnectCause.InvalidRegion:
                Debug.LogFormat(
                    "Disconnection we cannot automatically recover from, cause: {0}, report it if you think auto recovery is still possible",
                    cause);
                break;
        }
    }

    public override void OnConnectedToMaster()
    {
        if (this.reconnectCalled)
        {
            Debug.Log("Reconnect successful");
            this.reconnectCalled = false;
            connectionRetries = 0;
        }

        if (!PhotonNetwork.InRoom)
            PhotonNetwork.JoinRandomOrCreateRoom(null, 0, Photon.Realtime.MatchmakingMode.FillRoom, null, null,
                "Test");
    }

    public override void OnJoinRoomFailed(short returnCode, string message)
    {
        if (this.rejoinCalled)
        {
            Debug.LogErrorFormat("Quick rejoin failed with error code: {0} & error message: {1}", returnCode, message);
            this.rejoinCalled = false;
        }
    }

    public override void OnJoinedRoom()
    {
        inRoom = true;
        if (this.rejoinCalled)
        {
            Debug.Log("Rejoin successful");
            this.rejoinCalled = false;
            connectionRetries = 0;
        }

        Debug.Log("Joined room " + PhotonNetwork.CurrentRoom.Name);
        Debug.Log("Users in this Lobby " + PhotonNetwork.CurrentRoom.PlayerCount);

        if (PhotonNetwork.GetPhotonView(PhotonNetwork.SyncViewId) == null)
        {
            GameObject playerGameObject = PhotonNetwork.Instantiate(playerPrefab.name,
                spawnpoints[new System.Random().Next(spawnpoints.Length)].transform.position,
                Quaternion.identity);

            if (PunVoiceClient is not null)
            {
                PunVoiceClient.SpeakerPrefab = playerGameObject;
            }
        }

        Debug.Log("Creating");
    }

    public override void OnLeftRoom()
    {
        base.OnLeftRoom();
        inRoom = false;
    }

    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        base.OnPlayerEnteredRoom(newPlayer);
        Debug.Log(newPlayer.NickName + " joined!");
    }

    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        base.OnPlayerLeftRoom(otherPlayer);
        Debug.Log(otherPlayer.NickName + " left!");
    }

    string RandomString(int length)
    {
        const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
        return new string(Enumerable.Repeat(chars, length)
            .Select(s => s[new System.Random().Next(s.Length)]).ToArray());
    }
}