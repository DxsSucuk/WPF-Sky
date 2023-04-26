using System;
using System.Linq;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine;


public class ScoreboardManager : MonoBehaviour
{
    public ScoreboardRowUI rowUI;
    public Transform rowTransform;

    private void OnEnable()
    {
        foreach (Player player in PhotonNetwork.PlayerList.OrderBy(c => c.NickName))
        {
            var row = Instantiate(rowUI, rowTransform).GetComponent<ScoreboardRowUI>();
            row.name.text = player.NickName;
            row.death.text = "0";
            row.kills.text = "0";
            row.ping.text = "0";
        }
    }

    private void OnDisable()
    {
        for (int child = 0; child < rowTransform.childCount; ++child)
        {
            Destroy(rowTransform.GetChild(child).gameObject);
        }
    }
}