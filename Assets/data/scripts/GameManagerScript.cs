using System.Collections;
using System.Collections.Generic;
using data.scripts;
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


	// Start is called before the first frame update
	void Start()
	{
		rand = new Rand(RandomSeed);
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