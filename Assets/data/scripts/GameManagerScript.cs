using System;
using System.Collections;
using System.Collections.Generic;
using data.scripts;
using SimpleJSON;
using TMPro;
using UnityEngine;
using Random = Unity.Mathematics.Random;

public class GameManagerScript : MonoBehaviour
{
	public PlayerScript player;
	public DialogueScript dialogueScript;
	public Camera camera;
	public Transform customerQueueObj;
	public Transform cartNPCPos;
	public TMP_Text goldUI;
	public TMP_Text debtUI;
	public Rand rand;
	public int RandomSeed;
	public int maxCustomers;
	public int startingDebt;
	public int startingGold;
	public int minNPCStartingGold;
	public int maxNPCStartingGold;
	public int minNPCHaggleDisposition;
	public int maxNPCHaggleDisposition;
	public int minNPCPersonality;
	public int maxNPCPersonality;
	public NPCScript currentCustomer;
	public bool canSpawnCustomers;
	public float minTimeToSpawnNextCustomer;
	public float maxTimeToSpawnNextCustomer;
	public JSONNode itemNames;
	public JSONNode itemFlourishes;
	public JSONNode npcFirstNames;
	public JSONNode npcMiddleNames;
	public JSONNode npcLastNames;
	public JSONNode openingDialogues;
	public JSONNode badReviews;
	public JSONNode goodReviews;
	public JSONNode snark;


	// Start is called before the first frame update
	void Start()
	{
		rand = new Rand(RandomSeed);
		camera = Camera.main;

		itemNames = JSON.Parse(Resources.Load<TextAsset>("itemNames").ToString());
		itemFlourishes = JSON.Parse(Resources.Load<TextAsset>("itemFlourishes").ToString());
		npcFirstNames = JSON.Parse(Resources.Load<TextAsset>("npcFirstNames").ToString());
		npcMiddleNames = JSON.Parse(Resources.Load<TextAsset>("npcMiddleNames").ToString());
		npcLastNames = JSON.Parse(Resources.Load<TextAsset>("npcLastNames").ToString());
		openingDialogues = JSON.Parse(Resources.Load<TextAsset>("openingDialogues").ToString());
		badReviews = JSON.Parse(Resources.Load<TextAsset>("badReviews").ToString());
		goodReviews = JSON.Parse(Resources.Load<TextAsset>("goodReviews").ToString());
		snark = JSON.Parse(Resources.Load<TextAsset>("snark").ToString());
	}

	// Update is called once per frame
	void Update()
	{
		goldUI.text = player.gp + "GP";
		debtUI.text = player.debt + "GP";
		canSpawnCustomers = customerQueueObj.childCount < maxCustomers;
	}

	public void AcceptTrade()
	{
		//Customer is selling their item
		if (currentCustomer.selling && player.gp > 0)
		{
			if (currentCustomer.gp <= currentCustomer.purchaseValue)
			{
				currentCustomer.purchaseValue = currentCustomer.gp;
			}

			player.gp = Mathf.Max(0, player.gp - currentCustomer.purchaseValue);
			currentCustomer.gp += currentCustomer.purchaseValue;

			//TODO: make customer leave a review
		}

		//Customer is buying an item
		else if (!currentCustomer.selling && currentCustomer.gp > 0)
		{
			if (currentCustomer.gp <= currentCustomer.purchaseValue)
			{
				currentCustomer.purchaseValue = currentCustomer.gp;
			}

			currentCustomer.gp = Mathf.Max(0, currentCustomer.gp - currentCustomer.purchaseValue);
			player.gp += currentCustomer.purchaseValue;

			//TODO: make customer leave a review
		}
		else
		{
			Debug.Log("Player broke?");
		}


		currentCustomer.readyForDialogue = false;
		currentCustomer.ToggleAgent(true);
		currentCustomer = null;
	}

	public void HaggleTrade()
	{
		//Pick a possibility
		int r = rand.Range(1, 101);

		//If the number is lower than the currentCustomer.personality the customer will storm off
		if (r < currentCustomer.personality)
		{
			currentCustomer.SaySnark();
			FinishTrade();
		}
		else
		{
			currentCustomer.RollPurchaseValue();
		}
	}

	public void DeclineTrade()
	{
		//Pick a possibility
		int r = rand.Range(1, 101);
		//If the number is lower than the currentCustomer.personality, you'll get a bad review
		if (r < currentCustomer.personality)
		{
			int score = 0;
			string review = badReviews[rand.Range(0, badReviews.Count)];
			score = (int)Math.Ceiling(currentCustomer.personality / 10f);

			//Leave a review
			LeaveReview(review, -score);
		}

		FinishTrade();
	}

	void FinishTrade()
	{
		//Reset the customer back to base state
		currentCustomer.Reset();
		
		//Clear the current customer
		currentCustomer = null;
		
		//Reset the dialogue ui back to base state
		dialogueScript.Reset();
	}

	public void StartTrading()
	{
		dialogueScript.TradingButtons.SetActive(true);
	} 
	
	void LeaveReview(string review, int score)
	{
		player.reviews.Add(review);
		player.score += score;
	}

	public bool RollDice(int percentageChance)
	{
		if (rand.Range(1, 101) <= percentageChance)
			return true;
		else
			return false;
	}
}