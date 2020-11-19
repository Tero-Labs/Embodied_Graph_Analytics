using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem.EnhancedTouch;
using UnityEngine.InputSystem;
using System.Linq;

public class Paintable : MonoBehaviour
{

    public static bool allowOpacity = false;

    // Length, area, distance units
    public static float unitScale = 0.025f;

    // PAN AND ZOOM RELATED PARAMETERS
    public Camera main_camera;
	public float finger_move_tolerance = 100f;
	private static float zoom_multiplier = 0.5f;
	public static float zoom_min = 200;
	public static float zoom_max = 850;
	public float pan_amount = 5f;
	public Vector2 panTouchStart;
	public bool previousTouchEnded;
	public bool okayToPan = true;
	public bool panZoomLocked = false;


	// Prefabs
	public GameObject IconicElement;
    public GameObject EdgeElement;

    // Canvas buttons
    public static GameObject iconicElementButton;
    public static GameObject pan_button;
    public static GameObject graph_pen_button;
    public GameObject edgeline;

    // needed for drawing
    public GameObject templine;
	public int totalLines = 0;
    private static int min_point_count = 30;

    // holder of all game objects
    public GameObject Objects_parent;

    // edge paint control
    // box collider control
    public static bool enablecollider = false;
    public static int selected_obj_count = 0;
    public List<GameObject> selected_obj = new List<GameObject>();
    public GameObject edge_start, edge_end;

    // needed for graph
    public static int graph_count = 0;

    // objects history
    public List<GameObject> history = new List<GameObject>();

    // Action History
    public static bool ActionHistoryEnabled = false;

    // Start is called before the first frame update
    void Start()
	{
        iconicElementButton = GameObject.Find("IconicPen");
        pan_button = GameObject.Find("Pan");
        graph_pen_button = GameObject.Find("GraphPen");
    }

