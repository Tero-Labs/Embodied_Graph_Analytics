﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem.EnhancedTouch;
using UnityEngine.InputSystem;
using System.Linq;
using TMPro;
using Jobberwocky.GeometryAlgorithms.Source.API;
using Jobberwocky.GeometryAlgorithms.Source.Core;
using Jobberwocky.GeometryAlgorithms.Source.Parameters;

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
    public bool directed_edge = false;

    public bool vertex_add = true;
    public bool vertex_del = false;
    public bool edge_add = false;
    public bool edge_del = false;
    public bool free_hand_edge = false;

    private Vector2 prev_move_pos;
    public Vector3 touchDelta;

    // Prefabs
    public GameObject IconicElement;
    public GameObject ImageIconicElement;
    public GameObject EdgeElement;
    public GameObject SimplicialEdgeElement;
    public GameObject hyperEdgeElement;
    public GameObject CombineLineElement;
    public GameObject FunctionLineElement;
    public GameObject GraphElement;

    // Canvas buttons
    public static GameObject iconicElementButton;
    public static GameObject pan_button;
    public static GameObject graph_pen_button;
    public static GameObject simplicial_pen_button;
    public static GameObject hyper_pen_button;
    public GameObject eraser_button;
    public static GameObject copy_button;
    public static GameObject stroke_combine_button;
    public static GameObject fused_rep_button;
    public GameObject function_brush_button;
    public GameObject video_op_button;
    public GameObject history_view;
    public GameObject history_list_viewer;
    public GameObject canvas_radial;
    public GameObject color_picker;    
    public FlexibleColorPicker color_picker_script;

    public GameObject text_message_worldspace;

    public GameObject edgeline;
    public GameObject simplicialline;
    public GameObject hyperline;
    public GameObject setline;
    public GameObject function_menu;
    public GameObject edge_radial_menu;
    public GameObject node_radial_menu;
    public GameObject graph_radial_menu;   
    public GameObject function_radial_menu;

    // needed for drawing
    public GameObject templine;
	public int totalLines = 0;
    public int min_point_count = 10;

    // holder of all game objects
    public GameObject Objects_parent;

    // edge paint control
    // box collider control
    public static bool enablecollider = false;
    public int selected_obj_count = 0;
    public List<GameObject> selected_obj = new List<GameObject>();
    public GameObject edge_start, edge_end;

    // simplicial edge paint control
    public List<Vector3> SimplicialVertices = new List<Vector3>();
    public List<GameObject> Simplicialnodes= new List<GameObject>();

    // hyper edge paint control
    public List<Vector3> hyperVertices = new List<Vector3>();
    public List<GameObject> hypernodes = new List<GameObject>();

    // needed for graph
    public int graph_count = 0;

    // needed for panning
    private bool taping_flag;
    int LastPhaseHappened; // 1 = S, 2 = M, 3 = E
    float TouchTime; // Time elapsed between touch beginning and ending
    float StartTouchTime; // Time.realtimeSinceStartup at start of touch
    float EndTouchTime; // Time.realtimeSinceStartup at end of touch
    public Vector2 startPos;

    // needed for dragging using mouse
    public GameObject pen_dragged_obj;
    public Vector3 drag_prev_pos;

    // needed for function
    public bool no_func_menu_open;
    public bool function_name_performed = true;
    public GameObject functionline;
    public static int function_count = 0;
    public GameObject dragged_arg_textbox;
    public GameObject current_dragged_function;
    public GameObject potential_tapped_graph;    

    // needed for video
    public GameObject videoplayer;

    // needed for fusion
    public GameObject fused_obj;

    // objects history
    public List<GameObject> history = new List<GameObject>();
    public List<GameObject> new_drawn_icons = new List<GameObject>();
    public List<GameObject> new_drawn_edges = new List<GameObject>();
    public List<GameObject> new_drawn_function_lines = new List<GameObject>();

    // Action History
    public static bool ActionHistoryEnabled = false;

    public GameObject status_label_obj;

    // camera movement
    public float speed = 10.0f;

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
        fused_rep_button = GameObject.Find("Fused");
        function_brush_button = GameObject.Find("function_brush");
        video_op_button = GameObject.Find("video_op");

        text_message_worldspace = GameObject.Find("text_message_worldspace");

        canvas_radial = GameObject.Find("canvas_radial");

        no_func_menu_open = true;
        dragged_arg_textbox = null;
        potential_tapped_graph = null;

        vertex_add = true;
    }

    // Update is called once per frame
    void Update()
    {
        
        #region iconic element brush

        if (iconicElementButton.GetComponent<AllButtonsBehaviors>().selected)
        //!iconicElementButton.GetComponent<AllButtonsBehaviors>().isPredictivePen)
        {
            #region prevent unwanted touch on canvas
            // prevent touch or click being registered on the canvas when a gui button is clicked
            if (color_picker.activeSelf && color_picker.GetComponent<ColorPickerClickCheck>().pointer)
            {
                return;
            }
            #endregion

            //Debug.Log("entered");
            if (PenTouchInfo.PressedThisFrame)//currentPen.tip.wasPressedThisFrame)
            {
                // start drawing a new line
                var ray = Camera.main.ScreenPointToRay(PenTouchInfo.penPosition);
                RaycastHit Hit;
                if (Physics.Raycast(ray, out Hit) &&
                   (Hit.collider.gameObject.name == "Paintable" || Hit.collider.gameObject.tag == "video_player"))
                {
                    //Debug.Log("instantiated_templine");

                    Vector3 vec = Hit.point + new Vector3(0, 0, -40); // Vector3.up * 0.1f;
                                                                      //Debug.Log(vec);

                    totalLines++;
                    templine = Instantiate(IconicElement, vec, Quaternion.identity, Objects_parent.transform);
                    //templine.GetComponent<TrailRenderer>().material.color = Color.black;

                    templine.name = "iconic_" + totalLines.ToString();
                    templine.tag = "iconic";

                    templine.GetComponent<iconicElementScript>().points.Add(vec);
                    templine.GetComponent<iconicElementScript>().icon_number = totalLines;

                    templine.transform.GetComponent<LineRenderer>().material.SetColor("_Color", color_picker_script.color);
                    templine.transform.GetComponent<TrailRenderer>().material.SetColor("_Color", color_picker_script.color);
                    Debug.Log("colorpicker_color:" + color_picker_script.color.ToString());

                    
                    templine.GetComponent<TrailRenderer>().widthMultiplier = 2;
                    //pencil_button.GetComponent<AllButtonsBehavior>().penWidthSliderInstance.GetComponent<Slider>().value;

                    templine.GetComponent<LineRenderer>().widthMultiplier = 2;
                    //pencil_button.GetComponent<AllButtonsBehavior>().penWidthSliderInstance.GetComponent<Slider>().value;
                    new_drawn_icons.Add(templine);
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
                if (Physics.Raycast(ray, out Hit) &&
                    (Hit.collider.gameObject.name == "Paintable" || Hit.collider.gameObject.tag == "video_player"))
                {

                    Vector3 vec = Hit.point + new Vector3(0, 0, -40); // Vector3.up * 0.1f;

                    templine.GetComponent<TrailRenderer>().transform.position = vec;
                    templine.GetComponent<iconicElementScript>().points.Add(vec);


                    // Show the distance, format to a fixed decimal place.
                    //templine.GetComponent<iconicElementScript>().calculateLengthAttributeFromPoints();
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

                if (Physics.Raycast(ray, out Hit) &&
                    (Hit.collider.gameObject.name == "Paintable" || Hit.collider.gameObject.tag == "video_player"))
                {
                    if (templine.GetComponent<iconicElementScript>().points.Count > min_point_count)
                    {
                        
                        templine = transform.GetComponent<CreatePrimitives>().FinishPenLine(templine);
                        templine.GetComponent<iconicElementScript>().paintable_object = transform.gameObject;

                        //new_drawn_icons.Add(templine);
                        templine = null;

                    }
                    else
                    {
                        // delete the templine, not enough points
                        // Debug.Log("here_in_destroy");
                        Destroy(templine);
                        totalLines--;
                        //templine = null;
                    }
                }
                else
                {
                    // the touch didn't end on a line, destroy the line
                    // Debug.Log("here_in_destroy_different_Hit");
                    Destroy(templine);
                    totalLines--;
                    //templine = null;
                }

            }

        }


        #endregion

        #region pan

        var activeTouches = UnityEngine.InputSystem.EnhancedTouch.Touch.activeTouches;      

        // Handle screen touches.                     
        //https://docs.unity3d.com/ScriptReference/TouchPhase.Moved.html

        if (pan_button.GetComponent<AllButtonsBehaviors>().selected)
        {
            DragorMenuCreateOnClick();

            if (activeTouches.Count == 2 && !panZoomLocked) // && pan_button.GetComponent<PanButtonBehavior>().selected)
            {

                // NO ANCHOR TAPPED, JUST ZOOM IN/PAN
                //main_camera.GetComponent<MobileTouchCamera>().enabled = true;

                UnityEngine.InputSystem.EnhancedTouch.Touch touchzero = activeTouches[0];
                UnityEngine.InputSystem.EnhancedTouch.Touch touchone = activeTouches[1];

                Vector2 touchzeroprevpos = touchzero.screenPosition - touchzero.delta;
                Vector2 touchoneprevpos = touchone.screenPosition - touchone.delta;

                float prevmag = (touchzeroprevpos - touchoneprevpos).magnitude;
                float currmag = (touchzero.screenPosition - touchone.screenPosition).magnitude;

                float difference = currmag - prevmag;
                Camera.main.orthographicSize = Mathf.Clamp(Camera.main.orthographicSize - zoom_multiplier * difference, zoom_min, zoom_max);

                // CHECK AND DELETE INCOMPLETE LINES
                deleteTempLineIfDoubleFinger();

                // show zoom percentage text
                // Assuming these parameters for orthographic size: min: 200, max: 500
                int zoom = (int)((1f - ((main_camera.orthographicSize - zoom_min) / zoom_max)) * 100f);
                GameObject.Find("text_message_worldspace").GetComponent<TextMeshProUGUI>().text =
                    zoom.ToString("F0") + "%";
                //}

            }
            else if (Input.touchCount == 2 && !panZoomLocked) // && pan_button.GetComponent<PanButtonBehavior>().selected)
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

                Camera.main.orthographicSize = Mathf.Clamp(Camera.main.orthographicSize - zoom_multiplier * difference, zoom_min, zoom_max);

                // CHECK AND DELETE INCOMPLETE LINES
                deleteTempLineIfDoubleFinger();

                int zoom = (int)((1f - ((main_camera.orthographicSize - zoom_min) / zoom_max)) * 100f);
                text_message_worldspace.GetComponent<TextMeshProUGUI>().text = zoom.ToString("F0") + "%";


            }
            else if (activeTouches.Count == 1 && !panZoomLocked)
            {

                // Only pan when the touch is on top of the canvas. Otherwise,
                var ray = Camera.main.ScreenPointToRay(activeTouches[0].screenPosition);
                RaycastHit Hit;

                if (Physics.Raycast(ray, out Hit) && Hit.collider.gameObject.tag == "paintable_canvas_object")
                {

                    // EnhanchedTouch acts weirdly in the sense that the Start phase is not detected many times,
                    // only Moved, Stationary, and Ended phases are detected most of the times.
                    // So, introducing a bool that will only update the start pan position if a touch ended before.

                    if (activeTouches[0].phase == UnityEngine.InputSystem.TouchPhase.Ended)
                    {
                        previousTouchEnded = true;
                    }

                    //if (touchScreen.touches[0].phase.ReadValue() == UnityEngine.InputSystem.TouchPhase.Began)
                    else if (activeTouches[0].phase == UnityEngine.InputSystem.TouchPhase.Moved && previousTouchEnded && okayToPan)
                    {
                        panTouchStart = Camera.main.ScreenToWorldPoint(activeTouches[0].screenPosition);
                        //Debug.Log("touch start: " + panTouchStart.ToString());

                        previousTouchEnded = false;

                        if (canvas_radial.transform.childCount > 0)
                        {
                            for (int i = 0; i < canvas_radial.transform.childCount; i++)
                            {
                                Destroy(canvas_radial.transform.GetChild(i).gameObject);
                            }
                        }
                    }

                    //else if (touchScreen.touches[0].phase.ReadValue() == UnityEngine.InputSystem.TouchPhase.Moved)
                    else if (activeTouches[0].phase == UnityEngine.InputSystem.TouchPhase.Moved && !previousTouchEnded && okayToPan)
                    {
                        Vector2 panDirection = panTouchStart - (Vector2)Camera.main.ScreenToWorldPoint(activeTouches[0].screenPosition);
                        Camera.main.transform.position += (Vector3)panDirection;
                    }
                }

                // if not panning, check for tap
                if (activeTouches[0].isTap)
                {
                    // delegate single tap to short tap function
                    //Debug.Log("tap detected.");
                    menucreation(activeTouches[0].screenPosition);
                    // OnLongTap(activeTouches[0].screenPosition);
                }
            }
            else if (Input.touchCount == 1 && !panZoomLocked)
            {
                UnityEngine.Touch activeoldTouches = Input.GetTouch(0);

                // Only pan when the touch is on top of the canvas. Otherwise,
                var ray = Camera.main.ScreenPointToRay(activeoldTouches.position);
                RaycastHit Hit;

                if (Physics.Raycast(ray, out Hit) && Hit.collider.gameObject.tag != "simplicial")
                {

                    GameObject temp = Hit.collider.gameObject;
                    Debug.Log("collided_with" + temp.tag);

                    if (activeoldTouches.phase == UnityEngine.TouchPhase.Ended && okayToPan)
                    {
                        previousTouchEnded = true;
                        if (curtouched_obj.tag == "iconic")
                        {
                            curtouched_obj.transform.localScale = curtouched_obj.transform.localScale / 1.05f;
                            //curtouched_obj.transform.localScale = new Vector3(1f, 1f, 1f);
                            if (!graphlocked)
                                curtouched_obj.GetComponent<iconicElementScript>().searchFunctionAndUpdateLasso();
                        }

                        else if (curtouched_obj.tag == "video_player")
                        {
                            curtouched_obj.transform.parent.GetComponent<VideoPlayerChildrenAccess>().UIlayout();
                        }
                    }

                    else if (activeoldTouches.phase == UnityEngine.TouchPhase.Began && okayToPan)
                    {
                        curtouched_obj = temp;

                        if (curtouched_obj.tag == "iconic")
                        {
                            curtouched_obj.transform.localScale = curtouched_obj.transform.localScale * 1.05f; //new Vector3(1.25f, 1.25f, 1.25f);
                        }

                        Vector3 vec = Hit.point;
                        // enforce the same z coordinate as the rest of the points in the parent set object
                        vec.z = -5f;
                        touchDelta = curtouched_obj.transform.position - vec;
                    }

                    else if (activeoldTouches.phase == UnityEngine.TouchPhase.Moved && previousTouchEnded && okayToPan)//&& (curtouched_obj == temp))
                    {
                        panTouchStart = Camera.main.ScreenToWorldPoint(activeoldTouches.position);
                        //Debug.Log("touch start: " + panTouchStart.ToString());

                        previousTouchEnded = false;
                        prev_move_pos = panTouchStart;
                    }

                    else if (activeoldTouches.phase == UnityEngine.TouchPhase.Moved && !previousTouchEnded && okayToPan)// && (curtouched_obj == temp))
                    {
                        if (Vector2.Distance(prev_move_pos, (Vector2)Camera.main.ScreenToWorldPoint(activeoldTouches.position)) > 2)
                        {
                            Vector2 panDirection = panTouchStart - (Vector2)Camera.main.ScreenToWorldPoint(activeoldTouches.position);
                            //Debug.Log("position changed from "+ Camera.main.transform.position.ToString() + " to " + (Camera.main.transform.position + (Vector3)panDirection).ToString());

                            Vector3 vec = Hit.point;
                            // enforce the same z coordinate as the rest of the points in the parent set object
                            vec.z = -5f;

                            Vector3 diff = vec - curtouched_obj.transform.position + touchDelta;
                            diff.z = 0;

                            //transform.position += diff;

                            if (curtouched_obj.tag == "paintable_canvas_object")
                            {
                                Camera.main.transform.position += (Vector3)panDirection;
                            }
                            else if (curtouched_obj.tag == "iconic")
                            {
                                //curtouched_obj.transform.position -= (Vector3)panDirection;
                                if (graphlocked)
                                {
                                    if (curtouched_obj.transform.parent.tag == "node_parent"
                                        && curtouched_obj.transform.parent.parent.GetComponent<GraphElementScript>().video_graph == false)
                                    {
                                        curtouched_obj.transform.parent.parent.position += diff;
                                        curtouched_obj.transform.parent.parent.GetComponent<GraphElementScript>().checkHitAndMove(diff);
                                    }
                                }
                                else if (curtouched_obj.GetComponent<iconicElementScript>().video_icon == false)
                                {
                                    curtouched_obj.transform.position += diff;
                                    curtouched_obj.GetComponent<iconicElementScript>().edge_position += diff;
                                    //curtouched_obj.GetComponent<iconicElementScript>().edge_position -= (Vector3)panDirection;
                                    curtouched_obj.GetComponent<iconicElementScript>().searchNodeAndUpdateEdge();
                                }

                            }
                            else if (curtouched_obj.tag == "video_player")
                            {
                                curtouched_obj.transform.parent.GetComponent<VideoPlayerChildrenAccess>().checkHitAndMove(diff);
                            }
                            else if (curtouched_obj.tag == "hyper")
                            {
                                //curtouched_obj.transform.position -= (Vector3)panDirection;
                                curtouched_obj.transform.position += diff;
                                curtouched_obj.GetComponent<HyperElementScript>().UpdateChildren();
                            }

                            prev_move_pos = (Vector2)Camera.main.ScreenToWorldPoint(activeoldTouches.position);
                        }

                    }

                }

                OnShortTap();
            }
            else
            {
                previousTouchEnded = true;
                //main_camera.GetComponent<MobileTouchCamera>().enabled = false;
                text_message_worldspace.GetComponent<TextMeshProUGUI>().text = "";
            }

        }

        #endregion

        #region Graph Pen
        if (graph_pen_button.GetComponent<AllButtonsBehaviors>().selected)
        {
            #region prevent unwanted touch on canvas
            // prevent touch or click being registered on the canvas when a gui button is clicked
            if (color_picker.activeSelf && color_picker.GetComponent<ColorPickerClickCheck>().pointer)
            {
                return;
            }
            #endregion

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
                    LineRenderer l = edgeline.transform.GetComponent<LineRenderer>();
                    l.material.SetColor("_Color", color_picker_script.color);

                    new_drawn_edges.Add(edgeline);
                    /*l.startWidth = 2f;
                    l.endWidth = 2f;*/

                    // set up the line renderer
                    if (!free_hand_edge)
                    {
                        l.positionCount = 2;
                        l.SetPosition(0, cur.GetComponent<iconicElementScript>().edge_position);// + new Vector3(0, 0, -2f));
                        l.SetPosition(1, cur.GetComponent<iconicElementScript>().edge_position + new Vector3(1f, 0, -2f));
                    }
                    else
                    {
                        edgeline.GetComponent<EdgeElementScript>().free_hand = free_hand_edge;
                        edgeline.transform.GetComponent<TrailRenderer>().material.SetColor("_Color", color_picker_script.color);
                        Debug.Log("colorpicker_color:" + color_picker_script.color.ToString());

                        edgeline.GetComponent<TrailRenderer>().widthMultiplier = 1f;
                        //pencil_button.GetComponent<AllButtonsBehavior>().penWidthSliderInstance.GetComponent<Slider>().value;

                        edgeline.GetComponent<LineRenderer>().widthMultiplier = 1f;
                        //pencil_button.GetComponent<AllButtonsBehavior>().penWidthSliderInstance.GetComponent<Slider>().value;

                    }


                }

            }

            else if (PenTouchInfo.PressedNow)
            {
                // add points to the last line, but check that if an edge line has been created already
                var ray = Camera.main.ScreenPointToRay(PenTouchInfo.penPosition);
                RaycastHit Hit;
                if (Physics.Raycast(ray, out Hit) && 
                    (Hit.collider.gameObject.tag == "paintable_canvas_object" || Hit.collider.gameObject.tag == "iconic")
                    && edge_start != null)
                {
                    Vector3 vec = Hit.point + new Vector3(0, 0, -5f); // Vector3.up * 0.1f;   
                    edgeline.GetComponent<EdgeElementScript>().points.Add(vec);

                    if (!free_hand_edge)
                    {
                                    
                        edgeline.GetComponent<LineRenderer>().SetPosition(1, vec);// + new Vector3(0, 0, -2f));
                    }
                    else
                    {
                        edgeline.GetComponent<TrailRenderer>().transform.position = vec;

                        edgeline.GetComponent<EdgeElementScript>().updateLengthFromPoints();
                        edgeline.GetComponent<EdgeElementScript>().addPressureValue(PenTouchInfo.pressureValue);
                        edgeline.GetComponent<EdgeElementScript>().reNormalizeCurveWidth();
                        edgeline.GetComponent<TrailRenderer>().widthCurve = edgeline.GetComponent<EdgeElementScript>().widthcurve;
                    }
                    
                }
            }

            else if (PenTouchInfo.ReleasedThisFrame)
            {
                var ray = Camera.main.ScreenPointToRay(PenTouchInfo.penPosition);

                RaycastHit Hit;
                if (Physics.Raycast(ray, out Hit) && (Hit.collider.gameObject.tag == "temp_edge_primitive" || Hit.collider.gameObject.tag == "iconic")
                    && edge_start != null)
                {
                    // assign the end of edge object
                    if (Hit.collider.gameObject.tag == "temp_edge_primitive")
                        edge_end = Hit.collider.transform.parent.gameObject;
                    else
                        edge_end = Hit.collider.gameObject;

                    if (edge_end == edge_start)
                    {
                        Destroy(edgeline);
                        edge_end = null;
                        edge_start = null;
                        //edgeline = null;
                        selected_obj_count--;
                    }

                    else if (!free_hand_edge)
                    {

                        if (edgeline.GetComponent<EdgeElementScript>().points.Count > 2)
                        {                            
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
                                edgeline.GetComponent<EdgeElementScript>().directed_edge = directed_edge;

                                // set line renderer end point
                                edgeline.GetComponent<EdgeElementScript>().addEndPoint();

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
                            //edgeline = null;
                            selected_obj_count--;
                        }
                    }
                    else
                    {
                        if (edgeline.GetComponent<EdgeElementScript>().points.Count > min_point_count)
                        {
                            
                            edgeline.GetComponent<EdgeElementScript>().edge_start = edge_start;
                            edgeline.GetComponent<EdgeElementScript>().edge_end = edge_end;
                            edgeline.GetComponent<EdgeElementScript>().directed_edge = directed_edge;
                                                        
                            LineRenderer l = edgeline.transform.GetComponent<LineRenderer>();
                            l.positionCount = edgeline.GetComponent<EdgeElementScript>().points.Count;
                            l.SetPositions(edgeline.GetComponent<EdgeElementScript>().points.ToArray());
                            
                            edgeline = transform.GetComponent<CreatePrimitives>().FinishEdgeLine(edgeline);
                            

                            // set line renderer end point
                            edgeline.GetComponent<EdgeElementScript>().addFreeHandPoint();

                            GraphCreation();

                            edge_end = null;
                            edge_start = null;
                            edgeline = null;
                        }
                        else
                        {
                            // delete the templine, not enough points
                            // Debug.Log("here_in_destroy");
                            Destroy(edgeline);
                            edge_end = null;
                            edge_start = null;
                            //edgeline = null;
                            selected_obj_count--;
                        }
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
                    //edgeline = null;
                    selected_obj_count--;
                }

                // in all other cases, to be safe, just delete the entire edgeline structure
                else if (edge_start != null)
                {
                    Destroy(edgeline);
                    edge_start = null;
                    edge_end = null;
                    //edgeline = null;
                    selected_obj_count--;
                }


                else
                {
                    // the touch didn't end on a line, destroy the line
                    // Debug.Log("here_in_destroy_different_Hit");
                    Destroy(edgeline);
                    edge_start = null;
                    edge_end = null;
                    //edgeline = null;
                    selected_obj_count--;
                }

                // the touch has ended, destroy all temp edge cylinders now
                DeleteEmptyEdgeObjects();
            }

        }

        #endregion

        #region Simplicial Pen
        if (simplicial_pen_button.GetComponent<AllButtonsBehaviors>().selected)
        {
            #region prevent unwanted touch on canvas
            // prevent touch or click being registered on the canvas when a gui button is clicked
            if (color_picker.activeSelf && color_picker.GetComponent<ColorPickerClickCheck>().pointer)
            {
                return;
            }
            #endregion

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
                        simplicialline.GetComponent<SimplicialElementScript>().paintable = transform.gameObject;

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
            #region prevent unwanted touch on canvas
            // prevent touch or click being registered on the canvas when a gui button is clicked
            if (color_picker.activeSelf && color_picker.GetComponent<ColorPickerClickCheck>().pointer)
            {
                return;
            }
            #endregion

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
                        hyperline.GetComponent<HyperElementScript>().paintable = transform.gameObject;

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
                    Transform temp = Hit.collider.gameObject.transform.parent;                    

                    if (temp.tag == "node_parent")
                    {
                        if (graphlocked)
                        {
                            Destroy(temp.parent.gameObject);
                        }
                        else
                        {                            
                            searchNodeAndDeleteEdge(Hit.collider.gameObject);
                            Destroy(Hit.collider.gameObject);
                            temp.parent.GetComponent<GraphElementScript>().Graph_init();
                        } 
                    }
                    else
                    {
                        Destroy(Hit.collider.gameObject);
                    }

                }
                // simplicial edge
                else if (Hit.collider.gameObject.tag == "simplicial")
                {
                    //searchNodeAndDeleteEdge(possible_edge_node_name);
                    Transform temp = Hit.collider.gameObject.transform.parent;
                    Destroy(Hit.collider.gameObject);

                    if (temp.tag == "simplicial_parent")
                        temp.parent.GetComponent<GraphElementScript>().simplicial_init();
                }
                // set anchor
                else if (Hit.collider.gameObject.tag == "hyper")
                {
                    //searchNodeAndDeleteEdge(possible_edge_node_name);
                    Transform temp = Hit.collider.gameObject.transform.parent;
                    Destroy(Hit.collider.gameObject);

                    if (temp.tag == "hyper_parent")
                        temp.parent.GetComponent<GraphElementScript>().hyperedges_init();
                }
            }

            hit2d = Physics2D.GetRayIntersection(ray);
            if (hit2d.collider != null && hit2d.collider.gameObject.tag == "edge")
            {
                Transform temp = hit2d.collider.gameObject.transform.parent;
                Destroy(hit2d.collider.gameObject);
                if (temp.tag == "edge_parent")
                    temp.parent.GetComponent<GraphElementScript>().edges_init();
                //temp.parent.GetComponent<GraphElementScript>().edges_as_Str();

            }
            else if (hit2d.collider != null && hit2d.collider.gameObject.tag == "simplicial")
            {
                Transform temp = hit2d.collider.gameObject.transform.parent;
                Destroy(hit2d.collider.gameObject);
                if (temp.tag == "simplicial_parent")
                    temp.parent.GetComponent<GraphElementScript>().simplicial_init();
                //temp.parent.GetComponent<GraphElementScript>().simplicial_as_Str();
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

        #region fused representation

        if (fused_rep_button.GetComponent<AllButtonsBehaviors>().selected)
        {
            if (PenTouchInfo.PressedThisFrame)
            {
                var ray = Camera.main.ScreenPointToRay(PenTouchInfo.penPosition);
                RaycastHit Hit;
                if (Physics.Raycast(ray, out Hit) && Hit.collider.gameObject.tag == "iconic")
                {
                    fused_obj = Hit.collider.gameObject;
                    StartTouchTime = Time.realtimeSinceStartup;
                    fused_obj.transform.localScale = fused_obj.transform.localScale * 1.05f;
                }
            }            

            else if (fused_obj != null && PenTouchInfo.ReleasedThisFrame)
            {
                fused_obj.transform.localScale = fused_obj.transform.localScale / 1.05f;

                var ray = Camera.main.ScreenPointToRay(PenTouchInfo.penPosition); 
                RaycastHit Hit;

                if (Physics.Raycast(ray, out Hit) && Hit.collider.gameObject == fused_obj)
                {
                    EndTouchTime = Time.realtimeSinceStartup;
                    TouchTime = EndTouchTime - StartTouchTime;

                    if (TouchTime > 0.5f)
                        ConvertToFunction(fused_obj);
                }

                fused_obj = null;
            }

        }
        #endregion

        #region function brush

        if (function_brush_button.GetComponent<AllButtonsBehaviors>().selected)
        {
            //Debug.Log("entered");
            if (no_func_menu_open == false) return;
            if (dragged_arg_textbox == null)
            {
                OnGraphTap();

                // normal operation
                if (potential_tapped_graph == null)
                {
                    if (PenTouchInfo.PressedThisFrame)//currentPen.tip.wasPressedThisFrame)
                    {
                        // start drawing a new line
                        var ray = Camera.main.ScreenPointToRay(PenTouchInfo.penPosition);
                        RaycastHit Hit;
                        //Debug.Log("here");
                        if (Physics.Raycast(ray, out Hit) &&
                            (Hit.collider.gameObject.name == "Paintable" || Hit.collider.gameObject.tag == "video_player"))
                        {
                            Debug.Log("instantiated_templine");

                            Vector3 vec = Hit.point + new Vector3(0, 0, -5f);
                            functionline = Instantiate(FunctionLineElement, vec, Quaternion.identity, Objects_parent.transform);

                            functionline.name = "function_line_" + function_count.ToString();
                            function_count++;
                            functionline.GetComponent<FunctionElementScript>().AddPoint(vec);
                            functionline.GetComponent<FunctionElementScript>().paintable_object = transform.gameObject;

                            new_drawn_function_lines.Add(functionline);
                            //function_menu.GetComponent<FunctionMenuScript>().text_label.GetComponent<TextMeshProUGUI>().text = "Brush loaded";
                        }
                    }

                    else if (functionline != null &&
                        PenTouchInfo.PressedNow //currentPen.tip.isPressed
                        && (PenTouchInfo.penPosition -
                        (Vector2)functionline.GetComponent<FunctionElementScript>().points[functionline.GetComponent<FunctionElementScript>().points.Count - 1]).magnitude > 0f)
                    {
                        // add points to the last line
                        var ray = Camera.main.ScreenPointToRay(PenTouchInfo.penPosition);
                        RaycastHit Hit;
                        if (Physics.Raycast(ray, out Hit) &&
                            (Hit.collider.gameObject.name == "Paintable" || Hit.collider.gameObject.tag == "video_player"))
                        {

                            Vector3 vec = Hit.point + new Vector3(0, 0, -5f); // Vector3.up * 0.1f;

                            functionline.GetComponent<TrailRenderer>().transform.position = vec;
                            functionline.GetComponent<FunctionElementScript>().AddPoint(vec);
                            //functionline.GetComponent<FunctionElementScript>().calculateLengthAttributeFromPoints();

                            // pressure based pen width
                            functionline.GetComponent<FunctionElementScript>().updateLengthFromPoints();
                            functionline.GetComponent<FunctionElementScript>().addPressureValue(PenTouchInfo.pressureValue);
                            functionline.GetComponent<FunctionElementScript>().reNormalizeCurveWidth();
                            functionline.GetComponent<TrailRenderer>().widthCurve = functionline.GetComponent<FunctionElementScript>().widthcurve;

                        }
                    }

                    else if (functionline != null && PenTouchInfo.ReleasedThisFrame)
                    {
                        var ray = Camera.main.ScreenPointToRay(PenTouchInfo.penPosition);
                        RaycastHit Hit;

                        /*if (potential_tapped_graph == null)
                        {*/
                            if (Physics.Raycast(ray, out Hit) &&
                            (Hit.collider.gameObject.name == "Paintable" || Hit.collider.gameObject.tag == "video_player"))
                            {
                                if (functionline.GetComponent<FunctionElementScript>().points.Count > min_point_count)
                                {

                                    List<GameObject> selected_icons = new List<GameObject>();
                                    GameObject[] iconarray = GameObject.FindGameObjectsWithTag("iconic");

                                    for (int i = 0; i < iconarray.Length; i++)
                                    {
                                    
                                        if (functionline.GetComponent<FunctionElementScript>().isInsidePolygon(
                                                iconarray[i].GetComponent<iconicElementScript>().edge_position))//)
                                        {
                                            selected_icons.Add(iconarray[i]);
                                                // MARKER: as everything is recalculated in functionmenuscript, we added break for fast op.
                                                break;
                                        }
                                    }

                                    //Debug.Log("found graph count in lasso: " + selected_graphs.Count.ToString());
                                    if (selected_icons.Count == 0)
                                    {
                                        Destroy(functionline);
                                        //functionline = null;
                                        function_count--;
                                        return;
                                    }

                                    var hullAPI = new HullAPI();
                                    var hull = hullAPI.Hull2D(new Hull2DParameters()
                                    { Points = functionline.GetComponent<FunctionElementScript>().points.ToArray(), Concavity = 3000 });

                                    Vector3[] vertices = hull.vertices;

                                    functionline.GetComponent<FunctionElementScript>().points = vertices.ToList();
                                    functionline = transform.GetComponent<CreatePrimitives>().FinishFunctionLine(functionline);

                                    functionline.GetComponent<FunctionElementScript>().InstantiateNameBox();

                                    //StartCoroutine(HistoryModify(functionline));
                                    //functionline.GetComponent<FunctionElementScript>().grapharray = selected_graphs.ToArray();
                                    functionline = null;
                                }
                                else
                                {
                                    // delete the templine, not enough points
                                    // Debug.Log("here_in_destroy");
                                    Destroy(functionline);
                                    //functionline = null;
                                    function_count--;
                                }
                            }
                            else
                            {
                                // the touch didn't end on a line, destroy the line
                                // Debug.Log("here_in_destroy_different_Hit");
                                Destroy(functionline);
                                //functionline = null;
                                function_count--;
                            }
                        //}
                        // vertex_add_interaction
                        /*else
                        {                            
                        }
                        */
                    }
                }
                // interaction
                else if (potential_tapped_graph != null && activeTouches.Count > 1)
                {
                    
                    OnGraphAdditionInteraction();
                }                    
            }            
        }

        #endregion


        // HANDLE ANY RELEVANT KEY INPUT FOR PAINTABLE'S OPERATIONS
        handleKeyInteractions();
    }

    public void ConvertToFunction(GameObject fused_obj)
    {
        GameObject fused_function_lasso = Instantiate(FunctionLineElement, fused_obj.transform.position, Quaternion.identity, Objects_parent.transform);
        fused_function_lasso.name = "function_line_" + function_count.ToString();
        function_count++;

        var meshFilter = fused_function_lasso.GetComponent<FunctionElementScript>().mesh_holder.GetComponent<MeshFilter>();

        fused_function_lasso.GetComponent<FunctionElementScript>().mesh_holder.GetComponent<MeshRenderer>().sharedMaterial = fused_obj.transform.GetComponent<MeshRenderer>().sharedMaterial;

        Mesh mesh = fused_obj.GetComponent<MeshFilter>().sharedMesh;
        meshFilter.sharedMesh = mesh;

        fused_function_lasso.GetComponent<TrailRenderer>().enabled = false;
        fused_function_lasso.GetComponent<LineRenderer>().enabled = false;

        fused_function_lasso.GetComponent<FunctionElementScript>().edge_position = fused_obj.GetComponent<MeshFilter>().sharedMesh.bounds.center;

        fused_function_lasso.GetComponent<FunctionElementScript>().points = fused_obj.GetComponent<iconicElementScript>().points;

        fused_function_lasso.GetComponent<FunctionElementScript>().maxx = fused_obj.GetComponent<iconicElementScript>().maxx;
        fused_function_lasso.GetComponent<FunctionElementScript>().maxy = fused_obj.GetComponent<iconicElementScript>().maxy;
        fused_function_lasso.GetComponent<FunctionElementScript>().minx = fused_obj.GetComponent<iconicElementScript>().minx;
        fused_function_lasso.GetComponent<FunctionElementScript>().miny = fused_obj.GetComponent<iconicElementScript>().miny;

        fused_function_lasso.GetComponent<FunctionElementScript>().InstantiateNameBox();
        fused_function_lasso.GetComponent<FunctionElementScript>().fused_function = true;

        //StartCoroutine(HistoryModify(fused_function_lasso));
        Destroy(fused_obj);
    }

    public void DragorMenuCreateOnClick()
    {
        if (PenTouchInfo.PressedThisFrame)
        {
            var ray = Camera.main.ScreenPointToRay(PenTouchInfo.penPosition);
            RaycastHit Hit;

            if (Physics.Raycast(ray, out Hit) && Hit.collider.gameObject.tag != "simplicial")
            {
                pen_dragged_obj = Hit.collider.gameObject;
                Debug.Log("collided_with" + pen_dragged_obj.tag);

                if (pen_dragged_obj.tag == "iconic")
                {
                    pen_dragged_obj.transform.localScale = pen_dragged_obj.transform.localScale * 1.05f;

                    if (graphlocked)
                    {
                        if (pen_dragged_obj.transform.parent.tag == "node_parent")
                            drag_prev_pos = pen_dragged_obj.transform.parent.parent.position;
                        else
                        {
                            pen_dragged_obj = null;
                            return;
                        }
                            
                    }
                        
                    else drag_prev_pos = pen_dragged_obj.transform.position;
                }

                Vector3 vec = Hit.point;
                // enforce the same z coordinate as the rest of the points in the parent set object
                vec.z = -5f;
                touchDelta = pen_dragged_obj.transform.position - vec;                

                panTouchStart = Camera.main.ScreenToWorldPoint(PenTouchInfo.penPosition);
                prev_move_pos = panTouchStart;
            }
        }

        else if (PenTouchInfo.PressedNow && pen_dragged_obj != null)
        {
            var ray = Camera.main.ScreenPointToRay(PenTouchInfo.penPosition);
            RaycastHit Hit;

            if (Vector2.Distance(prev_move_pos, (Vector2)Camera.main.ScreenToWorldPoint(PenTouchInfo.penPosition)) < 2)
                return;

            if (Physics.Raycast(ray, out Hit))
            {
                Vector2 panDirection = panTouchStart - (Vector2)Camera.main.ScreenToWorldPoint(PenTouchInfo.penPosition);
                Vector3 vec = Hit.point;

                // enforce the same z coordinate as the rest of the points in the parent set object
                vec.z = -5f;
                Vector3 diff = vec - pen_dragged_obj.transform.position + touchDelta;
                diff.z = 0;
                
                if (pen_dragged_obj.tag == "paintable_canvas_object")
                {
                    //Camera.main.transform.position += (Vector3)panDirection;
                }
                else if (pen_dragged_obj.tag == "iconic")
                {
                    
                    if (graphlocked)
                    {
                        if (pen_dragged_obj.transform.parent.tag == "node_parent"
                            && pen_dragged_obj.transform.parent.parent.GetComponent<GraphElementScript>().video_graph==false)
                        {
                            pen_dragged_obj.transform.parent.parent.position += diff;
                            pen_dragged_obj.transform.parent.parent.GetComponent<GraphElementScript>().checkHitAndMove(diff);
                        }
                    }
                    else if(pen_dragged_obj.GetComponent<iconicElementScript>().video_icon == false)
                    {
                        pen_dragged_obj.transform.position += diff;
                        pen_dragged_obj.GetComponent<iconicElementScript>().edge_position += diff;
                        pen_dragged_obj.GetComponent<iconicElementScript>().searchNodeAndUpdateEdge();
                    }

                }
                else if (pen_dragged_obj.tag == "video_player")
                {
                    pen_dragged_obj.transform.parent.GetComponent<VideoPlayerChildrenAccess>().checkHitAndMove(diff);
                }
                else if (pen_dragged_obj.tag == "hyper")
                {
                    pen_dragged_obj.transform.position += diff;
                    pen_dragged_obj.GetComponent<HyperElementScript>().UpdateChildren();
                }

                prev_move_pos = (Vector2)Camera.main.ScreenToWorldPoint(PenTouchInfo.penPosition);
            }
        }

        else if (PenTouchInfo.ReleasedThisFrame && pen_dragged_obj != null)
        {            
            if (pen_dragged_obj.tag == "iconic")
            {
                if (canvas_radial.transform.childCount > 0)
                {
                    for (int i = 0; i < canvas_radial.transform.childCount; i++)
                    {
                        Destroy(canvas_radial.transform.GetChild(i).gameObject);
                    }
                }

                pen_dragged_obj.transform.localScale = pen_dragged_obj.transform.localScale / 1.05f;
                if (!graphlocked)
                    pen_dragged_obj.GetComponent<iconicElementScript>().searchFunctionAndUpdateLasso();

                if (graphlocked)
                {
                    if (pen_dragged_obj.transform.parent.tag == "node_parent")
                    {

                        if (Vector3.Distance(drag_prev_pos, pen_dragged_obj.transform.parent.parent.position) < 5f)
                        {
                            menucreation(PenTouchInfo.penPosition);
                        }
                    }
                }
                else
                {                    
                    if (Vector3.Distance(drag_prev_pos, pen_dragged_obj.transform.position) < 5f)
                    {
                        menucreation(PenTouchInfo.penPosition);
                    }
                }
            }

            else if (pen_dragged_obj.tag == "video_player")
            {
                pen_dragged_obj.transform.parent.GetComponent<VideoPlayerChildrenAccess>().UIlayout();
            }

            pen_dragged_obj = null;
        }
                
    }

    // create iconic element from an image
    public void createImageIcon(string FilePath)
    {
        GameObject temp = Instantiate(ImageIconicElement, new Vector3(0,0,-5f), Quaternion.identity, Objects_parent.transform);
        totalLines++;
        temp.name = "iconic_" + totalLines.ToString();
        temp.tag = "iconic";
        temp.GetComponent<iconicElementScript>().icon_number = totalLines;
        temp.GetComponent<iconicElementScript>().image_icon = true;
        temp.GetComponent<iconicElementScript>().LoadNewSprite(FilePath);        
    }

    void OnGraphAdditionInteractionOld()
    {
        UnityEngine.Touch currentTouch = Input.GetTouch(1);
        if (currentTouch.phase == UnityEngine.TouchPhase.Began)//currentPen.tip.wasPressedThisFrame)
        {
            // start drawing a new line
            var ray = Camera.main.ScreenPointToRay(currentTouch.position);
            RaycastHit Hit;
            Debug.Log("here_with_tap");
            if (Physics.Raycast(ray, out Hit) && Hit.collider.gameObject.name == "Paintable")
            {
                Debug.Log("instantiated_templine_with_tap");

                Vector3 vec = Hit.point + new Vector3(0, 0, -5);
                functionline = Instantiate(FunctionLineElement, vec, Quaternion.identity, Objects_parent.transform);

                functionline.name = "function_line_" + function_count.ToString();
                //function_count++;
                functionline.GetComponent<FunctionElementScript>().AddPoint(vec);
                functionline.GetComponent<FunctionElementScript>().paintable_object = transform.gameObject;

                new_drawn_function_lines.Add(functionline);
            }
        }

        else if (functionline != null &&
            currentTouch.phase == UnityEngine.TouchPhase.Moved //currentPen.tip.isPressed
            /*&& (currentTouch.position -
            (Vector2)functionline.GetComponent<FunctionElementScript>().points[functionline.GetComponent<FunctionElementScript>().points.Count - 1]).magnitude > 0f*/)
        {
            // add points to the last line
            var ray = Camera.main.ScreenPointToRay(currentTouch.position);
            RaycastHit Hit;
            if (Physics.Raycast(ray, out Hit) && Hit.collider.gameObject.name == "Paintable")
            {

                Vector3 vec = Hit.point + new Vector3(0, 0, -5); // Vector3.up * 0.1f;

                functionline.GetComponent<TrailRenderer>().transform.position = vec;
                functionline.GetComponent<FunctionElementScript>().AddPoint(vec);
                functionline.GetComponent<FunctionElementScript>().calculateLengthAttributeFromPoints();

                // pressure based pen width
                functionline.GetComponent<FunctionElementScript>().updateLengthFromPoints();
                functionline.GetComponent<FunctionElementScript>().addPressureValue(1);
                functionline.GetComponent<FunctionElementScript>().reNormalizeCurveWidth();
                functionline.GetComponent<TrailRenderer>().widthCurve = functionline.GetComponent<FunctionElementScript>().widthcurve;
            }
        }

        else if (functionline != null && currentTouch.phase == UnityEngine.TouchPhase.Ended)
        {
            var ray = Camera.main.ScreenPointToRay(currentTouch.position);
            RaycastHit Hit;

            // vertex_add_interaction            
            if (Physics.Raycast(ray, out Hit) && Hit.collider.gameObject.name == "Paintable")
            {
                if (functionline.GetComponent<FunctionElementScript>().points.Count > min_point_count)
                {
                    Debug.Log("finished_templine_with_tap");

                    if (vertex_add)
                    {
                        List<GameObject> selected_icons = new List<GameObject>();
                        GameObject[] iconarray = GameObject.FindGameObjectsWithTag("iconic");

                        for (int i = 0; i < iconarray.Length; i++)
                        {
                            // check if the lines are inside the drawn set polygon -- in respective local coordinates
                            // we checked if the icon is inside the drawn lasso
                            if (functionline.GetComponent<FunctionElementScript>().isInsidePolygon(iconarray[i].GetComponent<iconicElementScript>().edge_position))
                            {
                                selected_icons.Add(iconarray[i]);
                            }
                        }

                        //Debug.Log("found graph count in lasso: " + selected_graphs.Count.ToString());
                        if (selected_icons.Count == 0)
                        {
                            Destroy(functionline);
                            //functionline = null;
                            return;
                        }

                        GameObject tempnodeparent = potential_tapped_graph.transform.GetChild(0).gameObject;
                        GameObject tempedgeparent = potential_tapped_graph.transform.GetChild(1).gameObject;
                        GameObject tempsimplicialparent = potential_tapped_graph.transform.GetChild(2).gameObject;
                        GameObject temphyperparent = potential_tapped_graph.transform.GetChild(3).gameObject;

                        //change_parent
                        foreach (GameObject new_child_icon in selected_icons)
                        {
                            //change_parent 
                            if (new_child_icon.transform.parent.tag != "node_parent")
                            {
                                new_child_icon.transform.parent = tempnodeparent.transform;
                            }
                            // if already in a graph, change parent of every siblings of it,but make sure not under the current graph
                            else if (new_child_icon.transform.parent != potential_tapped_graph.transform)
                            {
                                Transform Prev_node_parent = new_child_icon.transform.parent;
                                Transform Prev_graph_parent = Prev_node_parent.parent;
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
                        }

                        potential_tapped_graph.GetComponent<GraphElementScript>().Graph_init();
                        //potential_tapped_graph.GetComponent<GraphElementScript>().Graph_as_Str();

                        Destroy(functionline);
                        //functionline = null;
                    }

                    else if (vertex_del)
                    {
                        GameObject tempnodeparent = potential_tapped_graph.transform.GetChild(0).gameObject;
                        Transform[] allChildrennode = tempnodeparent.GetComponentsInChildren<Transform>();

                        foreach (Transform child in allChildrennode)
                        {
                            if (child.tag != "iconic")
                                continue;
                            Debug.Log("checkfor_vertex_del:"+child.name);
                            if (functionline.GetComponent<FunctionElementScript>().isInsidePolygon(child.GetComponent<iconicElementScript>().edge_position))
                            {
                                searchNodeAndDeleteEdge(child.gameObject);
                                Destroy(child.gameObject);
                            }
                        }
                               
                        potential_tapped_graph.GetComponent<GraphElementScript>().Graph_init();
                        //potential_tapped_graph.GetComponent<GraphElementScript>().Graph_as_Str();

                        Destroy(functionline);
                        //functionline = null;
                    }

                    else if (edge_add)
                    {
                        List<string> selected_icons = new List<string>();
                        GameObject tempnodeparent = potential_tapped_graph.transform.GetChild(0).gameObject;
                        Transform[] allChildrennode = tempnodeparent.GetComponentsInChildren<Transform>();

                        foreach (Transform child in allChildrennode)
                        {
                            if (child.tag != "iconic")
                                continue;

                            if (functionline.GetComponent<FunctionElementScript>().isInsidePolygon(child.GetComponent<iconicElementScript>().edge_position))
                            {
                                selected_icons.Add(child.GetComponent<iconicElementScript>().icon_number.ToString());
                            }
                        }

                        if (selected_icons.Count == 0)
                        {
                            Destroy(functionline);
                            //functionline = null;
                            return;
                        }


                        for (int i = 0; i < selected_icons.Count; i++)
                        {                            
                            for (int j = (i+1); j < selected_icons.Count; j++)
                            {
                                List<string> icons = new List<string>();
                                icons.Add(selected_icons[i]);
                                icons.Add(selected_icons[j]);
                                potential_tapped_graph.GetComponent<GraphElementScript>().EdgeCreation("edge", icons.ToArray(), 1);
                            }
                        }

                        potential_tapped_graph.GetComponent<GraphElementScript>().edges_init();
                        //potential_tapped_graph.GetComponent<GraphElementScript>().Graph_as_Str();

                        Destroy(functionline);
                        //functionline = null;
                    }

                    else if (edge_del)
                    {
                        List<GameObject> selected_icons = new List<GameObject>();
                        GameObject tempnodeparent = potential_tapped_graph.transform.GetChild(0).gameObject;
                        Transform[] allChildrennode = tempnodeparent.GetComponentsInChildren<Transform>();

                        foreach (Transform child in allChildrennode)
                        {
                            if (child.tag != "iconic")
                                continue;
                            Debug.Log("checkfor_vertex_del:" + child.name);
                            if (functionline.GetComponent<FunctionElementScript>().isInsidePolygon(child.GetComponent<iconicElementScript>().edge_position))
                            {
                                selected_icons.Add(child.gameObject);
                            }
                        }

                        if (selected_icons.Count == 0)
                        {
                            Destroy(functionline);
                            //functionline = null;
                            return;
                        }

                        GameObject tempedgeparent = potential_tapped_graph.transform.GetChild(1).gameObject;
                        Transform[] allChildrenedge = tempedgeparent.GetComponentsInChildren<Transform>();

                        foreach (Transform child in allChildrenedge)
                        {
                            if (child.tag != "edge")
                                continue;
                            
                            if (selected_icons.Contains(child.GetComponent<EdgeElementScript>().edge_start) &&
                                selected_icons.Contains(child.GetComponent<EdgeElementScript>().edge_end))
                            {
                                Destroy(child.gameObject);
                            }
                        }

                        potential_tapped_graph.GetComponent<GraphElementScript>().edges_init();
                        //potential_tapped_graph.GetComponent<GraphElementScript>().Graph_as_Str();

                        Destroy(functionline);
                        //functionline = null;
                    }

                    else
                    {
                        Destroy(functionline);
                        //functionline = null;
                    }
                }
                else
                {
                    // delete the templine, not enough points
                    // Debug.Log("here_in_destroy");
                    Destroy(functionline);
                    //functionline = null;
                }
            }
            else
            {
                // the touch didn't end on a line, destroy the line
                // Debug.Log("here_in_destroy_different_Hit");
                Destroy(functionline);
                //functionline = null;
            }
        
        }
    }

    void OnGraphTapOld()
    {
        if (Input.touchCount == 0) return;
        
        UnityEngine.Touch currentTouch = Input.GetTouch(0);
        switch (currentTouch.phase)
        {
            case UnityEngine.TouchPhase.Began:
                if (LastPhaseHappened != 1)
                {
                    taping_flag = true;
                }
                LastPhaseHappened = 1;
                startPos = currentTouch.position;
                var ray = Camera.main.ScreenPointToRay(startPos);
                RaycastHit Hit;
                if (Physics.Raycast(ray, out Hit) && Hit.collider.gameObject.tag == "iconic")
                {
                    if (Hit.transform.parent.tag == "node_parent")
                    {
                        potential_tapped_graph = Hit.transform.parent.parent.gameObject;                        
                    }
                }

                break;

            case UnityEngine.TouchPhase.Moved:
                //if (LastPhaseHappend != 2)
                if (Vector2.Distance(currentTouch.position, startPos) > 5f)
                {
                    startPos = currentTouch.position;
                    ray = Camera.main.ScreenPointToRay(startPos);

                    // checking again, as unwanted move might have happened
                    if (Physics.Raycast(ray, out Hit) && Hit.collider.gameObject.tag == "iconic")
                    {
                        if (Hit.transform.parent.tag == "node_parent")
                        {
                            potential_tapped_graph = Hit.transform.parent.parent.gameObject;
                        }
                    }
                    else
                    {
                        taping_flag = false;
                        potential_tapped_graph = null;
                    }                    
                }
                LastPhaseHappened = 2;
                break;

            case UnityEngine.TouchPhase.Ended:
                
                LastPhaseHappened = 3;
                potential_tapped_graph = null;
                break;
        }

    }

    void OnGraphTap()
    {
        var activeTouches = UnityEngine.InputSystem.EnhancedTouch.Touch.activeTouches;

        if (activeTouches.Count == 0) return;
        

        if (activeTouches[0].phase == UnityEngine.InputSystem.TouchPhase.Moved && LastPhaseHappened != 1)
        {
            GameObject temp_stat = Instantiate(status_label_obj, canvas_radial.transform);

            if (LastPhaseHappened != 1)
            {
                taping_flag = true;
            }
            LastPhaseHappened = 1;

            startPos = activeTouches[0].screenPosition;
            var ray = Camera.main.ScreenPointToRay(startPos);
            RaycastHit Hit;

            if (Physics.Raycast(ray, out Hit))
            {
                if (Hit.collider.gameObject.tag == "iconic")
                {
                    if (Hit.transform.parent.tag == "node_parent")
                    {
                        potential_tapped_graph = Hit.transform.parent.parent.gameObject;
                        temp_stat.GetComponent<Status_label_text>().ChangeLabel("ptg: " + potential_tapped_graph.name);
                    }
                    else
                    {
                        potential_tapped_graph = null;
                    }
                }
                else if (Hit.collider.gameObject.tag == "simplicial")
                {
                    potential_tapped_graph = Hit.transform.parent.parent.gameObject;
                    temp_stat.GetComponent<Status_label_text>().ChangeLabel("ptg: " + potential_tapped_graph.name);
                }
            }

            else
            {
                potential_tapped_graph = null;
            }
        }               
                
        else if (activeTouches[0].phase == UnityEngine.InputSystem.TouchPhase.Ended)
        {
            LastPhaseHappened = 3;
            if (activeTouches[0].isTap == false) potential_tapped_graph = null;
            taping_flag = false;
            GameObject temp_stat = Instantiate(status_label_obj, canvas_radial.transform);
            temp_stat.GetComponent<Status_label_text>().ChangeLabel("tapping finished");
        }

    }

    void OnGraphAdditionInteraction()
    {
        int idx = UnityEngine.InputSystem.EnhancedTouch.Touch.activeTouches.Count - 1;

        var currentTouch = UnityEngine.InputSystem.EnhancedTouch.Touch.activeTouches[idx];

        if (functionline == null &&
            currentTouch.phase == UnityEngine.InputSystem.TouchPhase.Moved)
        {
            // start drawing a new line
            var ray = Camera.main.ScreenPointToRay(currentTouch.screenPosition);
            RaycastHit Hit;

            GameObject temp_stat = Instantiate(status_label_obj, canvas_radial.transform);
            temp_stat.GetComponent<Status_label_text>().ChangeLabel("here_with_tap");

            if (Physics.Raycast(ray, out Hit) && Hit.collider.gameObject.name == "Paintable")
            {
                Debug.Log("instantiated_templine_with_tap");

                Vector3 vec = Hit.point + new Vector3(0, 0, -5);
                functionline = Instantiate(FunctionLineElement, vec, Quaternion.identity, Objects_parent.transform);

                functionline.name = "function_line_" + function_count.ToString();
                //function_count++;
                functionline.GetComponent<FunctionElementScript>().AddPoint(vec);
                functionline.GetComponent<FunctionElementScript>().paintable_object = transform.gameObject;

                new_drawn_function_lines.Add(functionline);                
                temp_stat.GetComponent<Status_label_text>().ChangeLabel("instantiated_templine_with_tap");
            }
        }

        else if (functionline != null &&
            currentTouch.phase == UnityEngine.InputSystem.TouchPhase.Moved)
        {
            // add points to the last line
            var ray = Camera.main.ScreenPointToRay(currentTouch.screenPosition);
            RaycastHit Hit;
            if (Physics.Raycast(ray, out Hit) && Hit.collider.gameObject.name == "Paintable")
            {

                Vector3 vec = Hit.point + new Vector3(0, 0, -5); // Vector3.up * 0.1f;

                functionline.GetComponent<TrailRenderer>().transform.position = vec;
                functionline.GetComponent<FunctionElementScript>().AddPoint(vec);
                functionline.GetComponent<FunctionElementScript>().calculateLengthAttributeFromPoints();

                // pressure based pen width
                functionline.GetComponent<FunctionElementScript>().updateLengthFromPoints();
                functionline.GetComponent<FunctionElementScript>().addPressureValue(1);
                functionline.GetComponent<FunctionElementScript>().reNormalizeCurveWidth();
                functionline.GetComponent<TrailRenderer>().widthCurve = functionline.GetComponent<FunctionElementScript>().widthcurve;
            }
        }

        else if (functionline != null && currentTouch.phase == UnityEngine.InputSystem.TouchPhase.Ended)
        {
            var ray = Camera.main.ScreenPointToRay(currentTouch.screenPosition);
            RaycastHit Hit;

            // vertex_add_interaction            
            if (Physics.Raycast(ray, out Hit) && Hit.collider.gameObject.name == "Paintable")
            {
                if (functionline.GetComponent<FunctionElementScript>().points.Count > min_point_count)
                {
                    Debug.Log("finished_templine_with_tap");

                    GameObject temp_stat = Instantiate(status_label_obj, canvas_radial.transform);
                    temp_stat.GetComponent<Status_label_text>().ChangeLabel("finished_templine_with_tap");

                    if (vertex_add)
                    {
                        List<GameObject> selected_icons = new List<GameObject>();
                        GameObject[] iconarray = GameObject.FindGameObjectsWithTag("iconic");

                        for (int i = 0; i < iconarray.Length; i++)
                        {
                            // check if the lines are inside the drawn set polygon -- in respective local coordinates
                            // we checked if the icon is inside the drawn lasso
                            if (functionline.GetComponent<FunctionElementScript>().isInsidePolygon(iconarray[i].GetComponent<iconicElementScript>().edge_position))
                            {
                                selected_icons.Add(iconarray[i]);
                            }
                        }

                        //Debug.Log("found graph count in lasso: " + selected_graphs.Count.ToString());
                        if (selected_icons.Count == 0)
                        {
                            Destroy(functionline);
                            //functionline = null;
                            return;
                        }

                        GameObject tempnodeparent = potential_tapped_graph.transform.GetChild(0).gameObject;
                        GameObject tempedgeparent = potential_tapped_graph.transform.GetChild(1).gameObject;
                        GameObject tempsimplicialparent = potential_tapped_graph.transform.GetChild(2).gameObject;
                        GameObject temphyperparent = potential_tapped_graph.transform.GetChild(3).gameObject;

                        //change_parent
                        foreach (GameObject new_child_icon in selected_icons)
                        {
                            //change_parent 
                            if (new_child_icon.transform.parent.tag != "node_parent")
                            {
                                new_child_icon.transform.parent = tempnodeparent.transform;
                            }
                            // if already in a graph, change parent of every siblings of it,but make sure not under the current graph
                            else if (new_child_icon.transform.parent != potential_tapped_graph.transform)
                            {
                                Transform Prev_node_parent = new_child_icon.transform.parent;
                                Transform Prev_graph_parent = Prev_node_parent.parent;
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
                        }

                        potential_tapped_graph.GetComponent<GraphElementScript>().Graph_init();
                        //potential_tapped_graph.GetComponent<GraphElementScript>().Graph_as_Str();

                        Destroy(functionline);
                        //functionline = null;
                    }

                    else if (vertex_del)
                    {
                        GameObject tempnodeparent = potential_tapped_graph.transform.GetChild(0).gameObject;
                        Transform[] allChildrennode = tempnodeparent.GetComponentsInChildren<Transform>();

                        foreach (Transform child in allChildrennode)
                        {
                            if (child.tag != "iconic")
                                continue;
                            Debug.Log("checkfor_vertex_del:" + child.name);
                            if (functionline.GetComponent<FunctionElementScript>().isInsidePolygon(child.GetComponent<iconicElementScript>().edge_position))
                            {
                                searchNodeAndDeleteEdge(child.gameObject);
                                Destroy(child.gameObject);
                            }
                        }

                        potential_tapped_graph.GetComponent<GraphElementScript>().Graph_init();
                        //potential_tapped_graph.GetComponent<GraphElementScript>().Graph_as_Str();

                        Destroy(functionline);
                        //functionline = null;
                    }

                    else if (edge_add)
                    {
                        List<string> selected_icons = new List<string>();
                        GameObject tempnodeparent = potential_tapped_graph.transform.GetChild(0).gameObject;
                        Transform[] allChildrennode = tempnodeparent.GetComponentsInChildren<Transform>();

                        foreach (Transform child in allChildrennode)
                        {
                            if (child.tag != "iconic")
                                continue;

                            if (functionline.GetComponent<FunctionElementScript>().isInsidePolygon(child.GetComponent<iconicElementScript>().edge_position))
                            {
                                selected_icons.Add(child.GetComponent<iconicElementScript>().icon_number.ToString());
                            }
                        }

                        if (selected_icons.Count == 0)
                        {
                            Destroy(functionline);
                            //functionline = null;
                            return;
                        }


                        for (int i = 0; i < selected_icons.Count; i++)
                        {
                            for (int j = (i + 1); j < selected_icons.Count; j++)
                            {
                                List<string> icons = new List<string>();
                                icons.Add(selected_icons[i]);
                                icons.Add(selected_icons[j]);
                                potential_tapped_graph.GetComponent<GraphElementScript>().EdgeCreation("edge", icons.ToArray(), 1);
                            }
                        }

                        potential_tapped_graph.GetComponent<GraphElementScript>().edges_init();
                        //potential_tapped_graph.GetComponent<GraphElementScript>().Graph_as_Str();

                        Destroy(functionline);
                        //functionline = null;
                    }

                    else if (edge_del)
                    {
                        List<GameObject> selected_icons = new List<GameObject>();
                        GameObject tempnodeparent = potential_tapped_graph.transform.GetChild(0).gameObject;
                        Transform[] allChildrennode = tempnodeparent.GetComponentsInChildren<Transform>();

                        foreach (Transform child in allChildrennode)
                        {
                            if (child.tag != "iconic")
                                continue;
                            Debug.Log("checkfor_vertex_del:" + child.name);
                            if (functionline.GetComponent<FunctionElementScript>().isInsidePolygon(child.GetComponent<iconicElementScript>().edge_position))
                            {
                                selected_icons.Add(child.gameObject);
                            }
                        }

                        if (selected_icons.Count == 0)
                        {
                            Destroy(functionline);
                            //functionline = null;
                            return;
                        }

                        GameObject tempedgeparent = potential_tapped_graph.transform.GetChild(1).gameObject;
                        Transform[] allChildrenedge = tempedgeparent.GetComponentsInChildren<Transform>();

                        foreach (Transform child in allChildrenedge)
                        {
                            if (child.tag != "edge")
                                continue;

                            if (selected_icons.Contains(child.GetComponent<EdgeElementScript>().edge_start) &&
                                selected_icons.Contains(child.GetComponent<EdgeElementScript>().edge_end))
                            {
                                Destroy(child.gameObject);
                            }
                        }

                        potential_tapped_graph.GetComponent<GraphElementScript>().edges_init();
                        //potential_tapped_graph.GetComponent<GraphElementScript>().Graph_as_Str();

                        Destroy(functionline);
                        //functionline = null;
                    }

                    else
                    {
                        Destroy(functionline);
                        //functionline = null;
                    }
                }
                else
                {
                    // delete the templine, not enough points
                    // Debug.Log("here_in_destroy");
                    Destroy(functionline);
                    //functionline = null;
                }
            }
            else
            {
                // the touch didn't end on a line, destroy the line
                // Debug.Log("here_in_destroy_different_Hit");
                Destroy(functionline);
                //functionline = null;
            }

            potential_tapped_graph = null;
        }
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
                if (LastPhaseHappened != 1)
                {
                    StartTouchTime = Time.realtimeSinceStartup;
                    taping_flag = true;
                }
                LastPhaseHappened = 1;
                startPos = currentTouch.position;
                break;

            case UnityEngine.TouchPhase.Moved:
                //if (LastPhaseHappend != 2)
                if (Vector2.Distance(currentTouch.position, startPos) > 2)
                {
                    taping_flag = false;
                }
                LastPhaseHappened = 2;
                break;

            case UnityEngine.TouchPhase.Ended:
                if (LastPhaseHappened != 3)
                {
                    EndTouchTime = Time.realtimeSinceStartup;
                    TouchTime = EndTouchTime - StartTouchTime;
                }
                LastPhaseHappened = 3;

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
        bool edge_menu_create = false;

        if (canvas_radial.transform.childCount > 0)
        {
            return;
        }

        var ray = Camera.main.ScreenPointToRay(menu_position);

        RaycastHit Hit;
        RaycastHit2D hit2d;

        hit2d = Physics2D.GetRayIntersection(ray);
        if (hit2d.collider != null && hit2d.collider.gameObject.tag == "edge")
        {
            Debug.Log("hit:" + hit2d.collider.gameObject.tag);
            Vector3 vec_radius_offset = new Vector3(0f, 10f, 0f);
            GameObject radmenu = Instantiate(edge_radial_menu,
                        canvas_radial.transform.TransformPoint(hit2d.collider.gameObject.GetComponent<EdgeCollider2D>().bounds.center - vec_radius_offset)
                        /*hit2d.collider.gameObject.GetComponent<EdgeCollider2D>().bounds.center*/,
                        Quaternion.identity,
                        canvas_radial.transform);
            radmenu.GetComponent<EdgeMenuScript>().menu_parent = hit2d.collider.gameObject;
            edge_menu_create = true;
        }

        //if (edge_menu_create) return;

        if (Physics.Raycast(ray, out Hit))
        {
            Debug.Log("hit in menu creation: " + Hit.collider.gameObject.tag);

            if (Hit.collider.gameObject.tag == "iconic")
            {
                if(graphlocked)
                {
                    Debug.Log("graph_menu_created");
                    Transform node_parent = Hit.collider.gameObject.transform.parent;
                    if (node_parent.tag == "node_parent")
                    {
                        node_parent.parent.GetComponent<GraphElementScript>().createMenu(canvas_radial);
                        edge_menu_create = false;
                    }                    
                }
                else
                {
                    Debug.Log("node_menu_created");

                    var radius_offset = Hit.collider.gameObject.GetComponent<iconicElementScript>().radius;
                    Vector3 vec_radius_offset = new Vector3(0f, 1.25f * radius_offset, 0f);
                    GameObject radmenu = Instantiate(node_radial_menu,
                            canvas_radial.transform.TransformPoint(Hit.collider.gameObject.GetComponent<iconicElementScript>().edge_position - vec_radius_offset)
                            /*Hit.collider.gameObject.GetComponent<iconicElementScript>().edge_position*/,
                            Quaternion.identity,
                            canvas_radial.transform);
                    radmenu.GetComponent<NodeMenuScript>().menu_parent = Hit.collider.gameObject;
                    Hit.collider.gameObject.GetComponent<iconicElementScript>().menu_open = true;
                    edge_menu_create = false;
                }
            }

        }

        

               
    }

    // normal simple graph
    void GraphCreation()
    {
        // if they are already under the same graph, no need to create a new one. Just assign the new edgeline to the previous parent
        if (edge_start.transform.parent == edge_end.transform.parent && edge_start.transform.parent.tag == "node_parent")
        {
            Transform Prev_graph_parent = edge_start.transform.parent.transform.parent;
            Prev_graph_parent.GetComponent<GraphElementScript>().abstraction_layer = "graph";
            Prev_graph_parent.GetChild(1).gameObject.SetActive(true);
            edgeline.transform.parent = Prev_graph_parent.GetChild(1);
            Prev_graph_parent.GetComponent<GraphElementScript>().edges_init();
            //Prev_graph_parent.GetComponent<GraphElementScript>().edges_as_Str();
            return;
        }

        graph_count++;
        GameObject tempgraph = Instantiate(GraphElement);
        tempgraph.GetComponent<GraphElementScript>().abstraction_layer = "graph";
        tempgraph.GetComponent<GraphElementScript>().paintable = transform.gameObject;
        tempgraph.name = "graph_"+graph_count.ToString();
        tempgraph.tag = "graph";
        tempgraph.transform.parent = Objects_parent.transform;
        tempgraph.GetComponent<GraphElementScript>().graph_name = "G" + graph_count.ToString();

        GameObject tempnodeparent = tempgraph.transform.GetChild(0).gameObject;
        /*new GameObject("node_parent_" + graph_count.ToString());
        tempnodeparent.tag = "node_parent";
        tempnodeparent.transform.parent = tempgraph.transform;
        tempnodeparent.transform.SetSiblingIndex(0);*/

        GameObject tempedgeparent = tempgraph.transform.GetChild(1).gameObject;
        /*new GameObject("edge_parent_" + graph_count.ToString());
        tempedgeparent.tag = "edge_parent";
        tempedgeparent.transform.parent = tempgraph.transform;
        tempedgeparent.transform.SetSiblingIndex(1);*/

        GameObject tempsimplicialparent = tempgraph.transform.GetChild(2).gameObject;
        /*new GameObject("simplicial_parent_" + graph_count.ToString());
        tempsimplicialparent.tag = "simplicial_parent";
        tempsimplicialparent.transform.parent = tempgraph.transform;
        tempsimplicialparent.transform.SetSiblingIndex(2);*/

        GameObject temphyperparent = tempgraph.transform.GetChild(3).gameObject;
        /*new GameObject("hyper_parent_" + graph_count.ToString());
        temphyperparent.tag = "hyper_parent";
        temphyperparent.transform.parent = tempgraph.transform;
        temphyperparent.transform.SetSiblingIndex(3);*/

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

                if (Prev_edge_parent.gameObject.activeSelf)
                {
                    foreach (Transform child in allChildrenedge)
                    {
                        if (child.tag == "edge")
                            child.parent = tempedgeparent.transform;
                    }
                }

                if (Prev_simplicial_parent.gameObject.activeSelf)
                {
                    foreach (Transform child in allChildrensimpli)
                    {
                        if (child.tag == "simplicial")
                            child.parent = tempsimplicialparent.transform;
                    }
                }

                if (Prev_hyper_parent.gameObject.activeSelf)
                {
                    foreach (Transform child in allChildrenhyper)
                    {
                        if (child.tag == "hyper")
                            child.parent = temphyperparent.transform;
                    }
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

        //tempgraph.GetComponent<GraphElementScript>().Graph_as_Str();
        tempgraph.GetComponent<GraphElementScript>().Graph_init();
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
            Prev_graph_parent.GetComponent<GraphElementScript>().abstraction_layer = "simplicial";
            Prev_graph_parent.GetChild(2).gameObject.SetActive(true);
            simplicialline.transform.parent = Prev_graph_parent.GetChild(2);
            Prev_graph_parent.GetComponent<GraphElementScript>().simplicial_init();
            //Prev_graph_parent.GetComponent<GraphElementScript>().simplicial_as_Str();
            return;
        }

        graph_count++;
        GameObject tempgraph = Instantiate(GraphElement);
        tempgraph.GetComponent<GraphElementScript>().abstraction_layer = "simplicial";
        tempgraph.GetComponent<GraphElementScript>().paintable = transform.gameObject;
        tempgraph.name = "graph_" + graph_count.ToString();
        tempgraph.tag = "graph";
        tempgraph.transform.parent = Objects_parent.transform;
        tempgraph.GetComponent<GraphElementScript>().graph_name = "G" + graph_count.ToString();

        GameObject tempnodeparent = tempgraph.transform.GetChild(0).gameObject;
        /*new GameObject("node_parent_" + graph_count.ToString());
        tempnodeparent.tag = "node_parent";
        tempnodeparent.transform.parent = tempgraph.transform;
        tempnodeparent.transform.SetSiblingIndex(0);*/

        GameObject tempedgeparent = tempgraph.transform.GetChild(1).gameObject;
        /*new GameObject("edge_parent_" + graph_count.ToString());
        tempedgeparent.tag = "edge_parent";
        tempedgeparent.transform.parent = tempgraph.transform;
        tempedgeparent.transform.SetSiblingIndex(1);*/

        GameObject tempsimplicialparent = tempgraph.transform.GetChild(2).gameObject;
        /*new GameObject("simplicial_parent_" + graph_count.ToString());
        tempsimplicialparent.tag = "simplicial_parent";
        tempsimplicialparent.transform.parent = tempgraph.transform;
        tempsimplicialparent.transform.SetSiblingIndex(2);*/

        GameObject temphyperparent = tempgraph.transform.GetChild(3).gameObject;
        /*new GameObject("hyper_parent_" + graph_count.ToString());
        temphyperparent.tag = "hyper_parent";
        temphyperparent.transform.parent = tempgraph.transform;
        temphyperparent.transform.SetSiblingIndex(3);*/

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

        tempgraph.GetComponent<GraphElementScript>().Graph_init();
        //tempgraph.GetComponent<GraphElementScript>().Graph_as_Str();
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
            Prev_graph_parent.GetComponent<GraphElementScript>().abstraction_layer = "hypergraph";
            // 3 is hyper edge parent index
            Prev_graph_parent.GetChild(3).gameObject.SetActive(true);
            hyperline.transform.parent = Prev_graph_parent.GetChild(3);
            Prev_graph_parent.GetComponent<GraphElementScript>().hyperedges_init();
            //Prev_graph_parent.GetComponent<GraphElementScript>().hyperedges_as_Str();
            return;
        }

        graph_count++;
        GameObject tempgraph = Instantiate(GraphElement);
        tempgraph.GetComponent<GraphElementScript>().abstraction_layer = "hypergraph";
        tempgraph.GetComponent<GraphElementScript>().paintable = transform.gameObject;
        tempgraph.name = "graph_" + graph_count.ToString();
        tempgraph.tag = "graph";
        tempgraph.transform.parent = Objects_parent.transform;
        tempgraph.GetComponent<GraphElementScript>().graph_name = "G" + graph_count.ToString();

        GameObject tempnodeparent = tempgraph.transform.GetChild(0).gameObject;
        /*new GameObject("node_parent_" + graph_count.ToString());
        tempnodeparent.tag = "node_parent";
        tempnodeparent.transform.parent = tempgraph.transform;
        tempnodeparent.transform.SetSiblingIndex(0);*/

        GameObject tempedgeparent = tempgraph.transform.GetChild(1).gameObject;
        /*new GameObject("edge_parent_" + graph_count.ToString());
        tempedgeparent.tag = "edge_parent";
        tempedgeparent.transform.parent = tempgraph.transform;
        tempedgeparent.transform.SetSiblingIndex(1);*/

        GameObject tempsimplicialparent = tempgraph.transform.GetChild(2).gameObject;
        /*new GameObject("simplicial_parent_" + graph_count.ToString());
        tempsimplicialparent.tag = "simplicial_parent";
        tempsimplicialparent.transform.parent = tempgraph.transform;
        tempsimplicialparent.transform.SetSiblingIndex(2);*/

        GameObject temphyperparent = tempgraph.transform.GetChild(3).gameObject;
        /*new GameObject("hyper_parent_" + graph_count.ToString());
        temphyperparent.tag = "hyper_parent";
        temphyperparent.transform.parent = tempgraph.transform;
        temphyperparent.transform.SetSiblingIndex(3);*/

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

        tempgraph.GetComponent<GraphElementScript>().Graph_init();
        //tempgraph.GetComponent<GraphElementScript>().Graph_as_Str();
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

    void searchNodeAndDeleteEdge(GameObject node_name)
    {
        Transform Prev_node_parent = node_name.transform.parent;

        if (Prev_node_parent.tag != "node_parent") return;
        
        Transform Prev_graph_parent = Prev_node_parent.transform.parent;
        Transform Prev_edge_parent = Prev_graph_parent.GetChild(1);
        Transform Prev_simplicial_parent = Prev_graph_parent.GetChild(2);
        Transform Prev_hyper_parent = Prev_graph_parent.GetChild(3);

        Transform[] allChildrenedge = Prev_edge_parent.GetComponentsInChildren<Transform>();
        Transform[] allChildrensimpli = Prev_simplicial_parent.GetComponentsInChildren<Transform>();
        Transform[] allChildrenhyper = Prev_hyper_parent.GetComponentsInChildren<Transform>();
                        
        foreach (Transform child in allChildrenedge)
        {
            if (child.tag == "edge")
            {
                GameObject source = child.GetComponent<EdgeElementScript>().edge_start;
                GameObject target = child.GetComponent<EdgeElementScript>().edge_end;

                if (source == node_name || target == node_name)
                {
                    Destroy(child.gameObject);
                    //break;
                }
            }
        }
                
        foreach (Transform each_simplicial in allChildrensimpli)
        {
            if (each_simplicial.tag == "simplicial")
            {
                if (each_simplicial.GetComponent<SimplicialElementScript>() != null)
                {
                    List<GameObject> thenodes = each_simplicial.GetComponent<SimplicialElementScript>().thenodes;

                    foreach (GameObject each_node in thenodes)
                    {
                        if (each_node == node_name)
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

                    if (source == node_name || target == node_name)
                    {
                        Destroy(each_simplicial.gameObject);
                        //break;
                    }
                }
            }                
        }
                
        foreach (Transform child in allChildrenhyper)
        {
            if (child.tag == "hyper_child_edge")
            {
                if (child.GetComponent<HyperEdgeElement>().parent_node == node_name)
                {
                    Destroy(child.parent.gameObject);
                }
            }
        }
                
    }

    void handleKeyInteractions()
    {
        //we don't want any redundant operation when a function name is being typed
        if (pan_button.GetComponent<AllButtonsBehaviors>().selected)
        {
            if (Input.GetKeyUp(KeyCode.RightArrow))
            {
                Camera.main.transform.position += Vector3.right * speed /** Time.deltaTime*/;
            }
            if (Input.GetKeyUp(KeyCode.LeftArrow))
            {
                Camera.main.transform.position += Vector3.left * speed /** Time.deltaTime*/;
            }
            if (Input.GetKeyUp(KeyCode.UpArrow))
            {
                Camera.main.transform.position += Vector3.up * speed /** Time.deltaTime*/;
            }
            if (Input.GetKeyUp(KeyCode.DownArrow))
            {
                Camera.main.transform.position += Vector3.down * speed /** Time.deltaTime*/;
            }
            if (Input.GetKeyUp(KeyCode.Plus) || Input.GetKeyUp(KeyCode.KeypadPlus))
            {
                float difference = 100;
                Camera.main.orthographicSize = Mathf.Clamp(Camera.main.orthographicSize - zoom_multiplier * difference, zoom_min, zoom_max);
                                
                int zoom = (int)((1f - ((main_camera.orthographicSize - zoom_min) / zoom_max)) * 100f);
                text_message_worldspace.GetComponent<TextMeshProUGUI>().text = zoom.ToString("F0") + "%";
            }
            if (Input.GetKeyUp(KeyCode.Minus) || Input.GetKeyUp(KeyCode.KeypadMinus))
            {
                float difference = 100;
                Camera.main.orthographicSize = Mathf.Clamp(Camera.main.orthographicSize + zoom_multiplier * difference, zoom_min, zoom_max);

                int zoom = (int)((1f - ((main_camera.orthographicSize - zoom_min) / zoom_max)) * 100f);
                text_message_worldspace.GetComponent<TextMeshProUGUI>().text = zoom.ToString("F0") + "%";
            }
        }

        if (function_brush_button.GetComponent<AllButtonsBehaviors>().selected)
        {
            return;
        }

        if (Input.GetKeyUp(KeyCode.L))
        {
            panZoomLocked = !panZoomLocked;
            Debug.Log("panning_value_change"+ panZoomLocked.ToString());
            GameObject temp_stat = Instantiate(status_label_obj, canvas_radial.transform);
            temp_stat.GetComponent<Status_label_text>().ChangeLabel("panning: " + panZoomLocked.ToString());
        }

        if (Input.GetKeyUp(KeyCode.F))
        {
            free_hand_edge = !free_hand_edge;
            GameObject temp_stat = Instantiate(status_label_obj, canvas_radial.transform);
            temp_stat.GetComponent<Status_label_text>().ChangeLabel("free hand drawing: " + free_hand_edge.ToString());
            Debug.Log("NOT WORKING?");
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

            history_view.gameObject.SetActive(ActionHistoryEnabled);

            if (ActionHistoryEnabled)
            {
                StartCoroutine(HistoryShow());
            }
        }

        //Graph
        if (Input.GetKeyUp(KeyCode.G))
        {            
            graphlocked = !graphlocked;
            GameObject temp_stat = Instantiate(status_label_obj, canvas_radial.transform);
            temp_stat.GetComponent<Status_label_text>().ChangeLabel("graph lock: " + graphlocked.ToString());
        }

        if (Input.GetKeyUp(KeyCode.D))
        {
            directed_edge = !directed_edge;
            GameObject temp_stat = Instantiate(status_label_obj, canvas_radial.transform);
            temp_stat.GetComponent<Status_label_text>().ChangeLabel("directed edge: " + directed_edge.ToString());
        }

        if (Input.GetKeyUp(KeyCode.Alpha1) || Input.GetKeyUp(KeyCode.Keypad1))
        {
            vertex_add = !vertex_add;
            if (vertex_add)
            {
                vertex_del = false;
                edge_add = false;
                edge_del = false;
            }

            GameObject temp_stat = Instantiate(status_label_obj, canvas_radial.transform);
            temp_stat.GetComponent<Status_label_text>().ChangeLabel("vertex addition: " + vertex_add.ToString());
        }

        if (Input.GetKeyUp(KeyCode.Alpha2) || Input.GetKeyUp(KeyCode.Keypad2))
        {
            vertex_del = !vertex_del;
            if (vertex_del)
            {
                vertex_add = false;
                edge_add = false;
                edge_del = false;
            }

            GameObject temp_stat = Instantiate(status_label_obj, canvas_radial.transform);
            temp_stat.GetComponent<Status_label_text>().ChangeLabel("vertex deletion: " + vertex_del.ToString());
        }

        if (Input.GetKeyUp(KeyCode.Alpha3) || Input.GetKeyUp(KeyCode.Keypad3))
        {
            edge_add = !edge_add;
            if (edge_add)
            {
                vertex_add = false;
                vertex_del = false;
                edge_del = false;
            }

            GameObject temp_stat = Instantiate(status_label_obj, canvas_radial.transform);
            temp_stat.GetComponent<Status_label_text>().ChangeLabel("edge addition: " + edge_add.ToString());
        }

        if (Input.GetKeyUp(KeyCode.Alpha4) || Input.GetKeyUp(KeyCode.Keypad4))
        {
            edge_del = !edge_del;
            if (edge_del)
            {
                vertex_add = false;
                vertex_del = false;
                edge_add = false;
            }

            GameObject temp_stat = Instantiate(status_label_obj, canvas_radial.transform);
            temp_stat.GetComponent<Status_label_text>().ChangeLabel("edge deletion: " + edge_del.ToString());
        }
    }

    void deleteTempLineIfDoubleFinger()
    {
        // Should only be called when necessary -- to get rid of incomplete lines when a double finger is detected and we are not in the pan mode.
        if (Input.touchCount > 1 && templine != null && pan_button.GetComponent<AllButtonsBehaviors>().selected == false)
        {
            Destroy(templine);
        }
    }

    public IEnumerator HistoryModify(GameObject functionline)
    {
        Debug.Log("called from " + functionline.name);

        history.Add(functionline);
        yield return null;

        for (int j = 0; j < history.Count; j++)
        {
            // remove deleted functions
            if (history[j] == null) history.RemoveAt(j);
        }

        if (history.Count > 10)
        {            
            for(int j = 0; j < (history.Count - 10); j++)
            {
                history.RemoveAt(0);
            }
        }

        if (ActionHistoryEnabled)
        {
            StartCoroutine(HistoryShow());
        }

    }

    public IEnumerator HistoryShow()
    {
        Debug.Log("modifying ");
        GameObject first_item = history_list_viewer.transform.GetChild(0).gameObject;

        if (history_list_viewer.transform.childCount > 1)
        {
            for (int j = 1; j < history_list_viewer.transform.childCount; j++)
            {
                Destroy(history_list_viewer.transform.GetChild(j).gameObject);
            }
        }

        int child_iter = 1;

        for (int j = 0; j < history.Count; j++)
        {
            if (history[j] == null)
            {
                history.RemoveAt(j);
            }

            else if (history[j].GetComponent<FunctionElementScript>().graph_generation_done)
            {
                GameObject temp_func_item = Instantiate(first_item);
                temp_func_item.transform.parent = history_list_viewer.transform;
                temp_func_item.transform.SetSiblingIndex(child_iter);

                temp_func_item.GetComponent<TextMeshProUGUI>().text = child_iter.ToString() + ". " + history[j].name + ": " + "\n" +
                    history[j].transform.GetChild(0).GetComponent<FunctionMenuScript>().text_label.GetComponent<TextMeshProUGUI>().text;
                temp_func_item.GetComponent<TextMeshProUGUI>().fontSize = 8;
                //temp_func_item.transform.localScale = new Vector3(1f, 1f, 1f);
                child_iter++;

                yield return null;
            }
                            
        }
    }

    
}
