using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem.EnhancedTouch;
using UnityEngine.InputSystem;

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
                Destroy(templine);
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
                    Debug.Log("hit_iconic_elem_at"+ Hit.collider.gameObject.GetComponent<BoxCollider>().center.ToString());
                    if (selected_obj.Count == 0)
                    {
                        GameObject cur = Hit.collider.gameObject;
                        //selected_obj.Add(cur.GetComponent<Transform>().TransformPoint(Hit.collider.gameObject.GetComponent<BoxCollider>().center));
                        selected_obj.Add(Hit.collider.gameObject);
                        selected_obj_count++;
                    }
                    else if (selected_obj.Count > 0 &&
                        (Hit.collider.gameObject.GetComponent<BoxCollider>().center - selected_obj[selected_obj.Count - 1].GetComponent<BoxCollider>().center).magnitude > 0f)
                    {
                        GameObject cur = Hit.collider.gameObject;
                        //selected_obj.Add(cur.GetComponent<Transform>().TransformPoint(Hit.collider.gameObject.GetComponent<BoxCollider>().center));
                        selected_obj.Add(Hit.collider.gameObject);
                        selected_obj_count++;
                    }

                    if (selected_obj.Count == 2)
                    {
                        Debug.Log("two objects touched, now draw the edge");
                        GameObject edgeline = Instantiate(EdgeElement, selected_obj[0].GetComponent<BoxCollider>().center, Quaternion.identity, Objects_parent.transform);
                        edgeline.GetComponent<EdgeElementScript>().node_obj = selected_obj;

                        //edgeline.GetComponent<TrailRenderer>().minVertexDistance = 10000f;
                        //edgeline.GetComponent<TrailRenderer>().material.color = Color.black;

                        edgeline.name = "edge_" + selected_obj_count.ToString();
                        edgeline.tag = "edge";

                        //edgeline.GetComponent<EdgeElementScript>().points.Add(selected_obj[0]);

                        /*for (int i = 0; i < 100; i++)
                        {
                            Vector3 temp = Vector3.Lerp(selected_obj[0], selected_obj[1], i/100);
                            edgeline.GetComponent<TrailRenderer>().transform.position = temp;
                            edgeline.GetComponent<EdgeElementScript>().points.Add(temp);
                        }*/


                        //edgeline.GetComponent<TrailRenderer>().transform.position = selected_obj[1];
                        //edgeline.GetComponent<EdgeElementScript>().points.Add(selected_obj[1]);

                        List<Vector3> selected_vects = new List<Vector3>();

                        foreach (GameObject each_obj in selected_obj)
                        {
                            selected_vects.Add(each_obj.GetComponent<BoxCollider>().center);
                        }
                        

                        //https://generalistprogrammer.com/unity/unity-line-renderer-tutorial/
                        edgeline.GetComponent<LineRenderer>().material.color = Color.black;
                        LineRenderer l = edgeline.GetComponent<LineRenderer>();
                        l.startWidth = 2f;
                        l.endWidth = 2f;
                        l.SetPositions(selected_vects.ToArray());
                        l.useWorldSpace = true;

                        /*foreach (Vector3 vec in selected_obj)
                        {
                            Debug.Log("printing points "+vec.ToString());
                            edgeline.GetComponent<TrailRenderer>().transform.position = vec;
                            edgeline.GetComponent<EdgeElementScript>().points.Add(vec);

                            templine.GetComponent<EdgeElementScript>().calculateLengthAttributeFromPoints();
                            
                            // pressure based pen width
                            templine.GetComponent<EdgeElementScript>().updateLengthFromPoints();
                            templine.GetComponent<EdgeElementScript>().addPressureValue(PenTouchInfo.pressureValue);
                            templine.GetComponent<EdgeElementScript>().reNormalizeCurveWidth();
                            templine.GetComponent<TrailRenderer>().widthCurve = templine.GetComponent<EdgeElementScript>().widthcurve;
                        }*/

                        edgeline = edgeline.GetComponent<EdgeElementScript>().FinishEdgeLine();
                        edgeline = null;

                        selected_obj.Clear();
                    }
                }

            }

        }
        else
        {
            // just disabled edge button, disable all collider
            if (enablecollider == true)
            {
                enablecollider = false;
                GameObject[] drawnlist = GameObject.FindGameObjectsWithTag("iconic");

                foreach (GameObject icon in drawnlist)
                {
                    if (icon.GetComponent<BoxCollider>() != null)
                        icon.GetComponent<BoxCollider>().enabled = false;
                }
            }
        }

        #endregion

       // HANDLE ANY RELEVANT KEY INPUT FOR PAINTABLE'S OPERATIONS
        handleKeyInteractions();
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