	// Update is called once per frame
	void Update()
	{
        //var activeTouches = UnityEngine.InputSystem.EnhancedTouch.Touch.activeTouches;

        /*
		#region prevent unwanted touch on canvas
		// prevent touch or click being registered on the canvas when a gui button is clicked
		if (AllButtonsBehaviors.isPointerOverIconicPen || AllButtonsBehaviors.isPointerOverSelect || AllButtonsBehaviors.isPointerOverPan
		|| AllButtonsBehaviors.isPointerOverEdgeButton || AllButtonsBehaviors.isPointerOverGraphPen ||
		AllButtonsBehaviors.isPointerOverEraserButton || AllButtonsBehaviors.isPointerOverStaticPen || AllButtonsBehaviors.isPointerOverStrokeConvert
		|| AllButtonsBehaviors.isPointerOverPathDefinition || AllButtonsBehaviors.isPointerOverTextInput || AllButtonsBehaviors.isPointerOverCopy)
		{
			return;
		}
		#endregion
		*/

        #region iconic element brush

        if (iconicElementButton.GetComponent<AllButtonsBehaviors>().selected)
			//!iconicElementButton.GetComponent<AllButtonsBehaviors>().isPredictivePen)
		{
			//Debug.Log("entered");
			if (PenTouchInfo.PressedThisFrame)//currentPen.tip.wasPressedThisFrame)
			{
				// start drawing a new line
				var ray = Camera.main.ScreenPointToRay(PenTouchInfo.penPosition);
				RaycastHit Hit;
				if (Physics.Raycast(ray, out Hit) && Hit.collider.gameObject.name == "Paintable")
				{
					//Debug.Log("instantiated_templine");

					Vector3 vec = Hit.point + new Vector3(0, 0, -5); // Vector3.up * 0.1f;
																	  //Debug.Log(vec);

					totalLines++;
					templine = Instantiate(IconicElement, vec, Quaternion.identity, Objects_parent.transform);
					templine.GetComponent<TrailRenderer>().material.color = Color.black;

					templine.name = "iconic_" + totalLines.ToString();
					templine.tag = "iconic";

					templine.GetComponent<iconicElementScript>().points.Add(vec);

					/*
					// Initiate the length display (use an existing text box used for translation parameterization)
					templine.transform.GetChild(1).localScale = new Vector3(4, 4, 1);
					// fix the position of the text
					templine.transform.GetChild(1).transform.position = vec;

					templine.transform.GetChild(1).GetComponent<MeshRenderer>().enabled = true;
					templine.transform.GetChild(1).GetComponent<BoxCollider2D>().enabled = false;

					// color and width
					templine.GetComponent<TrailRenderer>().material.color =
						pencil_button.GetComponent<AllButtonsBehavior>().pickerInstance.transform.GetChild(0).GetChild(0).GetComponent<ColorPicker>().Result;

					templine.GetComponent<penLine_script>().pen_line_material.color =
						pencil_button.GetComponent<AllButtonsBehavior>().pickerInstance.transform.GetChild(0).GetChild(0).GetComponent<ColorPicker>().Result;

					templine.GetComponent<TrailRenderer>().widthMultiplier =
						pencil_button.GetComponent<AllButtonsBehavior>().penWidthSliderInstance.GetComponent<Slider>().value;

					templine.GetComponent<LineRenderer>().widthMultiplier =
						pencil_button.GetComponent<AllButtonsBehavior>().penWidthSliderInstance.GetComponent<Slider>().value;

					// add to history
					history.Add(templine);

					*/
				}
			}

			else if (templine != null &&
				PenTouchInfo.PressedNow //currentPen.tip.isPressed
				&& (PenTouchInfo.penPosition -
				(Vector2)templine.GetComponent<iconicElementScript>().points[templine.GetComponent<iconicElementScript>().points.Count - 1]).magnitude > 0f)
			{
				// add points to the last line
				var ray = Camera.main.ScreenPointToRay(PenTouchInfo.penPosition);
				RaycastHit Hit;
				if (Physics.Raycast(ray, out Hit) && Hit.collider.gameObject.name == "Paintable")
				{

					Vector3 vec = Hit.point + new Vector3(0, 0, -5); // Vector3.up * 0.1f;

					templine.GetComponent<TrailRenderer>().transform.position = vec;
					templine.GetComponent<iconicElementScript>().points.Add(vec);

					
					// Show the distance, format to a fixed decimal place.
					templine.GetComponent<iconicElementScript>().calculateLengthAttributeFromPoints();
					/*templine.transform.GetChild(1).GetComponent<TextMeshPro>().text =
						templine.GetComponent<penLine_script>().attribute.Length.ToString("F1");*/

					// pressure based pen width
					templine.GetComponent<iconicElementScript>().updateLengthFromPoints();
                    templine.GetComponent<iconicElementScript>().addPressureValue(PenTouchInfo.pressureValue);
                    templine.GetComponent<iconicElementScript>().reNormalizeCurveWidth();
					templine.GetComponent<TrailRenderer>().widthCurve = templine.GetComponent<iconicElementScript>().widthcurve;
					
				}
			}

			else if (templine != null && PenTouchInfo.ReleasedThisFrame)
			{
				var ray = Camera.main.ScreenPointToRay(PenTouchInfo.penPosition); //currentPen.position.ReadValue());// Input.GetTouch(0).position);
				RaycastHit Hit;

				if (Physics.Raycast(ray, out Hit) && Hit.collider.gameObject.name == "Paintable")
				{
					if (templine.GetComponent<iconicElementScript>().points.Count > min_point_count)
					{
                        // Debug.Log("here_in_save_mode " + templine.GetComponent<iconicElementScript>().points.Count.ToString());
                        // TODO: FINISH THE ICONIC ELEMENT
                        templine = transform.GetComponent<CreatePrimitives>().FinishPenLine(templine);

						// set templine to null, otherwise, if an existing touch from color picker makes it to the canvas,
						// then the object jumps to the color picker (as a new templine hasn't been initialized from touch.begin).
						templine = null;
					}
					else
					{
                        // delete the templine, not enough points
                        // Debug.Log("here_in_destroy");
                        Destroy(templine);
					}
				}
				else
				{
                    // the touch didn't end on a line, destroy the line
                    // Debug.Log("here_in_destroy_different_Hit");
                    Destroy(templine);
				}

			}

		}
        // when a new button is selected, a templine might still exist. We need to destroy that as well.
        else
        {
            if (templine != null)
            {
                Destroy(templine);
                templine = null;
            }
                
        }

        #endregion

        #region pan

        // Handle screen touches.                     
        //https://docs.unity3d.com/ScriptReference/TouchPhase.Moved.html

        if (pan_button.GetComponent<AllButtonsBehaviors>().selected)
        //!iconicElementButton.GetComponent<AllButtonsBehaviors>().isPredictivePen)
        {
            okayToPan = true;
        }
        else
        {
            okayToPan = false;
        }

        if (Input.touchCount == 2 && !panZoomLocked) // && pan_button.GetComponent<PanButtonBehavior>().selected)
        {
            Debug.Log("double_finger_tap");
            // NO ANCHOR TAPPED, JUST ZOOM IN/PAN
            //main_camera.GetComponent<MobileTouchCamera>().enabled = true;

            UnityEngine.Touch touchzero = Input.GetTouch(0); 
            UnityEngine.Touch touchone = Input.GetTouch(1);

            Vector2 touchzeroprevpos = touchzero.position - touchzero.deltaPosition;
            Vector2 touchoneprevpos = touchone.position - touchone.deltaPosition;

            float prevmag = (touchzeroprevpos - touchoneprevpos).magnitude;
            float currmag = (touchzero.position - touchone.position).magnitude;

            float difference = currmag - prevmag;

            Debug.Log("diff: " + difference + ", multiplier: " + zoom_multiplier + ", camera size: " + (Camera.main.orthographicSize - zoom_multiplier * difference).ToString());
            Debug.Log("Clamped: " + Mathf.Clamp(zoom_multiplier * (Camera.main.orthographicSize - difference), zoom_min, zoom_max));

            Camera.main.orthographicSize = Mathf.Clamp(Camera.main.orthographicSize - zoom_multiplier * difference, zoom_min, zoom_max);

            // CHECK AND DELETE INCOMPLETE LINES
            //deleteTempLineIfDoubleFinger();

            int zoom = (int)((1f - ((main_camera.orthographicSize - zoom_min) / zoom_max)) * 100f);
           

        }
        else if (Input.touchCount == 1 && !panZoomLocked)
        {
            UnityEngine.Touch activeTouches = Input.GetTouch(0);

            // Only pan when the touch is on top of the canvas. Otherwise,
            var ray = Camera.main.ScreenPointToRay(activeTouches.position);
            RaycastHit Hit;

            if (Physics.Raycast(ray, out Hit) && Hit.collider.gameObject.tag == "paintable_canvas_object")
            {
                

                // EnhanchedTouch acts weirdly in the sense that the Start phase is not detected many times,
                // only Moved, Stationary, and Ended phases are detected most of the times.
                // So, introducing a bool that will only update the start pan position if a touch ended before.

                if (activeTouches.phase == UnityEngine.TouchPhase.Ended)
                {
                    previousTouchEnded = true;
                }

                //if (touchScreen.touches[0].phase.ReadValue() == UnityEngine.InputSystem.TouchPhase.Began)
                else if (activeTouches.phase == UnityEngine.TouchPhase.Moved && previousTouchEnded && okayToPan)
                {
                    panTouchStart = Camera.main.ScreenToWorldPoint(activeTouches.position);
                    Debug.Log("touch start: " + panTouchStart.ToString());

                    previousTouchEnded = false;
                }

                //else if (touchScreen.touches[0].phase.ReadValue() == UnityEngine.InputSystem.TouchPhase.Moved)
                else if (activeTouches.phase == UnityEngine.TouchPhase.Moved && !previousTouchEnded && okayToPan)
                {
                    Vector2 panDirection = panTouchStart - (Vector2)Camera.main.ScreenToWorldPoint(activeTouches.position);
                    Debug.Log("position changed from "+ Camera.main.transform.position.ToString() + " to " + (Camera.main.transform.position + (Vector3)panDirection).ToString());
                    Camera.main.transform.position += (Vector3)panDirection;
                }

                //Debug.Log("single_finger_tap " + activeTouches.phase + " " + previousTouchEnded + " " + okayToPan);
            }

            // if not panning, check for tap
            /*if (activeTouches.isTap)
            {
                // delegate single tap to short tap function
                Debug.Log("tap detected.");
                OnShortTap(activeTouches[0].screenPosition);
                OnLongTap(activeTouches[0].screenPosition);
            }*/
        }


        #endregion

        #region Graph Pen
        if (graph_pen_button.GetComponent<AllButtonsBehaviors>().selected)
        {
            // just entered edge button, enable all collider 
            if (enablecollider == false)
            {
                enablecollider = true;
                GameObject[] drawnlist = GameObject.FindGameObjectsWithTag("iconic");

                foreach (GameObject icon in drawnlist)
                {
                    if (icon.GetComponent<BoxCollider>() != null)
                        icon.GetComponent<BoxCollider>().enabled = true;
                }
            }


            if (PenTouchInfo.PressedThisFrame)//currentPen.tip.wasPressedThisFrame)
            {
                // start drawing a new line
                var ray = Camera.main.ScreenPointToRay(PenTouchInfo.penPosition);
                RaycastHit Hit;
                if (Physics.Raycast(ray, out Hit) && Hit.collider.gameObject.tag == "iconic")
                {
                    //Debug.Log("hit_iconic_elem_at"+ Hit.collider.gameObject.GetComponent<BoxCollider>().center.ToString());
                    GameObject cur = Hit.collider.gameObject;
                    selected_obj_count++;

                    edgeline = Instantiate(EdgeElement, cur.GetComponent<BoxCollider>().center, Quaternion.identity, Objects_parent.transform);
                    edgeline.name = "edge_" + selected_obj_count.ToString();
                    edgeline.tag = "edge";

                    edgeline.GetComponent<EdgeElementScript>().points.Add(cur.GetComponent<BoxCollider>().center);
                    edge_start = cur;
                    CreateEmptyEdgeObjects();

                    //https://generalistprogrammer.com/unity/unity-line-renderer-tutorial/
                    LineRenderer l = edgeline.GetComponent<LineRenderer>();
                    l.material.color = Color.black;
                    l.startWidth = 2f;
                    l.endWidth = 2f;

                    // set up the line renderer
                    l.positionCount = 2;
                    l.SetPosition(0, cur.GetComponent<BoxCollider>().center);// + new Vector3(0, 0, -2f));
                    l.SetPosition(1, cur.GetComponent<BoxCollider>().center);// + new Vector3(1f, 0, -2f));
                                          
                }

            }
            else if (PenTouchInfo.PressedNow)
            {
                // add points to the last line, but check that if an edge line has been created already
                var ray = Camera.main.ScreenPointToRay(PenTouchInfo.penPosition);
                RaycastHit Hit;
                if (Physics.Raycast(ray, out Hit) && Hit.collider.gameObject.tag == "paintable_canvas_object" && edge_start != null)
                {
                    Vector3 vec = Hit.point + new Vector3(0, 0, -5); // Vector3.up * 0.1f;
                    edgeline.GetComponent<LineRenderer>().SetPosition(1, vec);// + new Vector3(0, 0, -2f));
                    edgeline.GetComponent<EdgeElementScript>().points.Add(vec);
                }
            }

            else if (PenTouchInfo.ReleasedThisFrame)
            {
                var ray = Camera.main.ScreenPointToRay(PenTouchInfo.penPosition);

                RaycastHit Hit1;
                Physics.Raycast(ray, out Hit1);

                RaycastHit Hit;
                if (Physics.Raycast(ray, out Hit) && (Hit.collider.gameObject.tag == "temp_edge_primitive" || Hit.collider.gameObject.tag == "iconic")
                    && edge_start != null)
                {

                    if (edgeline.GetComponent<EdgeElementScript>().points.Count > 2)
                    {
                        // assign the end of edge object
                        if (Hit.collider.gameObject.tag == "temp_edge_primitive")
                            edge_end = Hit.collider.transform.parent.gameObject;
                        else
                            edge_end = Hit.collider.gameObject;

                        // compute centroid and bounds
                        /*edgeline.GetComponent<EdgeElementScript>().computeCentroid();
                        edgeline.GetComponent<EdgeElementScript>().computeBounds();
                        */

                        // now create the edge in edge script
                        if (edge_end != null && edge_start != null)
                        {
                            // TODO: IF AN EDGELINE ALREADY EXISTS CONNECTING THESE TWO NODES, THEN DON'T CREATE ANOTHER ONE, DESTROY THE CURRENT.
                            edgeline.GetComponent<EdgeElementScript>().edge_start = edge_start;
                            edgeline.GetComponent<EdgeElementScript>().edge_end = edge_end;

                            // set line renderer end point
                            edgeline.GetComponent<LineRenderer>().SetPosition(1, edge_end.GetComponent<BoxCollider>().center);

                            // assuming edge_start is always an anchor
                            var edgepoints = new List<Vector3>() { edgeline.GetComponent<LineRenderer>().GetPosition(0),
                                edgeline.GetComponent<LineRenderer>().GetPosition(1)};

                            edgeline.GetComponent<EdgeCollider2D>().points = edgepoints.Select(x =>
                            {
                                var pos = edgeline.GetComponent<EdgeCollider2D>().transform.InverseTransformPoint(x);
                                return new Vector2(pos.x, pos.y);
                            }).ToArray();

                            edgeline.GetComponent<EdgeCollider2D>().edgeRadius = 10;
                            
                            // set line renderer texture scale
                            var linedist = Vector3.Distance(edgeline.GetComponent<LineRenderer>().GetPosition(0),
                                edgeline.GetComponent<LineRenderer>().GetPosition(1));
                            edgeline.GetComponent<LineRenderer>().materials[0].mainTextureScale = new Vector2(linedist, 1);

                            //edgeline = edgeline.GetComponent<EdgeElementScript>().FinishEdgeLine();                            
                            GraphCreation();

                            // set edge_end and edge_start back to null
                            edge_end = null;
                            edge_start = null;
                            edgeline = null;
                        }

                    }
                    else
                    {
                        // delete the templine, not enough points
                        Destroy(edgeline);
                        edge_start = null;
                        edge_end = null;
                        edgeline = null;
                    }
                }

                
                // in case a touch was rendered on the canvas (or not on the blue cylinders) and a line didn't finish drawing from source
                else if (Physics.Raycast(ray, out Hit) && Hit.collider.gameObject.tag != "temp_edge_primitive"
                    && edge_start != null)
                {
                    // delete the templine, not correctly drawn from feasible source to target
                    Destroy(edgeline);
                    edge_start = null;
                    edge_end = null;
                    edgeline = null;
                }

                // in all other cases, to be safe, just delete the entire edgeline structure
                else if (edge_start != null)
                {
                    Destroy(edgeline);
                    edge_start = null;
                    edge_end = null;
                    edgeline = null;
                }

                // the touch has ended, destroy all temp edge cylinders now
                GameObject[] tempcyls = GameObject.FindGameObjectsWithTag("temp_edge_primitive");
                for (int k = 0; k < tempcyls.Length; k++)
                {
                    Destroy(tempcyls[k]);
                }
            }

        }
        else
        {
            // just disabled edge button, disable all collider
            if (enablecollider == true)
            {
                enablecollider = false;
                DisableIconicCollider();
                edgeline = null;
            }
        }

        #endregion

       // HANDLE ANY RELEVANT KEY INPUT FOR PAINTABLE'S OPERATIONS
        handleKeyInteractions();
    }

