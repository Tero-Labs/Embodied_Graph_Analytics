using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;

public class FunctionMenuScript : MonoBehaviour
{
    public Toggle evaluation_type;
    public bool instant_eval;
    public Toggle keepchilden;
    public Toggle show_result;
    public bool keep_child_object;
    public bool eval_finished;

    public InputField mainInputField;
    public Button perform_action;
    public Button settings;
    public GameObject text_label;
    
    public GameObject input_option;
    public bool match_found;
    private bool textbox_open;
    private bool draggable_now;

    private Vector3 touchDelta = new Vector3();

    public GameObject paintable;
    public bool red_flag;
    public TMP_Text tmptextlabel;
    public Image img;

    public GameObject topo_label;

    public GameObject message_box;
    public GameObject argument_text_box;
    public GameObject dragged_arg_object;

    // functions_list
    private Dictionary<int,string> addition_dict;
    //private Dictionary<int, int> addition_order_dict;

    private Dictionary<int, string> topologicalsort_dict;
    private Dictionary<int, string> shortestpath_dict;
    private Dictionary<int, string> community_dict;

    private Dictionary<int, string> dummy_dict;
    //private Dictionary<int, int> dummy_order_dict;

    private GameObject argument_text;
    public GameObject drag_text_ui;

    public GameObject[] argument_objects;

    // current function dictionary
    public Dictionary<int, string> cur_dict;
    // current function order dictionary
    //public Dictionary<int, int> cur_order_dict;
    // needed to track the argument string for display
    public Dictionary<int, string> cur_arg_Str;
    public int cur_iter;

    public string output_type;

    // Start is called before the first frame update
    void Start()
    {
        //https://docs.unity3d.com/2018.4/Documentation/ScriptReference/UI.InputField-onEndEdit.html
        //Adds a listener that invokes the "LockInput" method when the player finishes editing the main input field.
        //Passes the main input field into the method when "LockInput" is invoked
        mainInputField.onValueChanged.AddListener(delegate { LockInput(mainInputField); });
        perform_action.onClick.AddListener(delegate { OnClickButton(perform_action); });
        settings.onClick.AddListener(delegate { OnSettingsButton(settings); });
        show_result.onValueChanged.AddListener(delegate { ResultVisiblity(show_result); });

        argument_text = null;
        dragged_arg_object = null;

        keepchilden.onValueChanged.AddListener(delegate { ChildToggle(keepchilden); });
        evaluation_type.onValueChanged.AddListener(delegate { InstantEvalToggle(evaluation_type); });
        instant_eval = false;
        keep_child_object = true;

        textbox_open = false;
        eval_finished = false;
        draggable_now = false;

        # region function arguments and types dictionary
        addition_dict = new Dictionary<int, string>()
        {
            {0, "graph"},
            {1, "graph"},
        };

        /*addition_order_dict = new Dictionary<int, int>()
        {
            {0, 0},
            {1, 1},
        };*/

        topologicalsort_dict = new Dictionary<int, string>()
        {
            {0, "graph"},
            {1, "string"},
        };

        shortestpath_dict = new Dictionary<int, string>()
        {
            {0, "graph"},
            {1, "iconic"},
            {2, "iconic"},
        };

        community_dict = new Dictionary<int, string>()
        {
            {0, "graph"},
        };

        dummy_dict = new Dictionary<int, string>()
        {
            {0, "string"},
            {1, "iconic"},
        };


        # endregion
    }

