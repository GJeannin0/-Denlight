using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CursorFollower : MonoBehaviour
{
	private Camera myCam;

	private void Start()
	{
		myCam = FindObjectOfType<Camera>();
	}

	void Update()
    {
		transform.position = new Vector3(myCam.ScreenToWorldPoint(Input.mousePosition).x, myCam.ScreenToWorldPoint(Input.mousePosition).y, 0.0f);
    }
}
