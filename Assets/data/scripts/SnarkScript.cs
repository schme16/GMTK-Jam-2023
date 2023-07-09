using System.Collections;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using TMPro;
using UnityEngine;

public class SnarkScript : MonoBehaviour
{
	public TMP_Text SnarkText;

	public GameManagerScript gm;
	public float lifetime;
	public int waitBeforeFade;
	public float time = 0;
	private bool fade;
	private Color hiddenColour;

	// Start is called before the first frame update
	void Start()
	{
		gm = FindObjectOfType<GameManagerScript>();
		hiddenColour = new Color(SnarkText.color.r, SnarkText.color.g, SnarkText.color.b, 0);
		StartFade();
	}

	// Update is called once per frame
	void Update()
	{
		if (fade)
		{
			time += Time.deltaTime;
			float alphaPercent = time / lifetime;
			SnarkText.color = Color.Lerp(SnarkText.color, hiddenColour, alphaPercent);

			if (alphaPercent > 1)
			{
				Destroy(gameObject, 1f);
			}
		}

		transform.LookAt(gm.camera.transform.position);
		transform.eulerAngles = new Vector3(0, transform.eulerAngles.y, 0);
	}

	async void StartFade()
	{
		await UniTask.Delay(waitBeforeFade * 1000);
		fade = true;
	}
}