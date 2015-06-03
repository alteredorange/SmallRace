using UnityEngine; 
using System.Collections;

public class OnlineLobby : MonoBehaviour {
	int m_lastLevelPrefix;
	int m_PlayersConnected = 1;
	NetworkView m_networkView;

	public void Awake()
	{
		GameObject networkViewObject = GameObject.Find ("NetworkViewObject");
		if (networkViewObject != null) {
			m_networkView = networkViewObject.GetComponent<NetworkView> ();
			//if(Network.peerType == NetworkPeerType.Client || Network.peerType == NetworkPeerType.Connecting)
			//	m_networkView.viewID = Network.AllocateViewID();
		}
		else
			AddNetworkView();
	}
	
	[RPC]
	private void AddNetworkView()
	{
		gameObject.AddComponent<NetworkView>();
		gameObject.GetComponent<NetworkView>().observed = null;
		gameObject.GetComponent<NetworkView>().stateSynchronization = NetworkStateSynchronization.Off;
		gameObject.GetComponent<NetworkView>().viewID = Network.AllocateViewID();
		m_networkView = GetComponent<NetworkView>().GetComponent<NetworkView>();

	}
	
	public void OnPlayerConnected(NetworkPlayer player)
	{
		if (Network.peerType == NetworkPeerType.Server) {
			m_PlayersConnected++;
			Debug.Log(m_PlayersConnected.ToString() + "Players connected");

			if (m_PlayersConnected == HostOptions.MaxRacers) {
				GameObject networkViewObject = GameObject.Find ("NetworkViewObject");
				if (networkViewObject != null)
					DontDestroyOnLoad (networkViewObject);
				m_networkView.RPC ("LoadLevel", RPCMode.AllBuffered, "Track1", m_lastLevelPrefix);
				Debug.Log("Max players reached, starting race");
			}
		}
	}

	public void OnPlayerDisconnected(NetworkPlayer player)
	{
		if (Network.peerType == NetworkPeerType.Server) {
			m_PlayersConnected--;
			Debug.Log("Player disconnected");
		}
	}

	[RPC]
	void LoadLevel (string level, int levelPrefix)
	{
		m_lastLevelPrefix = levelPrefix;
		
		// There is no reason to send any more data over the network on the default channel,
		// because we are about to load the level, thus all those objects will get deleted anyway
		Network.SetSendingEnabled(0, false);    
		
		// We need to stop receiving because first the level must be loaded first.
		// Once the level is loaded, rpc's and other state update attached to objects in the level are allowed to fire
		Network.isMessageQueueRunning = false;
		
		// All network views loaded from a level will get a prefix into their NetworkViewID.
		// This will prevent old updates from clients leaking into a newly created scene.
		Network.SetLevelPrefix(levelPrefix);
		Application.LoadLevel(level);
		//yield return;
		
		// Allow receiving data again
		Network.isMessageQueueRunning = true;
		// Now the level has been loaded and we can start sending out data to clients
		Network.SetSendingEnabled(0, true);
	}
}
