using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WallRandomizer : MonoBehaviour
{
	[SerializeField] private GameObject spike;

	private GameObject[] spikes;

	[SerializeField] private int amountOfSpikes;

	[SerializeField] private float maximumX;
	[SerializeField] private float minimumX;
	[SerializeField] private float maximumY;
	[SerializeField] private float minimumY;

	[SerializeField] private float maximumSize;
	[SerializeField] private float minimumSize;

	[SerializeField] private Vector2 orientation;

	void Start()
    {
		spikes = new GameObject[amountOfSpikes];

		for (int i = 0; i < amountOfSpikes; i++)
		{
			spikes[i] = Instantiate(spike, transform);
			spikes[i].transform.localPosition = new Vector3(Random.Range(minimumX + i * (maximumX - minimumX) / amountOfSpikes, minimumX + (1 + i) * (maximumX - minimumX) / amountOfSpikes), Random.Range(minimumY, maximumY), 0.0f);

			if (Random.value >= 0.5)
			{
				spikes[i].transform.localScale = new Vector3 (spikes[i].transform.localScale.x, -spikes[i].transform.localScale.y, spikes[i].transform.localScale.z);
			}
			spikes[i].transform.localScale *= Random.Range(minimumSize, maximumSize);
		}
		transform.up = orientation;
    }
}
