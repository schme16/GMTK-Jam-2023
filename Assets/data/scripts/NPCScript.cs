using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using Cysharp.Threading.Tasks;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UIElements;
using Random = UnityEngine.Random;

public class NPCScript : MonoBehaviour
{
	public GameManagerScript gm;
	public GameObject SnarkPrefab;
	public float wanderDistance = 20;
	public int gp;
	public int haggleDisposition;
	public int personality;
	public int purchaseValue;
	public GameObject[] accessories;
	public bool selling;
	public bool setUp;
	public bool readyForDialogue;
	public string NPCFirstName;
	public string NPCLastName;
	public SaleItem item;
	public string NPCMiddleName;
	public string NPCName;
	public string openingDialogue;
	NavMeshAgent agent;
	NavMeshObstacle agentObstacle;

	// Start is called before the first frame update
	void Start()
	{
		CreateNPC();
	}

	// Update is called once per frame
	void Update()
	{
		if (!setUp)
		{
			return;
		}

		//Agent is active
		if (agent.enabled)
		{
			//No agent paths pending 
			if (!agent.pathPending)
			{
				//Agent distance to target <= stopping distance + buffer
				if (agent.remainingDistance <= (agent.stoppingDistance + 2))
				{
					//If the agent has no path, and has slowed to a stop
					if (!agent.hasPath || agent.velocity.sqrMagnitude == 0f)
					{
						//No current customer
						if (!gm.currentCustomer)
						{
							//Roll the dice, if 190 or higher
							if (gm.RollDice(5) && gp > 0)
							{
								if (!gm.GameOver)
								{
									//Make this agent the customer
									gm.currentCustomer = this;

									//Increase the avoidance priority while seeking the cart
									agent.avoidancePriority = 10;

									//Seek the cart
									Seek(new Vector3(gm.cartNPCPos.position.x, transform.position.y, gm.cartNPCPos.position.z));
								}
							}

							//189 or less
							else
							{
								//Find a new wander target
								Wander();
							}
						}

						//If the current customer is not this agent
						else if (gm.currentCustomer != this)
						{
							//Make the agent find a new wander target
							Wander();
						}

						//Current customer is this agent
						//This is what triggers when the agent enters dialogue mode
						else if (gm.currentCustomer == this && !readyForDialogue)
						{
							//Marks the 
							readyForDialogue = true;


							//Increase the avoidance priority while seeking the cart
							agent.avoidancePriority = 50;

							StartDialogue();
						}
					}
				}
			}
		}

		//TODO: Re-write this to show dialogue and choices, not just auto play
		if (agentObstacle.enabled) { }

		if (this == gm.currentCustomer)
		{
			//Face the player
			transform.LookAt(gm.player.transform);
			transform.eulerAngles = new Vector3(0, transform.eulerAngles.y, 0);
		}
	}

	public static Vector3 GetRandomPoint(Vector3 center, float maxDistance)
	{
		// Get Random Point inside Sphere which position is center, radius is maxDistance
		Vector3 randomPos = Random.insideUnitSphere * maxDistance + center;

		NavMeshHit hit; // NavMesh Sampling Info Container

		// from randomPos find a nearest point on NavMesh surface in range of maxDistance
		NavMesh.SamplePosition(randomPos, out hit, maxDistance, NavMesh.AllAreas);

		return hit.position;
	}

	void Wander()
	{
		var targetWorld = GetRandomPoint(transform.position, wanderDistance);
		Seek(targetWorld);
	}

	void Seek(Vector3 location)
	{
		agent.SetDestination(location);
	}

	async public void ToggleAgent(bool state)
	{
		agentObstacle.enabled = false;
		agent.enabled = false;
		await UniTask.Delay((int)(Time.deltaTime * 1000));
		agentObstacle.enabled = !state;
		await UniTask.Delay((int)(Time.deltaTime * 1000));
		agent.enabled = state;
	}

