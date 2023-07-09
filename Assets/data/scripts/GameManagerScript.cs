using System;
using System.Collections;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using data.scripts;
using SimpleJSON;
using TMPro;
using UnityEngine;
using UnityEngine.Networking;
using Random = Unity.Mathematics.Random;

public class GameManagerScript : MonoBehaviour
{
	public PlayerScript player;
	public DialogueScript dialogueScript;
	public Camera camera;
	public Transform customerQueueObj;
	public Transform cartNPCPos;
	public GameObject GameOverUI;
	public GameObject BankruptUI;
	public GameObject OutOfBusinessUI;
	public GameObject HighscoresUI;
	public TMP_Text goldUI;
	public TMP_Text debtUI;
	public TMP_Text debtLabelUI;
	public TMP_Text debtCounterUI;
	public TMP_Text scoreUI;
	public TMP_InputField playersNameUI;
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
	public int DebtRepaymentAmount;
	public int TimeBetweenPayments;
	public NPCScript currentCustomer;
	public bool canSpawnCustomers;
	public bool GameOver;
	public float minTimeToSpawnNextCustomer;
	public float maxTimeToSpawnNextCustomer;
	public float NextDebtPaymentCounter;
	public AudioClip dialogueOpenSFX;
	public AudioClip snarkSFX;
	public AudioClip successfulTransactionSFX;
	public AudioClip uiButtonClickSFX;
	public AudioClip sucessfulHaggleSFX;
	public AudioClip gameOverSFX;
	public AudioSource sfxSource;
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
		NextDebtPaymentCounter = TimeBetweenPayments;
		NewGame(true);
	}

	// Update is called once per frame
	void Update()
	{
		goldUI.text = player.gp + "GP";
		debtUI.text = player.debt + "GP";
		canSpawnCustomers = customerQueueObj.childCount < maxCustomers;


		//Update the customer satisfaction ui
		scoreUI.text = player.score switch
		{
			>= 30 => "Very positive",
			>= 25 => "Somewhat positive",
			>= 20 => "Positive",
			<= 0 => "Very Negative",
			<= 5 => "Negative",
			<= 10 => "Somewhat negative",
			_ => "Neutral"
		};


		//Lerp the colour
		scoreUI.color = Color.Lerp(Color.red, Color.green, (float)(player.score) / 30f);

		if (!GameOver && (currentCustomer == null || !currentCustomer.readyForDialogue))
		{
			NextDebtPaymentCounter -= Time.deltaTime;
			if (NextDebtPaymentCounter <= 0)
			{
				NextDebtPaymentCounter = TimeBetweenPayments;
				player.gp = Mathf.Max(0, player.gp - DebtRepaymentAmount);
				player.debt += DebtRepaymentAmount;
				if (player.debt > 0)
				{
					debtLabelUI.text = "Profit";
				}

				if (player.gp <= 0)
				{
					ShowGameOver();
				}
			}

			debtCounterUI.text = ((int)Mathf.Max(0, NextDebtPaymentCounter)).ToString();
		}
	}

	public void AcceptTrade()
	{
		//Customer is selling their item
		if (currentCustomer.selling)
		{
			//Add the gp to the players gp
			player.gp = Mathf.Max(0, player.gp - currentCustomer.purchaseValue);


			//Fetch a good review
			string review = goodReviews[rand.Range(0, goodReviews.Count)];


			//Leave a review
			LeaveReview(review, 1);
		}

		//Customer is buying an item
		else if (!currentCustomer.selling)
		{
			//Add the gp to the players gp
			player.gp += currentCustomer.purchaseValue;

			//Fetch a good review
			string review = goodReviews[rand.Range(0, goodReviews.Count)];

			//Leave a review
			LeaveReview(review, 1);
		}

		sfxSource.PlayOneShot(successfulTransactionSFX);
		FinishTrade();
	}

	public void HaggleTrade()
	{
		//Check to see how willing they are to haggle, then roll the dice to see if you pass the check
		bool haggleCheck = RollDice(currentCustomer.haggleDisposition);

		//Failed haggle check
		if (!haggleCheck)
		{
			//Spawn a snark box
			currentCustomer.SaySnark();
			sfxSource.PlayOneShot(snarkSFX);
			LeaveReview("", -2);

			//Finish up the trade cycle
			FinishTrade();
		}
		
		//Successful haggle check
		else
		{
			bool crit = RollDice(5);
			if (crit)
			{
				//Re-roll the price
				currentCustomer.purchaseValue = currentCustomer.RollPurchaseValue(currentCustomer.item.ItemValue - 1, currentCustomer.item.ItemValue + 15);
			}
			else
			{
				//Re-roll the price
				currentCustomer.purchaseValue = currentCustomer.RollPurchaseValue();
			}

			sfxSource.PlayOneShot(successfulTransactionSFX);
		}
	}

	public void DeclineTrade()
	{
		//Pick a possibility
		int r = rand.Range(1, 101);
		//If the number is lower than the currentCustomer.personality, you'll get a bad review
		if (r < currentCustomer.personality)
		{
			string review = badReviews[rand.Range(0, badReviews.Count)];
			//Leave a review
			LeaveReview(review, -1);
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

		if (player.gp <= 0)
		{
			GameOver = true;
			ShowGameOver();
		}

		if (player.score <= 0)
		{
			GameOver = true;
			ShowGameOver();
		}
	}

	public void StartTrading()
	{
		dialogueScript.OKButton.SetActive(false);
		dialogueScript.TradingButtons.SetActive(true);

		string intro = "";

		string haggleDisposition = currentCustomer.haggleDisposition switch
		{
			< 50 => "not very willing to haggle",
			< 75 => "somewhat willing to haggle",
			_ => "pretty willing to haggle"
		};
		if (currentCustomer.selling)
		{
			intro = $"I have here the finest <#3cc1ffff>{currentCustomer.item.ItemType}</color>!\n" +
					$"I'm looking to get {currentCustomer.purchaseValue} gp for it.\n\n" +
					$"<size=14>(Looks like they're {haggleDisposition})</size>";
		}
		else
		{
			intro = $"I'm looking to purchase your <#3cc1ffff>{currentCustomer.item.ItemType}</color>, how does {currentCustomer.purchaseValue} gp sound?.\n\n" +
					$"<size=14>(Looks like they're {haggleDisposition})</size>";
		}

		dialogueScript.DialogueText.text = intro;
	}

	void LeaveReview(string review, int score)
	{
		if (review != "")
		{
			player.reviews.Add(review);
		}

		player.score += score;
		player.score = Mathf.Min(30, Mathf.Max(0, player.score));
	}

	public bool RollDice(int percentageChance)
	{
		if (rand.Range(1, 101) <= percentageChance)
			return true;
		else
			return false;
	}

	async public void ShowGameOver()
	{
		sfxSource.PlayOneShot(gameOverSFX);

		GameOver = true;

		if (PlayerPrefs.GetString("PersonalBestCheck") == "true")
		{
			int personalBestValue = PlayerPrefs.GetInt("PersonalBestValue");
			if (player.debt > personalBestValue)
			{
				PlayerPrefs.SetInt("PersonalBestValue", player.debt);
			}
		}
		else
		{
			PlayerPrefs.SetInt("PersonalBestValue", player.debt);
		}

		PlayerPrefs.SetString("PersonalBestCheck", "true");


		if (currentCustomer != null)
		{
			currentCustomer.Wander();
			currentCustomer = null;
		}

		string name = PlayerPrefs.GetString("Player Name");
		if (name.Length > 0)
		{
			string url = $"https://scores.shanegadsby.com/post-score?game={UnityWebRequest.EscapeURL("GMTK Game Jam 2023 - A merchants work")}&score={UnityWebRequest.EscapeURL(player.debt.ToString())}&name={UnityWebRequest.EscapeURL(name)}";
			UnityWebRequest.Get(url).SendWebRequest();
		}

		//Reset the dialogue ui back to base state
		dialogueScript.Reset();

		//Show the GameOver UI
		GameOverUI.SetActive(true);
		if (player.gp <= 0)
		{
			OutOfBusinessUI.SetActive(false);
			BankruptUI.SetActive(true);
		}
		else
		{
			BankruptUI.SetActive(false);
			OutOfBusinessUI.SetActive(true);
		}
	}


	public void ShowHighscores()
	{
		//Show the GameOver UI
		GameOverUI.SetActive(false);
		HighscoresUI.SetActive(true);
	}

	public void NewGame(bool skipHideHighscoresOnFirstLoad = false)
	{
		playersNameUI.text = PlayerPrefs.GetString("Player Name");
		debtLabelUI.text = "Debt Remaining";

		//Reset the dialogue ui
		dialogueScript.Reset();

		//Hide the various UIs
		GameOverUI.SetActive(false);
		if (skipHideHighscoresOnFirstLoad)
		{
			GameOver = true;
			HighscoresUI.SetActive(true);
		}
		else
		{
			GameOver = false;
			HighscoresUI.SetActive(false);
		}

		player.Reset();

		//Clear the NPCs
		foreach (Transform child in customerQueueObj)
		{
			Destroy(child.gameObject);
		}
	}

	public void SavePlayerName()
	{
		PlayerPrefs.SetString("Player Name", playersNameUI.text);
	}

	public void playButtonClick()
	{
		sfxSource.PlayOneShot(uiButtonClickSFX);
	}
}