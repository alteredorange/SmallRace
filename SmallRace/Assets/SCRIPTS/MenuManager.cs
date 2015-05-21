using UnityEngine;
using System.Collections;

public class MenuManager : MonoBehaviour {

    public void OnHostClick()
    {
        Network.InitializeSecurity();
        Network.InitializeServer(6, 8303, false);
    }
    public void OnLocalJoinClick()
    {
        Network.Connect("127.0.0.1", 8303);
    }

    void OnServerInitialized()
    {
        Network.isMessageQueueRunning = false;
        CContext.Context.ChangeLevel("SimpleMap");
    }

    void OnConnectedToServer()
    {
        Network.isMessageQueueRunning = false;
        CContext.Context.ChangeLevel("SimpleMap");
    }

}
