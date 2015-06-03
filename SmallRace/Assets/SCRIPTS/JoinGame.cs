using UnityEngine;
using System.Collections;

public class JoinGame : MonoBehaviour {
	protected int m_Racers = 6;


	public void Awake()
	{
		Network.natFacilitatorIP = "104.236.179.57";
		Network.natFacilitatorPort = 50005;
		MasterServer.ipAddress = "104.236.179.57";
		MasterServer.port = 23466;

		MasterServer.ClearHostList();
	}

	public void ReturnToOnlineMenu()
	{
		Application.LoadLevel("OnlineMenu");
	}

	public void FindQuickMatch()
	{
		string GameTypeName = "SmallRace" + m_Racers.ToString() + "PlayerGameType";
		MasterServer.RequestHostList(GameTypeName);
	}

	public void OnMasterServerEvent(MasterServerEvent msEvent)
	{
		if (msEvent == MasterServerEvent.HostListReceived) {
			HostData[] hostData = MasterServer.PollHostList();
			Debug.Log(hostData.Length.ToString() + " servers found");

			foreach(HostData host in hostData)
			{
				//if there is room for players to join
				if(host.connectedPlayers < host.playerLimit)
				{
					NetworkConnectionError error = Network.Connect(host);
					Debug.Log("attempting to connect to " + host.gameName);

					//if we succeed in connecting, stop searching
					if(error == NetworkConnectionError.NoError)
					{
						Debug.Log("Successfully connected");
						GameObject networkViewObject = GameObject.Find ("NetworkViewObject");
						if (networkViewObject != null)
							DontDestroyOnLoad (networkViewObject);
						Application.LoadLevel("OnlineLobby");
						break;
					}
				}
			}
		}
	}

	public void _2playersSelected(bool selected)
	{
		if (selected) {
			m_Racers = 2;
		}
	}
	
	public void _4playersSelected(bool selected)
	{
		if (selected) {
			m_Racers = 4;
		}
	}
	
	public void _6playersSelected(bool selected)
	{
		if (selected) {
			m_Racers = 5;
		}
	}
}
