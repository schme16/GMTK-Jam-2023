using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.AI;

public class NPCScript : MonoBehaviour
{
	public GameManagerScript gm;
	public float wanderDistance = 20;
	public Vector3 wanderTarget;
	NavMeshAgent agent;
	float waiting = 0;

	// Start is called before the first frame update
	void Start()
	{
		gm = FindObjectOfType<GameManagerScript>();
		agent = GetComponent<NavMeshAgent>();
		wanderTarget = Vector3.zero;
	}

	// Update is called once per frame
	void Update()
	{
		if (!agent.pathPending)
		{
			if (agent.remainingDistance <= (agent.stoppingDistance + 2))
			{
				if (!agent.hasPath || agent.velocity.sqrMagnitude == 0f)
				{
					//No current customer, roll the dice
					if (!gm.currentCustomer)
					{
						if (gm.rand.Range(0, 200) > 190)
						{
							gm.currentCustomer = this;
							agent.isStopped = false;
							Seek(new Vector3(gm.cartNPCPos.position.x, 0, gm.cartNPCPos.position.z));
						}
						else
						{
							agent.isStopped = false;
							Wander();
						}
					}
					else if (gm.currentCustomer != this)
					{
							agent.isStopped = false;
							Wander();
					}
					else
					{
						transform.LookAt(gm.player.transform);
						waiting += Time.deltaTime;
						agent.isStopped = true;
						if (waiting > 5)
						{
							gm.currentCustomer = null;
						}
						
					}
				}
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