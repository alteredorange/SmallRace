using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class MenuManager : MonoBehaviour
	
{
	
	string ipToConnect = "104.236.179.57";
	
	public Image TwoPlayers;
	public Image FourPlayers;
	public Image SixPlayers;
	HostData[] Hosts;
	public string gamerNamer;

	public int PIN;

	private int roomCounter = 0;
	private int masterEventCounter = 0;
	
	bool createHost = false;
	
	GUIStyle largeFont;
	
	void Start () 
	{

		HostData[] hostData = MasterServer.PollHostList();
		foreach (HostData element in hostData) {
			Debug.Log(element.connectedPlayers + " are connected to " + element.gameName);
			Debug.Log (element.gameName + " max spots: " + element.playerLimit);
		}


		
		largeFont = new GUIStyle();
		
		
		largeFont.fontSize = 25;
		largeFont.normal.textColor = Color.white;
	}
	
	void Awake() {



		MasterServer.ipAddress = "104.236.179.57";
		MasterServer.port = 23466;
		Network.natFacilitatorIP = "104.236.179.57";
		Network.natFacilitatorPort = 50005;
		MasterServer.RequestHostList("RaceScene");
	//	Hosts = MasterServer.PollHostList();
	//	windowFunction (1);
	}



	void Update()
	{




		if (MasterServer.PollHostList().Length != 0)
		{
			HostData[] hostData = MasterServer.PollHostList();
			for (int i = 0; i < hostData.Length; i++)
			{
				roomCounter++;
			}
			//	MasterServer.ClearHostList();
		}
		

	Hosts = MasterServer.PollHostList();
	}
	
	IEnumerator QuickMatch()
	{

	
		const int MaxTries = 20;
		for (int i = 0; i < MaxTries; i++) {
			yield return new WaitForSeconds (0.15f);
			if (Hosts.Length > 0)
				Debug.Log ("hi3");
			Debug.Log (Hosts.Length);	
			break;
		}
		if (Hosts.Length > 0) { 
			// found some matches
			Debug.Log ("Found Some Games");

			foreach (HostData host in Hosts) {
		
				//	
				Debug.Log ("Game Here");
		
				Network.Connect (host);
				//Will not connect without this waitforseconds! (happens to fast)
				yield return new WaitForSeconds (0.75f);

			}
	
		}
			

		int port = Random.Range (20000, 30000);
		int randmRoom = Random.Range (10000, 19999);
		bool useNat = true;
		string gameRoomName = randmRoom.ToString ();
		Network.maxConnections = 500;
		Network.InitializeSecurity ();
		Network.InitializeServer (1, port, useNat);
		MasterServer.RegisterHost ("RaceScene", gameRoomName, "hi");
	}


	public void OnGUI()
	{
		

		if (GUILayout.Button ("Refresh")) {
			MasterServer.RequestHostList ("RaceScene");
		}
		
		GUILayout.BeginHorizontal ();
		GUILayout.Box ("Server Name");
		GUILayout.EndHorizontal ();
		
		if (MasterServer.PollHostList ().Length != 0) {
			HostData[] data = MasterServer.PollHostList ();
			foreach (HostData balls in data) {
				GUILayout.BeginHorizontal ();
				GUILayout.Box (balls.gameName);
				if (GUILayout.Button ("connect")) {
					Network.Connect (balls);
				}
				GUILayout.EndHorizontal ();
			}
		}




//		GameObject.Find("CONT").GetComponent<UnityEngine.UI.Text>().text = "Counter: " + masterEventCounter;
		GameObject.Find("ROOMS").GetComponent<UnityEngine.UI.Text>().text = "Rooms: " + MasterServer.PollHostList().Length.ToString();
	}
	
	public void ListRefresh() {
		MasterServer.RequestHostList ("RaceScene");
	}
	
	public void OnMasterServerEvent(MasterServerEvent msEvent) 
	{
		if (msEvent == MasterServerEvent.HostListReceived)
		{  
			masterEventCounter++;
		}
	}
	
	
	public void OnQuickMatchClick()
	{
		MasterServer.UnregisterHost();
		StopAllCoroutines();
		MasterServer.ClearHostList();
		MasterServer.RequestHostList("RaceScene");
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
		Debug.Log("Is client? " + Network.isClient);
		Network.isMessageQueueRunning = false;
		CContext.Context.ChangeLevel("Track1");
	}
}