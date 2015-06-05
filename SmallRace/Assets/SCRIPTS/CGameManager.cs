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
	public GameObject InputPrefab;
	[HideInInspector]public InputSerializer[] m_SerializedInputs = null;
	float m_NextPlayersUpdate = 0f;

    void Awake()
    {
		ins = this;
		OnLap(0);

		if (Network.isServer) {
			m_Cars = new GameObject[HostOptions.MaxRacers];
			m_SerializedInputs = new InputSerializer[HostOptions.MaxRacers];
		}

		NetView = gameObject.GetComponent<NetworkView> ();

		//since we aren't using a dedicated server we need to instantiate a racer for the srever's client
		RequestInstatiation();
	}

	void RequestInstatiation()
	{
		if (Network.isClient) {
			NetView.RPC ("HandleInstantiation", RPCMode.Server);
		} else {
			NetworkViewID viewID = NetworkViewID.unassigned;
			int RacerNumber;
			
			RacerNumber = SpawnPlayer(out viewID);

			RecieveViewID(viewID, RacerNumber);
		}
	}

	int SpawnPlayer(out NetworkViewID viewID)
	{
		int retval = -1;
		viewID = NetworkViewID.unassigned;

		if (Network.isServer) {
			//make sure there are available spawn points
			if(m_SpawnedPlayers < m_SpawnPoints.Length)
			{
				Transform spawnPoint;
				GameObject newRacer;
				
				//make sure the spawn point is populated
				if(m_SpawnPoints[m_SpawnedPlayers] == null)
				{
					Debug.LogError("Spawn points not set! Please use the editor to set CGameManager's spawnPoints.");
					return retval;
				}
				
				spawnPoint = m_SpawnPoints[m_SpawnedPlayers];
				newRacer = (GameObject)Network.Instantiate(CarPrefab, spawnPoint.position, spawnPoint.rotation, 0);
				m_Cars[m_SpawnedPlayers] = newRacer;
				
				viewID = newRacer.GetComponent<NetworkView>().viewID;

				retval = m_SpawnedPlayers;
				m_SpawnedPlayers++;
			}
		}
		return retval;
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
		CCar car;
		//use network id to find gameobject for things like attaching the camera
		m_LocalObj = NetworkView.Find(Id).gameObject;
		m_myNumber = RacerNumber;

	    car = m_LocalObj.GetComponent<CCar> ();
		car.UpdateCamera = true;
		car.InputSerializer = ((GameObject)Network.Instantiate(InputPrefab, Vector3.zero, Quaternion.identity, 0)).GetComponent<InputSerializer>();
		car.InputSerializer.Initialize(m_myNumber);
	}

	//Server RPCs
	[RPC]
	void HandleInstantiation(NetworkMessageInfo info)
	{
		if (Network.isServer) {
			NetworkViewID viewID = NetworkViewID.unassigned;
			int RacerNumber;

			RacerNumber = SpawnPlayer(out viewID);

			NetView.RPC("RecieveViewID", info.sender, viewID, RacerNumber);
		}
	}

	public void Update()
	{
		//update all the cars
		if (Network.isServer) {
			for (int i = 0; i < m_Cars.Length; ++i) {
				if (m_SerializedInputs [i] != null) {
					int racer = m_SerializedInputs [i].RacerNumber;

					if (m_Cars [racer] != null) {
						CCar car = m_Cars [racer].GetComponent<CCar> ();
						if (car != null && car.RigidBody != null) {
							car.RigidBody.AddForce (m_SerializedInputs [i].ForwardForce * 0.25f);
							car.RigidBody.AddRelativeTorque (m_SerializedInputs [i].TurningForce * 0.25f);
						}
					}
				}
			}
		}
	}

	//original code
	public void OnLap(int Lap)
	{
		LAP_COUNTER.text = Lap + " / " + m_LapsNeeded;
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
	void OnApplicationQuit()
	{
		if (Network.isServer)
			MasterServer.UnregisterHost();
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
