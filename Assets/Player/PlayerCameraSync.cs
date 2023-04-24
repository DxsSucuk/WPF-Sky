using UnityEngine;

public class PlayerCameraSync : MonoBehaviour
{
    public PlayerCamera playerCameraController;
    public Transform playerCamera;

    // Update is called once per frame
    void Update()
    {
        if (playerCamera is null) return;

        //// transform.position = playerCamera.position;

        transform.rotation = playerCamera.rotation;
    }
}