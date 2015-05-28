using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class CPlayerStat
{
    public string Name;
    public float Time;
    public int Rank;

    public CPlayerStat()
    {
        Name = "";
        Time = 0f;
        Rank = 0;
    }
}

public class CPlayer
{
    public string m_Name;
    public int m_ID;
    public int m_Score;
    public GameObject m_Obj;
    public NetworkPlayer m_NetPlayer;

    public CPlayer()
    {
        m_Name = "";
        m_ID = 0;
        m_Score = 0;
        m_Obj = null;
    }
}

public class CGameManager : MonoBehaviour {

    public static CGameManager ins = null;

    public GameObject CarPrefab;
    public GameObject StatsHolder;
    public Text TIME_COUNTER;
    [HideInInspector] public float m_TimePassed = -1;
    public Text LAP_COUNTER;

    public int m_LapsNeeded;
    public int m_CheckpointsNeeded;
    public Transform[] m_SpawnPoints;
    [HideInInspector] public NetworkView NetView;
    [HideInInspector] public CPlayer[] m_Players = new CPlayer[6];
    [HideInInspector] public CPlayerStat[] m_PlayerStats = new CPlayerStat[6];
    int m_StatCounter = 0;
    public static int m_LocalID = -1;
    public static GameObject m_LocalObj;

    float m_NextPlayersUpdate = 0f;

    void Awake()
    {
        ins = this;
        NetView = GetComponent<NetworkView>();
        for (int i = 0; i < 6; i++)
        {
            m_Players[i] = null;
            m_PlayerStats[i] = null;
        }
        if (Network.isServer)
        {
            m_LocalID = 0;
            OnLap(0);
            SubmitPlayer(0, Network.player);
            m_LocalObj = (GameObject)Network.Instantiate(CarPrefab, Vector3.down * 200, Quaternion.identity, 0);
            m_LocalObj.transform.position = m_SpawnPoints[m_LocalID].position;
            m_LocalObj.GetComponent<CCar>().m_Player = m_Players[m_LocalID];
            m_LocalObj.GetComponent<CCar>().OnRestart();
            GameObject.Find("IPT").GetComponent<UnityEngine.UI.Text>().text = Network.player.ipAddress;

            MasterServer.RegisterHost("SmallRace2", Random.Range(100, 999).ToString());
        }
    }

    void OnFailedToConnectToMasterServer(NetworkConnectionError info)
    {
        Debug.LogError("Could not connect to master server: " + info);
    }
    void OnMasterServerEvent(MasterServerEvent msEvent)
    {
        if (msEvent == MasterServerEvent.HostListReceived)
            return;
        else if (msEvent == MasterServerEvent.RegistrationSucceeded)
            Debug.Log("RegistrationSucceeded!");
        else if (Network.isServer)
            MasterServer.RegisterHost("SmallRace2", Random.Range(100, 999).ToString());
    }
    void OnApplicationQuit()
    {
        if (Network.isServer)
            MasterServer.UnregisterHost();
    }

    void Update()
    {
        if (Network.isServer)
        {
            if (Time.time > m_NextPlayersUpdate)
            {
                for (int i = 0; i < 6; i++)
                {
                    if (m_Players[i] != null)
                    {
                        NetView.RPC("Server_UpdatePlayer", RPCMode.Others, i, m_Players[i].m_Name, m_Players[i].m_Score);
                    }
                }
                m_NextPlayersUpdate = Time.time + 0.2f;
            }
        }
        TIME_COUNTER.text = (m_TimePassed == -1) ? "" : CGlobal.FormatTime(m_TimePassed);
    }

    public void OnLap(int Lap)
    {
        LAP_COUNTER.text = Lap + " / " + m_LapsNeeded;
    }

    IEnumerator OnPlayerConnected(NetworkPlayer netPlayer)
    {
        int i = 0;
        for (; i < 6; i++)
            if (m_Players[i] == null)
                break;
        if (i == 0) // no slot
            yield break;
        SubmitPlayer(i, netPlayer);
        NetView.RPC("Server_Info", netPlayer, i);
        yield return new WaitForSeconds(1f);
        if (PlayerNum() == Network.maxConnections)
        {
            NetView.RPC("Server_Start", RPCMode.All);
            MasterServer.UnregisterHost();
        }
    }
    void OnPlayerDisconnected(NetworkPlayer netPlayer)
    {
        int id = GetPlayerID(netPlayer);
        if (id == -1)
            return;
        Network.DestroyPlayerObjects(netPlayer);
        m_Players[id] = null;
        NetView.RPC("Server_RemovePlayer", RPCMode.Others, id);
    }

