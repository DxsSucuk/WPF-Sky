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
        foreach (Player player in PhotonNetwork.PlayerList.OrderBy(c => (int)c.CustomProperties["kills"]))
        {
            var row = Instantiate(rowUI, rowTransform).GetComponent<ScoreboardRowUI>();
            
            int death = (int)player.CustomProperties["deaths"],
                kills = (int)player.CustomProperties["kills"],
                ping = (int)player.CustomProperties["ping"];
            
            row.name.text = player.NickName;
            row.death.text = death.ToString();
            row.kills.text = kills.ToString();
            row.ping.text = ping.ToString();
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