using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem.EnhancedTouch;
using UnityEngine.InputSystem;
using System.Linq;
using TMPro;

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
    public Vector2 moveTouchStart;
    public bool previousTouchEnded;
    public GameObject curtouched_obj = null;
    public bool okayToPan = true;
	public bool panZoomLocked = false;
    public bool graphlocked;

    private Vector2 prev_move_pos;

	// Prefabs
	public GameObject IconicElement;
    public GameObject EdgeElement;
    public GameObject SimplicialEdgeElement;
    public GameObject hyperEdgeElement;
    public GameObject CombineLineElement;
    public GameObject GraphElement;

    // Canvas buttons
    public static GameObject iconicElementButton;
    public static GameObject pan_button;
    public static GameObject graph_pen_button;
    public static GameObject simplicial_pen_button;
    public static GameObject hyper_pen_button;
    public static GameObject eraser_button;
    public static GameObject copy_button;
    public static GameObject stroke_combine_button;
    public static GameObject canvas_radial;

    public GameObject edgeline;
    public GameObject simplicialline;
    public GameObject hyperline;
    public GameObject setline;
    public GameObject edge_radial_menu;
    public GameObject node_radial_menu;
    public GameObject graph_radial_menu;

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

    // simplicial edge paint control
    public List<Vector3> SimplicialVertices = new List<Vector3>();
    public List<GameObject> Simplicialnodes= new List<GameObject>();

    // hyper edge paint control
    public List<Vector3> hyperVertices = new List<Vector3>();
    public List<GameObject> hypernodes = new List<GameObject>();

    // needed for graph
    public static int graph_count = 0;

    // needed for panning
    private bool taping_flag;
    int LastPhaseHappend; // 1 = S, 2 = M, 3 = E
    float TouchTime; // Time elapsed between touch beginning and ending
    float StartTouchTime; // Time.realtimeSinceStartup at start of touch
    float EndTouchTime; // Time.realtimeSinceStartup at end of touch
    public Vector2 startPos;

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
        simplicial_pen_button = GameObject.Find("SimplicialPen");
        hyper_pen_button = GameObject.Find("HyperPen"); 
        eraser_button = GameObject.Find("Eraser");
        copy_button = GameObject.Find("Copy");
        stroke_combine_button = GameObject.Find("StrokeCombine");
        
        canvas_radial = GameObject.Find("canvas_radial");
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
                    templine.GetComponent<iconicElementScript>().icon_number = totalLines;

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
        

        #endregion

        #region pan

        // Handle screen touches.                     
        //https://docs.unity3d.com/ScriptReference/TouchPhase.Moved.html

        
        if (Input.touchCount == 2 && !panZoomLocked) // && pan_button.GetComponent<PanButtonBehavior>().selected)
        {
            //Debug.Log("double_finger_tap");
            // NO ANCHOR TAPPED, JUST ZOOM IN/PAN
            //main_camera.GetComponent<MobileTouchCamera>().enabled = true;

            UnityEngine.Touch touchzero = Input.GetTouch(0); 
            UnityEngine.Touch touchone = Input.GetTouch(1);

            Vector2 touchzeroprevpos = touchzero.position - touchzero.deltaPosition;
            Vector2 touchoneprevpos = touchone.position - touchone.deltaPosition;

            float prevmag = (touchzeroprevpos - touchoneprevpos).magnitude;
            float currmag = (touchzero.position - touchone.position).magnitude;

            float difference = currmag - prevmag;

            //Debug.Log("diff: " + difference + ", multiplier: " + zoom_multiplier + ", camera size: " + (Camera.main.orthographicSize - zoom_multiplier * difference).ToString());
            //Debug.Log("Clamped: " + Mathf.Clamp(zoom_multiplier * (Camera.main.orthographicSize - difference), zoom_min, zoom_max));

            Camera.main.orthographicSize = Mathf.Clamp(Camera.main.orthographicSize - zoom_multiplier * difference, zoom_min, zoom_max);

            // CHECK AND DELETE INCOMPLETE LINES
            deleteTempLineIfDoubleFinger();

            int zoom = (int)((1f - ((main_camera.orthographicSize - zoom_min) / zoom_max)) * 100f);
            GameObject.Find("text_message_worldspace").GetComponent<TextMeshProUGUI>().text = zoom.ToString("F0") + "%";


        }
        else if (Input.touchCount == 1 && !panZoomLocked)
        {            
            UnityEngine.Touch activeTouches = Input.GetTouch(0);

            // Only pan when the touch is on top of the canvas. Otherwise,
            var ray = Camera.main.ScreenPointToRay(activeTouches.position);
            RaycastHit Hit;

            if (Physics.Raycast(ray, out Hit) && Hit.collider.gameObject.tag != "simplicial")
            {
                
                GameObject temp = Hit.collider.gameObject;

                if (activeTouches.phase == UnityEngine.TouchPhase.Ended && okayToPan)
                {
                    previousTouchEnded = true;
                    if (curtouched_obj.tag == "iconic")
                        curtouched_obj.transform.localScale = new Vector3(1f, 1f, 1f);
                }

                else if (activeTouches.phase == UnityEngine.TouchPhase.Began && okayToPan)
                {
                    curtouched_obj = temp;                   

                    if (curtouched_obj.tag == "iconic")
                    {
                        //does_not_work
                        //curtouched_obj.GetComponent<MeshRenderer>().material.color = Color.red;
                        curtouched_obj.transform.localScale = new Vector3(1.25f, 1.25f, 1.25f);
                    }                        
                }

                else if (activeTouches.phase == UnityEngine.TouchPhase.Moved && previousTouchEnded && okayToPan )//&& (curtouched_obj == temp))
                {
                    panTouchStart = Camera.main.ScreenToWorldPoint(activeTouches.position);
                    //Debug.Log("touch start: " + panTouchStart.ToString());

                    previousTouchEnded = false;
                    prev_move_pos = panTouchStart;
                }

                else if (activeTouches.phase == UnityEngine.TouchPhase.Moved && !previousTouchEnded && okayToPan)// && (curtouched_obj == temp))
                {
                    if (Vector2.Distance(prev_move_pos, (Vector2)Camera.main.ScreenToWorldPoint(activeTouches.position)) > 2)
                    {
                        Vector2 panDirection = panTouchStart - (Vector2)Camera.main.ScreenToWorldPoint(activeTouches.position);
                        //Debug.Log("position changed from "+ Camera.main.transform.position.ToString() + " to " + (Camera.main.transform.position + (Vector3)panDirection).ToString());

                        if (curtouched_obj.tag == "paintable_canvas_object")
                        {
                            Camera.main.transform.position += (Vector3)panDirection;
                        }
                        else if (curtouched_obj.tag == "iconic")
                        {
                            curtouched_obj.transform.position -= (Vector3)panDirection;
                            curtouched_obj.GetComponent<iconicElementScript>().edge_position -= (Vector3)panDirection;
                            searchNodeAndUpdateEdge(curtouched_obj, panDirection);
                        }
                        else if (curtouched_obj.tag == "hyper")
                        {
                            curtouched_obj.transform.position -= (Vector3)panDirection;
                            curtouched_obj.GetComponent<HyperElementScript>().UpdateChildren();
                        }

                        prev_move_pos = (Vector2)Camera.main.ScreenToWorldPoint(activeTouches.position);
                    }
                    
                }

            }

            OnShortTap();
            // if not panning, check for tap
            /*if (activeTouches.isTap)
            {
                // delegate single tap to short tap function
                Debug.Log("tap detected.");
                OnShortTap(activeTouches[0].screenPosition);
                OnLongTap(activeTouches[0].screenPosition);
            }*/
        }
        else
        {
            previousTouchEnded = true;
            //main_camera.GetComponent<MobileTouchCamera>().enabled = false;
            GameObject.Find("text_message_worldspace").GetComponent<TextMeshProUGUI>().text = "";
        }

        #endregion

        #region Graph Pen
        if (graph_pen_button.GetComponent<AllButtonsBehaviors>().selected)
        {
            
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

                    edgeline = Instantiate(EdgeElement, cur.GetComponent<iconicElementScript>().edge_position, Quaternion.identity, Objects_parent.transform);
                    edgeline.name = "edge_" + selected_obj_count.ToString();
                    edgeline.tag = "edge";

                    edgeline.GetComponent<EdgeElementScript>().points.Add(cur.GetComponent<iconicElementScript>().edge_position);
                    edge_start = cur;
                    CreateEmptyEdgeObjects();

                    //https://generalistprogrammer.com/unity/unity-line-renderer-tutorial/
                    LineRenderer l = edgeline.GetComponent<LineRenderer>();
                    l.material.color = Color.black;
                    l.startWidth = 2f;
                    l.endWidth = 2f;

                    // set up the line renderer
                    l.positionCount = 2;
                    l.SetPosition(0, cur.GetComponent<iconicElementScript>().edge_position);// + new Vector3(0, 0, -2f));
                    l.SetPosition(1, cur.GetComponent<iconicElementScript>().edge_position + new Vector3(1f, 0, -2f));
                                          
                }

            }
            else if (PenTouchInfo.PressedNow)
            {
                // add points to the last line, but check that if an edge line has been created already
                var ray = Camera.main.ScreenPointToRay(PenTouchInfo.penPosition);
                RaycastHit Hit;
                if (Physics.Raycast(ray, out Hit) && Hit.collider.gameObject.tag == "paintable_canvas_object" && edge_start != null)
                {
                    Vector3 vec = Hit.point + new Vector3(0, 0, -5f); // Vector3.up * 0.1f;                    
                    edgeline.GetComponent<LineRenderer>().SetPosition(1, vec);// + new Vector3(0, 0, -2f));
                    edgeline.GetComponent<EdgeElementScript>().points.Add(vec);
                }
            }

            else if (PenTouchInfo.ReleasedThisFrame)
            {
                var ray = Camera.main.ScreenPointToRay(PenTouchInfo.penPosition);


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
                            edgeline.GetComponent<LineRenderer>().SetPosition(1, edge_end.GetComponent<iconicElementScript>().edge_position);

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
                            edgeline.GetComponent<EdgeElementScript>().addDot();
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
                DeleteEmptyEdgeObjects();
            }

        }

        #endregion

        #region Simplicial Pen
        if (simplicial_pen_button.GetComponent<AllButtonsBehaviors>().selected)
        {

            if (PenTouchInfo.PressedThisFrame)//currentPen.tip.wasPressedThisFrame)
            {
                // start drawing a new line
                var ray = Camera.main.ScreenPointToRay(PenTouchInfo.penPosition);
                RaycastHit Hit;
                if (Physics.Raycast(ray, out Hit) && Hit.collider.gameObject.tag == "iconic")
                {
                    //Debug.Log("hit_iconic_elem_at"+ Hit.collider.gameObject.GetComponent<BoxCollider>().center.ToString());
                    GameObject cur = Hit.collider.gameObject;
                    SimplicialVertices.Add(cur.GetComponent<iconicElementScript>().edge_position);
                    Simplicialnodes.Add(cur);

                    if (SimplicialVertices.Count == 3)
                    {
                        selected_obj_count++;
                        simplicialline = Instantiate(SimplicialEdgeElement, Objects_parent.transform.position, Quaternion.identity, Objects_parent.transform);
                        simplicialline.name = "simplicial_" + selected_obj_count.ToString();
                        simplicialline.tag = "simplicial";

                        simplicialline.GetComponent<SimplicialElementScript>().theVertices = new List<Vector3>(SimplicialVertices);
                        simplicialline.GetComponent<SimplicialElementScript>().thenodes = new List<GameObject>(Simplicialnodes);

                        simplicialline.GetComponent<SimplicialElementScript>().updatePolygon();
                        SimplicialCreation();
                        SimplicialVertices.Clear();
                        Simplicialnodes.Clear();
                        DeleteEmptyEdgeObjects();
                        simplicialline = null;
                    }
                    else if (SimplicialVertices.Count == 1)
                    {
                        CreateEmptyEdgeObjects();
                    }                                         

                }
                else
                {
                    // some icons might have been touched before, remove those
                    if (SimplicialVertices.Count > 0)
                    {
                        SimplicialVertices.Clear();
                        Simplicialnodes.Clear();
                        DeleteEmptyEdgeObjects();
                    }
                }
            }
                        
        }
        #endregion

        #region hypergraph Pen
        if (hyper_pen_button.GetComponent<AllButtonsBehaviors>().selected)
        {

            if (PenTouchInfo.PressedThisFrame)//currentPen.tip.wasPressedThisFrame)
            {
                // start drawing a new line
                var ray = Camera.main.ScreenPointToRay(PenTouchInfo.penPosition);
                RaycastHit Hit;
                if (Physics.Raycast(ray, out Hit) && Hit.collider.gameObject.tag == "iconic")
                {
                    //Debug.Log("hit_iconic_elem_at"+ Hit.collider.gameObject.GetComponent<BoxCollider>().center.ToString());
                    GameObject cur = Hit.collider.gameObject;
                    hyperVertices.Add(cur.GetComponent<iconicElementScript>().edge_position);
                    hypernodes.Add(cur);

                    if (hyperVertices.Count == 3)
                    {
                        selected_obj_count++;

                        //instantiate the hyper node in the middle of the selected nodes
                        Vector3 vec = new Vector3(0, 0, 0); 
                        foreach (Vector3 child in hyperVertices)
                        {
                            vec.x += child.x;
                            vec.y += child.y;
                            vec.z += child.z;
                        }

                        vec.x /= hyperVertices.Count;
                        vec.y /= hyperVertices.Count;
                        vec.z /= hyperVertices.Count;

                        hyperline = Instantiate(hyperEdgeElement, vec, Quaternion.identity, Objects_parent.transform);
                        hyperline.name = "hyper_" + selected_obj_count.ToString();
                        hyperline.tag = "hyper";

                        hyperline.GetComponent<HyperElementScript>().theVertices = new List<Vector3>(hyperVertices);
                        hyperline.GetComponent<HyperElementScript>().thenodes = new List<GameObject>(hypernodes);

                        hyperline.GetComponent<HyperElementScript>().addChildren();
                        hypergraphCreation();
                        hyperVertices.Clear();
                        hypernodes.Clear();
                        DeleteEmptyEdgeObjects();
                        hyperline = null;
                    }
                    else if (hyperVertices.Count == 1)
                    {
                        CreateEmptyEdgeObjects();
                    }

                }
                else
                {
                    // some icons might have been touched before, remove those
                    if (hyperVertices.Count > 0)
                    {
                        hyperVertices.Clear();
                        hypernodes.Clear();
                        DeleteEmptyEdgeObjects();
                    }
                }
            }

        }
        #endregion

        // ERASER BRUSH
        #region eraser
        if (PenTouchInfo.PressedNow && eraser_button.GetComponent<AllButtonsBehaviors>().selected)
        {
            
            var ray = Camera.main.ScreenPointToRay(PenTouchInfo.penPosition);

            RaycastHit Hit;
            RaycastHit2D hit2d;

            if (Physics.Raycast(ray, out Hit))
            {
                // handle individually -- in case a type requires special treatment or extra code in the future
                // delete edges which has ties to erased nodes (sets, penlines, functions etc.)

                if (Hit.collider.gameObject.tag == "iconic")
                {
                    string possible_edge_node_name = Hit.collider.gameObject.name;
                    searchNodeAndDeleteEdge(possible_edge_node_name);
                    Destroy(Hit.collider.gameObject);
                }
                // simplicial edge
                else if (Hit.collider.gameObject.tag == "simplicial")
                {
                    //searchNodeAndDeleteEdge(possible_edge_node_name);
                    Destroy(Hit.collider.gameObject);
                }
                // set anchor
                else if(Hit.collider.gameObject.tag == "hyper")
                {
                    //searchNodeAndDeleteEdge(possible_edge_node_name);
                    Destroy(Hit.collider.gameObject);
                }             


            }

            hit2d = Physics2D.GetRayIntersection(ray);
            if (hit2d.collider != null && hit2d.collider.gameObject.tag == "edge")
            {
                Destroy(hit2d.collider.gameObject);
            }
            else if (hit2d.collider != null && hit2d.collider.gameObject.tag == "simplicial")
            {
                Destroy(hit2d.collider.gameObject);
            }
        }
        #endregion

        #region stroke combine brush

        if (stroke_combine_button.GetComponent<AllButtonsBehaviors>().selected)
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

                    Vector3 vec = Hit.point + new Vector3(0, 0, -5);
                    setline = Instantiate(CombineLineElement, vec, Quaternion.identity, Objects_parent.transform);
                    setline.GetComponent<TrailRenderer>().material.color = Color.black;

                    setline.name = "temp_set_line";
                    setline.GetComponent<iconicElementScript>().points.Add(vec);

                }
            }

            else if (setline != null &&
                PenTouchInfo.PressedNow //currentPen.tip.isPressed
                && (PenTouchInfo.penPosition -
                (Vector2)setline.GetComponent<iconicElementScript>().points[setline.GetComponent<iconicElementScript>().points.Count - 1]).magnitude > 0f)
            {
                // add points to the last line
                var ray = Camera.main.ScreenPointToRay(PenTouchInfo.penPosition);
                RaycastHit Hit;
                if (Physics.Raycast(ray, out Hit) && Hit.collider.gameObject.name == "Paintable")
                {

                    Vector3 vec = Hit.point + new Vector3(0, 0, -5); // Vector3.up * 0.1f;

                    setline.GetComponent<TrailRenderer>().transform.position = vec;
                    setline.GetComponent<iconicElementScript>().points.Add(vec);
                    setline.GetComponent<iconicElementScript>().calculateLengthAttributeFromPoints();

                    // pressure based pen width
                    setline.GetComponent<iconicElementScript>().updateLengthFromPoints();
                    setline.GetComponent<iconicElementScript>().addPressureValue(PenTouchInfo.pressureValue);
                    setline.GetComponent<iconicElementScript>().reNormalizeCurveWidth();
                    setline.GetComponent<TrailRenderer>().widthCurve = setline.GetComponent<iconicElementScript>().widthcurve;

                }
            }

            else if (setline != null && PenTouchInfo.ReleasedThisFrame)
            {
                var ray = Camera.main.ScreenPointToRay(PenTouchInfo.penPosition); //currentPen.position.ReadValue());// Input.GetTouch(0).position);
                RaycastHit Hit;

                if (Physics.Raycast(ray, out Hit) && Hit.collider.gameObject.name == "Paintable")
                {
                    if (setline.GetComponent<iconicElementScript>().points.Count > min_point_count)
                    {
                        
                        List<GameObject> icon_meshobjs = new List<GameObject>();
                        GameObject[] iconarray = GameObject.FindGameObjectsWithTag("iconic");
                                                
                        for (int i = 0; i < iconarray.Length; i++)
                        {
                            
                            // check if the lines are inside the drawn set polygon -- in respective local coordinates
                            if (setline.GetComponent<iconicElementScript>().isInsidePolygon(
                                //setline.GetComponent<iconicElementScript>().transform.InverseTransformPoint(
                                iconarray[i].GetComponent<iconicElementScript>().edge_position)
                                )//)
                                {
                                    icon_meshobjs.Add(iconarray[i].transform.gameObject);
                                    //penLines[i].transform.SetParent(templine.transform);
                                    //Debug.Log("iconic found");

                                }
                        }

                        GameObject[] selected_icons = new GameObject[icon_meshobjs.Count];
                        int temp_iter = 0;
                        foreach (var p in icon_meshobjs)
                        {
                            selected_icons[temp_iter] = p;
                            temp_iter += 1;
                        }

                        Destroy(setline);
                        transform.GetComponent<CreatePrimitives>().CreatePenLine(selected_icons);
                        setline = null;
                    }
                    else
                    {
                        // delete the templine, not enough points
                        // Debug.Log("here_in_destroy");
                        Destroy(setline);
                        setline = null;
                    }
                }
                else
                {
                    // the touch didn't end on a line, destroy the line
                    // Debug.Log("here_in_destroy_different_Hit");
                    Destroy(setline);
                    setline = null;
                }

            }

        }

        #endregion

        
        // HANDLE ANY RELEVANT KEY INPUT FOR PAINTABLE'S OPERATIONS
        handleKeyInteractions();
    }

    //https://stackoverflow.com/questions/38728714/unity3d-how-to-detect-taps-on-android
    void OnShortTap()
    {
        UnityEngine.Touch currentTouch = Input.GetTouch(0);
        if (canvas_radial.transform.childCount > 0)
        {
            for (int i = 0; i < canvas_radial.transform.childCount; i++)
            {
                Destroy(canvas_radial.transform.GetChild(i).gameObject);
            }
        }
        switch (currentTouch.phase)
        {
            case UnityEngine.TouchPhase.Began:
                if (LastPhaseHappend != 1)
                {
                    StartTouchTime = Time.realtimeSinceStartup;
                    taping_flag = true;
                }
                LastPhaseHappend = 1;
                startPos = currentTouch.position;
                break;

            case UnityEngine.TouchPhase.Moved:
                //if (LastPhaseHappend != 2)
                if (Vector2.Distance(currentTouch.position, startPos) > 2)
                {
                    taping_flag = false;
                }
                LastPhaseHappend = 2;
                break;

            case UnityEngine.TouchPhase.Ended:
                if (LastPhaseHappend != 3)
                {
                    EndTouchTime = Time.realtimeSinceStartup;
                    TouchTime = EndTouchTime - StartTouchTime;
                }
                LastPhaseHappend = 3;

                if (taping_flag && TouchTime > 0.5f)
                // TouchTime for a tap can be further defined
                {
                    //Tap has happened;
                    Debug.Log("tap_detected for duration: " + TouchTime.ToString());
                    if (TouchTime > 1f)
                        graphlocked = true;
                    else
                        graphlocked = false;

                    menucreation(currentTouch.position);
                }
                break;
        }
        
    }

    void menucreation(Vector2 menu_position)
    {

        if (canvas_radial.transform.childCount > 0)
        {
            return;
        }

        var ray = Camera.main.ScreenPointToRay(menu_position);

        RaycastHit Hit;
        RaycastHit2D hit2d;

        if (Physics.Raycast(ray, out Hit))
        {

            if (Hit.collider.gameObject.tag == "iconic")
            {
                if(graphlocked)
                {
                    Transform node_parent = Hit.collider.gameObject.transform.parent;
                    if (node_parent.tag == "node_parent")
                    {
                        //node_parent.parent.GetComponent<GraphElementScript>().Graph_as_Str();
                        GameObject radmenu = Instantiate(graph_radial_menu,
                            canvas_radial.transform.TransformPoint(Hit.collider.gameObject.GetComponent<iconicElementScript>().edge_position + new Vector3(5f, 5f, 0f))
                            /*Hit.collider.gameObject.GetComponent<iconicElementScript>().edge_position*/,
                            Quaternion.identity,
                            canvas_radial.transform);
                        radmenu.GetComponent<GraphSliderMenu>().setparent(node_parent.parent.gameObject);
                        radmenu.GetComponent<GraphSliderMenu>().UpdateLayer(node_parent.parent.GetComponent<GraphElementScript>().abstraction_layer);
                    }                    
                }
                else
                {
                    GameObject radmenu = Instantiate(node_radial_menu,
                            canvas_radial.transform.TransformPoint(Hit.collider.gameObject.GetComponent<iconicElementScript>().edge_position)
                            /*Hit.collider.gameObject.GetComponent<iconicElementScript>().edge_position*/,
                            Quaternion.identity,
                            canvas_radial.transform);
                    radmenu.GetComponent<NodeMenuScript>().menu_parent = Hit.collider.gameObject;
                }
            }


        }

        hit2d = Physics2D.GetRayIntersection(ray);
        if (hit2d.collider != null && hit2d.collider.gameObject.tag == "edge")
        {
            GameObject radmenu = Instantiate(edge_radial_menu,
                        canvas_radial.transform.TransformPoint(hit2d.collider.gameObject.GetComponent<EdgeCollider2D>().bounds.center)
                        /*hit2d.collider.gameObject.GetComponent<EdgeCollider2D>().bounds.center*/,
                        Quaternion.identity,
                        canvas_radial.transform);
            radmenu.GetComponent<EdgeMenuScript>().menu_parent = hit2d.collider.gameObject;
        }

       
    }

    // normal simple graph
    void GraphCreation()
    {
        // if they are already under the same graph, no need to create a new one. Just assign the new edgeline to the previous parent
        if (edge_start.transform.parent == edge_end.transform.parent && edge_start.transform.parent.tag == "node_parent")
        {
            Transform Prev_graph_parent = edge_start.transform.parent.transform.parent;
            edgeline.transform.parent = Prev_graph_parent.GetChild(1);
            Prev_graph_parent.GetComponent<GraphElementScript>().edges_as_Str();
            return;
        }

        graph_count++;
        GameObject tempgraph = Instantiate(GraphElement);
        tempgraph.name = "graph_"+graph_count.ToString();
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

        GameObject tempsimplicialparent = new GameObject("simplicial_parent_" + graph_count.ToString());
        tempsimplicialparent.tag = "simplicial_parent";
        tempsimplicialparent.transform.parent = tempgraph.transform;
        tempsimplicialparent.transform.SetSiblingIndex(2);

        GameObject temphyperparent = new GameObject("hyper_parent_" + graph_count.ToString());
        temphyperparent.tag = "hyper_parent";
        temphyperparent.transform.parent = tempgraph.transform;
        temphyperparent.transform.SetSiblingIndex(3);

        //assign_the_newly_created_Edge_to_temp_edge_parent_object
        edgeline.transform.parent = tempedgeparent.transform;

        List<GameObject> temp_edge_obj = new List<GameObject>();
        temp_edge_obj.Add(edge_start);
        temp_edge_obj.Add(edge_end);

        //change_parent
        foreach (GameObject each_node in temp_edge_obj)
        {
            //change_parent 
            // if already in a graph, change parent of every siblings of it,but make sure not under the current graph
            if (each_node.transform.parent.tag == "node_parent" && each_node.transform.parent != tempnodeparent.transform)
            {
                Transform Prev_node_parent = each_node.transform.parent;
                Transform Prev_graph_parent = Prev_node_parent.transform.parent;
                Transform Prev_edge_parent = Prev_graph_parent.GetChild(1);
                Transform Prev_simplicial_parent = Prev_graph_parent.GetChild(2);
                Transform Prev_hyper_parent = Prev_graph_parent.GetChild(3);
                Transform[] allChildrennode = Prev_node_parent.GetComponentsInChildren<Transform>();
                Transform[] allChildrenedge = Prev_edge_parent.GetComponentsInChildren<Transform>();
                Transform[] allChildrensimpli = Prev_simplicial_parent.GetComponentsInChildren<Transform>();
                Transform[] allChildrenhyper = Prev_hyper_parent.GetComponentsInChildren<Transform>();

                foreach (Transform child in allChildrennode)
                {
                    child.parent = tempnodeparent.transform;
                }

                foreach (Transform child in allChildrenedge)
                {
                    if (child.tag == "edge")
                        child.parent = tempedgeparent.transform;
                }

                foreach (Transform child in allChildrensimpli)
                {
                    if (child.tag == "simplicial")
                        child.parent = tempsimplicialparent.transform;
                }

                foreach (Transform child in allChildrenhyper)
                {
                    if (child.tag == "hyper")
                        child.parent = temphyperparent.transform;
                }

                Destroy(Prev_graph_parent.gameObject);
                Destroy(Prev_node_parent.gameObject);
                Destroy(Prev_edge_parent.gameObject);
                Destroy(Prev_simplicial_parent.gameObject);
                Destroy(Prev_hyper_parent.gameObject);
            }
            else
            {
                each_node.transform.parent = tempnodeparent.transform;
            }
        }

        tempgraph.GetComponent<GraphElementScript>().Graph_as_Str();
    }

    // normal simple graph
    void SimplicialCreation()
    {
        // if they are already under the same graph, no need to create a new one. Just assign the new edgeline to the previous parent
        bool share_same_parent = true;
        for (int x = 1; x < (Simplicialnodes.Count); x++)
        {
            if (Simplicialnodes[x].transform.parent != Simplicialnodes[x-1].transform.parent || Simplicialnodes[x].transform.parent.tag != "node_parent")
            {
                share_same_parent = false;
                break;
            }
        }
        if (share_same_parent)
        {
            Transform Prev_graph_parent = Simplicialnodes[0].transform.parent.transform.parent;
            simplicialline.transform.parent = Prev_graph_parent.GetChild(2);
            Prev_graph_parent.GetComponent<GraphElementScript>().simplicial_as_Str();
            return;
        }

        graph_count++;
        GameObject tempgraph = Instantiate(GraphElement);
        tempgraph.name = "graph_" + graph_count.ToString();
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

        GameObject tempsimplicialparent = new GameObject("simplicial_parent_" + graph_count.ToString());
        tempsimplicialparent.tag = "simplicial_parent";
        tempsimplicialparent.transform.parent = tempgraph.transform;
        tempsimplicialparent.transform.SetSiblingIndex(2);

        GameObject temphyperparent = new GameObject("hyper_parent_" + graph_count.ToString());
        temphyperparent.tag = "hyper_parent";
        temphyperparent.transform.parent = tempgraph.transform;
        temphyperparent.transform.SetSiblingIndex(3);

        //assign_the_newly_created_simplicial_edge_to_temp_siplicial_parent_object
        simplicialline.transform.parent = tempsimplicialparent.transform;

        foreach (GameObject each_node in Simplicialnodes)
        {
            //change_parent 
            // if already in a graph, change parent of every siblings of it
            if (each_node.transform.parent.tag == "node_parent" && each_node.transform.parent != tempnodeparent.transform)
            {
                Transform Prev_node_parent = each_node.transform.parent;
                Transform Prev_graph_parent = Prev_node_parent.transform.parent;
                Transform Prev_edge_parent = Prev_graph_parent.GetChild(1);
                Transform Prev_simplicial_parent = Prev_graph_parent.GetChild(2);
                Transform Prev_hyper_parent = Prev_graph_parent.GetChild(3);
                Transform[] allChildrennode = Prev_node_parent.GetComponentsInChildren<Transform>();
                Transform[] allChildrenedge = Prev_edge_parent.GetComponentsInChildren<Transform>();
                Transform[] allChildrensimpli = Prev_simplicial_parent.GetComponentsInChildren<Transform>();
                Transform[] allChildrenhyper = Prev_hyper_parent.GetComponentsInChildren<Transform>();

                foreach (Transform child in allChildrennode)
                {
                    child.parent = tempnodeparent.transform;
                }

                foreach (Transform child in allChildrenedge)
                {
                    if (child.tag == "edge")
                        child.parent = tempedgeparent.transform;
                }

                foreach (Transform child in allChildrensimpli)
                {
                    if (child.tag == "simplicial")
                        child.parent = tempsimplicialparent.transform;
                }

                foreach (Transform child in allChildrenhyper)
                {
                    if (child.tag == "hyper")
                        child.parent = temphyperparent.transform;
                }

                Destroy(Prev_graph_parent.gameObject);
                Destroy(Prev_node_parent.gameObject);
                Destroy(Prev_edge_parent.gameObject);
                Destroy(Prev_simplicial_parent.gameObject);
                Destroy(Prev_hyper_parent.gameObject);
            }
            else
            {
                each_node.transform.parent = tempnodeparent.transform;
            }
        }

        tempgraph.GetComponent<GraphElementScript>().Graph_as_Str();
    }

    void hypergraphCreation()
    {
        // if they are already under the same graph, no need to create a new one. Just assign the new edgeline to the previous parent
        bool share_same_parent = true;
        for (int x = 1; x < (hypernodes.Count); x++)
        {
            if (hypernodes[x].transform.parent != hypernodes[x - 1].transform.parent || hypernodes[x].transform.parent.tag != "node_parent")
            {
                share_same_parent = false;
                break;
            }
        }
        if (share_same_parent)
        {
            Transform Prev_graph_parent = hypernodes[0].transform.parent.transform.parent;
            // 3 is hyper edge parent index
            hyperline.transform.parent = Prev_graph_parent.GetChild(3);
            Prev_graph_parent.GetComponent<GraphElementScript>().hyperedges_as_Str();
            return;
        }

        graph_count++;
        GameObject tempgraph = Instantiate(GraphElement);
        tempgraph.name = "graph_" + graph_count.ToString();
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

        GameObject tempsimplicialparent = new GameObject("simplicial_parent_" + graph_count.ToString());
        tempsimplicialparent.tag = "simplicial_parent";
        tempsimplicialparent.transform.parent = tempgraph.transform;
        tempsimplicialparent.transform.SetSiblingIndex(2);

        GameObject temphyperparent = new GameObject("hyper_parent_" + graph_count.ToString());
        temphyperparent.tag = "hyper_parent";
        temphyperparent.transform.parent = tempgraph.transform;
        temphyperparent.transform.SetSiblingIndex(3);

        //assign_the_newly_created_simplicial_edge_to_temp_siplicial_parent_object
        hyperline.transform.parent = temphyperparent.transform;

        foreach (GameObject each_node in hypernodes)
        {
            //change_parent 
            // if already in a graph, change parent of every siblings of it
            if (each_node.transform.parent.tag == "node_parent" && each_node.transform.parent != tempnodeparent.transform)
            {
                Transform Prev_node_parent = each_node.transform.parent;
                Transform Prev_graph_parent = Prev_node_parent.transform.parent;
                Transform Prev_edge_parent = Prev_graph_parent.GetChild(1);
                Transform Prev_simplicial_parent = Prev_graph_parent.GetChild(2);
                Transform Prev_hyper_parent = Prev_graph_parent.GetChild(3);
                Transform[] allChildrennode = Prev_node_parent.GetComponentsInChildren<Transform>();
                Transform[] allChildrenedge = Prev_edge_parent.GetComponentsInChildren<Transform>();
                Transform[] allChildrensimpli = Prev_simplicial_parent.GetComponentsInChildren<Transform>();
                Transform[] allChildrenhyper = Prev_hyper_parent.GetComponentsInChildren<Transform>();

                foreach (Transform child in allChildrennode)
                {
                    child.parent = tempnodeparent.transform;
                }

                foreach (Transform child in allChildrenedge)
                {
                    if (child.tag == "edge")
                        child.parent = tempedgeparent.transform;
                }

                foreach (Transform child in allChildrensimpli)
                {
                    if (child.tag == "simplicial")
                        child.parent = tempsimplicialparent.transform;
                }

                foreach (Transform child in allChildrenhyper)
                {
                    if (child.tag == "hyper")
                        child.parent = temphyperparent.transform;
                }

                Destroy(Prev_graph_parent.gameObject);
                Destroy(Prev_node_parent.gameObject);
                Destroy(Prev_edge_parent.gameObject);
                Destroy(Prev_simplicial_parent.gameObject);
                Destroy(Prev_hyper_parent.gameObject);
            }
            else
            {
                each_node.transform.parent = tempnodeparent.transform;
            }
        }

        tempgraph.GetComponent<GraphElementScript>().Graph_as_Str();
    }

    void CreateEmptyEdgeObjects()
    {
        // for all penline objects, create an anchor on top of them for a possible edge end
        GameObject[] penobjs = GameObject.FindGameObjectsWithTag("iconic");
        for (int i = 0; i < penobjs.Length; i++)
        {            
                GameObject tempcyl = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                tempcyl.tag = "temp_edge_primitive";
                tempcyl.transform.position = penobjs[i].GetComponent<iconicElementScript>().edge_position + new Vector3(0f, 0f, 20f);
                tempcyl.transform.localScale = new Vector3(20f, 20f, 20f);
                tempcyl.transform.Rotate(new Vector3(90f, 0f, 0f));
                tempcyl.transform.parent = penobjs[i].transform;
                tempcyl.GetComponent<Renderer>().material.color = Color.blue;            
        }
    }

    public void DeleteEmptyEdgeObjects()
    {
        GameObject[] tempcyls = GameObject.FindGameObjectsWithTag("temp_edge_primitive");
        for (int k = 0; k < tempcyls.Length; k++)
        {
            Destroy(tempcyls[k]);
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

    void searchNodeAndDeleteEdge(string node_name)
    {
        GameObject[] edges = GameObject.FindGameObjectsWithTag("edge");
        List<GameObject> edgeList = new List<GameObject>(edges);
        for (int i = 0; i < edgeList.Count; i++)
        {
            GameObject source = edgeList[i].GetComponent<EdgeElementScript>().edge_start;
            GameObject target = edgeList[i].GetComponent<EdgeElementScript>().edge_end;

            if (source.name == node_name || target.name == node_name)
            {
                Destroy(edgeList[i].gameObject);
                //break;
            }
        }

        GameObject[] simplicials = GameObject.FindGameObjectsWithTag("simplicial");
        foreach (GameObject each_simplicial in simplicials)
        {
            if (each_simplicial.GetComponent<SimplicialElementScript>() != null)
            {
                List<GameObject> thenodes = each_simplicial.GetComponent<SimplicialElementScript>().thenodes;

                foreach (GameObject each_node in thenodes)
                {
                    if (each_node.name == node_name)
                    {
                        Destroy(each_simplicial.gameObject);
                        break;
                    }
                }
            }
            else if (each_simplicial.GetComponent<EdgeElementScript>() != null)
            {
                GameObject source = each_simplicial.GetComponent<EdgeElementScript>().edge_start;
                GameObject target = each_simplicial.GetComponent<EdgeElementScript>().edge_end;

                if (source.name == node_name || target.name == node_name)
                {
                    Destroy(each_simplicial.gameObject);
                    //break;
                }
            }

        }

        GameObject[] hyper_edges = GameObject.FindGameObjectsWithTag("hyper_child_edge");
        foreach (GameObject each_child_edge in hyper_edges)
        {
            if (each_child_edge.GetComponent<HyperEdgeElement>().parent_node.name == node_name)
            {
                Destroy(each_child_edge);
            }
        }
    }

    void searchNodeAndUpdateEdge(GameObject node_name, Vector3 panDirection)
    {
        GameObject[] edges = GameObject.FindGameObjectsWithTag("edge");
        List<GameObject> edgeList = new List<GameObject>(edges);
        for (int i = 0; i < edgeList.Count; i++)
        {
            edgeList[i].GetComponent<EdgeElementScript>().updateEndPoint(node_name);            
        }

        GameObject[] simplicials = GameObject.FindGameObjectsWithTag("simplicial");
        foreach (GameObject each_simplicial in simplicials)
        {
            if (each_simplicial.GetComponent<SimplicialElementScript>() != null)
            {
                List<GameObject> thenodes = each_simplicial.GetComponent<SimplicialElementScript>().thenodes;
                int x = 0;
                foreach (GameObject each_node in thenodes)
                {
                    if (each_node == node_name)
                    {
                        each_simplicial.GetComponent<SimplicialElementScript>().theVertices[x] = node_name.GetComponent<iconicElementScript>().edge_position;
                        each_simplicial.GetComponent<SimplicialElementScript>().updatePolygon();
                        break;
                    }
                    x++;
                }
            }
            else
            {
                each_simplicial.GetComponent<EdgeElementScript>().updateEndPoint(node_name);
            }            
        }


        GameObject[] hyper_edges = GameObject.FindGameObjectsWithTag("hyper_child_edge");
        foreach (GameObject each_child_edge in hyper_edges)
        {                        
            if (each_child_edge.GetComponent<HyperEdgeElement>().parent_node == node_name)
            {
                each_child_edge.GetComponent<HyperEdgeElement>().UpdateSingleEndpoint(node_name.GetComponent<iconicElementScript>().edge_position);
            }
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

        //Graph
        if (Input.GetKeyUp(KeyCode.G))
        {
            
            graphlocked = true;
            Debug.Log("NOT WORKING?");
        }/*
        else
        {
            graphlocked = false;
        }*/
    }

    void deleteTempLineIfDoubleFinger()
    {
        // Should only be called when necessary -- to get rid of incomplete lines when a double finger is detected and we are not in the pan mode.
        if (Input.touchCount > 1 && templine != null && pan_button.GetComponent<AllButtonsBehaviors>().selected == false)
        {
            Destroy(templine);
        }
    }
}
