using System.Collections;
using System.Collections.Generic;
using System;
using System.Linq;
using UnityEngine;

using UnityEngine.UI;
using UnityEngine.InputSystem;

public class iconicElementScript : MonoBehaviour
{


    // previous first child, instead of creating a separate child, we now want to keep it in the script
    public Mesh _mesh;

    // visual representation
    // public List<Vector2> points;

    // object drawing properties
    public List<Vector3> points = new List<Vector3>();
    public Vector3 centroid;
    public float radius;
    public float radius_margin;

    public float maxx = -100000f, maxy = -100000f, minx = 100000f, miny = 100000f;
    public bool draggable_now = false;

    public Material icon_elem_material;
    public GameObject paintable_object;
    public string icon_name;
    // needed for abstraction conversion
    public int icon_number;

    // get-able attributes
    public struct Attributes
    {
        public float Length;
        public float Area;
    };

    public Attributes attribute;
    public float current_attribute;

    // movement related variables
    public Vector3 previous_position;	// used for checking if the pen object is under a set or function

    // path record
    public List<Vector3> recorded_path = new List<Vector3>();
    public List<Vector3> hull_pts = new List<Vector3>();
    public bool record_path_enable = false;
    public Vector3 position_before_record;
    public Vector3 edge_position;

    // path translate
    public Vector3 position_before_translation;
    public List<Vector3> translation_path = new List<Vector3>();
    public SortedList<float, int> translation_sections_indices = new SortedList<float, int>();
    public int path_index = 0;
    public float eps_path_radius = 10f;
    public bool has_continuous_path = true;
    public float heightScale = 20, xScale = 100;

    // causal transformation parameters
    public float max_possible_scale = 2.0f;
    public float max_possible_rotation = 360.0f;
    public float rotation_angle, scale_value;
    public float min_rotation_val = 0f;
    public float max_rotation_val = 5f;
    public float min_scale_val = 1f;
    public float max_scale_val = 5f;
    public bool apply_rotation = false;
    public bool apply_scale = false;

    // other interaction variables
    private Vector3 touchDelta = new Vector3();

    // pressure based line width
    public List<float> pressureValues = new List<float>();
    public AnimationCurve widthcurve = new AnimationCurve();
    float totalLength = 0f;
    List<float> cumulativeLength = new List<float>();
    int movingWindow = 3;

    // Pen Interaction
    Pen currentPen;

    // double function related variables
    public bool is_this_double_function_operand = false;

    // play log related variables
    public Dictionary<string, Vector2> saved_positions = new Dictionary<string, Vector2>();

    // global stroke details
    public GameObject details_dropdown;
    public bool global_details_on_path = true;

    public void computeCentroid()
    {
        float totalx = 0, totaly = 0, totalz = 0;
        for (int i = 0; i < points.Count; i++)
        {
            totalx += points[i].x;
            totaly += points[i].y;
            totalz += points[i].z;
        }
        centroid = new Vector3(totalx / points.Count, totaly / points.Count, points[0].z); // including z in the average calc. created problem
                                                                                           // so just used a constant value from the points list.

        computeRadius();
    }

    public void computeRadius()
    {
        int Count = 0;
        int interval = (int)Math.Floor((float)(points.Count / 10));
        float totalx = 0, totaly = 0, totalz = 0;
        float distance = 0;
        for (int i = 0; i < points.Count; i = i + interval)
        {
            totalx = points[i].x - centroid.x;
            totaly = points[i].y - centroid.y;
            totalz = points[i].z - centroid.z;
            distance += (float)Math.Sqrt((totalx * totalx) + (totaly * totaly) + (totalz * totalz));
            Count++;
        }
        radius = distance / Count;
    }

    public List<Vector3> hullPoints(float margin = 0f)
    {
        if (margin == 0f)
            margin = radius_margin;

        hull_pts = new List<Vector3>();

        for (int i = 0; i < 361; i = i + 36)
        {
            hull_pts.Add(new Vector3(edge_position.x + ((radius + margin) * (float)Math.Cos(i)),
                edge_position.y + ((radius + margin) * (float)Math.Sin(i)),
                -5f));
        }

        return hull_pts;
    }

    public void computeBounds()
    {
        maxx = -100000f; maxy = -100000f; minx = 100000f; miny = 100000f;

        for (int i = 0; i < points.Count; i++)
        {
            if (maxx < points[i].x) maxx = points[i].x;
            if (maxy < points[i].y) maxy = points[i].y;
            if (minx > points[i].x) minx = points[i].x;
            if (miny > points[i].y) miny = points[i].y;
        }
    }

    public void fromGlobalToLocalPoints()
    {
        // transform all points from world to local coordinate with respect to the transform position of the set game object
        for (int i = 0; i < points.Count; i++)
        {
            points[i] = transform.InverseTransformPoint(points[i]);
        }
    }

    public List<Vector3> fromLocalToGlobalPoints()
    {
        // Assumes fromGlobalToLocalPoints() has already been called on points set
        List<Vector3> gbpoints = new List<Vector3>();

        for (int i = 0; i < points.Count; i++)
        {
            gbpoints.Add(transform.TransformPoint(points[i]));
        }

        return gbpoints;
    }

    public void updateLengthFromPoints()
    {
        totalLength = 0f;
        cumulativeLength.Clear();
        for (int i = 1; i < points.Count; i++)
        {
            totalLength += Vector3.Distance(points[i - 1], points[i]);
            cumulativeLength.Add(totalLength);
        }
    }

    public void addPressureValue(float val)
    {
        pressureValues.Add(val);
    }

    public void reNormalizeCurveWidth()
    {
        // create a curve with as many points as the current number of pressure values
        int numPts = cumulativeLength.Count;
        widthcurve = new AnimationCurve();

        if (numPts > movingWindow)
        {
            List<float> smoothedPressureValues = smoothenList(pressureValues);

            for (int i = 0; i < numPts - movingWindow; i++)
            {
                widthcurve.AddKey(cumulativeLength[i] / totalLength, Mathf.Clamp(pressureValues[i], 0f, 1f));
            }
        }
    }

    public List<float> smoothenList(List<float> values)
    {
        // take the width curve (float values) and run a moving average operation
        return Enumerable
                .Range(0, values.Count - movingWindow)
                .Select(n => values.Skip(n).Take(movingWindow).Average())
                .ToList();
    }