	async private void CreateNPC()
	{
		await UniTask.Delay((int)(Time.deltaTime * 1000));

		//Get the game manager
		gm = FindObjectOfType<GameManagerScript>();

		//Get the agent
		agent = GetComponent<NavMeshAgent>();

		//Get the agent obstacle
		agentObstacle = GetComponent<NavMeshObstacle>();

		//determine a random amount of gold
		gp = gm.rand.Range(gm.minNPCStartingGold, gm.maxNPCStartingGold + 1);

		//TODO: Make this affect stuff
		//Determine a random disposition
		//Higher numbers mean more likely to haggle when selling
		//and less likely to accept a haggle on sale
		haggleDisposition = gm.rand.Range(gm.minNPCHaggleDisposition, gm.maxNPCHaggleDisposition + 1);

		//TODO: Make this affect stuff
		//Determine a random personality value
		//Higher numbers mean more likely to leave good reviews
		personality = gm.rand.Range(gm.minNPCPersonality, gm.maxNPCPersonality + 1);

		foreach (var accessory in accessories)
		{
			accessory.SetActive(gm.rand.Range(0, 11) > 8);
		}

		ToggleAgent(true);


		//Create the NPC's name
		int numberOfNames = gm.rand.Range(2, 4);
		if (numberOfNames == 2)
		{
			NPCFirstName = gm.npcFirstNames[gm.rand.Range(0, gm.npcFirstNames.Count)];
			NPCLastName = gm.npcLastNames[gm.rand.Range(0, gm.npcLastNames.Count)];
			NPCName = NPCFirstName + " " + NPCLastName;
		}
		else
		{
			NPCFirstName = gm.npcFirstNames[gm.rand.Range(0, gm.npcFirstNames.Count)];
			NPCLastName = gm.npcLastNames[gm.rand.Range(0, gm.npcLastNames.Count)];
			NPCMiddleName = gm.npcMiddleNames[gm.rand.Range(0, gm.npcMiddleNames.Count)];
			NPCName = NPCFirstName + " " + NPCMiddleName + " " + NPCLastName;
		}


		setUp = true;
	}

	public int RollPurchaseValue(int min = 1, int max = 10)
	{
		//TODO: Make more dynamic pricing
		return gm.rand.Range(min, max + 1);
	}

	public void Reset()
	{
		readyForDialogue = false;
		item = null;
		selling = false;
		openingDialogue = "";
		ToggleAgent(true);
	}

	public SaleItem CreateItem()
	{
		//Create a new item
		SaleItem item = new SaleItem();

		//Pick a random item type
		item.ItemType = gm.itemNames[gm.rand.Range(0, gm.itemNames.Count)];

		//Determine if it's special
		bool special = gm.RollDice(10);

		//Special items get a flourish, and are have higher odds of higher prices
		if (special)
		{
			//Pick a flourish
			item.ItemFlourish = gm.itemFlourishes[gm.rand.Range(0, gm.itemFlourishes.Count)];

			//Roll for the item value
			item.ItemValue = RollPurchaseValue(5, 20);
		}
		else
		{
			//Roll for the item value
			item.ItemValue = RollPurchaseValue(1, 10);
		}

		//Hand back the item
		return item;
	}

	public void SaySnark()
	{
		string snarkText = gm.snark[gm.rand.Range(0, gm.snark.Count)];
		SnarkScript snark = Instantiate(SnarkPrefab).GetComponent<SnarkScript>();
		snark.transform.parent = transform;
		snark.transform.localPosition = Vector3.zero;
		snark.SnarkText.text = snarkText;
	}

	public void StartDialogue()
	{
		Debug.Log(1111);
		
		//Show the portrait container
		gm.dialogueScript.NPCPortraitParent.SetActive(true);

		//Clear the NPC portrait container
		foreach (Transform child in gm.dialogueScript.NPCPortraitParent.transform)
		{
			GameObject.Destroy(child.gameObject);
		}


		//Clone the customer into the portrait container
		GameObject customerPortrait = Instantiate(gameObject);

		//Disable unneeded the scripts
		DestroyImmediate(customerPortrait.GetComponent<NavMeshAgent>());
		DestroyImmediate(customerPortrait.GetComponent<NavMeshObstacle>());
		DestroyImmediate(customerPortrait.GetComponent<NPCScript>());

		//Add the portrait clone to the portrait container
		customerPortrait.transform.SetParent(gm.dialogueScript.NPCPortraitParent.transform);

		//Set the position to 0,0,0
		customerPortrait.transform.localPosition = Vector3.zero;
		customerPortrait.transform.localEulerAngles = Vector3.zero;


		//Determine if the customer is buying or selling
		selling = gm.RollDice(50);

		//Create and item to buy/sell
		item = CreateItem();

		//Sync the opening price to the price of the object +- a few gp depending on if buying or selling
		//Clamp it to 1 gp min
		//TODO: Make some jitter in the value
		purchaseValue = Math.Max(1, selling ? item.ItemValue + 2 : item.ItemValue - 2);

		int dialogueIndex = gm.rand.Range(0, gm.openingDialogues.Count);
		
		//Get an intro text
		openingDialogue = gm.openingDialogues[dialogueIndex];
		
		//Set the opening dialogue
		gm.dialogueScript.DialogueText.text = openingDialogue;

		//Disable the agent
		ToggleAgent(false);
	}
}

public class SaleItem
{
	public string ItemType;
	public string ItemFlourish;
	public int ItemValue;
}