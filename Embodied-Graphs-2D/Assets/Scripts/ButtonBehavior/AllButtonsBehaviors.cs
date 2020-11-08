using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;


public class AllButtonsBehaviors : MonoBehaviour
{

	public bool selected = false;

	public static bool isPointerOverStaticPen = false;
	public static bool isPointerOverEdgeButton = false;
	public static bool isPointerOverEraserButton = false;
	public static bool isPointerOverGraphPen = false;
	public static bool isPointerOverPan = false;
	public static bool isPointerOverIconicPen = false;
	public static bool isPointerOverSelect = false;
	public static bool isPointerOverStrokeConvert = false;
	public static bool isPointerOverPathDefinition = false;
	public static bool isPointerOverCopy = false;
	public static bool isPointerOverTextInput = false;

	GameObject[] buttons;

	public void whenSelected()
	{
		selected = true;

		// change icon color
		transform.GetComponent<Image>().color = new Color(0f, 0f, 0f, 1f); //new Color(1, 1, 1, 0.5f);

		// change scale
		transform.localScale = new Vector3(1.25f, 1.25f, 1.25f);

		// deselect all other buttons
		for (int i = 0; i < buttons.Length; i++)
		{
			if (buttons[i].name == this.name) continue;

			buttons[i].GetComponent<AllButtonsBehaviors>().whenDeselected();
		}

	}

	public void whenDeselected()
	{
		selected = false;

		// change icon color
		transform.GetComponent<Image>().color = new Color(0.4f, 0.4f, 0.4f, 1f);

		// change scale
		transform.localScale = new Vector3(1f, 1f, 1f);


	}

	// Start is called before the first frame update
	void Start()
	{
		buttons = GameObject.FindGameObjectsWithTag("canvas_mode_button");
	}

	// Update is called once per frame
	void Update()
	{
		if (EventSystem.current.IsPointerOverGameObject(0))
		{
			if (this.name == "Pan")
				isPointerOverPan = true;
			else isPointerOverPan = false;

			if (this.name == "Select")
				isPointerOverSelect = true;
			else isPointerOverSelect = false;

			if (this.name == "IconicPen")
				isPointerOverIconicPen = true;
			else isPointerOverIconicPen = false;

			if (this.name == "Edge_draw")
				isPointerOverEdgeButton = true;
			else isPointerOverEdgeButton = false;

			if (this.name == "GraphPen")
				isPointerOverGraphPen = true;
			else isPointerOverGraphPen = false;

			if (this.name == "StaticPen")
				isPointerOverStaticPen = true;
			else isPointerOverStaticPen = false;

			if (this.name == "Text_Input")
				isPointerOverTextInput = true;
			else isPointerOverTextInput = false;

			if (this.name == "Copy")
				isPointerOverCopy = true;
			else isPointerOverCopy = false;

			if (this.name == "Path_Definition")
				isPointerOverPathDefinition = true;
			else isPointerOverPathDefinition = false;

			if (this.name == "Stroke_Conversion")
				isPointerOverStrokeConvert = true;
			else isPointerOverStrokeConvert = false;
		}
		else
		{
			isPointerOverPan = false;
			isPointerOverSelect = false;
			isPointerOverIconicPen = false;
			isPointerOverEdgeButton = false;
			isPointerOverGraphPen = false;
			isPointerOverStaticPen = false;
			isPointerOverTextInput = false;
			isPointerOverCopy = false;
			isPointerOverPathDefinition = false;
			isPointerOverStrokeConvert = false;
		}
	}
}
