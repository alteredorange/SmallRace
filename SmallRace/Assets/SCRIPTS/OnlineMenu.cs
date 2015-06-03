using UnityEngine;
using System.Collections;

public class OnlineMenu : MonoBehaviour {

	public void GoToHostOptions()
	{
		GameObject networkViewObject = GameObject.Find ("NetworkViewObject");
		if(networkViewObject != null)
			DontDestroyOnLoad(networkViewObject);
		Application.LoadLevel("HostOptions");
	}

	public void GoToJoinGame()
	{
		GameObject networkViewObject = GameObject.Find ("NetworkViewObject");
		if (networkViewObject != null)
			DontDestroyOnLoad (networkViewObject);
		Application.LoadLevel("JoinGame");
	}
}
  