using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Projectile : MonoBehaviour
{

	private Rigidbody2D myRigidbody2D;

	[SerializeField] private float speed;
	[SerializeField] private float lifetime;
	[SerializeField] private bool wallHack;

	private float timer = 0;

    void Start()
    {
		myRigidbody2D = GetComponent<Rigidbody2D>();
		myRigidbody2D.velocity = transform.up * speed;
    }

	private void Update()
	{
		timer += Time.deltaTime;
		if (timer >= lifetime)
			Destroy(gameObject);
	}

	private void OnTriggerEnter2D(Collider2D collision)
	{
		if (!wallHack && collision.gameObject.tag == ("Wall"))
		{
			Destroy(gameObject);
		}
	}
}
