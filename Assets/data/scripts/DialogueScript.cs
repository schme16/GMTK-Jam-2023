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
	public GameObject DialogueBox;
	public bool playerSpeaking;

	// Start is called before the first frame update
	void Start() { }

	// Update is called once per frame
	void Update()
	{
		if (gm.currentCustomer != null && gm.currentCustomer.readyForDialogue)
		{
			DialogueBox.SetActive(true);

			//Set the dialogue scripts customer name
			CustomerName.text = gm.currentCustomer.NPCName;

			//TODO: make flavour text generator
			DialogueText.text = "Yeah... Hi?";
			BuyingIndicator.text = !gm.currentCustomer.selling ? "Buying" : "Selling";
		}
		else
		{
			DialogueBox.SetActive(false);
		}
	}
}