using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NPCSpawnerScript : MonoBehaviour
{
	public GameObject[] customerPrefabs;
	public GameManagerScript gm;
	public float spawnNextCustomer;
	public float timeToSpawnNextCustomer;

	// Start is called before the first frame update
	void Start()
	{
		timeToSpawnNextCustomer = 0;
	}

	// Update is called once per frame
	void Update()
	{
		if (gm.canSpawnCustomers)
		{
			spawnNextCustomer += Time.deltaTime;
			if (spawnNextCustomer > timeToSpawnNextCustomer)
			{
				int prefabInt = gm.rand.Range(0, customerPrefabs.Length);
				SpawnCustomer(customerPrefabs[prefabInt]);
			}
		}
	}

	void SpawnCustomer(GameObject prefab)
	{
		GameObject customer = Instantiate(prefab);
		customer.transform.SetParent(gm.customerQueueObj);
		customer.transform.localPosition = new Vector3(0, 1f, 0);

		timeToSpawnNextCustomer = Random.Range(gm.minTimeToSpawnNextCustomer, gm.maxTimeToSpawnNextCustomer);
		spawnNextCustomer = 0;
	}
}