    public void checkHitAndMove()
    {
        // TO CONSIDER: INSTEAD OF CHECKING TOUCH-MOVED, CAN WE CHECK TOUCH INIT, WAIT TILL TOUCH END, AND THEN MOVE OBJECT? WOULD THAT BE
        // TOO CLUMSY? IT SURE WOULD BE FAST.
        // assumes a touch has registered and pan mode is selected

        if (Paintable.pan_button.GetComponent<AllButtonsBehaviors>().selected == true)
        {
            //currentPen = Pen.current;

            if (PenTouchInfo.PressedThisFrame)//currentPen.tip.wasPressedThisFrame)
            {

                var ray = Camera.main.ScreenPointToRay(PenTouchInfo.penPosition);
                RaycastHit Hit;

                // check for a hit on the anchor object
                if (Physics.Raycast(ray, out Hit) && Hit.collider.gameObject == transform.gameObject)
                {
                    draggable_now = true;
                    Vector3 vec = Hit.point; //+ new Vector3(0, 0, -40); // Vector3.up * 0.1f;
                                             //Debug.Log(vec);

                    // enforce the same z coordinate as the rest of the points in the parent set object
                    vec.z = -5f;

                    touchDelta = transform.position - vec;

                    GetComponent<TrailRenderer>().Clear();

                }
            }

            if (PenTouchInfo.PressedNow)//currentPen.tip.isPressed)
            {
                //Debug.Log("penline touched");

                var ray = Camera.main.ScreenPointToRay(PenTouchInfo.penPosition);
                RaycastHit Hit;

                // check for a hit on the anchor object
                if (Physics.Raycast(ray, out Hit) && Hit.collider.gameObject == this.gameObject)
                {
                    //Debug.Log(transform.name);

                    draggable_now = true;

                    Vector3 vec = Hit.point; //+ new Vector3(0, 0, -40); // Vector3.up * 0.1f;
                                             //Debug.Log(vec);

                    // enforce the same z coordinate as the rest of the points in the parent set object
                    vec.z = -5f;

                    Vector3 diff = vec - transform.position + touchDelta;
                    diff.z = 0;

                    // update the drawn object position.
                    transform.position += diff;

                    // update the previous_position variable: since user moved it by hand, not driven by a set or function
                    previous_position = transform.position;

                    // update the menu position if a menu is currently open/created
                    /*GameObject menu_obj = GameObject.Find(menu_name);
                    if (menu_obj != null)
                    {
                        menu_obj.transform.position = vec;
                    }*/

                    // record path UI
                    if (record_path_enable)
                    {
                        recordPath();

                        // enable trail renderer to show the drawn path
                        GetComponent<TrailRenderer>().enabled = true;
                        GetComponent<TrailRenderer>().material.color = new Color(110, 0, 0);
                    }

                    // experimental: update membership while being dragged
                    // checkAndUpdateMembership();
                }
            }

            else if (PenTouchInfo.ReleasedThisFrame)//currentPen.tip.wasReleasedThisFrame)
            {
                // important: touch can start and end when interacting with other UI elements like a set's slider.
                // so, double check that this touch was still on top of the penline object, not, say, on a slider.
                var ray = Camera.main.ScreenPointToRay(PenTouchInfo.penPosition);
                RaycastHit Hit;

                if (Physics.Raycast(ray, out Hit) && Hit.collider.gameObject == this.gameObject)
                {
                    // if record path UI was on, then take care of the recorded path and return pen object to original position,
                    // don't do any parent object containment checking in that case
                    if (record_path_enable)
                    {
                        recordPath();

                        transform.position = position_before_record;

                        draggable_now = false;

                        // Add a line renderer component and re-distribute the points across the line
                        this.gameObject.AddComponent<LineRenderer>();

                        // ToDo: bezier spline needs to be incorporated later again
                        // UNIFORMLY DISTRIBUTE THE POINTS ALONG THE PATH
                        /*GameObject spline = new GameObject("spline");
                        spline.AddComponent<BezierSpline>();
                        BezierSpline bs = spline.transform.GetComponent<BezierSpline>();
                        bs.Initialize(recorded_path.Count);
                        for (int ss = 0; ss < recorded_path.Count; ss++)
                        {
                            bs[ss].position = recorded_path[ss];
                        }

                        // Now sample 50 points across the spline with a [0, 1] parameter sweep
                        recorded_path = new List<Vector3>(50);
                        for (int i = 0; i < 50; i++)
                        {
                            recorded_path.Add(bs.GetPoint(Mathf.InverseLerp(0, 49, i)));
                        }

                        Destroy(spline);*/

                        record_path_enable = false;

                        GetComponent<TrailRenderer>().enabled = false;

                        // convert global recorded coordinates to local, wrt pen object
                        for (int k = 0; k < recorded_path.Count; k++)
                        {
                            recorded_path[k] = transform.InverseTransformPoint(recorded_path[k]);
                        }

                        // calculate the actual (global) translation path
                        // calculateTranslationPath();

                        // (re)create param text and text box
                        // createParamGUIText();

                        // indicate what kind of path this penline has
                        has_continuous_path = true;
                        return;
                    }

                    draggable_now = false;

                    // the object has been moved by hand. Recalculate the new translation path.
                    // this does nothing if a translation path isn't defined yet
                    // calculateTranslationPathIfAlreadyDefined();

                    // checkAndUpdateMembership();
                }
            }
        }
    }

