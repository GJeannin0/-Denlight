using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class Player : MonoBehaviour
{
	private Manager manager;

	private Rigidbody2D myRigidbody2D;
	private uint shapeIndex = 0;                            // 0 = circle, 1 = circle2, 2 = circle3, 20 = spiky, 21 = spiky2, 22 = spiky3, 40 = triangle, 41 = triangle2, 42 = triangle3,  

	private float movementSpeed;
	[SerializeField] private float baseMovementSpeed = 10.0f;

	private Vector3 baseScale;
	[SerializeField] private Vector3 minimumScale;

	private Vector2 right = new Vector2(1.0f, 0.0f);
	private Vector2 upRight = new Vector2(1 / (float)Math.Sqrt(2), 1 / (float)Math.Sqrt(2));
	private Vector2 up = new Vector2(0.0f, 1.0f);
	private Vector2 upLeft = new Vector2(- 1 / (float)Math.Sqrt(2), 1 / (float)Math.Sqrt(2));
	private Vector2 left = new Vector2(- 1.0f, 0.0f);
	private Vector2 downLeft = new Vector2(- 1 / (float)Math.Sqrt(2), - 1 / (float)Math.Sqrt(2));
	private Vector2 down = new Vector2(0.0f, - 1.0f);
	private Vector2 downRight = new Vector2(1 / (float)Math.Sqrt(2), - 1 / (float)Math.Sqrt(2));

	private int life;
	[SerializeField] private int baseLife;

	[SerializeField] private int dropCost;


	[SerializeField] private HoveringLight lightDrop;

	[SerializeField] private Oscillator myOscillator;
	private SpriteRenderer mySpriteRenderer;

	[SerializeField] private float minimunTransparency = 0.50f;
	[SerializeField] private float minimunLight = 0.30f;

	[SerializeField] private Projectile myProjectile;
	[SerializeField] private float projectileCd;
	[SerializeField] private int attackDamage;
	private float projectileTimer;
	private Camera myCamera;

	[SerializeField] private float attackKnockback;
	[SerializeField] private float shooterOffset;


	

	[SerializeField] private int circle1Damages;
	[SerializeField] private int circle2Damages;
	[SerializeField] private int circle3Damages;

	[SerializeField] private float damageFromWalls;
	private float wallTimer = 0;

	void Start()
    {
		manager = FindObjectOfType<Manager>();
		myCamera = FindObjectOfType<Camera>();
		myRigidbody2D = GetComponent<Rigidbody2D>();
		movementSpeed = baseMovementSpeed;
		life = baseLife;
		mySpriteRenderer = myOscillator.gameObject.GetComponent<SpriteRenderer>();
		baseScale = transform.localScale;
    }
   
    void Update()
    {
		if (Input.GetButtonDown("Fire2"))
		{
			if (life > dropCost)
			{
				ChangeLife(-dropCost);
				Instantiate(lightDrop, transform.position, transform.rotation);
			}
		}

		if (shapeIndex < 40)
		{
			if (shapeIndex < 20)
			{
				if (projectileTimer >= projectileCd)
				{
					if (Input.GetButton("Fire1"))
					{
						Instantiate(myProjectile, transform.position + (new Vector3(myCamera.ScreenToWorldPoint(Input.mousePosition).x, myCamera.ScreenToWorldPoint(Input.mousePosition).y, 0.0f) - transform.position).normalized * shooterOffset,
							Quaternion.LookRotation(transform.forward,(new Vector3(myCamera.ScreenToWorldPoint(Input.mousePosition).x, myCamera.ScreenToWorldPoint(Input.mousePosition).y, 0.0f) - transform.position).normalized));
						projectileTimer = 0.0f;
					}
				}
				else
				{
					projectileTimer += Time.deltaTime;
				}
			}

			if (Input.GetAxis("Horizontal") > 0.0f)
			{
				if (Input.GetAxis("Vertical") > 0.0f)
				{
					MovePlayer(1);
				}
				else
				{
					if (Input.GetAxis("Vertical") < 0.0f)
					{
						MovePlayer(7);
					}
					else
					{
						MovePlayer(0);
					}
				}
			}
			else
			{
				if (Input.GetAxis("Horizontal") < 0.0f)
				{
					if (Input.GetAxis("Vertical") > 0.0f)
					{
						MovePlayer(3);
					}
					else
					{
						if (Input.GetAxis("Vertical") < 0.0f)
						{
							MovePlayer(5);
						}
						else
						{
							MovePlayer(4);
						}
					}
				}
				else
				{
					if (Input.GetAxis("Vertical") > 0.0f)
					{
						MovePlayer(2);
					}
					else
					{
						if (Input.GetAxis("Vertical") < 0.0f)
						{
							MovePlayer(6);
						}
					}
				}
			}
		}
	}

	private void MovePlayer(uint directionIndex)		// 0 right, 1 up right, 2 up, 3 up left, 4 left, 5 down left, 6 down, 7 down right
	{
		if (directionIndex == 0)
		{
			myRigidbody2D.velocity = right * movementSpeed;
		}
		if (directionIndex == 1)
		{
			myRigidbody2D.velocity = upRight * movementSpeed;
		}
		if (directionIndex == 2)
		{
			myRigidbody2D.velocity = up * movementSpeed;
		}
		if (directionIndex == 3)
		{
			myRigidbody2D.velocity = upLeft * movementSpeed;
		}
		if (directionIndex == 4)
		{
			myRigidbody2D.velocity = left * movementSpeed;
		}
		if (directionIndex == 5)
		{
			myRigidbody2D.velocity = downLeft * movementSpeed;
		}
		if (directionIndex == 6)
		{
			myRigidbody2D.velocity = down * movementSpeed;
		}
		if (directionIndex == 7)
		{
			myRigidbody2D.velocity = downRight * movementSpeed;
		}
	}

	public void ChangeLife(int amount)
	{
		life += amount;
		if (life <= 0)
		{
			manager.LoadMenu();
		}
		float lightPercentage = (float)life /baseLife;
		float remainingLight = minimunLight + lightPercentage * (1.0f - minimunLight);
		transform.localScale = (baseScale - minimumScale) * lightPercentage + minimumScale;
		mySpriteRenderer.color = new Color(remainingLight, remainingLight, remainingLight, minimunTransparency + lightPercentage * (1.0f - minimunTransparency));
	}

	public int GetAttackDamage()
	{
		return attackDamage;
	}

	public float GetAttackKnockback()
	{
		return attackKnockback;
	}

	public Vector2 GetVelocity()
	{
		return myRigidbody2D.velocity;
	}

	private void OnTriggerEnter2D(Collider2D collision)
	{
		if (collision.gameObject.tag == ("Circle1Attack"))
		{
			ChangeLife(-circle1Damages);
			Destroy(collision.gameObject);
		}

		if (collision.gameObject.tag == ("Circle2Attack"))
		{
			ChangeLife(-circle2Damages);
			Destroy(collision.gameObject);
		}

		if (collision.gameObject.tag == ("Circle3Attack"))
		{
			ChangeLife(-circle3Damages);
			Destroy(collision.gameObject);
		}

		if (collision.gameObject.tag == ("Finish"))
		{
			manager.Reload();
		}
	}

	private void OnTriggerStay2D(Collider2D collision)
	{
		if (collision.gameObject.tag == ("SoftWall"))
		{
			wallTimer += Time.deltaTime * damageFromWalls;
			if (wallTimer >= 1)
			{
				wallTimer = 0;
				ChangeLife( - 1);
			}
		}
	}

	public Vector3 GetSpeed()
	{
		return myRigidbody2D.velocity;
	}
}
