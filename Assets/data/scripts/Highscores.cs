using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using LitJson;
using System;
using System.Collections;
using TMPro;
using UnityEngine.Networking;

public class parseJSON
{
	public string score;
	public string name;
}

public class Highscores : MonoBehaviour
{
	public Highscore[] highscoreEntries;
	public Transform highscoreWrapper;
	public TextMeshProUGUI PersonalBest;
	public float timeBetweenChecks = 2f;
	public float lastCheck = 0;

	// Update is called once per frame
	void Update()
	{
		lastCheck += Time.deltaTime;
		if (lastCheck > timeBetweenChecks)
		{
			lastCheck = 0;
			StartCoroutine(getRequest());
		}
	}


	// Sample JSON for the following script has attached.
	void Start()
	{
		StartCoroutine(getRequest());
		int personalBestValue = PlayerPrefs.GetInt("PersonalBestValue");

		string personalBestValueCheck = PlayerPrefs.GetString("PersonalBestCheck");
		if (personalBestValueCheck == "true")
		{
			PersonalBest.text = $"{personalBestValue} gp";
		}
		else
		{
			PersonalBest.text = "-";
		}

		int i = 0;
		foreach (Highscore hs in highscoreEntries)
		{
			i++;
			hs._index = (i).ToString();
			hs._name = "";
			hs._score = "";
		}
	}

	private void ProcessJSON(string jsonString)
	{
		int personalBestValue = PlayerPrefs.GetInt("PersonalBestValue");

		string personalBestValueCheck = PlayerPrefs.GetString("PersonalBestCheck");
		if (personalBestValueCheck == "true")
		{
			PersonalBest.text = $"{personalBestValue} gp";
		}
		else
		{
			PersonalBest.text = "-";
		}

		int i = 0;
		foreach (Highscore hs in highscoreEntries)
		{
			i++;
			hs._index = (i).ToString();
			hs._name = "";
			hs._score = "";
		}

		
		i = 0;
		foreach (JsonData score in JsonMapper.ToObject(jsonString))
		{
			Highscore hs = highscoreEntries[i];
			hs._name = score["name"].ToString();
			hs._score = score["score"].ToString();
			i++;
		}
	}

	IEnumerator getRequest()
	{
		UnityWebRequest uwr = UnityWebRequest.Get($"https://scores.shanegadsby.com/get-scores?game={UnityWebRequest.EscapeURL("GMTK Game Jam 2023 - A merchants work")}");
		yield return uwr.SendWebRequest();

		if (uwr.isNetworkError) { }
		else
		{
			ProcessJSON(uwr.downloadHandler.text);
		}
	}
}