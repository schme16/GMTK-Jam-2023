using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerScript : MonoBehaviour
{
	public GameManagerScript gm;

	public int debt;
	public int gp;
	public int score;
	public List<string> reviews;
	
	// Start is called before the first frame update
	void Start()
	{
		Reset();
	}

	// Update is called once per frame
	void Update() { }

	public void Reset()
	{
		gp = gm.startingGold;
		debt = gm.startingDebt;
		score = 15;
		reviews.Clear();
	}
}