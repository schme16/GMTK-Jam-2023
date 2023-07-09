using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class Highscore : MonoBehaviour {
	public string _name;
	public string _score;
	public string _index;
	public TMP_Text NameUI;
	public TMP_Text ValueUI;

	// Start is called before the first frame update
	void Start() {
	}

	// Update is called once per frame
	void Update()
	{
		NameUI.text = $"{_index}. {_name}";
		ValueUI.text =  $"{_score} {(_score.Length > 0 ? "gp" : "")}";
	}
}