    /*public void checkContinuousPathDefinitionInteraction()
    {

        if (Paintable.pathdefinition_button.GetComponent<AllButtonsBehavior>().selected &&
            Paintable.pathdefinition_button.GetComponent<AllButtonsBehavior>().isContinuousPathDefinition)
        {
            //currentPen = Pen.current;

            //if (Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Began)
            if (PenTouchInfo.PressedThisFrame)//currentPen.tip.wasPressedThisFrame)
            {
                var ray = Camera.main.ScreenPointToRay(PenTouchInfo.penPosition);
                RaycastHit Hit;

                // check for a hit on the anchor object
                if (Physics.Raycast(ray, out Hit) && Hit.collider.gameObject == this.gameObject)
                {

                    current_property = Properties.translation;
                    record_path_enable = true;
                    position_before_record = transform.position;
                    // empty the list
                    recorded_path.Clear();

                    draggable_now = true;

                    Vector3 vec = Hit.point; //+ new Vector3(0, 0, -40); // Vector3.up * 0.1f;
                                             //Debug.Log(vec);

                    // enforce the same z coordinate as the rest of the points in the parent set object
                    vec.z = -40f;

                    touchDelta = transform.position - vec;

                    GetComponent<TrailRenderer>().Clear();

                }
            }

            //if (Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Moved)
            if (PenTouchInfo.PressedNow)//currentPen.tip.isPressed)
            {
                //Debug.Log("penline touched");

                var ray = Camera.main.ScreenPointToRay(PenTouchInfo.penPosition);
                RaycastHit Hit;

                // check for a hit on the anchor object
                if (Physics.Raycast(ray, out Hit) && Hit.collider.gameObject == this.gameObject)
                {
                    //Debug.Log(transform.name);

                    draggable_now = true;

                    Vector3 vec = Hit.point; //+ new Vector3(0, 0, -40); // Vector3.up * 0.1f;
                                             //Debug.Log(vec);

                    // enforce the same z coordinate as the rest of the points in the parent set object
                    vec.z = -40f;

                    Vector3 diff = vec - transform.position + touchDelta;
                    diff.z = 0;

                    // update the drawn object position.
                    transform.position += diff;

                    // update the previous_position variable: since user moved it by hand, not driven by a set or function
                    previous_position = transform.position;

                    // record path UI
                    if (record_path_enable)
                    {
                        recordPath();

                        // enable trail renderer to show the drawn path
                        GetComponent<TrailRenderer>().enabled = true;
                        GetComponent<TrailRenderer>().material.color = new Color(110, 0, 0);
                    }
                }
            }

            //else if (Input.touchCount > 0 && (Input.GetTouch(0).phase == TouchPhase.Ended || Input.GetTouch(0).phase == TouchPhase.Canceled))
            else if (PenTouchInfo.ReleasedThisFrame)//currentPen.tip.wasReleasedThisFrame)
            {
                // important: touch can start and end when interacting with other UI elements like a set's slider.
                // so, double check that this touch was still on top of the penline object, not, say, on a slider.
                var ray = Camera.main.ScreenPointToRay(PenTouchInfo.penPosition);
                RaycastHit Hit;

                if (Physics.Raycast(ray, out Hit) && Hit.collider.gameObject == this.gameObject)
                {
                    // if record path UI was on, then take care of the recorded path and return pen object to original position.
                    if (record_path_enable)
                    {
                        recordPath();

                        transform.position = position_before_record;

                        draggable_now = false;

                        // Add a line renderer component and re-distribute the points across the line
                        this.gameObject.AddComponent<LineRenderer>();

                        // UNIFORMLY DISTRIBUTE THE POINTS ALONG THE PATH
                        GameObject spline = new GameObject("spline");
                        spline.AddComponent<BezierSpline>();
                        BezierSpline bs = spline.transform.GetComponent<BezierSpline>();
                        bs.Initialize(recorded_path.Count);
                        for (int ss = 0; ss < recorded_path.Count; ss++)
                        {
                            bs[ss].position = recorded_path[ss];
                        }

                        // Now sample 50 points across the spline with a [0, 1] parameter sweep
                        recorded_path = new List<Vector3>(50);
                        for (int i = 0; i < 50; i++)
                        {
                            recorded_path.Add(bs.GetPoint(Mathf.InverseLerp(0, 49, i)));
                        }

                        Destroy(spline);


                        record_path_enable = false;

                        GetComponent<TrailRenderer>().enabled = false;

                        // convert global recorded coordinates to local, wrt pen object
                        for (int k = 0; k < recorded_path.Count; k++)
                        {
                            recorded_path[k] = transform.InverseTransformPoint(recorded_path[k]);
                        }

                        // calculate the actual (global) translation path
                        calculateTranslationPath();

                        // indicate what kind of path this penline has
                        has_continuous_path = true;

                        // set the upper_val manually, doesn't seem to initialize with default value;

                        // (re)create param text and text box
                        createParamGUIText();

                        return;
                    }
                }
            }
        }
    }*/