    private void Update()
    {
        
        if (paintable.GetComponent<Paintable>().function_brush_button.GetComponent<AllButtonsBehaviors>().selected)
        {
            if (PenTouchInfo.PressedThisFrame)
            {
                if (!eval_finished) message_box.GetComponent<TextMeshProUGUI>().text = "";
                if (TMP_TextUtilities.IsIntersectingRectTransform(tmptextlabel.rectTransform, PenTouchInfo.penPosition, paintable.GetComponent<Paintable>().main_camera))
                {
                    paintable.GetComponent<Paintable>().dragged_arg_textbox = transform.gameObject;

                    /*if (!textbox_open && !eval_finished)
                    {
                        if (input_option.activeSelf) input_option.SetActive(false);
                        else input_option.SetActive(true);
                    }*/

                    if (match_found && !eval_finished)
                    {
                        var index = TMP_TextUtilities.FindIntersectingCharacter(tmptextlabel, PenTouchInfo.penPosition, paintable.GetComponent<Paintable>().main_camera, false);
                        //Debug.Log("Found character at " + index.ToString());

                        if (!textbox_open && index != -1)
                        {
                            //extra check for better UX
                            /*if (cur_dict.ContainsKey(index))
                                index = index;
                            else if(cur_dict.ContainsKey(index - 1))
                                index = index - 1;
                            else if (cur_dict.ContainsKey(index + 1))
                                index = index + 1;
                            else
                                index = -1;*/
                            if (tmptextlabel.textInfo.characterInfo[index].elementType.ToString() != "Sprite")
                            {
                                index = -1;
                            }

                        }

                        if (index != -1 && !textbox_open)
                        {

                            if (/*cur_dict[index] == "string" || cur_dict[index] == "int"*/ tmptextlabel.textInfo.characterInfo[index].spriteIndex == 2)
                            {
                                Vector3 TouchStart = Vector3.zero;

                                RectTransformUtility.ScreenPointToWorldPointInRectangle(tmptextlabel.rectTransform, PenTouchInfo.penPosition, 
                                    paintable.GetComponent<Paintable>().main_camera, out TouchStart);
                                                                
                                TouchStart = paintable.GetComponent<Paintable>().main_camera.transform.InverseTransformPoint(TouchStart);

                                //Debug.Log(TouchStart.ToString());
                                //todo:not_being__correctly_mapped
                                argument_text = Instantiate(argument_text_box,
                                    //TouchStart,
                                    paintable.GetComponent<Paintable>().canvas_radial.transform.TransformPoint(TouchStart),
                                    Quaternion.identity,
                                    paintable.GetComponent<Paintable>().canvas_radial.transform/*transform*/);

                                argument_text.GetComponent<FunctionTextInputMenu>().setArgument(transform.gameObject, /*index*/cur_iter);
                                textbox_open = true;
                            }
                        }
                    }
                }
                //else check if any other object is clicked on
                else
                {
                    
                    //remove active textbox, if any                    
                    if (textbox_open && argument_text.GetComponent<FunctionTextInputMenu>().str_arg.Length > 0)
                    {
                        int index = argument_text.GetComponent<FunctionTextInputMenu>().str_index;
                        string str_arg = argument_text.GetComponent<FunctionTextInputMenu>().str_arg;

                        if (str_arg.Length > 0)
                        {
                            cur_arg_Str[index] = str_arg;
                            string arg_Str = get_arguments_string();
                            text_label.GetComponent<TextMeshProUGUI>().text = mainInputField.text.Substring(0, 1).ToUpper() +
                                                    mainInputField.text.Substring(1).ToLower() + arg_Str;
                            cur_iter++;
                            argument_objects[index] = text_label;
                        }

                        Destroy(argument_text);
                        argument_text = null;
                        textbox_open = false;
                    }

                    var ray = Camera.main.ScreenPointToRay(PenTouchInfo.penPosition);
                    RaycastHit Hit;

                    if (Physics.Raycast(ray, out Hit) && Hit.collider.gameObject.tag == "iconic")
                    {
                        dragged_arg_object = Hit.collider.gameObject;

                        // label instantiate
                        drag_text_ui = Instantiate(topo_label,
                            paintable.GetComponent<Paintable>().canvas_radial.transform.TransformPoint(Hit.point),
                            Quaternion.identity,
                            paintable.GetComponent<Paintable>().canvas_radial.transform);

                        drag_text_ui.GetComponent<TMP_Text>().text = "icon: " +
                            dragged_arg_object.GetComponent<iconicElementScript>().icon_number.ToString();

                        //show graph name too
                        if (dragged_arg_object.transform.parent.tag == "node_parent")
                        {
                            drag_text_ui.GetComponent<TMP_Text>().text += "\n" + "graph: " +
                            dragged_arg_object.transform.parent.parent.GetComponent<GraphElementScript>().graph_name;
                        }                            
                    }

                    RaycastHit2D hit2d = Physics2D.GetRayIntersection(ray);
                    if (hit2d.collider != null && hit2d.collider.gameObject.tag == "edge")
                    {
                        dragged_arg_object = hit2d.collider.gameObject.transform.parent.parent.gameObject;
                        Debug.Log("graph picked from edge cllick");
                    }
                }
            }

            else if (PenTouchInfo.ReleasedThisFrame
                && (dragged_arg_object != null ||
                (paintable.GetComponent<Paintable>().dragged_arg_textbox != null && paintable.GetComponent<Paintable>().dragged_arg_textbox != transform.gameObject)))
            {
                if (drag_text_ui != null)
                {
                    Destroy(drag_text_ui);
                    drag_text_ui = null;
                }

                if (TMP_TextUtilities.IsIntersectingRectTransform(tmptextlabel.rectTransform, PenTouchInfo.penPosition, paintable.GetComponent<Paintable>().main_camera))
                {                    
                    var index = TMP_TextUtilities.FindIntersectingCharacter(tmptextlabel, PenTouchInfo.penPosition, paintable.GetComponent<Paintable>().main_camera, false);

                    if (dragged_arg_object == null && paintable.GetComponent<Paintable>().dragged_arg_textbox != null)
                    {
                        dragged_arg_object = paintable.GetComponent<Paintable>().dragged_arg_textbox;
                    }

                    //Debug.Log("Found draaged object" + dragged_arg_object.name + " character at " + index.ToString());

                    if (index != -1)
                    {
                        //extra check for better UX
                        /*if (cur_dict.ContainsKey(index))
                            index = index;
                        else if(cur_dict.ContainsKey(index - 1))
                            index = index - 1;
                        else if (cur_dict.ContainsKey(index + 1))
                            index = index + 1;
                        else
                            index = -1;*/
                        if (tmptextlabel.textInfo.characterInfo[index].elementType.ToString() != "Sprite")
                        {
                            index = -1;
                        }

                    }

                    if (index != -1)
                    {
                        index = cur_iter;
                        Transform temp = null;
                        // whether graph or icon is expected as arg.type
                        if (cur_dict[index] == "graph")
                        {
                            if (dragged_arg_object.transform.parent.tag == "node_parent")
                            {
                                temp = dragged_arg_object.transform.parent.parent;
                                cur_arg_Str[index] = temp.GetComponent<GraphElementScript>().graph_name;
                                // argument obj referece update for calculation
                                //argument_objects[cur_order_dict[index]] = temp.gameObject;
                                argument_objects[index] = temp.gameObject;
                            }
                            else if (paintable.GetComponent<Paintable>().dragged_arg_textbox != null && paintable.GetComponent<Paintable>().dragged_arg_textbox != transform.gameObject)
                            {
                                temp = paintable.GetComponent<Paintable>().dragged_arg_textbox.transform;
                                cur_arg_Str[index] = temp.GetComponent<FunctionMenuScript>().message_box/*text_label*/.GetComponent<TextMeshProUGUI>().text;
                                argument_objects[index] = temp.parent.GetChild(1).gameObject;
                            }
                        }
                        else if (cur_dict[index] == "iconic" && dragged_arg_object.tag == "iconic")
                        {
                            temp = dragged_arg_object.transform;
                            cur_arg_Str[index] = temp.GetComponent<iconicElementScript>().icon_number.ToString();
                            // argument obj referece update for calculation
                            //argument_objects[cur_order_dict[index]] = temp.gameObject;
                            argument_objects[index] = temp.gameObject;
                        }
                        else if (cur_dict[index] == "string" || cur_dict[index] == "int")
                        {
                            // double check if the draaged object is not null
                            if (paintable.GetComponent<Paintable>().dragged_arg_textbox != null && paintable.GetComponent<Paintable>().dragged_arg_textbox != transform.gameObject)
                            {
                                temp = paintable.GetComponent<Paintable>().dragged_arg_textbox.transform;
                                cur_arg_Str[index] = temp.GetComponent<FunctionMenuScript>().message_box/*text_label*/.GetComponent<TextMeshProUGUI>().text;
                                // argument obj referece update for calculation
                                //TodO:add_for_int_arguments
                                //argument_objects[cur_order_dict[index]] = temp.gameObject;
                                argument_objects[index] = temp.gameObject;
                            }
                        }


                        if (temp != null)
                        {
                            // argument string update for display
                            string arg_Str = get_arguments_string();
                            text_label.GetComponent<TextMeshProUGUI>().text = mainInputField.text.Substring(0, 1).ToUpper() +
                                                    mainInputField.text.Substring(1).ToLower() + arg_Str;
                            cur_iter++;
                        }
                        else
                        {
                            message_box.GetComponent<TextMeshProUGUI>().text = "Invalid argument!";
                        }

                        
                        // clearing the clicked gameobject insided if block for ensuring the previous information is not lost
                        // in other FunctionMenuScripts that are not clicked on
                        paintable.GetComponent<Paintable>().dragged_arg_textbox = null;
                    }
                    //paintable.GetComponent<Paintable>().dragged_arg_textbox = null;
                }

                dragged_arg_object = null;
            }

            else if (PenTouchInfo.PressedNow
                && drag_text_ui != null)
            {
                var ray = Camera.main.ScreenPointToRay(PenTouchInfo.penPosition);
                RaycastHit Hit;
                if (Physics.Raycast(ray, out Hit))
                {
                    drag_text_ui.transform.position = paintable.GetComponent<Paintable>().canvas_radial.transform.TransformPoint(Hit.point);
                }                
            }

            else if (PenTouchInfo.ReleasedThisFrame
                && drag_text_ui != null)
            {
                Destroy(drag_text_ui);
                drag_text_ui = null;
            }
        }

        else if (PenTouchInfo.PressedNow && paintable.GetComponent<Paintable>().eraser_button.GetComponent<AllButtonsBehaviors>().selected)
        {
            if (TMP_TextUtilities.IsIntersectingRectTransform(tmptextlabel.rectTransform, PenTouchInfo.penPosition, paintable.GetComponent<Paintable>().main_camera))
            {
                Destroy(transform.parent.gameObject);
            }
        }
        else if (!(paintable.GetComponent<Paintable>().panZoomLocked))
        {
            checkHitAndMove();
        }
        else
            return;
    }

