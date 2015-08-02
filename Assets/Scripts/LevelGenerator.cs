using UnityEngine;
using System.Collections;
using System;
using System.Collections.Generic;
using UnityEngine.UI;


public class LevelGenerator : MonoBehaviour {

	public int height = 3;
	public int width = 6;
	private int[,] grid;

	private float nextmovey = 1f;

	public GameObject block;
	private GameObject curBlock;
	private Color nextColor;

	private List<GameObject> gameobjects = new List<GameObject>();

	private float Score;

	private float multiplicator;
	private float multiplicator_coolDown;
	private float multiplicator_coolDown_Time;

	public Text scoreText;
	public Text multiplicatorText;
	public Text highScore;

	void createGrid(){
		grid = new int[width+1, height];
	}

	void setNextColor(){
		GameObject.FindGameObjectWithTag ("color").GetComponent<MeshRenderer> ().material.color = nextColor;
	}

	void UpdateView(){
		scoreText.text = Score.ToString ();
		multiplicatorText.text = "x" + multiplicator.ToString ();
	}


	// Use this for initialization
	void Start () {
		createGrid ();
		curBlock = createBlock (createColor());
		nextColor = createColor();
		setNextColor();

		Score = 0;
		multiplicator = 1;

		scoreText.text = Score.ToString ();
		multiplicatorText.text = "x" + multiplicator.ToString ();

		multiplicator_coolDown=0;
		multiplicator_coolDown_Time=3;
		float highscore = PlayerPrefs.GetFloat ("HighScore");
		if (highscore == null) {
			highScore.text = "0";
		} else {
			highScore.text = highscore.ToString();
		}

	
	}
	
	// Update is called once per frame
	void FixedUpdate () {
		moveBlock ();
		multiplicator_coolDown -= Time.deltaTime;
		multiplicator_coolDown = Mathf.Clamp (multiplicator_coolDown, 0, multiplicator_coolDown_Time);
		if (multiplicator_coolDown == 0) {
			multiplicator--;
			multiplicator_coolDown = multiplicator_coolDown_Time;
			if(multiplicator<1){
				multiplicator=1;
			}
		}
		UpdateView ();
	}

	void Update(){
		int curposy = Mathf.RoundToInt(curBlock.transform.position.y);
		int curposx = Mathf.RoundToInt(curBlock.transform.position.x);
		if (Input.GetKeyDown (KeyCode.D) && curposx<width-0.1 && (grid[curposx+1, curposy]==0)) {
			curBlock.transform.position = new Vector3(curposx+1, curposy, curBlock.transform.position.z);
		}
		if (Input.GetKeyDown (KeyCode.A) && curposx>0 && (grid[curposx-1, curposy]==0)) {
			curBlock.transform.position = new Vector3(curposx-1, curposy, curBlock.transform.position.z);
		}
		if ((Input.GetKey (KeyCode.S) && curposy>0) && (grid[curposx, curposy-1]==0)) {
			curBlock.transform.position = new Vector3(curposx, curposy-1, curBlock.transform.position.z);
		}
	}

	void moveBlock(){
		if (curBlock == null) {
			curBlock = createBlock (nextColor);
			nextColor = createColor();
			setNextColor();
		}

		int curposy = Mathf.RoundToInt(curBlock.transform.position.y);
		int curposx = Mathf.RoundToInt(curBlock.transform.position.x);
		if (nextmovey <= 0 && curposy > 0) {
			if(grid[curposx, curposy-1]==1){
				//final placing 
				if(curposy > height-1){
					gameOver();
					return;
				}
				grid[Mathf.RoundToInt(curBlock.transform.position.x), Mathf.RoundToInt(curBlock.transform.position.y)]=1;
				Debug.Log ("Starte LookingforPattern");
				List<GameObject> neighbours = lookforpattern(null, curBlock, null, true);
				Debug.Log (neighbours.Count);
				if(neighbours.Count>=4){
					DeleteBlocks(neighbours);
				}
				curBlock = createBlock (nextColor);
				nextColor = createColor();
				setNextColor();
			}else{
				curBlock.transform.position = new Vector3 (curposx, curposy - 1, curBlock.transform.position.z);
			}
			nextmovey = .5f;
		} else {
			if(curposy == 0){
				//final placing
				grid[curposx, curposy]=1;
				Debug.Log ("Starte LookingforPattern");
				List<GameObject> neighbours = lookforpattern(null, curBlock, null, true);
				Debug.Log (neighbours.Count);
				if(neighbours.Count>=4){
					DeleteBlocks(neighbours);
				}
				curBlock = createBlock (nextColor);
				nextColor = createColor();
				setNextColor();
			}
		}




		nextmovey -= Time.deltaTime;

	}

	void gameOver(){
		Debug.Log ("Gameover");
		PlayerPrefs.SetFloat ("CurScore", Score);
		if (Score > PlayerPrefs.GetFloat ("HighScore")) {
			PlayerPrefs.SetFloat ("HighScore", Score);
		}
		Application.LoadLevel (2);
	}

