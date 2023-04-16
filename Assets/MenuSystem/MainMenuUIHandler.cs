using System.Collections.Generic;
using Photon.Pun;
using Photon.Realtime;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MainMenuUIHandler : MonoBehaviourPunCallbacks
{
    public GameObject ShipSelectionManagementObject;
    public GameObject ShipSelecetionUIObject;
    
    public GameObject MainMenuManagementObject;
    public GameObject MainMenuUIObject;

    public GameObject DefaultGameListEntryObject;
    public GameObject GameListObject;
    
    // Start is called before the first frame update
    void Start()
    {
        
    }

    public override void OnConnectedToMaster()
    {
        base.OnConnectedToMaster();
        RefreshGames();
    }

    public void CreateGame()
    {
        
    }

    public void JoinGame(string lobbyId)
    {
        
    }

    public void RefreshGames()
    {
        if (!PhotonNetwork.IsConnected)
        {
            return;
        }
        
        if (GameListObject is null || !GameListObject.activeSelf)
        {
            return;
        }

        if (DefaultGameListEntryObject is null)
        {
            return;
        }

        foreach (LayoutElement layoutElement in GameListObject.GetComponentsInChildren<LayoutElement>())
        {
            if (layoutElement.isActiveAndEnabled)
            {
                GameObject.Destroy(layoutElement.gameObject);
            }
        }

        PhotonNetwork.GetCustomRoomList(TypedLobby.Default, "");
    }

    public override void OnRoomListUpdate(List<RoomInfo> roomList)
    {
        base.OnRoomListUpdate(roomList);
        
        if (GameListObject is null || !GameListObject.activeSelf)
        {
            return;
        }

        if (DefaultGameListEntryObject is null)
        {
            return;
        }

        foreach (RoomInfo roomInfo in roomList)
        {
            GameObject currentObject = GameObject.Instantiate(DefaultGameListEntryObject, GameListObject.transform);
            TextMeshPro contentButton = currentObject.GetComponentInChildren<TextMeshPro>();
            contentButton.text = roomInfo.Name + " " + roomInfo.PlayerCount + "/" + roomInfo.MaxPlayers;
        }
    }

    public void SwitchToShipSelection()
    {
        MainMenuUIObject.SetActive(false);
        ShipSelecetionUIObject.SetActive(true);
        MainMenuManagementObject.SetActive(false);
        ShipSelectionManagementObject.SetActive(true);
    }

    public void SwitchToMainMenu()
    {
        ShipSelecetionUIObject.SetActive(false);
        MainMenuUIObject.SetActive(true);
        ShipSelectionManagementObject.SetActive(false);
        MainMenuManagementObject.SetActive(true);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