    /*public void checkDiscretePathDefinitionInteraction()
    {
        if (Paintable_Script.pathdefinition_button.GetComponent<AllButtonsBehavior>().selected &&
            Paintable_Script.pathdefinition_button.GetComponent<AllButtonsBehavior>().isContinuousPathDefinition == false)
        {

            // if a key press is detected, leave the record path mode. 
            // Do it for all other pen objects too, as any pen object could be suspending in that state while this pen object is clicked
            if (Input.GetKeyUp(KeyCode.Space) || Input.GetKeyUp(KeyCode.Escape) || Input.GetKeyUp(KeyCode.Return) || Input.anyKeyDown)
            {
                record_path_enable = false;

                GameObject[] allpens = GameObject.FindGameObjectsWithTag("penline");
                for (int i = 0; i < allpens.Length; i++)
                {
                    allpens[i].GetComponent<penLine_script>().record_path_enable = false;
                }
            }

            //currentPen = Pen.current;

            // let the user create some keypoints
            if (PenTouchInfo.PressedThisFrame)
            {
                //Vector2 penpos = Camera.main.ScreenToWorldPoint(currentPen.position.ReadValue());

                var ray = Camera.main.ScreenPointToRay(PenTouchInfo.penPosition);
                RaycastHit Hit;

                // check for a hit on the anchor object
                if (Physics.Raycast(ray, out Hit) && Hit.collider.gameObject == transform.gameObject)
                {
                    current_property = Properties.translation;
                    record_path_enable = true;
                    position_before_record = transform.position;
                    // empty the list
                    recorded_path.Clear();

                    // indicate what kind of path this penline has
                    has_continuous_path = false;

                    // 
                }

            }
            if (PenTouchInfo.ReleasedThisFrame && record_path_enable)
            {
                Vector3 penpos = Camera.main.ScreenToWorldPoint(PenTouchInfo.penPosition);

                penpos.z = -40.5f;

                recorded_path.Add(penpos);

                // if it's the first point, skip (it's meant for param_text_start at 0), then the next point to param_text_end, and then to new ones.
                if (recorded_path.Count == 2)
                {
                    transform.GetChild(4).GetChild(0).GetComponent<param_text_Script>().upper_val = 5.0f;

                    transform.GetChild(4).GetChild(0).GetComponent<TextMeshPro>().text = 5.0f.ToString();

                    max_parameter_value = 5;

                    // update translation path
                    transform.GetChild(4).GetChild(0).GetComponent<param_text_Script>().translation_path.Add(recorded_path[0]);

                    float length = (recorded_path[1] - recorded_path[0]).magnitude;
                    Vector3 unit = (recorded_path[1] - recorded_path[0]) / length;


                    // sample 50 points along the unit vector direction
                    for (int k = 1; k < 50; k++)
                    {
                        transform.GetChild(4).GetChild(0).GetComponent<param_text_Script>().translation_path.Add(
                            recorded_path[0] + unit * (length / 50) * k);
                    }

                    // update full translation path
                    updateFullTranslationPathFromAllChildren();

                    // add a line renderer in case it doesn't exist
                    gameObject.AddComponent<LineRenderer>();

                    setupLineRendererFromTranslationPath();

                    // show the line renderer
                    showTranslationPath();

                    // Update properties of text display
                    createParamGUIText();

                }
                else if (recorded_path.Count > 2)
                {
                    // create a parameter text box at this location, assign appropriate value according to path index
                    GameObject newparamtext = Instantiate(transform.GetChild(4).GetChild(0).gameObject, transform.GetChild(4));

                    // clear the translation_path in the script, every property gets copied it seems.
                    newparamtext.GetComponent<param_text_Script>().translation_path.Clear();

                    newparamtext.name = transform.GetChild(4).childCount.ToString();
                    newparamtext.transform.position = recorded_path[recorded_path.Count - 1];
                    newparamtext.GetComponent<BoxCollider2D>().size = newparamtext.GetComponent<RectTransform>().sizeDelta;

                    int newparamindex = transform.GetChild(4).childCount - 1;

                    transform.GetChild(4).GetChild(newparamindex).GetComponent<param_text_Script>().upper_val =
                         transform.GetChild(4).GetChild(newparamindex - 1).GetComponent<param_text_Script>().upper_val + 5.0f;

                    max_parameter_value = (int)transform.GetChild(4).GetChild(newparamindex).GetComponent<param_text_Script>().upper_val;

                    transform.GetChild(4).GetChild(newparamindex).GetComponent<TextMeshPro>().text =
                        transform.GetChild(4).GetChild(newparamindex).GetComponent<param_text_Script>().upper_val.ToString();

                    // update translation path
                    transform.GetChild(4).GetChild(newparamindex).GetComponent<param_text_Script>().translation_path.Add(recorded_path[recorded_path.Count - 1 - 1]);

                    float length = (recorded_path[recorded_path.Count - 1] - recorded_path[recorded_path.Count - 2]).magnitude;
                    Vector3 unit = (recorded_path[recorded_path.Count - 1] - recorded_path[recorded_path.Count - 2]) / length;


                    // sample 50 points along the unit vector direction
                    for (int k = 1; k < 50; k++)
                    {
                        transform.GetChild(4).GetChild(newparamindex).GetComponent<param_text_Script>().translation_path.Add(
                            recorded_path[recorded_path.Count - 2] + unit * (length / 50) * k);
                    }

                    // update full translation path
                    updateFullTranslationPathFromAllChildren();

                    setupLineRendererFromTranslationPath();

                    // show the line renderer
                    showTranslationPath();

                    // Update properties of text display
                    createParamGUIText();
                }

                /*
				// is this position close to one of the points of translation_path?
				for (int i = 0; i < translation_path.Count; i++)
				{
					if (Vector2.Distance(penpos, (Vector2)translation_path[i]) < eps_path_radius * 1.5f)
					{
						// Snap? What's the closest whole value index? Snap to there and create a text box
						//float param = (float)(i * 1.0f / translation_path.Count);

						// create a parameter text box at this location, assign appropriate value according to path index
						GameObject newparamtext = Instantiate(transform.GetChild(1).gameObject, transform.GetChild(5));
						newparamtext.name = transform.GetChild(5).childCount.ToString();
						newparamtext.transform.position = translation_path[i];
						newparamtext.GetComponent<BoxCollider2D>().size = newparamtext.GetComponent<RectTransform>().sizeDelta;

						float param = (float)(i * 1.0f / translation_path.Count);
						newparamtext.GetComponent<TextMeshPro>().text = (param * max_parameter_value).ToString("F2");

						//translation_sections_indices.Add((float)(decimal.Round((decimal)param * max_parameter_value), i));

						break;
					}
				}
				*/
    //}

    /*
    // if this is already a warped path, then can't add new keypoints. start fresh. Delete all current keypoints, and add one point inbetween the two ends
    else if (currentPen.tip.wasReleasedThisFrame && translation_path.Count > 1 && translation_sections_indices.Count > 2)
    {
        Vector2 penpos = Camera.main.ScreenToWorldPoint(currentPen.position.ReadValue());

        // is this position close to one of the points of translation_path?
        for (int i = 0; i < translation_path.Count; i++)
        {
            if (Vector2.Distance(penpos, (Vector2)translation_path[i]) < eps_path_radius * 1.5f)
            {
                // Delete all current keypoints, and add one point inbetween the two ends

                break;
            }
        }
    }
    */
    /*}
}*/

    /*public void recalibrateWarpedTranslationPath()
    {
        // UNIFORMLY DISTRIBUTE THE POINTS ALONG THE PATH
        GameObject spline = new GameObject("spline");
        spline.AddComponent<BezierSpline>();
        BezierSpline bs = spline.transform.GetComponent<BezierSpline>();
        bs.Initialize(recorded_path.Count);
        bs[0].position = translation_path[0];
        for (int ss = 1; ss < recorded_path.Count; ss++)
        {
            bs[ss].position = translation_path[0] + recorded_path[ss];
        }

        // Now sample 50 points, but decide how many to sample in each section
        // ...



        // Now sample 50 points across the spline with a [0, 1] parameter sweep
        recorded_path = new List<Vector3>(50);
        for (int i = 0; i < 50; i++)
        {
            recorded_path.Add(bs.GetPoint(Mathf.InverseLerp(0, 49, i)));
        }

        Destroy(spline);


        record_path_enable = false;

        GetComponent<TrailRenderer>().enabled = false;

        // convert global recorded coordinates to local, wrt pen object
        for (int k = 0; k < recorded_path.Count; k++)
        {
            recorded_path[k] = transform.InverseTransformPoint(recorded_path[k]);
        }
    }
    */

