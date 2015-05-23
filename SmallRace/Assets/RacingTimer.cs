using UnityEngine;
using System.Collections;

public class RacingTimer : MonoBehaviour {

	float startTime = 0;
	float timeTaken;

	void Update () {
		timeTaken = startTime + Time.time;
		GameObject.Find("TIMER").GetComponent<UnityEngine.UI.Text>().text = (FormatTime(timeTaken));
	}


	private string FormatTime (float time){
		float totalTime = time;
		//int hours = (int) (totalTime / 3600);
		int minutes = (int) (totalTime / 60) % 60;
		int seconds = (int)totalTime  % 60;
		float tenth = time * 10 % 10;
		string answer = minutes.ToString("0") + "." + seconds.ToString("00") + "." + tenth.ToString("0");
		return answer;
	}

}