    void GraphCreation()
    {
        // if they are already under the same graph, no need to create a new one. Just assign the new edgeline to the previous parent
        if (edge_start.transform.parent == edge_end.transform.parent && edge_start.transform.parent.tag == "node_parent")
        {
            Transform Prev_graph_parent = edge_start.transform.parent.transform.parent;
            edgeline.transform.parent = Prev_graph_parent.GetChild(1);
            return;
        }

        graph_count++;
        GameObject tempgraph = new GameObject("graph_"+graph_count.ToString());
        tempgraph.tag = "graph";
        tempgraph.transform.parent = Objects_parent.transform;

        GameObject tempnodeparent = new GameObject("node_parent_" + graph_count.ToString());
        tempnodeparent.tag = "node_parent";
        tempnodeparent.transform.parent = tempgraph.transform;
        tempnodeparent.transform.SetSiblingIndex(0);

        GameObject tempedgeparent = new GameObject("edge_parent_" + graph_count.ToString());
        tempedgeparent.tag = "edge_parent";
        tempedgeparent.transform.parent = tempgraph.transform;
        tempedgeparent.transform.SetSiblingIndex(1);

        //assign_the_newly_created_Edge_to_temp_edge_parent_object
        edgeline.transform.parent = tempedgeparent.transform;


        //change_parent
        if (edge_start.transform.parent.tag == "node_parent")
        {
            Transform Prev_node_parent = edge_start.transform.parent;
            Transform Prev_graph_parent = Prev_node_parent.transform.parent;
            Transform Prev_edge_parent = Prev_graph_parent.GetChild(1);
            Transform[] allChildrennode = Prev_node_parent.GetComponentsInChildren<Transform>();
            Transform[] allChildrenedge = Prev_edge_parent.GetComponentsInChildren<Transform>();

            foreach (Transform child in allChildrennode)
            {
                child.parent = tempnodeparent.transform;
            }


            foreach (Transform child in allChildrenedge)
            {
                child.parent = tempedgeparent.transform;
            }
            Destroy(Prev_graph_parent.gameObject);
            Destroy(Prev_node_parent.gameObject);
            Destroy(Prev_edge_parent.gameObject);
        }
        else
        {
            edge_start.transform.parent = tempnodeparent.transform;
        }

        if (edge_end.transform.parent.tag == "node_parent")
        {
            Transform Prev_node_parent = edge_end.transform.parent;
            Transform Prev_graph_parent = Prev_node_parent.transform.parent;
            Transform Prev_edge_parent = Prev_graph_parent.GetChild(1);
            Transform[] allChildrennode = Prev_node_parent.GetComponentsInChildren<Transform>();
            Transform[] allChildrenedge = Prev_edge_parent.GetComponentsInChildren<Transform>();

            foreach (Transform child in allChildrennode)
            {
                child.parent = tempnodeparent.transform;
            }
                        
            
            foreach (Transform child in allChildrenedge)
            {
                child.parent = tempedgeparent.transform;
            }
            Destroy(Prev_graph_parent.gameObject);
            Destroy(Prev_node_parent.gameObject);
            Destroy(Prev_edge_parent.gameObject);
        }
        else
        {
            edge_end.transform.parent = tempnodeparent.transform;
        }             
    }

