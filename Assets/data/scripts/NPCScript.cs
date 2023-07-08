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
	public bool selling;
	NavMeshAgent agent;
	NavMeshObstacle agentObstacle;
	float waiting = 0;

	// Start is called before the first frame update
	void Start()
	{
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
		
		ToggleAgent(true);
	}

	// Update is called once per frame
	void Update()
	{
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
								selling = gm.rand.Range(0, 2) > 0;

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
						else if (gm.currentCustomer == this)
						{
							//Face the player
							transform.LookAt(gm.player.transform);

							//Increase the avoidance priority while seeking the cart
							agent.avoidancePriority = 50;

							//Disable the agent
							ToggleAgent(false);
						}
					}
				}
			}
		}

		//TODO: Re-write this to show dialogue and choices, not just auto play
		if (agentObstacle.enabled)
		{
			waiting += Time.deltaTime;

			if (waiting > 5)
			{
				//Customer is selling their item
				if (selling && gm.player.gp > 0)
				{
					int purchaseValue = gm.rand.Range(1, 10);
					if (gm.player.gp <= purchaseValue)
					{
						purchaseValue = gm.player.gp;
					}
					gm.player.gp = Mathf.Max(0, gm.player.gp - purchaseValue);
					gp += purchaseValue;
					
					//TODO: make customer leave a review
					
				}
				
				//Customer is buying an item
				else if (!selling && gp > 0)
				{
					int purchaseValue = gm.rand.Range(1, 10);
					if (gp <= purchaseValue)
					{
						purchaseValue = gp;
					}
					gp = Mathf.Max(0, gp - purchaseValue);
					gm.player.gp += purchaseValue;
					
					//TODO: make customer leave a review
				}
				
				waiting = 0;
				gm.currentCustomer = null;
				ToggleAgent(true);
			}
		}

		if (this == gm.currentCustomer)
		{
			transform.LookAt(gm.player.transform);
		}
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

	public static Vector3 GetRandomPoint(Vector3 center, float maxDistance)
	{
		// Get Random Point inside Sphere which position is center, radius is maxDistance
		Vector3 randomPos = Random.insideUnitSphere * maxDistance + center;

		NavMeshHit hit; // NavMesh Sampling Info Container

		// from randomPos find a nearest point on NavMesh surface in range of maxDistance
		NavMesh.SamplePosition(randomPos, out hit, maxDistance, NavMesh.AllAreas);

		return hit.position;
	}
}