    void RestartGame()
    {
        if (!Network.isServer)
            return;
        for (int i = 0; i < 6; i++)
            m_PlayerStats[i] = null;
        m_StatCounter = 0;

        NetView.RPC("Server_Restart", RPCMode.All);
    }
    IEnumerator RestartTheGame()
    {
        string strToSend = "Stats:\n";
        for (int i = 0; i < 6; i++)
            if (m_PlayerStats[i] != null)
                strToSend += (m_PlayerStats[i].Rank+1).ToString() + ". " + m_PlayerStats[i].Name + "\t" + CGlobal.FormatTime(m_PlayerStats[i].Time) + "\n";
        NetView.RPC("Server_ShowStat", RPCMode.All, strToSend);
        yield return new WaitForSeconds(3f);
        RestartGame();
    }

    // RPCs
    [RPC]
    void Server_Start()
    {
        m_LocalObj.GetComponent<CCar>().m_CanMove = true;
    }
    [RPC]
    void Server_ShowStat(string str)
    {
        StatsHolder.SetActive(true);
        StatsHolder.transform.FindChild("Text").GetComponent<Text>().text = str;
    }
    [RPC]
    void Server_Restart()
    {
        m_TimePassed = 0;
        StatsHolder.SetActive(false);
        foreach (CCar car in GameObject.FindObjectsOfType<CCar>())
            car.OnRestart();
    }

    [RPC]
    public void Client_Finish(float time, int ID, NetworkMessageInfo info)
    {
        int id = GetPlayerID(info.sender);
        if (ID != -1)
            id = ID;
        if (id == -1)
            return;
        m_PlayerStats[m_StatCounter] = new CPlayerStat();
        m_PlayerStats[m_StatCounter].Name = m_Players[id].m_Name;
        m_PlayerStats[m_StatCounter].Time = time;
        m_PlayerStats[m_StatCounter].Rank = m_StatCounter;
        m_StatCounter++;

        if (m_StatCounter >= PlayerNum())
        {
            StartCoroutine(RestartTheGame());
        }
    }

    [RPC]
    void Server_UpdatePlayer(int ID, string Name, int Score)
    {
        if (m_Players[ID] == null)
            m_Players[ID] = new CPlayer();
        m_Players[ID].m_ID = ID;
        m_Players[ID].m_Name = Name;
        m_Players[ID].m_Score = Score;
    }
    [RPC]
    void Server_RemovePlayer(int ID)
    {
        m_Players[ID] = null;
    }
    [RPC]
    void Server_Info(int ID)
    {
        if (m_Players[ID] == null)
            SubmitPlayer(ID, Network.player);
        m_LocalID = ID;
        m_LocalObj = (GameObject)Network.Instantiate(CarPrefab, Vector3.down * 200, Quaternion.identity, 0);
        m_LocalObj.transform.position = m_SpawnPoints[m_LocalID].position;
        m_LocalObj.GetComponent<CCar>().m_Player = m_Players[ID];
        m_LocalObj.GetComponent<CCar>().OnRestart();
        OnLap(0);
    }

    // Helper Functions
    void SubmitPlayer(int ID, NetworkPlayer netPlayer)
    {
        m_Players[ID] = new CPlayer();
        m_Players[ID].m_ID = ID;
        m_Players[ID].m_NetPlayer = netPlayer;
    }
    public int GetPlayerID(NetworkPlayer netPlayer)
    {
        int ID = -1;
        for (int i = 0; i < 6; i++)
            if (m_Players[i] != null && m_Players[i].m_NetPlayer == netPlayer)
                ID = i;
        return ID;
    }
    public int PlayerNum()
    {
        int num = 0;
        for (int i = 0; i < 6; i++)
            if (m_Players[i] != null)
                num++;
        return num;
    }


    // Button Functions
    public void OnStartClick()
    {
        if (!Network.isServer)
            return;
        NetView.RPC("Server_Start", RPCMode.All);
    }
}
