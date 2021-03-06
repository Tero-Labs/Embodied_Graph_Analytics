using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;


public class AllButtonsBehaviors : MonoBehaviour
{

	public bool selected = false;


	public static bool isPointerOverGraphPen = false;
	public static bool isPointerOverPan = false;
	public static bool isPointerOverIconicPen = false;
	public static bool isPointerOverEraser = false;
	public static bool isPointerOverCopy = false;

	GameObject[] buttons;
    GameObject paint_canvas;

    public void whenSelected()
	{
		selected = true;

		// change icon color
		transform.GetComponent<Image>().color = new Color(0f, 0f, 0f, 1f); //new Color(1, 1, 1, 0.5f);

		// change scale
		transform.localScale = new Vector3(1.25f, 1.25f, 1.25f);

        if (this.name == "Pan")
        {
            // enable all colliders to move primitives around
            enableAllPenObjectColliders();
            enableplayerColliders();
            paint_canvas.GetComponent<Paintable>().okayToPan = true;
            paint_canvas.GetComponent<Paintable>().panZoomLocked = false;

            paint_canvas.GetComponent<Paintable>().color_picker.SetActive(false);
        }

        else if (this.name == "IconicPen")
        {
            // allow drawing over existing pen/set etc. objects without interfering
            disableAllPenObjectColliders();
            disablesimplicialColliders();

            paint_canvas.GetComponent<Paintable>().color_picker.SetActive(true);
            paint_canvas.GetComponent<Paintable>().color_picker_script.color = Color.red;
        }

        else if (this.name == "GraphPen")
        {
            enableAllPenObjectColliders();
            disablesimplicialColliders();

            paint_canvas.GetComponent<Paintable>().color_picker.SetActive(true);
            paint_canvas.GetComponent<Paintable>().color_picker_script.color = new Color(0.5f, 0.5f, 0.5f, 0.5f);
        }

        else if (this.name == "SimplicialPen")
        {
            enableAllPenObjectColliders();
            enablesimplicialColliders();
            paint_canvas.GetComponent<Paintable>().SimplicialVertices.Clear();
            paint_canvas.GetComponent<Paintable>().Simplicialnodes.Clear();

            paint_canvas.GetComponent<Paintable>().color_picker.SetActive(true);
            paint_canvas.GetComponent<Paintable>().color_picker_script.color = new Color(0f, 0f, 1f, 0.3f); 
        }

        else if (this.name == "HyperPen")
        {
            enableAllPenObjectColliders();
            //enablesimplicialColliders();
            paint_canvas.GetComponent<Paintable>().hyperVertices.Clear();
            paint_canvas.GetComponent<Paintable>().hypernodes.Clear();

            paint_canvas.GetComponent<Paintable>().color_picker.SetActive(true);
            paint_canvas.GetComponent<Paintable>().color_picker_script.color = new Color(0.5f, 0.5f, 0.5f, 0.5f);
        }

        else if (this.name == "Eraser")
        {
            enablesimplicialColliders();
            enableAllPenObjectColliders();

            paint_canvas.GetComponent<Paintable>().color_picker.SetActive(false);
        }

        else if (this.name == "Copy")
        {
            enableAllPenObjectColliders();
            disablesimplicialColliders();
            //paint_canvas.GetComponent<Paintable>().okayToPan = false;
            this.transform.GetComponent<CopyIconicObject>().start_copying = false;

            paint_canvas.GetComponent<Paintable>().color_picker.SetActive(false);
        }

        else if (this.name == "StrokeCombine")
        {
            disablesimplicialColliders();
            paint_canvas.GetComponent<Paintable>().color_picker.SetActive(false);
        }

        else if (this.name == "function_brush")
        {
            enableAllPenObjectColliders();
            //enablesimplicialColliders();
            //disable_menu_creation
            paint_canvas.GetComponent<Paintable>().panZoomLocked = true;

            paint_canvas.GetComponent<Paintable>().color_picker.SetActive(false);
        }

        else if (this.name == "video_op")
        {            
            //paint_canvas.GetComponent<Paintable>().videoplayer.transform.parent.gameObject.SetActive(true);
            paint_canvas.GetComponent<FileLoadDialog>().DialogShow();

            paint_canvas.GetComponent<Paintable>().color_picker.SetActive(false);
        }

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

        // when a new button is selected, a templine might still exist. We need to destroy that as well.
        if (this.name == "IconicPen")
        {
            if (paint_canvas.GetComponent<Paintable>().templine != null)
            {
                Destroy(paint_canvas.GetComponent<Paintable>().templine);
                paint_canvas.GetComponent<Paintable>().templine = null;
            }
        }
        // incase any temp cylinder is left, we will clear them up 
        else if (this.name == "GraphPen")
        {
            paint_canvas.GetComponent<Paintable>().DeleteEmptyEdgeObjects();
        }

        else if (this.name == "Pan")
        {
            paint_canvas.GetComponent<Paintable>().okayToPan = false;
            disableplayerColliders();

            if (paint_canvas.GetComponent<Paintable>().canvas_radial.transform.childCount > 0)
            {
                for (int i = 0; i < paint_canvas.GetComponent<Paintable>().canvas_radial.transform.childCount; i++)
                {
                    Destroy(paint_canvas.GetComponent<Paintable>().canvas_radial.transform.GetChild(i).gameObject);
                }
            }
        }

        else if (this.name == "SimplicialPen")
        {
            paint_canvas.GetComponent<Paintable>().SimplicialVertices.Clear();
            paint_canvas.GetComponent<Paintable>().Simplicialnodes.Clear();
            paint_canvas.GetComponent<Paintable>().DeleteEmptyEdgeObjects();
        }
        else if (this.name == "HyperPen")
        {
            paint_canvas.GetComponent<Paintable>().hyperVertices.Clear();
            paint_canvas.GetComponent<Paintable>().hypernodes.Clear();
            paint_canvas.GetComponent<Paintable>().DeleteEmptyEdgeObjects();
        }        
        else if (this.name == "StrokeCombine")
        {
            if (paint_canvas.GetComponent<Paintable>().setline != null)
            {
                Destroy(paint_canvas.GetComponent<Paintable>().setline);
                paint_canvas.GetComponent<Paintable>().setline = null;
            }
        }
        else if (this.name == "function_brush")
        {           
            if (paint_canvas.GetComponent<Paintable>().functionline != null)
            {
                Destroy(paint_canvas.GetComponent<Paintable>().functionline);
                paint_canvas.GetComponent<Paintable>().functionline = null;
            }
        }
    }

