using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerScript : MonoBehaviour
{
	public GameManagerScript gm;

	// Start is called before the first frame update
	void Start()
	{
		gm = FindObjectOfType<GameManagerScript>();
	}

	// Update is called once per frame
	void Update() { }
}