    void CreateEmptyEdgeObjects()
    {
        // for all penline objects, create an anchor on top of them for a possible edge end
        GameObject[] penobjs = GameObject.FindGameObjectsWithTag("iconic");
        for (int i = 0; i < penobjs.Length; i++)
        {            
                GameObject tempcyl = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                tempcyl.tag = "temp_edge_primitive";
                tempcyl.transform.position = penobjs[i].GetComponent<BoxCollider>().center + new Vector3(0f, 0f, 20f);
                tempcyl.transform.localScale = new Vector3(20f, 20f, 20f);
                tempcyl.transform.Rotate(new Vector3(90f, 0f, 0f));
                tempcyl.transform.parent = penobjs[i].transform;
                tempcyl.GetComponent<Renderer>().material.color = Color.blue;            
        }
    }

    void DisableIconicCollider()
    {
        GameObject[] drawnlist = GameObject.FindGameObjectsWithTag("iconic");

        foreach (GameObject icon in drawnlist)
        {
            if (icon.GetComponent<BoxCollider>() != null)
                icon.GetComponent<BoxCollider>().enabled = false;
        }
    }

    void handleKeyInteractions()
    {
        if (Input.GetKeyUp(KeyCode.L))
        {
            panZoomLocked = !panZoomLocked;
            Debug.Log("panning_value_change"+ panZoomLocked.ToString());
        }

        if (Input.GetKeyUp(KeyCode.Escape))
        {
            //cleanUpRadialCanvas();
        }

        if (Input.GetKeyUp(KeyCode.F10))
        {
            allowOpacity = !allowOpacity;
            Debug.Log("allow opacity: " + allowOpacity.ToString());
        }

        if (Input.GetKeyUp(KeyCode.Delete))
        {
            // delete game objects from history, starting with the latest
            if (history.Count > 0)
            {
                Destroy(history[history.Count - 1]);
                history.RemoveAt(history.Count - 1);
            }
        }

        // test thumbnail
        if (Input.GetKeyUp(KeyCode.T))
        {
            /*
			if (gameObject.transform.childCount > 0)
			{
				RuntimePreviewGenerator.PreviewDirection = new Vector3(0, 0, 1);
				RuntimePreviewGenerator.BackgroundColor = new Color(0.3f, 0.3f, 0.3f, 0f);
				RuntimePreviewGenerator.OrthographicMode = true;

				Sprite action = Sprite.Create(RuntimePreviewGenerator.GenerateModelPreview(gameObject.transform.GetChild(0), 128, 128)
					, new Rect(0, 0, 128, 128), new Vector2(0.5f, 0.5f), 20f);
				GameObject.Find("Action").GetComponent<Image>().sprite = action;
			}
			*/
        }

        // test adding to action history from one template
        if (Input.GetKeyUp(KeyCode.M))
        {
            // This test works
            /*
			GameObject actionhist = GameObject.Find("ActionHistory");
			GameObject listtocopy = actionhist.transform.GetChild(0).GetChild(0).GetChild(1).gameObject;
			GameObject newitem = Instantiate(listtocopy, listtocopy.transform.parent);
			*/

            ActionHistoryEnabled = !ActionHistoryEnabled;

            if (ActionHistoryEnabled) GameObject.Find("Canvas").transform.Find("ActionHistory").gameObject.SetActive(true);
            else GameObject.Find("Canvas").transform.Find("ActionHistory").gameObject.SetActive(false);
        }
    }
}
