using System;
using Photon.Pun;
using Photon.Voice.Unity;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PauseMenu : MonoBehaviour
{
    private void OnEnable()
    {
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.Confined;
    }

    private void OnDisable()
    {
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
    }

    public void LeaveMatch()
    {
        PhotonNetwork.LeaveRoom();
        Destroy(FindObjectOfType<NetworkManager>().gameObject);
        Destroy(FindObjectOfType<VoiceLogger>().gameObject);
        SceneManager.LoadScene(SceneManagerHelper.ActiveSceneBuildIndex - 1);
    }
    
}