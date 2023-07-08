using System.Collections;
using System.Collections.Generic;
using data.scripts;
using TMPro;
using UnityEngine;
using Random = Unity.Mathematics.Random;

public class GameManagerScript : MonoBehaviour
{
	public PlayerScript player;
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
}