    public void checkHitAndMove()
    {
       
        if (PenTouchInfo.PressedThisFrame)
        {
            if (TMP_TextUtilities.IsIntersectingRectTransform(tmptextlabel.rectTransform, PenTouchInfo.penPosition, paintable.GetComponent<Paintable>().main_camera))
            {
                paintable.GetComponent<Paintable>().current_dragged_function = transform.gameObject;
                Debug.Log("started drag");

                draggable_now = true;
                Vector3 vec = Vector3.zero;
                RectTransformUtility.ScreenPointToWorldPointInRectangle(tmptextlabel.rectTransform, PenTouchInfo.penPosition, paintable.GetComponent<Paintable>().main_camera, out vec);

                // enforce the same z coordinate as the rest of the points in the parent set object
                vec.z = -5f;

                touchDelta = text_label.transform.position - vec;
                // change anchor color
                img.color = Color.gray;
            }

            else
            {
                return;
            }
        }

        else if (PenTouchInfo.PressedNow)
        {
            
            if (TMP_TextUtilities.IsIntersectingRectTransform(tmptextlabel.rectTransform, PenTouchInfo.penPosition, paintable.GetComponent<Paintable>().main_camera))
            {
                //Debug.Log(transform.name);

                draggable_now = true;
                Vector3 vec = Vector3.zero;
                RectTransformUtility.ScreenPointToWorldPointInRectangle(tmptextlabel.rectTransform, PenTouchInfo.penPosition, paintable.GetComponent<Paintable>().main_camera, out vec);

                // enforce the same z coordinate as the rest of the points in the parent set object
                vec.z = -5f;
                Vector3 diff = vec - text_label.transform.position + touchDelta;
                diff.z = 0;                

                // don't move right away, move if a threshold has been crossed
                // 5 seems to work well in higher zoom levels and for my finger
                //if (Vector3.Distance(transform.position, vec) > 5)
                // update the function position. 
                transform.parent.position += diff;

                if (transform.parent.childCount > 1)
                    transform.parent.GetChild(1).GetComponent<GraphElementScript>().checkHitAndMove(diff);


                // update the menu position if a menu is currently open/created
                /*GameObject menu_obj = GameObject.Find(menu_name);
                if (menu_obj != null)
                {
                    GameObject.Find(menu_name).transform.position = vec;
                }*/

                // if there are pen objects in the immediate hierarchy, and they have a path defined and abs. layer = 1, 
                // then update the red path to move with the function hierarchy
                /*for (int i = 2; i < transform.childCount; i++)
                {
                    if (transform.GetChild(i).tag == "penline" &&
                        transform.GetChild(i).GetComponent<penLine_script>().abstraction_layer == 1)
                    {
                        if (transform.GetChild(i).GetComponent<penLine_script>().translation_path.Count > 0)
                            transform.GetChild(i).GetComponent<penLine_script>().calculateTranslationPathIfAlreadyDefined();
                    }
                }*/
            }
                        
        }

        else if (PenTouchInfo.ReleasedThisFrame && draggable_now)//(Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Ended && draggable_now)
        {
            draggable_now = false;

            touchDelta = new Vector3(); // reset touchDelta
            img.color = new Color32(125, 255, 165, 255);

            // change anchor color

            // TODO: as an added safety measure, we could use a raycast at the touch position to see if it hits the set anchor
            // This might be the reason for weird function behavior when changing the slider or moving the function around??
            // Comments from penline object's checkHitAndMove() Touch.ended block:
            // // important: touch can start and end when interacting with other UI elements like a set's slider.
            // // so, double check that this touch was still on top of the penline object, not, say, on a slider.

            // check if the function came out of any parent function's area, and went into another function
            /*GameObject[] functions = GameObject.FindGameObjectsWithTag("function");
            bool in_canvas = false;

            for (int i = 0; i < functions.Length; i++)
            {
                // is anchor ( child(0) ) inside the polygon?
                if (functions[i].GetComponent<functionLine_script>().isInsidePolygon(
                    functions[i].GetComponent<functionLine_script>().transform.InverseTransformPoint(
                    transform.GetChild(0).position))
                    &&
                    (functions[i].GetComponent<functionLine_script>().abstraction_layer == 1 ||
                    functions[i].GetComponent<functionLine_script>().abstraction_layer == 2)
                    )
                {
                    transform.parent = functions[i].transform;
                    in_canvas = true;

                    // save log
                    // save coordinates wrt parent center
                    if (abstraction_layer == 1)
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
                // in case it was invisible, make it visible
                parent_asked_to_lower_layer = false;

                // save log
                string tkey = "paintable|" + DateTime.Now.ToString("MM/dd/yyyy HH:mm:ss.fff");
                if (!saved_positions.ContainsKey(tkey))
                    saved_positions.Add(tkey, (Vector2)transform.position);
            }*/
        }
        
    }