    /*public void menuButtonClick(GameObject radmenu, Button btn, int buttonNumber)
    {
        if (buttonNumber == 0)
        {
            btn.onClick.AddListener(() => setPropertyAsTranslation(radmenu));
        }
        else if (buttonNumber == 1)
        {
            btn.onClick.AddListener(() => setPropertyAsRotation(radmenu));
        }
        else if (buttonNumber == 2)
        {
            btn.onClick.AddListener(() => setPropertyAsScale(radmenu));
        }
        else if (buttonNumber == 3)
        {
            // clicking attribute shouldn't do anything, except maybe submenu opens and closes
            btn.onClick.AddListener(() => interactWithAttributeMenu(radmenu));
        }
        else if (buttonNumber == 4)
        {
            btn.onClick.AddListener(() => deleteObject(radmenu));
        }
        else if (buttonNumber == 5)
        {
            btn.onClick.AddListener(() => createRotationMenu(radmenu));
        }
        else if (buttonNumber == 6)
        {
            btn.onClick.AddListener(() => copyObject(radmenu));
        }
        else if (buttonNumber == 7)
        {
            btn.onClick.AddListener(() => setAreaAsAttribute(radmenu));
        }
        else if (buttonNumber == 8)
        {
            btn.onClick.AddListener(() => setLengthAsAttribute(radmenu));
        }
    }*/

    public void deleteObject(GameObject radmenu)
    {
        // Destroy the radial menu
        Destroy(radmenu);

        // TODO: DELETE ANY EDGELINE ASSOCIATED WITH THIS PEN OBJECT

        // Destroy the object attached to this script
        Destroy(this.gameObject);
    }

    public void updateFeatureAction(float val)
    {
        // if the object is being dragged by pen/touch, don't update at that time
        if (draggable_now) return;

        // apply rotation, if there's a positive rotation value
        if (apply_rotation)
        {
            float rot_param = val / max_rotation_val;
            Quaternion rot_target = Quaternion.Euler(0, 0, -max_possible_rotation * rot_param);
            // Dampen towards the target rotation
            transform.rotation = Quaternion.Slerp(transform.rotation, rot_target, Time.deltaTime * 2.0f);
        }

        // apply scale, if any positive scale value
        if (apply_scale)
        {
            float scale_param = val / max_scale_val;
            Vector3 scale_target = new Vector3(scale_param * max_possible_scale, scale_param * max_possible_scale, 1);
            // Dampen towards the target scale
            transform.localScale = Vector3.Lerp(transform.localScale, scale_target, Time.deltaTime * 2.0f);
        }

        if (Paintable.allowOpacity)
        {
            float opacity_param = val / 110;
            Color col = transform.GetComponent<MeshRenderer>().materials[0].color;
            transform.GetComponent<MeshRenderer>().materials[0].color = new Color(col.r, col.g, col.b, opacity_param);
        }

        // =============
        // Take care of translation parameter

        /*
        if (translation_path.Count > 0)
        {
            // determine which section val belongs to
            int section = -1;
            float remaining_section_length = 0;
            for (int i = 0; i < transform.GetChild(4).childCount; i++)
            {
                if (val <= transform.GetChild(4).GetChild(i).GetComponent<param_text_Script>().upper_val)
                {
                    section = i;

                    remaining_section_length = transform.GetChild(4).GetChild(i).GetComponent<param_text_Script>().upper_val - val;

                    break;
                }
                else
                {
                    if ((i == transform.GetChild(4).childCount - 1) &&
                        val >= transform.GetChild(4).GetChild(i).GetComponent<param_text_Script>().upper_val)
                    // are we at the last section and value is over the section? do things manually for this edge case. (for continuous, this'll always be the case)
                    {
                        section = i;
                        remaining_section_length = 0f;
                        break;
                    }
                }
            }

            // if not found in range, return. should not happen.
            if (section == -1) { Debug.Log("section = - 1"); return; }

            path_index = section * 49 + (int)(49 * (1 - (remaining_section_length * 1f / transform.GetChild(4).GetChild(section).GetComponent<param_text_Script>().section_length)));

            // param is in [0, 1]. Not needed for translation.
            float param = val / max_parameter_value;

            //Debug.Log(param);
            //path_index = (int)((float)translation_path.Count * param);

            // clamp path_index to prevent access errors.
            if (path_index > translation_path.Count - 1) path_index = translation_path.Count - 1;
            if (path_index < 0) path_index = 0;

            Vector3 target;
            if (path_index == 0 && translation_path.Count == 0) target = transform.position;    // prevent accessing translation_path as it has no elements in this case (no path defined).
            else target = translation_path[path_index];

            // experimental: add perlin noise. Needs more exploration.
            float height = heightScale * Mathf.PerlinNoise(Time.time * (transform.position.x - translation_path[0].x), 0.0f) - heightScale / 2; //heightScale * Mathf.PerlinNoise(Time.time * xScale, 0.0f);
            target.y += height;
            //target = target + target * Mathf.PerlinNoise(target.x, target.y) * 0.5f;

            //Debug.Log(this.name + " " + target.ToString());

            // Dampen towards the target translation
            transform.position = Vector3.Lerp(transform.position, target, Time.deltaTime * 2.0f);
            //transform.position = target;

            //(new Vector3(0, 0, 1), param);

            // update the max_parameter_value of this script
            //max_parameter_value = (int)capacity;

            // update param text label and fix the begin and end position, otherwise they move with transform
            transform.GetChild(2).GetComponent<TextMeshPro>().text = val.ToString(); // (param * max_parameter_value).ToString();
                                                                                     // float text position
            transform.GetChild(2).transform.position = transform.GetComponent<BoxCollider>().bounds.max;
            //transform.GetChild(1).GetComponent<TextMeshPro>().text = min_parameter_value.ToString();
            //transform.GetChild(4).GetChild(transform.GetChild(4).childCount - 1).GetComponent<TextMeshPro>().text = max_parameter_value.ToString();

            transform.GetChild(1).transform.position = translation_path[0];

            // update all keypoint labels, fix their position, otherwise they move with transform.
            for (int s = 0; s < transform.GetChild(4).childCount; s++)
            {
                transform.GetChild(4).GetChild(s).transform.position =
                    transform.GetChild(4).GetChild(s).GetComponent<param_text_Script>().translation_path[49];   // place text at the end of the path, as these are upper vals.
            }
                        
        }
        */
    }

