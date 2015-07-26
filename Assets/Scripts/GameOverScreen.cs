using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class GameOverScreen : MonoBehaviour {
	public Text highScore;
	public Text score;

	void Start(){
		highScore.text = "Highscore: " + PlayerPrefs.GetFloat ("HighScore").ToString();
		score.text = "Score: " + PlayerPrefs.GetFloat ("CurScore").ToString();
	}

	void back(){
		Application.LoadLevel (0);
	}
}
