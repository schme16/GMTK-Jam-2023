using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class DialogueScript : MonoBehaviour
{
	public GameManagerScript gm;
	public Camera DialogueCamera;
	public Transform PlayerPortraitParent;
	public Transform NPCPortraitParent;
	public TMP_Text DialogueText;
	public TMP_Text BuyingIndicator;
	public TMP_Text PriceIndicator;
	public TMP_Text CustomerName;
	public Transform HaggleButton;
	public Transform AcceptButton;
	public Transform DeclineButton;
	public GameObject OKButton;
	public GameObject DialogueBox;
	public GameObject TradingButtons;
	public bool playerSpeaking;

	// Start is called before the first frame update
	void Start()
	{
		Reset();
	}

	// Update is called once per frame
	void Update()
	{
		if (gm.currentCustomer != null && gm.currentCustomer.readyForDialogue)
		{
			DialogueBox.SetActive(true);

			//Set the dialogue scripts customer name
			CustomerName.text = gm.currentCustomer.NPCName;

	
			BuyingIndicator.text = !gm.currentCustomer.selling ? "Buying" : "Selling";
			gm.dialogueScript.PriceIndicator.text = "GP: " + gm.currentCustomer.purchaseValue;
		}
		else
		{
			DialogueBox.SetActive(false);
		}
	}

	public void Reset()
	{
		TradingButtons.SetActive(false);
		OKButton.SetActive(true);
	}
}