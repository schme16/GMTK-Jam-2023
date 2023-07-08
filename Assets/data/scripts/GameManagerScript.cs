using System.Collections;
using System.Collections.Generic;
using data.scripts;
using UnityEngine;
using Random = Unity.Mathematics.Random;

public class GameManagerScript : MonoBehaviour
{
	public PlayerScript player;
	public Transform customerQueueObj;
	public Transform cartNPCPos;
	public Rand rand;
	public int RandomSeed;
	public int maxCustomers;
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
		canSpawnCustomers = customerQueueObj.childCount < maxCustomers;
	}
}