using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Manager : MonoBehaviour
{
	[SerializeField] private Automata mapGenerator;
	[SerializeField] private bool usingRandomSeed = true;

	[SerializeField] private int seed =0;
	private bool waitingForLoad;
	private bool generatorFound = false;
	private int difficulty = 0;  // 1 for normal, 2 for hard, 3 for EXTREME

	private float maximumSeed = 4000000000;

	void Start()
	{
		if (usingRandomSeed)
		{
			seed = (int)Random.Range(0.0f, maximumSeed);
		}
		DontDestroyOnLoad(gameObject);
		SceneManager.LoadScene("Menu");
	}

	void Update()
	{
		if (Input.GetButtonDown("Cancel"))
		{
			Application.Quit();
		}

		if (Input.GetButtonDown("Jump"))
		{
			SceneManager.LoadScene("Loading");
			generatorFound = false;
			seed++;
		}

		if (SceneManager.GetActiveScene().name == "Loading")
		{
			SceneManager.LoadScene("PlayScene1");
		}
	}

	public void FindGenerator()
	{
		mapGenerator = FindObjectOfType<Automata>();
		generatorFound = true;
	}

	public int GetSeed()
	{
		return seed;
	}

	public void Reload()
	{
		SceneManager.LoadScene("Loading");
		seed++;
	}

	public int GetDifficulty()
	{
		return difficulty;
	}

	public void SetDifficulty(int newDifficulty)
	{
		difficulty = newDifficulty;
	}

	public void LoadMenu()
	{
		SceneManager.LoadScene("Menu");
	}
}