    string get_arguments_string()
    {
        string argument_str = "( ";

        /*foreach (GameObject each_graph in transform.parent.GetComponent<FunctionElementScript>().grapharray)
        {
            argument_str += each_graph.GetComponent<GraphElementScript>().graph_name + ", ";
        }*/

        foreach (int cur_key in cur_arg_Str.Keys)
        {
            argument_str += cur_arg_Str[cur_key] + ", ";
        }
        argument_str = argument_str.Remove(argument_str.Length - 2) + " )";

        return argument_str;
    }

    // Checks if there is anything entered into the input field.
    void LockInput(InputField input)
    {
        // if a function is getting evaluated now, we will not receive any further input
        /*if (paintable.GetComponent<Paintable>().no_func_menu_open)
            return;*/
        if (input.text.Length > 0)
        {
            match_found = false;
            

            // ToDo: show red flag where no match

            // check if a valid function has been mapped, then inform paintable they can now instantiate drawing functions
            if (input.text.ToLower().Equals("addition"))
            {
                match_found = true;
                cur_dict = addition_dict;
                //cur_order_dict = addition_order_dict;
                
                output_type = "graph";
            }

            else if (input.text.ToLower().Equals("topological") || input.text.ToLower().Equals("degree_measure"))
            {
                match_found = true;
                cur_dict = topologicalsort_dict;
                //cur_order_dict = dummy_order_dict;

                output_type = "graph";
            }

            else if (input.text.ToLower().Equals("shortestpath"))
            {
                match_found = true;
                cur_dict = shortestpath_dict;
                //cur_order_dict = dummy_order_dict;

                output_type = "graph";
            }

            else if (input.text.ToLower().Equals("community"))
            {
                match_found = true;
                cur_dict = community_dict;
                //cur_order_dict = dummy_order_dict;

                output_type = "graph";
            }


            else if (input.text.ToLower().Equals("dummy"))
            {
                match_found = true;
                cur_dict = dummy_dict;
                //cur_order_dict = dummy_order_dict;

                output_type = "scalar";
            }

            if (match_found)
            {
                cur_iter = 0;
                cur_arg_Str = new Dictionary<int, string>();
                argument_objects = new GameObject[cur_dict.Count];

                foreach (int key in cur_dict.Keys)
                {
                    if (cur_dict[key] == "graph")
                        cur_arg_Str.Add(key, "<sprite name=\"graph_box\">");
                    else if (cur_dict[key] == "iconic")
                        cur_arg_Str.Add(key, "<sprite name=\"node_box\">");
                    else
                        cur_arg_Str.Add(key, "<sprite name=\"text_box\">");
                }

                string argument_str = get_arguments_string();

                text_label.GetComponent<TextMeshProUGUI>().text = input.text.Substring(0,1).ToUpper() +
                                                    input.text.Substring(1).ToLower() + argument_str;

                paintable.GetComponent<Paintable>().no_func_menu_open = false;
            }

        }
    }

