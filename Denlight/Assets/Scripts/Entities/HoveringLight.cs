using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HoveringLight : MonoBehaviour
{
	[SerializeField] private float timeBeforeActive;
	private float timer = 0.0f;
	[SerializeField] private bool active = false;

	[SerializeField] private float speed;
	private Rigidbody2D myRigidbody2D;

	private Player myPlayer;

	private bool goingToPlayer = false;
	[SerializeField] private float absorbtionDistance = 0.50f;
	[SerializeField] private float accelerationTowardPlayer = 50.0f;
	[SerializeField] private int dropGain = 4;

	[SerializeField] private float predictingTime;

	[SerializeField] private float minSpeedMultiplier;
	[SerializeField] private float maxSpeedMultiplier;

	void Start()
	{
		myPlayer = FindObjectOfType<Player>();
		myRigidbody2D = GetComponent<Rigidbody2D>();
		if (!active)
		{
			AddSpeed();
		}
	}

	void Update()
	{
		if (!active)
		{
			if (timer < timeBeforeActive)
			{
				timer += Time.deltaTime;
			}
			else
			{
				gameObject.tag = ("HoveringLight");
				active = true;
			}
		}

		if (goingToPlayer)
		{
			myRigidbody2D.velocity += (new Vector2(((myPlayer.transform.position + new Vector3(myPlayer.GetVelocity().x, myPlayer.GetVelocity().y, 0.0f) * predictingTime) - transform.position).normalized.x,
				(myPlayer.transform.position - transform.position).normalized.y)) * accelerationTowardPlayer * Time.deltaTime;

			if ((myPlayer.transform.position - transform.position).magnitude <= absorbtionDistance)
			{
				myPlayer.ChangeLife(dropGain);
				Destroy(gameObject);
			}
		}
	}

	private void AddSpeed()
	{
		float angle = Random.Range(-2 * Mathf.PI, 2 * Mathf.PI);
		Vector2 direction = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle));
		myRigidbody2D.velocity += speed * direction * Random.Range(minSpeedMultiplier, maxSpeedMultiplier);
	}

	private void OnTriggerEnter2D(Collider2D collision)
	{
		if (collision.gameObject.tag == ("Player") && gameObject.tag == ("HoveringLight"))
		{
			goingToPlayer = true;
		}
	}
}