	Color createColor(){
		Color myColor;
		int help = new System.Random ().Next (0, 4);
		switch (help) {
		case 0: myColor = Color.red;
			break;
		case 1: myColor = Color.blue;
			break;
		case 2: myColor = Color.yellow;
			break;
		case 3: myColor = Color.green;
			break;
		default: myColor = Color.red;
			break;
		}
		return myColor;
	}

	GameObject createBlock(Color myColor){
		GameObject tempBlock = (GameObject) Instantiate (block, new Vector3 (UnityEngine.Random.Range(0,width), height, 0), Quaternion.identity);

		tempBlock.GetComponent<MeshRenderer> ().material.color  = myColor;
		gameobjects.Add (tempBlock);
		return tempBlock;
	}

	List<GameObject> lookforpattern(List<GameObject> allBlocks, GameObject target, GameObject parent , bool start){
		if (start) {
			allBlocks = new List<GameObject>();
			allBlocks.Add (target);
		}

		List<GameObject> blocks = scanForBlocks (target);
		Debug.Log ("Scan beendet");

		if (!start) {
			blocks.Remove(parent);
		}

		foreach (GameObject current in blocks) {
			allBlocks.Add(current);
		}

		/*if (allBlocks.Count >= 3) {
			return allBlocks;
		} else {*/
			foreach(GameObject next in blocks){

				allBlocks = lookforpattern(allBlocks, next, target, false);
			}
		//}
		if (allBlocks.Count >= 4) {
			return allBlocks;
		}

		return allBlocks;
	
	}

	void DeleteBlocks(List<GameObject> blocks){
		addScorePoints (blocks.Count);
		multiplicator_coolDown = multiplicator_coolDown_Time;
		UpdateView ();
		foreach (GameObject current in blocks) {
			int curx=Mathf.RoundToInt(current.transform.position.x);
			int cury=Mathf.RoundToInt(current.transform.position.y);
			grid[curx, cury]=0;
			gameobjects.Remove(current);
			Destroy(current);
		}
		fixGrid ();
	}

	void addScorePoints(int destroySize){
		Score += 10.0f*destroySize*multiplicator;
		switch (destroySize) {
		case 3:
		case 4:
			multiplicator++;
			break;
		case 5: 
		case 6:
			multiplicator += 2;
			break;
		case 7:
		case 8:
			multiplicator += 3;
			break;
		case 9:
		case 10:
			multiplicator += 5;
			break;
		}

	}

	List<GameObject> scanForBlocks(GameObject obj){
		List<GameObject> blocks = new List<GameObject>();
		Color curCol =  obj.GetComponent<MeshRenderer>().material.color;
		foreach (GameObject current in gameobjects) {
			Color currentColor = current.GetComponent<MeshRenderer>().material.color;

			if((current.transform.position.x == obj.transform.position.x && current.transform.position.y == obj.transform.position.y+1)){
				if(currentColor == curCol){
					blocks.Add(current);				
				}
			}
			if((current.transform.position.x == obj.transform.position.x -1 && current.transform.position.y == obj.transform.position.y)){
				if(currentColor == curCol){
					blocks.Add(current);
				}
			}
			if((current.transform.position.x == obj.transform.position.x +1 && current.transform.position.y == obj.transform.position.y)){
				if(currentColor == curCol){
					blocks.Add(current);
				}
			}
			if((current.transform.position.x == obj.transform.position.x && current.transform.position.y == obj.transform.position.y-1)){
				if(currentColor == curCol){
					blocks.Add(current);
				}
			}			
		}
		return blocks;
	}

	void fixGrid(){
		Debug.Log ("========================");
		Debug.Log ("FIX GRID");

		for (int curHeight = 1; curHeight <= height; curHeight++) { 
			Debug.Log ("Height: " + curHeight);
			foreach (GameObject myBlock in gameobjects) {
				if(myBlock != null){
				bool changedPos = false;
				int posx = Mathf.RoundToInt(myBlock.transform.position.x);
				int posy = Mathf.RoundToInt(myBlock.transform.position.y);
				if (posy == curHeight) {
					grid[posx, posy] = 0;

					Debug.Log ("Gameobject check");
					Debug.Log ("Cur X:" + posx);
					Debug.Log ("Cur Y:" + posy);

					for(int i=posy-1; i>= 0; i--){
						if(grid[posx, i] ==0){
							posy = i;
							changedPos = true;
						}
						else{
							i=-1;
						}
					}

					Debug.Log ("New X:" + posx);
					Debug.Log ("New Y:" + posy);

					myBlock.transform.position = new Vector3(posx, posy, myBlock.transform.position.z);
					grid[posx, posy] = 1;
					if(changedPos){
						Debug.Log("Look for Pattern");
						List<GameObject> neighbours = lookforpattern(null, myBlock, null, true);
						Debug.Log (neighbours.Count);
						if(neighbours.Count>=4){
							wait(0.25f);
							DeleteBlocks(neighbours);
						}
					}
				}
				}
			}
		}
		Debug.Log ("========================");
	}

	IEnumerator wait(float waitTime) {
		yield return new WaitForSeconds(waitTime);
	}

}