    void OnClickButton(Button perform_action)
    {
        Debug.Log("evaluation_began");
        if (match_found)
        {
            transform.parent.GetComponent<FunctionElementScript>().updateLassoPoints();
            // ToDo: update string parsing, pass current func name too
            if (output_type != "scalar")
            {                
                transform.parent.GetComponent<FunctionCaller>().GetGraphJson(argument_objects);
                transform.parent.GetComponent<FunctionCaller>().Function_Caller(mainInputField.text.ToLower());  
            }
            
            input_option.SetActive(false);
            paintable.GetComponent<Paintable>().no_func_menu_open = true;            
        }
    }
    
    public void Hide()
    {
        foreach (GameObject child_graph in argument_objects)
        {
            if (child_graph.tag == "graph")
            {
                // if it is under a function, hide that as well 
                if (child_graph.transform.parent.name.Contains("function_line_"))
                {
                    child_graph.transform.parent.GetChild(0).GetComponent<FunctionMenuScript>().Hide();
                    child_graph.transform.parent.gameObject.SetActive(false);
                }
                else
                {
                    child_graph.SetActive(false);
                }
            }
        }
    }

    public void PostProcess(string output = null)
    {
        
        if (mainInputField.text.ToLower().Equals("topological") || mainInputField.text.ToLower().Equals("degree_measure"))
        {
            transform.parent.GetComponent<FunctionElementScript>().InstantiateTopoGraph();
        }
        else if (mainInputField.text.ToLower().Equals("shortestpath"))
        {
            transform.parent.GetComponent<FunctionElementScript>().InstantiatePathGraph();
        }
        else if (mainInputField.text.ToLower().Equals("community"))
        {
            Debug.Log("finished");
            transform.parent.GetComponent<FunctionElementScript>().InstantiateCommunityGraph();
        }
        else
        {
            if (output_type == "graph") transform.parent.GetComponent<FunctionElementScript>().InstantiateGraph(); //(output);
        }

        if (instant_eval)
        {
            // show results instantly
            transform.parent.GetComponent<MeshFilter>().sharedMesh.Clear();            
            settings.transform.gameObject.SetActive(false);
        }
        else
        {
            if (output_type == "graph")
            {
                transform.parent.GetChild(1).gameObject.SetActive(false);
            }
        }

        if (!keep_child_object)
        {
            foreach (GameObject child_graph in argument_objects)
            {
                if (child_graph.tag == "graph")
                {
                    // if it is under a function, hide that as well 
                    if (child_graph.transform.parent.name.Contains("function_line_"))
                    {
                        // recursive hide call
                        child_graph.transform.parent.GetChild(0).GetComponent<FunctionMenuScript>().Hide();
                        child_graph.transform.parent.gameObject.SetActive(false);
                    }
                    else
                    {
                        child_graph.SetActive(false);
                    }
                }
            }
        }
                
        text_label.GetComponent<TextMeshProUGUI>().text = text_label.GetComponent<TextMeshProUGUI>().text.Replace(" ", "");

        message_box.GetComponent<TextMeshProUGUI>().text = "<color=\"black\">" + text_label.GetComponent<TextMeshProUGUI>().text ;
        if (output_type == "scalar")   text_label.GetComponent<TextMeshProUGUI>().text = output;
        //else text_label.transform.parent.gameObject.SetActive(false);
        perform_action.transform.gameObject.SetActive(false);
        input_option.SetActive(false);
        eval_finished = true;
    }

    void ChildToggle(Toggle toggle)
    {
        if (toggle.isOn) keep_child_object = true;
        else keep_child_object = false;
    }

    void InstantEvalToggle(Toggle toggle)
    {
        if (toggle.isOn) instant_eval = true;
        else instant_eval = false;
    }

    void OnSettingsButton(Button settings)
    {
        input_option.SetActive(!(input_option.activeSelf));
        if (eval_finished)
        {
            evaluation_type.gameObject.SetActive(false);
            keepchilden.gameObject.SetActive(false);
            mainInputField.gameObject.SetActive(false);
            show_result.gameObject.SetActive(true);
            /*keepchilden.interactable = false;
            mainInputField.interactable = false;*/
        }
    }

    void ResultVisiblity(Toggle toggle)
    {
        if (eval_finished)
        {
            if (output_type == "graph")
            {
                transform.parent.GetChild(1).gameObject.SetActive(toggle.isOn);
                transform.parent.GetComponent<MeshRenderer>().enabled = !toggle.isOn;
            }
        }
    }

    private void OnDestroy()
    {
        if (textbox_open)
        {
            Destroy(argument_text);
            argument_text = null;
            textbox_open = false;
        }
    }
}
