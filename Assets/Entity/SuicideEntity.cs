using System;
using Photon.Pun;
using UnityEngine;

public class SuicideEntity : MonoBehaviourPun
{
    public float delay = 5;
    public bool destroyOnCollision = false;

    private void Awake()
    {
        Invoke(nameof(KillMe), delay);
    }

    void KillMe()
    {
        if (!photonView.IsMine) return;
        PhotonNetwork.Destroy(gameObject);
    }

    private void OnCollisionEnter(Collision other)
    {
        if (!photonView.IsMine) return;
        if (other.gameObject.tag.ToLower().Equals("ship")) return;
        KillMe();
    }
}