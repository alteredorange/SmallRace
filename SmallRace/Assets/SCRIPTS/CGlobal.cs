using UnityEngine;


public static class CGlobal
{
    public static string FormatTime (float time){
		float totalTime = time;
		//int hours = (int) (totalTime / 3600);
		int minutes = (int) (totalTime / 60) % 60;
		int seconds = (int)totalTime  % 60;
		float tenth = time * 10 % 10;
		string answer = minutes.ToString("0") + "." + seconds.ToString("00") + "." + tenth.ToString("0");
		return answer;
	}
}