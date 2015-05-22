using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class MenuManager : MonoBehaviour {

    string ipToConnect = "192.168.1.45";
    public InputField ipInput;

    void Awake()
    {
        ipInput.onValueChange.AddListener(OnIPChange);
    }

    public void OnHostClick()
    {
        Network.InitializeSecurity();
        Network.InitializeServer(6, 8303, false);
    }
    public void OnLocalJoinClick()
    {
        Network.Connect(ipToConnect, 8303);
    }

    public void OnIPChange(string IP)
    {
        ipToConnect = IP;
    }

    void OnServerInitialized()
    {
        Network.isMessageQueueRunning = false;
        CContext.Context.ChangeLevel("Track4");
    }

    void OnConnectedToServer()
    {
        Network.isMessageQueueRunning = false;
        CContext.Context.ChangeLevel("Track4");
    }

}
