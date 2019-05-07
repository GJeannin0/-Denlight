using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy : MonoBehaviour
{
	private Automata mapGenerator;

	[SerializeField] private int baseLife;
	private int life;

	private Vector3 baseScale;
	[SerializeField] private Vector3 minimumScale;

	[SerializeField] private float minimunTransparency = 0.50f;
	[SerializeField] private float minimunLight = 0.30f;
	private SpriteRenderer mySpriteRenderer;

	private Rigidbody2D myRigidbody2D;

	private Player myPlayer;

	[SerializeField] private float knockbackSensibility;
	[SerializeField] private GameObject mySpriteHolder;

	[SerializeField] private int lootAmount;
	[SerializeField] private GameObject lootHoveringLight;

	[SerializeField] private float speed;
	[SerializeField] private float aggroRange;
	[SerializeField] private float aggroLimit;

	[SerializeField] private float aggroCheckPeriod;
	private float aggroTimer;

	[SerializeField] private float pathCheckPeriod;
	private float pathTimer;

	[SerializeField] private GameObject projectile;
	[SerializeField] private float nearLimit = 0.6f;
	[SerializeField] private float minimumDistanceToRepath = 3.0f;

	private List<Vector2> path;
	private int pathStep = 0;

	private bool aggroed;

	private float pathRelocateTimer;
	[SerializeField] private float pathRelocatePeriod = 5.0f;

	private bool following = false;

	[SerializeField] private float framesAhead = 2;

	[SerializeField] private float shotPeriod;
	private float shotTimer = 0;

	void Start()
	{
		mapGenerator = FindObjectOfType<Automata>();
		myRigidbody2D = GetComponent<Rigidbody2D>();
		baseScale = transform.localScale;
		myPlayer = FindObjectOfType<Player>();
		life = baseLife;
		mySpriteRenderer = mySpriteHolder.GetComponent<SpriteRenderer>();
		pathTimer = pathCheckPeriod;
	}

	void Update()
	{
		if (pathStep < 0)
			pathStep = 0;

		if (aggroTimer >= aggroCheckPeriod)
		{
			if ((myPlayer.transform.position - transform.position).magnitude <= aggroRange)
			{
				aggroed = true;
			}
			else
			{
				if ((myPlayer.transform.position - transform.position).magnitude >= aggroLimit)
					aggroed = false;
			}
			aggroTimer = 0;
		}
		else
			aggroTimer += Time.deltaTime;

		if (!aggroed)
		{
			myRigidbody2D.velocity = Vector2.zero;
			following = false;
		}
		else
		{

			if (pathTimer >= pathCheckPeriod)
			{
				following = true;

				path = mapGenerator.FindPathFromTo(new Vector2((int)transform.position.x, (int)transform.position.y), new Vector2((int)myPlayer.transform.position.x, (int)myPlayer.transform.position.y));

				Debug.Log("InitialPathCount " + path.Count);
				pathStep = path.Count - 1;
				MoveTo(path[pathStep]);
				pathTimer = 0;
			}
			else
				pathTimer += Time.deltaTime;

			if (shotTimer >= shotPeriod)
			{
				Instantiate(projectile, transform.position, Quaternion.LookRotation(transform.forward, (myPlayer.transform.position + myPlayer.GetSpeed() * Time.deltaTime * framesAhead) - transform.position));
				shotTimer = 0;
			}
			else
				shotTimer += Time.deltaTime;
		}

		if (following)
		{
			FollowPath(path);
		}
	}

	private void OnTriggerEnter2D(Collider2D collision)
	{
		if (collision.gameObject.tag == ("Attack"))
		{
			ChangeLife(-myPlayer.GetAttackDamage());
			Knockback(myPlayer.GetAttackKnockback(), collision.gameObject.transform.position);
			RotateTowards(myPlayer.transform.position);
			Destroy(collision.gameObject);
		}
	}

	private void Knockback(float force, Vector3 knockerPos)
	{
		Vector2 direction = new Vector2(transform.position.x - knockerPos.x, transform.position.y - knockerPos.y).normalized;
		myRigidbody2D.velocity += force * direction * knockbackSensibility;
	}

	public void ChangeLife(int amount)
	{
		if (amount < 0)
			aggroed = true;
		life += amount;
		if (life <= 0)
		{
			spawnHoveringLights(lootAmount);
			Destroy(gameObject);
		}
		float lightPercentage = (float)life / baseLife;
		float remainingLight = minimunLight + lightPercentage * (1.0f - minimunLight);
		transform.localScale = (baseScale - minimumScale) * lightPercentage + minimumScale;
		mySpriteRenderer.color = new Color(mySpriteRenderer.color.r, mySpriteRenderer.color.g, mySpriteRenderer.color.b, minimunTransparency + lightPercentage * (1.0f - minimunTransparency));
	}

	private void RotateTowards(Vector3 target)
	{
		transform.up = (target - transform.position).normalized;
	}

	private void spawnHoveringLights(int amount)
	{
		for (int i = 0; i < amount; i++)
		{
			Instantiate(lootHoveringLight, transform.position, transform.rotation);
		}
	}

	private void FollowPath(List<Vector2> path)
	{
		if (IsNear(path[pathStep]))
		{
			if (pathStep > 0)
				pathStep--;
			else
			{
				myRigidbody2D.velocity = Vector2.zero;
				return;
			}

			MoveTo(path[pathStep]);
			pathRelocateTimer = 0;
		}

		if (pathRelocateTimer >= pathRelocatePeriod)
		{
			MoveTo(path[pathStep]);
			pathRelocateTimer = 0;
		}
		else
		{
			pathRelocateTimer += Time.deltaTime;
		}
	}

	private bool IsNear(Vector2 location)
	{
		if ((new Vector2(transform.position.x, transform.position.y) - location).magnitude <= nearLimit)
		{
			return true;
		}
		else
			return false;
	}

	private void MoveTo(Vector2 location)
	{
		Debug.Log("Moved to " + location);
		myRigidbody2D.velocity = (location - new Vector2(transform.position.x, transform.position.y)).normalized * speed;
	}
}