    public void enableAllPenObjectColliders()
    {
        // enable the pen box colliders that are immediate children of the paintable object
        GameObject[] drawnlist = GameObject.FindGameObjectsWithTag("iconic");

        foreach (GameObject icon in drawnlist)
        {
            if (icon.GetComponent<BoxCollider>() != null)
                icon.GetComponent<BoxCollider>().enabled = true;
        }
    }

    public void disableAllPenObjectColliders()
    {
        // disable the pen box colliders that are immediate children of the paintable object
        GameObject[] drawnlist = GameObject.FindGameObjectsWithTag("iconic");

        foreach (GameObject icon in drawnlist)
        {
            if (icon.GetComponent<BoxCollider>() != null)
                icon.GetComponent<BoxCollider>().enabled = false;
        }
    }

    public void enablesimplicialColliders()
    {
        GameObject[] drawnlist = GameObject.FindGameObjectsWithTag("simplicial");

        foreach (GameObject simplicial in drawnlist)
        {
            if (simplicial.GetComponent<BoxCollider>() != null)
                simplicial.GetComponent<BoxCollider>().enabled = true;
        }
    }

    public void disablesimplicialColliders()
    {
        GameObject[] drawnlist = GameObject.FindGameObjectsWithTag("simplicial");

        foreach (GameObject simplicial in drawnlist)
        {
            if (simplicial.GetComponent<BoxCollider>() != null)
                simplicial.GetComponent<BoxCollider>().enabled = false;
        }
    }

    public void enableplayerColliders()
    {
        GameObject[] videoplayers = GameObject.FindGameObjectsWithTag("video_player");

        foreach (GameObject vp in videoplayers)
        {
            if (vp.GetComponent<MeshCollider>() != null)
                vp.GetComponent<MeshCollider>().enabled = true;
        }
    }

    public void disableplayerColliders()
    {
        GameObject[] videoplayers = GameObject.FindGameObjectsWithTag("video_player");

        foreach (GameObject vp in videoplayers)
        {
            if (vp.GetComponent<MeshCollider>() != null)
                vp.GetComponent<MeshCollider>().enabled = false;
        }
    }

    // Start is called before the first frame update
    void Start()
	{
		buttons = GameObject.FindGameObjectsWithTag("canvas_mode_button");
        paint_canvas = GameObject.FindGameObjectWithTag("paintable_canvas_object");

    }

	// Update is called once per frame
	void Update()
	{
		if (EventSystem.current.IsPointerOverGameObject(0))
		{
			if (this.name == "Pan")
				isPointerOverPan = true;
			else isPointerOverPan = false;

			if (this.name == "Eraser")
                isPointerOverEraser = true;
			else isPointerOverEraser = false;

			if (this.name == "IconicPen")
				isPointerOverIconicPen = true;
			else isPointerOverIconicPen = false;

			if (this.name == "GraphPen")
				isPointerOverGraphPen = true;
			else isPointerOverGraphPen = false;
            
			if (this.name == "Copy")
				isPointerOverCopy = true;
			else isPointerOverCopy = false;

			
		}
		else
		{
			isPointerOverPan = false;
            isPointerOverEraser = false;
			isPointerOverIconicPen = false;
			isPointerOverGraphPen = false;
			isPointerOverCopy = false;
		}
	}
}
