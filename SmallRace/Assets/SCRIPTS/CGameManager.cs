using UnityEngine;
using System.Collections;

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
    public Transform[] m_SpawnPoints;
    [HideInInspector] public NetworkView NetView;
    [HideInInspector] public CPlayer[] m_Players = new CPlayer[6];
    public static int m_LocalID = -1;
    public static GameObject m_LocalObj;

    float m_NextPlayersUpdate = 0f;

    void Awake()
    {
        ins = this;
        NetView = GetComponent<NetworkView>();
        for (int i = 0; i < 6; i++)
            m_Players[i] = null;
        if (Network.isServer)
        {
            m_LocalID = 0;
            SubmitPlayer(0, Network.player);
            m_LocalObj = (GameObject)Network.Instantiate(CarPrefab, Vector3.down * 200, Quaternion.identity, 0);
            m_LocalObj.transform.position = m_SpawnPoints[m_LocalID].position;
            m_LocalObj.GetComponent<CCar>().m_Player = m_Players[m_LocalID];
            GameObject.Find("IPT").GetComponent<UnityEngine.UI.Text>().text = Network.player.ipAddress;
        }
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
    }

    void OnPlayerConnected(NetworkPlayer netPlayer)
    {
        int i = 0;
        for (; i < 6; i++)
            if (m_Players[i] == null)
                break;
        if (i == 0) // no slot
            return;
        SubmitPlayer(i, netPlayer);
        NetView.RPC("Server_Info", netPlayer, i);
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


    // RPCs
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
}
