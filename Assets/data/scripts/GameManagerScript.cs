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
		itemNames = JSON.Parse(Resources.Load<TextAsset>("itemNames").ToString());
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
		currentCustomer.RollPurchaseValue();
	}

	public void DeclineTrade()
	{
		//TODO: make customer leave a review
		currentCustomer.readyForDialogue = false;
		currentCustomer.ToggleAgent(true);
		currentCustomer = null;
	}
}