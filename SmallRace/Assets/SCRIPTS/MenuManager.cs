using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class MenuManager : MonoBehaviour
{

    string ipToConnect = "192.168.1.45";

    public Image TwoPlayers;
    public Image FourPlayers;
    public Image SixPlayers;
    HostData[] Hosts;

    void Update()
    {
        MasterServer.RequestHostList("SmallRace2");
        Hosts = MasterServer.PollHostList();
    }

    IEnumerator QuickMatch()
    {
        const int MaxTries = 20;
        bool createHost = false;
        for (int i = 0; i < MaxTries; i++)
        {
            yield return new WaitForSeconds(0.15f);
            if (Hosts.Length > 0)
                break;
        }
        if (Hosts.Length > 0) // found some matches
        {
            Debug.Log("Found");
            foreach (HostData host in Hosts)
            {
                if (!host.passwordProtected && host.playerLimit == (TwoPlayers.enabled ? 2 : FourPlayers.enabled ? 4 : 6))
                {
                    Network.Connect(host);
                    Debug.Log("Connected");
                    yield break;
                }
            }
            createHost = true;
        }
        else
            createHost = true;
        if (createHost)
        {
            Network.InitializeSecurity();
            Network.InitializeServer(TwoPlayers.enabled ? 2 : FourPlayers.enabled ? 4 : 6, 27015, false);
        }
    }
    public void OnQuickMatchClick()
    {
        MasterServer.UnregisterHost();
        StopAllCoroutines();
        MasterServer.ClearHostList();
        MasterServer.RequestHostList("SmallRace2");
        StartCoroutine(QuickMatch());
    }

    /*public void OnHostClick()
    {
        Network.InitializeSecurity();
        Network.InitializeServer(6, 8303, false);
    }*/
    public void OnLocalJoinClick()
    {
        Network.Connect(ipToConnect, 8303);
    }
    public void On2PlayersChange(bool val)
    {
        TwoPlayers.enabled = true;
        FourPlayers.enabled = false;
        SixPlayers.enabled = false;
    }
    public void On4PlayersChange(bool val)
    {
        TwoPlayers.enabled = false;
        FourPlayers.enabled = true;
        SixPlayers.enabled = false;
    }
    public void On6PlayersChange(bool val)
    {
        TwoPlayers.enabled = false;
        FourPlayers.enabled = false;
        SixPlayers.enabled = true;
    }

    void OnServerInitialized()
    {
        Network.isMessageQueueRunning = false;
        CContext.Context.ChangeLevel("Track1");
    }

    void OnConnectedToServer()
    {
        Network.isMessageQueueRunning = false;
        CContext.Context.ChangeLevel("Track1");
    }

}
