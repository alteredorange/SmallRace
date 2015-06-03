using UnityEngine;
using System.Collections;

public class HostOptions : MonoBehaviour {
	//default to 6 players
	static int m_MaxRacers = 6;
	//assign random server name incase one isnt specified
	private string m_ServerName = "Server";

	static public int MaxRacers {
		get {
			return m_MaxRacers;
		}
	}

	public void Awake()
	{
		m_ServerName += Random.Range(0, 100000).ToString();
		Network.natFacilitatorIP = "104.236.179.57";
		Network.natFacilitatorPort = 50005;
		MasterServer.ipAddress = "104.236.179.57";
		MasterServer.port = 23466;
	}

	public void CreateServer()
	{
		string GameTypeName = "SmallRace" + m_MaxRacers.ToString() + "PlayerGameType";

		Network.InitializeServer(m_MaxRacers, Random.Range (20000, 30000), true);
		MasterServer.RegisterHost(GameTypeName, m_ServerName);
		Debug.Log("server: " + m_ServerName + " created");
		Debug.Log("server: " + m_ServerName + " registered");

		GameObject networkViewObject = GameObject.Find ("NetworkViewObject");
		if (networkViewObject != null)
			DontDestroyOnLoad (networkViewObject);
		Application.LoadLevel("OnlineLobby");
	}

	public void ReturnToOnlineMenu()
	{
		Application.LoadLevel("OnlineMenu");
	}

	public void ServerNameEndEdit(string name)
	{
		if(name != string.Empty)
			m_ServerName = name;
	}

	public void _2playersSelected(bool selected)
	{
		if (selected) {
			m_MaxRacers = 2;
		}
	}

	public void _4playersSelected(bool selected)
	{
		if (selected) {
			m_MaxRacers = 4;
		}
	}

	public void _6playersSelected(bool selected)
	{
		if (selected) {
			m_MaxRacers = 5;
		}
	}
}
