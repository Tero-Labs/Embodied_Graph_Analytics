using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;


public class AllButtonsBehaviors : MonoBehaviour
{

	public bool selected = false;
    public float width, height;
    public int macro_state_cnt;

    public Sprite record, play, normal;

    /*public static bool isPointerOverStaticPen = false;
    public static bool isPointerOverIconicPen = false;
    public static bool isPointerOverGraphPen = false;
	public static bool isPointerOverSimplicialPen = false;
	public static bool isPointerOverHyperPen = false;
	public static bool isPointerOverPan = false;	
	public static bool isPointerOverEraser = false;
	public static bool isPointerOverCopy = false;
	public static bool isPointerOverCombine = false;
	public static bool isPointerOverFuse = false;
	public static bool isPointerOverFunction = false;
	public static bool isPointerOverAnalysis = false;
	public static bool isPointerOverLoad = false;*/



	GameObject[] buttons;
    GameObject paint_canvas;
        
    public void whenSelected()
	{
		selected = true;

		// change icon color
		transform.GetComponent<Image>().color = new Color(0f, 0f, 0f, 1f); //new Color(1, 1, 1, 0.5f);
        		        
        // change scale
        transform.localScale = new Vector3(1.25f, 1.25f, 1.25f);

        Vector3[] v = new Vector3[4];
        transform.GetComponent<RectTransform>().GetWorldCorners(v);

        Vector3 center = v[3];//(v[0] + v[3]) / 2;
        center.y -= (v[1].y - v[0].y);

        //https://docs.unity3d.com/ScriptReference/RectTransform.GetWorldCorners.html

        var temp_stat = Instantiate(paint_canvas.GetComponent<Paintable>().status_label_obj,
            center,
            Quaternion.identity, transform.parent);

        temp_stat.GetComponent<Status_label_text>().ChangeLabel(this.name + "\n selected");

        if (this.name == "Pan")
        {
            // enable all colliders to move primitives around
            enableAllPenObjectColliders();
            enableplayerColliders();
            paint_canvas.GetComponent<Paintable>().okayToPan = true;
            paint_canvas.GetComponent<Paintable>().panZoomLocked = false;

            /*ColorPickerClickCheck.Rpointer = false;
            ColorPickerClickCheck.Gpointer = false;
            ColorPickerClickCheck.Bpointer = false;
            ColorPickerClickCheck.Apointer = false;
            ColorPickerClickCheck.previewpointer = false;*/
            paint_canvas.GetComponent<Paintable>().color_picker.SetActive(false);
        }

        else if (this.name == "IconicPen")
        {
            // allow drawing over existing pen/set etc. objects without interfering
            disableAllPenObjectColliders();
            disablesimplicialColliders();

            /*ColorPickerClickCheck.Rpointer = false;
            ColorPickerClickCheck.Gpointer = false;
            ColorPickerClickCheck.Bpointer = false;
            ColorPickerClickCheck.Apointer = false;
            ColorPickerClickCheck.previewpointer = false;*/
            paint_canvas.GetComponent<Paintable>().color_picker.SetActive(true);
            paint_canvas.GetComponent<Paintable>().color_picker_script.color = Color.red;
        }

        else if (this.name == "Annotation")
        {
            // allow drawing over existing pen/set etc. objects without interfering
            disableAllPenObjectColliders();
            disablesimplicialColliders();

            /*ColorPickerClickCheck.Rpointer = false;
            ColorPickerClickCheck.Gpointer = false;
            ColorPickerClickCheck.Bpointer = false;
            ColorPickerClickCheck.Apointer = false;
            ColorPickerClickCheck.previewpointer = false;*/
            paint_canvas.GetComponent<Paintable>().color_picker.SetActive(true);
            paint_canvas.GetComponent<Paintable>().color_picker_script.color = Color.red;
        }

        else if (this.name == "EdgeBrush")
        {
            enableAllPenObjectColliders();
            disablesimplicialColliders();

            /*ColorPickerClickCheck.Rpointer = false;
            ColorPickerClickCheck.Gpointer = false;
            ColorPickerClickCheck.Bpointer = false;
            ColorPickerClickCheck.Apointer = false;
            ColorPickerClickCheck.previewpointer = false;*/
            paint_canvas.GetComponent<Paintable>().color_picker.SetActive(true);
            paint_canvas.GetComponent<Paintable>().color_picker_script.color = new Color(0.5f, 0.5f, 0.5f, 0.5f);
        }

        else if (this.name == "SimplicialPen")
        {
            enableAllPenObjectColliders();
            enablesimplicialColliders();
            paint_canvas.GetComponent<Paintable>().SimplicialVertices.Clear();
            paint_canvas.GetComponent<Paintable>().Simplicialnodes.Clear();

            /*ColorPickerClickCheck.Rpointer = false;
            ColorPickerClickCheck.Gpointer = false;
            ColorPickerClickCheck.Bpointer = false;
            ColorPickerClickCheck.Apointer = false;
            ColorPickerClickCheck.previewpointer = false;*/
            paint_canvas.GetComponent<Paintable>().color_picker.SetActive(true);
            paint_canvas.GetComponent<Paintable>().color_picker_script.color = new Color(0f, 0f, 1f, 0.3f); 
        }

        else if (this.name == "HyperGraphPen")
        {
            enableAllPenObjectColliders();
            //enablesimplicialColliders();
            paint_canvas.GetComponent<Paintable>().hyperVertices.Clear();
            paint_canvas.GetComponent<Paintable>().hypernodes.Clear();

            /*ColorPickerClickCheck.Rpointer = false;
            ColorPickerClickCheck.Gpointer = false;
            ColorPickerClickCheck.Bpointer = false;
            ColorPickerClickCheck.Apointer = false;
            ColorPickerClickCheck.previewpointer = false;*/
            paint_canvas.GetComponent<Paintable>().color_picker.SetActive(true);
            paint_canvas.GetComponent<Paintable>().color_picker_script.color = new Color(0.5f, 0.5f, 0.5f, 0.5f);
        }

        else if (this.name == "Eraser")
        {
            enablesimplicialColliders();
            enableAllPenObjectColliders();

            /*ColorPickerClickCheck.Rpointer = false;
            ColorPickerClickCheck.Gpointer = false;
            ColorPickerClickCheck.Bpointer = false;
            ColorPickerClickCheck.Apointer = false;
            ColorPickerClickCheck.previewpointer = false;*/
            paint_canvas.GetComponent<Paintable>().color_picker.SetActive(false);
        }

        else if (this.name == "Copy")
        {
            enableAllPenObjectColliders();
            disablesimplicialColliders();
            //paint_canvas.GetComponent<Paintable>().okayToPan = false;
            this.transform.GetComponent<CopyIconicObject>().start_copying = false;

            /*ColorPickerClickCheck.Rpointer = false;
            ColorPickerClickCheck.Gpointer = false;
            ColorPickerClickCheck.Bpointer = false;
            ColorPickerClickCheck.Apointer = false;
            ColorPickerClickCheck.previewpointer = false;*/
            paint_canvas.GetComponent<Paintable>().color_picker.SetActive(false);
        }

        else if (this.name == "StrokeCombine")
        {
            disablesimplicialColliders();

            /*ColorPickerClickCheck.Rpointer = false;
            ColorPickerClickCheck.Gpointer = false;
            ColorPickerClickCheck.Bpointer = false;
            ColorPickerClickCheck.Apointer = false;
            ColorPickerClickCheck.previewpointer = false;*/
            paint_canvas.GetComponent<Paintable>().color_picker.SetActive(false);
        }

        else if (this.name == "FunctionConversion")
        {
            enableAllPenObjectColliders();
            disablesimplicialColliders();

            /*ColorPickerClickCheck.Rpointer = false;
            ColorPickerClickCheck.Gpointer = false;
            ColorPickerClickCheck.Bpointer = false;
            ColorPickerClickCheck.Apointer = false;
            ColorPickerClickCheck.previewpointer = false;*/
            paint_canvas.GetComponent<Paintable>().color_picker.SetActive(false);
        }

        else if (this.name == "FunctionBrush")
        {
            enableAllPenObjectColliders();
            //enablesimplicialColliders();
            //disable_menu_creation
            paint_canvas.GetComponent<Paintable>().panZoomLocked = true;
            Paintable.dragged_arg_textbox = null;

            /*ColorPickerClickCheck.Rpointer = false;
            ColorPickerClickCheck.Gpointer = false;
            ColorPickerClickCheck.Bpointer = false;
            ColorPickerClickCheck.Apointer = false;
            ColorPickerClickCheck.previewpointer = false;*/

            paint_canvas.GetComponent<Paintable>().color_picker.SetActive(false);
        }

        else if (this.name == "FileLoad")
        {            
            //paint_canvas.GetComponent<Paintable>().videoplayer.transform.parent.gameObject.SetActive(true);
            paint_canvas.GetComponent<FileLoadDialog>().DialogShow();

            /*ColorPickerClickCheck.Rpointer = false;
            ColorPickerClickCheck.Gpointer = false;
            ColorPickerClickCheck.Bpointer = false;
            ColorPickerClickCheck.Apointer = false;
            ColorPickerClickCheck.previewpointer = false;*/

            paint_canvas.GetComponent<Paintable>().color_picker.SetActive(false);
        }

        else if (this.name == "AnalysisPen")
        {
            enableAllPenObjectColliders();
            Paintable.dragged_arg_textbox = null;

            if (!paint_canvas.GetComponent<Paintable>().no_analysis_menu_open)
            {
                var analysis_menu = Instantiate(paint_canvas.GetComponent<Paintable>().analysis_radial_menu, transform.parent);
                analysis_menu.GetComponent<Graphplot>().paintable = paint_canvas;
                paint_canvas.GetComponent<Paintable>().no_analysis_menu_open = true;
            }

            /*ColorPickerClickCheck.Rpointer = false;
            ColorPickerClickCheck.Gpointer = false;
            ColorPickerClickCheck.Bpointer = false;
            ColorPickerClickCheck.Apointer = false;
            ColorPickerClickCheck.previewpointer = false;*/

            paint_canvas.GetComponent<Paintable>().color_picker.SetActive(false);
        }

        else if (this.name == "MacroRecord")
        {
            if (macro_state_cnt == 0)
            {
                transform.GetComponent<Image>().sprite = record;
                macro_state_cnt++;
            }
            else if (macro_state_cnt == 1)
            {
                transform.GetComponent<Image>().sprite = play;
                macro_state_cnt++;
            }
            else
            {
                macro_state_cnt = 0;
                transform.GetComponent<Image>().sprite = normal;
            }                

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
        if (this.name == "IconicPen" || this.name == "Annotation")
        {
            if (paint_canvas.GetComponent<Paintable>().templine != null)
            {
                Destroy(paint_canvas.GetComponent<Paintable>().templine);
                paint_canvas.GetComponent<Paintable>().templine = null;
            }

            StartCoroutine(CorrectIcons());
        }

        // incase any temp cylinder is left, we will clear them up 
        else if (this.name == "EdgeBrush")
        {
            paint_canvas.GetComponent<Paintable>().DeleteEmptyEdgeObjects();

            StartCoroutine(CorrectEdges());
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
        else if (this.name == "HyperGraphPen")
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
        else if (this.name == "FunctionBrush")
        {           
            if (paint_canvas.GetComponent<Paintable>().functionline != null)
            {
                Destroy(paint_canvas.GetComponent<Paintable>().functionline);
                //paint_canvas.GetComponent<Paintable>().functionline = null;
            }

            StartCoroutine(CorrectFunctionLines());
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

        width = transform.GetComponent<RectTransform>().sizeDelta.x * transform.GetComponent<RectTransform>().localScale.x;
        height = transform.GetComponent<RectTransform>().sizeDelta.y * transform.GetComponent<RectTransform>().localScale.y;
        macro_state_cnt = 0;
    }

    // Update is called once per frame
    void Update()
	{
		/*if (EventSystem.current.IsPointerOverGameObject(0))
		{
            if (this.name == "StaticPen")
                isPointerOverStaticPen = true;
            else isPointerOverStaticPen = false;

            if (this.name == "IconicPen")
                isPointerOverIconicPen = true;
            else isPointerOverIconicPen = false;

            if (this.name == "GraphPen")
                isPointerOverGraphPen = true;
            else isPointerOverGraphPen = false;                                             

            if (this.name == "SimplicialPen")
                isPointerOverSimplicialPen = true;
            else isPointerOverSimplicialPen = false;

            if (this.name == "HyperPen")
                isPointerOverHyperPen = true;
            else isPointerOverHyperPen = false;

            if (this.name == "Pan")
                isPointerOverPan = true;
            else isPointerOverPan = false;

            if (this.name == "Eraser")
                isPointerOverEraser = true;
            else isPointerOverEraser = false;

            if (this.name == "Copy")
                isPointerOverCopy = true;
            else isPointerOverCopy = false;

            if (this.name == "StrokeCombine")
                isPointerOverCombine = true;
            else isPointerOverCombine = false;

            if (this.name == "Fused")
                isPointerOverFuse = true;
            else isPointerOverFuse = false;

            if (this.name == "FunctionPen")
                isPointerOverFunction = true;
            else isPointerOverFunction = false;

            if (this.name == "AnalysisPen")
                isPointerOverAnalysis = true;
            else isPointerOverAnalysis = false;

            if (this.name == "FileLoad")
                isPointerOverLoad = true;
            else isPointerOverLoad = false;

        }
		else
		{
            isPointerOverStaticPen = false;
            isPointerOverIconicPen = false;
            isPointerOverGraphPen = false;
            isPointerOverSimplicialPen = false;
            isPointerOverHyperPen = false;
            isPointerOverPan = false;
            isPointerOverEraser = false;
            isPointerOverCopy = false;
            isPointerOverCombine = false;
            isPointerOverFuse = false;
            isPointerOverFunction = false;
            isPointerOverAnalysis = false;
            isPointerOverLoad = false;
}*/
	}

    IEnumerator CorrectIcons()
    {        
        yield return null;

        foreach(GameObject cur in paint_canvas.GetComponent<Paintable>().new_drawn_icons)
        {
            if (cur == null) continue;
            if (cur.GetComponent<iconicElementScript>() != null)
            {
                if (cur.GetComponent<iconicElementScript>().points.Count < paint_canvas.GetComponent<Paintable>().min_point_count)
                {
                    Destroy(cur);
                }
            }
            
            else if(cur.GetComponent<BoxCollider>() == null)
            {
                Destroy(cur);
            }

            else if (cur.GetComponent<MeshFilter>().sharedMesh == null)
            {
                Destroy(cur);
            }
        }

        yield return null;
        paint_canvas.GetComponent<Paintable>().new_drawn_icons.Clear();
    }

    IEnumerator CorrectEdges()
    {
        yield return null;

        foreach (GameObject cur in paint_canvas.GetComponent<Paintable>().new_drawn_edges)
        {
            if (cur == null) continue;
            if (cur.GetComponent<EdgeElementScript>().edge_start == null ||
                cur.GetComponent<EdgeElementScript>().edge_end == null)
            {
                Destroy(cur);
            }
            else if (cur.GetComponent<EdgeCollider2D>() == null)
            {
                Destroy(cur);
            }
        }

        yield return null;
        paint_canvas.GetComponent<Paintable>().new_drawn_edges.Clear();
    }
        
    IEnumerator CorrectFunctionLines()
    {
        yield return null;

        foreach (GameObject cur in paint_canvas.GetComponent<Paintable>().new_drawn_function_lines)
        {
            if (cur == null) continue;
            if (cur.transform.childCount < 3)
            {
                Destroy(cur);
            }
        }

        yield return null;
        paint_canvas.GetComponent<Paintable>().new_drawn_function_lines.Clear();
    }

}
