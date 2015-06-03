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
	private int m_SpawnedPlayers = 0;
    [HideInInspector] public NetworkView NetView;
    [HideInInspector] public CPlayer[] m_Players = new CPlayer[6];
    [HideInInspector] public CPlayerStat[] m_PlayerStats = new CPlayerStat[6];
	GameObject[] m_Cars;
	int m_myNumber;
    int m_StatCounter = 0;
    public static int m_LocalID = -1;
    public static GameObject m_LocalObj;

    void Awake()
    {
		if (Network.isServer) {
			m_Cars = new GameObject[HostOptions.MaxRacers];
		}

		NetView = GetComponent<NetworkView> ();

		//since we aren't using a dedicated server we need to instantiate a racer for the srever's client
		RequestInstatiation();
	}

	void RequestInstatiation()
	{
		NetView.RPC("HandleInstantiation", RPCMode.Server);
	}

	void ServerStartCountdown()
	{
		if (Network.isServer){
			return;
		}
	}

	// Client RPCs
	[RPC]
	void RecieveViewID(NetworkViewID Id, int RacerNumber)
	{
		//use network id to find gameobject for things like attaching the camera
		m_LocalObj = NetworkView.Find(Id).gameObject;
		m_myNumber = RacerNumber;
	}

	//Server RPCs
	[RPC]
	void HandleInstantiation(NetworkMessageInfo info)
	{
		if (Network.isServer) {
			//make sure there are available spawn points
			if(m_SpawnedPlayers < m_SpawnPoints.Length)
			{
				GameObject spawnPoint;
				GameObject newRacer;
				NetworkViewID viewID;

				//make sure the spawn point is populated
				if(m_SpawnPoints[m_SpawnedPlayers] == null)
				{
					Debug.LogError("Spawn points not set! Please use the editor to set CGameManager's spawnPoints.");
					return;
				}

				spawnPoint = m_SpawnPoints[m_SpawnedPlayers];
				newRacer = Network.Instantiate(CarPrefab, spawnPoint.transform.position, spawnPoint.transform.rotation);

				viewID = newRacer.GetComponent<NetworkView>().viewID;

				NetView.RPC("RecieveViewID", info.sender, viewID, m_SpawnedPlayers);

				m_SpawnedPlayers++;
			}
		}
	}

}
