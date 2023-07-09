using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UIElements;
using Random = UnityEngine.Random;

public class NPCScript : MonoBehaviour
{
	public GameManagerScript gm;
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
	public string NPCMiddleName;
	public string NPCName;
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
							if (gm.rand.Range(0, 200) > 190 && gp > 0)
							{
								//Make this agent the customer
								gm.currentCustomer = this;

								//Increase the avoidance priority while seeking the cart
								agent.avoidancePriority = 10;

								//Seek the cart
								Seek(new Vector3(gm.cartNPCPos.position.x, transform.position.y, gm.cartNPCPos.position.z));
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

							readyForDialogue = true;
							
							//Face the player
							transform.LookAt(gm.player.transform);

							//Increase the avoidance priority while seeking the cart
							agent.avoidancePriority = 50;


							//Determine if the customer is buying or selling
							Debug.Log(gm.rand == null);
							selling = gm.rand.Range(0, 2) > 0;

							//Roll a price
							RollPurchaseValue();

								
							//Disable the agent
							ToggleAgent(false);
						}
					}
				}
			}
		}

		//TODO: Re-write this to show dialogue and choices, not just auto play
		if (agentObstacle.enabled) { }

		if (this == gm.currentCustomer)
		{
			transform.LookAt(gm.player.transform);
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

		//TODO: make a name generator
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

	public void RollPurchaseValue()
	{
		//TODO: Make more dynamic pricing
		purchaseValue = gm.rand.Range(1, 10);
		gm.dialogueScript.PriceIndicator.text = "GP: " + purchaseValue;

	}
}