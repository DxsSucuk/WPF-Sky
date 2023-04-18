using System;
using Photon.Pun;
using TMPro;
using UnityEngine;

public class NameTag : MonoBehaviourPun
{
    [SerializeField]
    private TMP_Text NametagText;

    public bool RotateNametag = true;
    private Transform mainCameraTransform;

    private void Start()
    {
        mainCameraTransform = Camera.main.transform;
        if (photonView.IsMine)
        {
            return;
        }

        SetName();
    }

    private void LateUpdate()
    {
        if (!RotateNametag) return;
        transform.LookAt(transform.position + mainCameraTransform.rotation * Vector3.forward,
            mainCameraTransform.rotation * Vector3.up);
    }

    private void SetName()
    {
        if (photonView.Owner is null) return;
        
        NametagText.text = photonView.Owner.NickName;
    }
}