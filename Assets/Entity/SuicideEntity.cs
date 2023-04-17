using Photon.Pun;

public class SuicideEntity : MonoBehaviourPun
{
    public float delay;

    private void Awake()
    {
        Invoke(nameof(KillMe), delay);
    }

    void KillMe()
    {
        if (!photonView.IsMine) return;
        PhotonNetwork.Destroy(gameObject);
    }
}