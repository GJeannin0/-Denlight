using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ActiveOnView : MonoBehaviour
{
	[SerializeField] private GameObject myCamera;

	[SerializeField] private float viewRange = 10;

	[SerializeField] private float checkPeriod = 1.0f;
	private float timer;

	[SerializeField] private GameObject visibleParts;

	private void Start()
	{
		myCamera = FindObjectOfType<Camera>().gameObject;
	}

	private void Update()
	{
		if (timer >= checkPeriod)
		{
			timer = 0.0f;
			if (Mathf.Abs(transform.position.x - myCamera.transform.position.x) + Mathf.Abs(transform.position.y - myCamera.transform.position.y) <= viewRange)
			{
				visibleParts.SetActive(true);
			}
			else
			{
				visibleParts.SetActive(false);
			}
		}
		else
		{
			timer += Time.deltaTime;
		}
	}
}
