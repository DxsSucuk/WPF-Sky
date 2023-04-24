using UnityEngine;

public class PlayerCameraSync : MonoBehaviour
{
    public PlayerCamera playerCameraController;
    public Transform playerCamera;

    // Update is called once per frame
    void Update()
    {
        if (playerCamera == null) return;

        transform.position = playerCamera.position;

        playerCamera.rotation = playerCameraController.playerCamera.transform.rotation;
    }
}