    public void recordPath()
    {
        if (recorded_path.Count > 1)
        {
            // if the dragged distance is not too low, or to prevent adding points when object is stationary
            if (Vector3.Distance(recorded_path[recorded_path.Count - 1], transform.position) > 0.1)
            {
                recorded_path.Add(transform.position);
            }
        }
        else
        {
            recorded_path.Add(transform.position);
        }
    }

    public void calculateTranslationPath()
    {
        // if it's a continuous path
        /*
        if (transform.GetChild(4).childCount == 1 && has_continuous_path)
        {
            // clear the current translation path
            transform.GetChild(4).GetChild(0).GetComponent<param_text_Script>().translation_path.Clear();
            // create a new list of positions, based on current transform.position and the local coord. recorded path
            transform.GetChild(4).GetChild(0).GetComponent<param_text_Script>().translation_path.Add(transform.position);
            for (int i = 0; i < recorded_path.Count; i++)
            {
                transform.GetChild(4).GetChild(0).GetComponent<param_text_Script>().translation_path.Add(transform.position + recorded_path[i]);
            }

            // UPDATE THE FULL TRANSLATION PATH
            translation_path.Clear();
            translation_path.AddRange(transform.GetChild(4).GetChild(0).GetComponent<param_text_Script>().translation_path);
        }
        */

        setupLineRendererFromTranslationPath();

        // show the translation path
        //showTranslationPath();
    }

    public void calculateTranslationPathIfAlreadyDefined()
    {
        /*
        if (translation_path.Count > 0 && path_index > -1)
        {
            // translate the translation path, according to the current path_index (where the object is along the path)
            //Vector3 delta_path = transform.position - transform.GetChild(4).GetChild(0).GetComponent<param_text_Script>().translation_path[path_index];
            Vector3 delta_path = transform.position - translation_path[path_index]; // use global translation path. assume it's calculated already.

            // update all section's translation paths in case it is a discrete keypoint path, or the loop stops after first iteration if it's a continuous path.
            for (int i = 0; i < transform.GetChild(4).childCount; i++)
            {
                for (int j = 0; j < transform.GetChild(4).GetChild(i).GetComponent<param_text_Script>().translation_path.Count; j++)
                {
                    transform.GetChild(4).GetChild(i).GetComponent<param_text_Script>().translation_path[j] += delta_path;
                }
            }
        }*/

        // update the full translation path after updating the children paths
        // updateFullTranslationPathFromAllChildren();

        // set up line renderer
        setupLineRendererFromTranslationPath();

        // show the translation path
        // showTranslationPath();
    }

    public void setupLineRendererFromTranslationPath()
    {
        // set up line renderer
        LineRenderer lr = transform.GetComponent<LineRenderer>();
        if (lr != null)
        {
            lr.positionCount = translation_path.Count;
            lr.SetPositions(translation_path.ToArray());
            lr.material.color = new Color(255, 0, 0);
            lr.startWidth = 0.5f;
            lr.endWidth = 0.4f;
        }
    }

    /*public void showTranslationPath()
    {
        LineRenderer lr = transform.GetComponent<LineRenderer>();
        if (lr != null)
        {
            lr.enabled = true;

            // update limit texts
            for (int i = 0; i < transform.GetChild(4).childCount; i++)
            {
                transform.GetChild(4).GetChild(i).gameObject.SetActive(true);
                transform.GetChild(4).GetChild(i).GetComponent<TextMeshPro>().text =
                    transform.GetChild(4).GetChild(i).GetComponent<param_text_Script>().upper_val.ToString();
            }

            // update the beginning and float text
            transform.GetChild(1).gameObject.SetActive(true);
            transform.GetChild(1).GetComponent<TextMeshPro>().text = min_parameter_value.ToString();
            transform.GetChild(2).gameObject.SetActive(true); // this will get updated in updateFeatureAction() if there's an edge
        }
    }
    */

    /*public void hideTranslationPath()
    {
        LineRenderer lr = transform.GetComponent<LineRenderer>();
        if (lr != null)
        {
            lr.enabled = false;

            // update limit texts
            for (int i = 0; i < transform.GetChild(4).childCount; i++)
            {
                transform.GetChild(4).GetChild(i).gameObject.SetActive(false);
                transform.GetChild(4).GetChild(i).GetComponent<TextMeshPro>().text =
                    transform.GetChild(4).GetChild(i).GetComponent<param_text_Script>().upper_val.ToString();
            }

            // limit texts: min and max
            transform.GetChild(1).gameObject.SetActive(false);
            transform.GetChild(2).gameObject.SetActive(false);
        }
    }
    */

    public void checkMove()
    {
        // NOTE: WHAT THRESHOLD DISTANCE WOULD BE SUITABLE IF THE USER WANTS FINE-GRAINED PARENT CHANGING BEHAVIOR?
        // NOTE: CHECK IF PAINTABLE OBJECT IS ASSIGNED YET, OTHERWISE UNITY THROWS AN ERROR (WHEN TEMPLINE SETUP IS NOT COMPLETE)

        /*
		 * The object moves abruptly when it's already moving in a path controlled by an edgeline.
		 * There are three possible cases of external movement here: 1. object is being dragged, 2. moved by edgeLine, 3. moved by parent movement.
		 * 1 is taken care of, by detecting when draggable_now is true.
		 * 2 can be approximately detected by: if path_index > 0.
		 * 3. To detect 3 and stop edgeLine from reacting when 3 is happening
		 */
        if (Vector3.Distance(transform.position, previous_position) > 10f && paintable_object != null)
        {
            //Debug.Log(">5");
            previous_position = transform.position;

            //draggable_now = true;

            //checkAndUpdateMembership();

            if (draggable_now)
            {
                calculateTranslationPathIfAlreadyDefined();
            }
        }
        /*else
        {
            //draggable_now = false;
        }*/
    }

