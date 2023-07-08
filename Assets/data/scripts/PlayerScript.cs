using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerScript : MonoBehaviour
{
	public GameManagerScript gm;

	public int gp;
	
	public int debt;
	// Start is called before the first frame update
	void Start()
	{
		gm = FindObjectOfType<GameManagerScript>();
		gp = gm.startingGold;
		debt = gm.startingDebt;
	}

	// Update is called once per frame
	void Update() { }
}