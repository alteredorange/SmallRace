using UnityEngine;
using System.Collections;

public class CContext : MonoBehaviour {

    public static CContext Context = null;
    GameObject m_LoadingScreen;

    void Awake()
    {
        if (Context == null)
            Context = this;
        else
        {
            Destroy(gameObject);
            return;
        }

        DontDestroyOnLoad(gameObject);
        OnLevelLoad();
    }

    void OnLevelLoad()
    {
        CheckCamera(Camera.main);
        m_LoadingScreen = GameObject.Find("LoadingScreen");
        Network.isMessageQueueRunning = true;
    }


    IEnumerator ChangeLevelIE(string name)
    {
        m_LoadingScreen.transform.FindChild("Graphics").gameObject.SetActive(true);
        yield return new WaitForSeconds(.3f);
        Application.LoadLevel(name);
    }
    public void ChangeLevel(string name)
    {
        StopAllCoroutines();
        StartCoroutine(ChangeLevelIE(name));
    }

    public void CheckCamera(Camera camera)
    {
        float targetaspect = 16.0f / 10.0f;
        float windowaspect = (float)Screen.width / (float)Screen.height;
        float scaleheight = windowaspect / targetaspect;
        if (scaleheight < 1.0f)
        {
            Rect rect = camera.rect;
            rect.width = 1.0f;
            rect.height = scaleheight;
            rect.x = 0;
            rect.y = (1.0f - scaleheight) / 2.0f;
            camera.rect = rect;
        }
        else
        {
            float scalewidth = 1.0f / scaleheight;
            Rect rect = camera.rect;
            rect.width = scalewidth;
            rect.height = 1.0f;
            rect.x = (1.0f - scalewidth) / 2.0f;
            rect.y = 0;
            camera.rect = rect;
        }
    }

    void OnLevelWasLoaded()
    {
        OnLevelLoad();
    }

}