    /*public void checkAndUpdateMembership()
    {
        // check for type of parent membership -- the pen line is moving.

        // if this penline is already part of a set or function, and it's in abs. layer = 3, then don't check for further membership,
        // the pen line should move with the set or function. period. 
        if (transform.parent.tag == "set")
        {
            if ((transform.parent.GetComponent<setLine_script>().abstraction_layer == 3 ||
                transform.parent.GetComponent<setLine_script>().abstraction_layer == 0))
            {
                return;
            }
        }
        if (transform.parent.tag == "function")
        {
            if ((transform.parent.GetComponent<functionLine_script>().abstraction_layer == 3 ||
                transform.parent.GetComponent<functionLine_script>().abstraction_layer == 0))
            {
                return;
            }
        }

        
        // check if the drawn object came out of any parent set's area, and went into another's
        GameObject[] sets = GameObject.FindGameObjectsWithTag("set");
        bool in_canvas = false;
        for (int i = 0; i < sets.Length; i++)
        {
            // if the set is in an abstraction layer that still shows the set polygon area, then check for point in polygon
            if ((sets[i].GetComponent<setLine_script>().abstraction_layer == 1 ||
                sets[i].GetComponent<setLine_script>().abstraction_layer == 2)
                &&
                sets[i].GetComponent<setLine_script>().isInsidePolygon(
                sets[i].GetComponent<setLine_script>().transform.InverseTransformPoint(
                transform.position))
                )
            {
                // if already under a set or function, don't change membership
                if ((transform.parent.tag == "set" || transform.parent.tag == "function") && transform.parent.gameObject != sets[i]) return;

                // else, change parent to new set
                transform.parent = sets[i].transform;
                //sets[i].GetComponent<setLine_script>().updateLegibleLayer();
                in_canvas = true;

                // save log
                // save coordinates wrt parent center
                if (abstraction_layer == 1) // don't need to log when it's hidden under a parent, otherwise, many extra log lines are added for each frame
                {
                    string tkey = sets[i].name + "|" + DateTime.Now.ToString("MM/dd/yyyy HH:mm:ss.fff");
                    if (!saved_positions.ContainsKey(tkey))
                        saved_positions.Add(tkey, (Vector2)(transform.position - sets[i].transform.position));
                }

                break;
            }
        }

        // check if it went inside a function
        GameObject[] functions = GameObject.FindGameObjectsWithTag("function");
        for (int i = 0; i < functions.Length; i++)
        {
            // if the set is in an abstraction layer that still shows the set polygon area, then check for point in polygon
            if ((functions[i].GetComponent<functionLine_script>().abstraction_layer == 1 ||
                functions[i].GetComponent<functionLine_script>().abstraction_layer == 2)
                &&
                !in_canvas
                &&
                functions[i].GetComponent<functionLine_script>().CheckCategoryFilterCriteria(this.gameObject)
                &&
                functions[i].GetComponent<functionLine_script>().isInsidePolygon(
                functions[i].GetComponent<functionLine_script>().transform.InverseTransformPoint(
                transform.position))
                )
            {
                // if already under a set or function, don't change membership
                if ((transform.parent.tag == "set" || transform.parent.tag == "function") && transform.parent.gameObject != functions[i]) return;

                transform.parent = functions[i].transform;
                in_canvas = true;

                // save log
                if (abstraction_layer == 1) // don't need to log when it's hidden under a parent, otherwise, many extra log lines are added for each frame
                {
                    string tkey = functions[i].name + "|" + DateTime.Now.ToString("MM/dd/yyyy HH:mm:ss.fff");
                    if (!saved_positions.ContainsKey(tkey))
                        saved_positions.Add(tkey, (Vector2)(transform.position - functions[i].transform.position));
                }

                break;
            }
        }

        // if none of the sets or functions contain it, then it should be a child of the paintable canvas
        if (!in_canvas)
        {
            transform.parent = paintable_object.transform;

            // change the abstraction layer in case they were part of a hidden layer before
            parent_asked_to_change_layer = false;

            // save log
            // FIXED: seconds may not be the lowest resolution. There could be dragging happening twice within a second and
            // an exception is then thrown.
            string tkey = "paintable|" + DateTime.Now.ToString("MM/dd/yyyy HH:mm:ss.fff");
            if (!saved_positions.ContainsKey(tkey))
                saved_positions.Add(tkey, (Vector2)transform.position);
        }
    }*/

    /*public Mesh SpriteToMesh()
    {
        Mesh mesh = new Mesh();
        mesh.vertices = Array.ConvertAll(recognized_sprite.vertices, i => (Vector3)i);
        mesh.uv = recognized_sprite.uv;
        mesh.triangles = Array.ConvertAll(recognized_sprite.triangles, i => (int)i);

        return mesh;
    }*/

    public Mesh createQuad(float width, float height)
    {
        var mesh = new Mesh();

        var vertices = new Vector3[4]
        {
            new Vector3(0, 0, 0),
            new Vector3(width, 0, 0),
            new Vector3(0, height, 0),
            new Vector3(width, height, 0)
        };
        mesh.vertices = vertices;

        var tris = new int[6]
        {
            // lower left triangle
            0, 2, 1,
            // upper right triangle
            2, 3, 1
        };
        mesh.triangles = tris;

        var normals = new Vector3[4]
        {
            -Vector3.forward,
            -Vector3.forward,
            -Vector3.forward,
            -Vector3.forward
        };
        mesh.normals = normals;

        var uv = new Vector2[4]
        {
            new Vector2(0, 0),
            new Vector2(1, 0),
            new Vector2(0, 1),
            new Vector2(1, 1)
        };
        mesh.uv = uv;

        return mesh;
    }

    public void calculateLengthAttributeFromPoints()
    {
        attribute.Length = 0f;
        for (int i = 1; i < points.Count; i++)
        {
            attribute.Length += Vector3.Distance(points[i - 1], points[i]);
        }
        // scale to global unit scale
        attribute.Length *= Paintable.unitScale;
    }

