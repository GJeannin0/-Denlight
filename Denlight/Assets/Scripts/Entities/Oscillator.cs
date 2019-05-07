using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Oscillator : MonoBehaviour
{
	[SerializeField] private float minimumSpeedX;
	[SerializeField] private float maximumSpeedX;
	[SerializeField] private float speedVariationX;
	[SerializeField] private float rangeX;
	private float speedX;

	[SerializeField] private float minimumSpeedY;
	[SerializeField] private float maximumSpeedY;
	[SerializeField] private float speedVariationY;
	[SerializeField] private float rangeY;
	private float speedY;
	[SerializeField] private float timeResetLimit = 4000000000;
	private float currentTime = 0;

	void Start()
    {
		speedX = Random.Range(minimumSpeedX, maximumSpeedX);
		speedY = Random.Range(minimumSpeedY, maximumSpeedY);
	}

    void Update()
    {
		currentTime += Time.deltaTime;
		if (currentTime >= timeResetLimit)
			currentTime = 0;
		UpdateSpeed();
		transform.localPosition = new Vector3(Mathf.Cos((currentTime) * speedX) * rangeX, Mathf.Cos((currentTime) * speedY) * rangeY, 0.0f);
	}

	private void UpdateSpeed()
	{
		speedX += Random.Range(-1.0f, 1.0f) * speedVariationX * Time.deltaTime;
		speedY += Random.Range(-1.0f, 1.0f) * speedVariationY * Time.deltaTime;

		if (speedX < minimumSpeedX)
		{
			speedX = minimumSpeedX;
		}
		if (speedX > maximumSpeedX)
		{
			speedX = maximumSpeedX;
		}
		if (speedY < minimumSpeedY)
		{
			speedY = minimumSpeedY;
		}
		if (speedY > maximumSpeedY)
		{
			speedY = maximumSpeedY;
		}
	}
}
