using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ColorsHomogenizer : MonoBehaviour
{
	[SerializeField] private int size;
	[SerializeField] private GameObject[] coloredComponents;
	[SerializeField] private int sizeAOE;
	[SerializeField] private GameObject[] coloredComponentsAOE;

	private SpriteRenderer mySpriteRenderer;
	private Color myColor;
	[SerializeField] private float aOETransparency;

    void Start()
    {
		mySpriteRenderer = GetComponent<SpriteRenderer>();
		myColor = mySpriteRenderer.color;
    }

    void Update()
    {
		for (int i = 0; i < size; i++)
		{
			coloredComponents[i].GetComponent<SpriteRenderer>().color = myColor;
		}

		for (int i = 0; i < sizeAOE; i++)
		{
			coloredComponentsAOE[i].GetComponent<SpriteRenderer>().color = new Color (myColor.r,myColor.g,myColor.b,aOETransparency);
		}
	}
}