    // keep checking every frame if it came out from a double function lasso or if it's still a child of it.
    /*public void checkIfThisIsPartOfDoubleFunction()
    {
        if (is_this_double_function_operand)
        {
            if (transform.parent.tag == "function" && transform.parent.GetComponent<functionLine_script>().is_this_a_double_function)
            {
                // keep it true
                is_this_double_function_operand = true;
            }
            else
            {
                // turn off the operand argument_label
                is_this_double_function_operand = false;
                transform.GetChild(3).gameObject.SetActive(false);
            }
        }
    }*/

    /*public void applyGlobalStrokeDetails()
    {
        // act only if visible, and if pen line is finished drawing and there's a translation path available
        if (abstraction_layer == 1 && translation_path.Count > 1)
        {
            //if (details_dropdown.GetComponent<DropdownMultiSelect>().transform.Find("List") != null)
            //Debug.Log("not null.");

            bool state = details_dropdown.GetComponent<DropdownMultiSelect>().transform.GetChild(1).GetChild(0).GetChild(1).GetChild(0).GetChild(2).GetComponent<Toggle>().isOn;

            if (state != global_details_on_path)
            {
                // change needed
                if (!state)
                {
                    hideTranslationPath();
                    global_details_on_path = false;
                }
                else if (state)
                {
                    showTranslationPath();
                    global_details_on_path = true;
                }
            }
        }
    }*/


    private void Awake()
    {
        // setting these at the declaration section does not work for some reason, so declaring here
        min_rotation_val = 0f;
        max_rotation_val = 5f;
        min_scale_val = 1f;
        max_scale_val = 5f;

        //details_dropdown = GameObject.Find("Details_Dropdown");
        paintable_object = GameObject.Find("Paintable");
    }


    // Start is called before the first frame update
    void Start()
    {
        radius_margin = 40;
    }

    // Update is called once per frame
    void Update()
    {
        //checkHitAndMove();
        //checkContinuousPathDefinitionInteraction();
        //checkDiscretePathDefinitionInteraction();
        //checkMove();
        //checkIfThisIsPartOfDoubleFunction();
        //onAbstractionLayerChange();

        // should be called after abstraction layer changes.
        //applyGlobalStrokeDetails();
    }

    public bool isInsidePolygon(Vector3 p)
    {
        //Debug.Log("vector_"+p.ToString()+"_own_"+this.transform.position.ToString());
        int j = points.Count - 1;
        bool inside = false;
        for (int i = 0; i < points.Count; j = i++)
        {
            if (((points[i].y <= p.y && p.y < points[j].y) || (points[j].y <= p.y && p.y < points[i].y)) &&
               (p.x < (points[j].x - points[i].x) * (p.y - points[i].y) / (points[j].y - points[i].y) + points[i].x))
            {
                inside = !inside;

            }

        }
        return inside;
    }

    void OnDestroy()
    {
        Transform node_parent = transform.parent;
        if (node_parent.tag == "node_parent")
        {
            node_parent.parent.GetComponent<GraphElementScript>().Graph_init();
            //node_parent.parent.GetComponent<GraphElementScript>().Graph_as_Str();
        }
    }

    public void searchNodeAndUpdateEdge()
    {
        if (transform.parent.tag != "node_parent") return;

        Transform Prev_node_parent = transform.parent;
        Transform Prev_graph_parent = Prev_node_parent.transform.parent;
        Transform Prev_edge_parent = Prev_graph_parent.GetChild(1);
        Transform Prev_simplicial_parent = Prev_graph_parent.GetChild(2);
        Transform Prev_hyper_parent = Prev_graph_parent.GetChild(3);
        Transform[] allChildrenedge = Prev_edge_parent.GetComponentsInChildren<Transform>();
        Transform[] simplicials = Prev_simplicial_parent.GetComponentsInChildren<Transform>();
        Transform[] hyper_edges = Prev_hyper_parent.GetComponentsInChildren<Transform>();

        foreach (Transform child in allChildrenedge)
        {
            if (child.tag == "edge")
                child.GetComponent<EdgeElementScript>().updateEndPoint(transform.gameObject);
        }



        foreach (Transform each_simplicial in simplicials)
        {
            if (each_simplicial.tag != "simplicial")
                continue;

            if (each_simplicial.GetComponent<SimplicialElementScript>() != null)
            {
                List<GameObject> thenodes = each_simplicial.GetComponent<SimplicialElementScript>().thenodes;
                int x = 0;
                foreach (GameObject each_node in thenodes)
                {
                    if (each_node == transform.gameObject)
                    {
                        each_simplicial.GetComponent<SimplicialElementScript>().theVertices[x] = edge_position;
                        each_simplicial.GetComponent<SimplicialElementScript>().updatePolygon();
                        break;
                    }
                    x++;
                }
            }
            else
            {
                each_simplicial.GetComponent<EdgeElementScript>().updateEndPoint(transform.gameObject);
            }
        }


        foreach (Transform each_child_edge in hyper_edges)
        {
            if (each_child_edge.tag != "hyper_child_edge")
                continue;

            if (each_child_edge.GetComponent<HyperEdgeElement>().parent_node == transform.gameObject)
            {
                each_child_edge.GetComponent<HyperEdgeElement>().UpdateSingleEndpoint(edge_position);
            }
        }
    }

    // called only when dragging is done, to save computation cost
    public void searchFunctionAndUpdateLasso()
    {

        GameObject[] all_functions = GameObject.FindGameObjectsWithTag("function");

        foreach (GameObject cur_function in all_functions)
        {
            if (!cur_function.transform.GetChild(0).GetComponent<FunctionMenuScript>().instant_eval)
            {
                foreach (GameObject function_argument in cur_function.transform.GetChild(0).GetComponent<FunctionMenuScript>().argument_objects)
                {
                    // if the argument is a graph which contains this object, then call update lasso
                    if (function_argument.tag == "graph" &&
                        transform.parent.tag == "node_parent" &&
                        transform.parent.parent.gameObject == function_argument)
                    {
                        cur_function.GetComponent<FunctionElementScript>().updateLassoPoints();
                        break;
                    }
                    // else if the argument is an icon and matches with this object, call update lasso
                    else if (function_argument.tag == "iconic" && function_argument == transform.gameObject)
                    {
                        cur_function.GetComponent<FunctionElementScript>().updateLassoPoints();
                        break;
                    }
                }
            }
        }

    }
}