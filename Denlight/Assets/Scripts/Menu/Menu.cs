using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Menu : MonoBehaviour
{
	private Manager myManager;

    void Start()
    {
		myManager = FindObjectOfType<Manager>();
    }

	public void SetDifficultyExtreme()
	{
		myManager.SetDifficulty(3);
		myManager.Reload();
	}

	public void SetDifficultyHard()
	{
		myManager.SetDifficulty(2);
		myManager.Reload();
	}

	public void SetDifficultyNormal()
	{
		myManager.SetDifficulty(1);
		myManager.Reload();
	}
}
