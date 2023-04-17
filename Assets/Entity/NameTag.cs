using System;
using Photon.Pun;
using TMPro;
using UnityEngine;

public class NameTag : MonoBehaviourPun
{
    [SerializeField]
    private TMP_Text NametagText;

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
        transform.LookAt(transform.position + mainCameraTransform.rotation * Vector3.forward,
            mainCameraTransform.rotation * Vector3.up);
    }

    private void SetName() => NametagText.text = photonView.Owner.NickName;
}