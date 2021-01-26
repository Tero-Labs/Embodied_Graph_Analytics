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
    public bool keep_child_object;
    public bool eval_finished;

    public InputField mainInputField;
    public Button perform_action;
    public GameObject text_label;
    public GameObject input_option;
    public bool match_found;
    private bool textbox_open;


    public GameObject paintable;
    public bool red_flag;
    public TMP_Text tmptextlabel;

    string sprite_arg;
    public GameObject message_box;
    public GameObject argument_text_box;
    public GameObject dragged_arg_object;

    // functions_list
    private Dictionary<int,string> addition_dict;
    private Dictionary<int, int> addition_order_dict;
    private Dictionary<int, string> dummy_dict;
    private Dictionary<int, int> dummy_order_dict;

    private GameObject argument_text;

    public GameObject[] argument_objects;

    // current function dictionary
    public Dictionary<int, string> cur_dict;
    // current function order dictionary
    public Dictionary<int, int> cur_order_dict;
    // needed to track the argument string for display
    public Dictionary<int, string> cur_arg_Str;

    public string output_type;

    // Start is called before the first frame update
    void Start()
    {
        //https://docs.unity3d.com/2018.4/Documentation/ScriptReference/UI.InputField-onEndEdit.html
        //Adds a listener that invokes the "LockInput" method when the player finishes editing the main input field.
        //Passes the main input field into the method when "LockInput" is invoked
        mainInputField.onValueChanged.AddListener(delegate { LockInput(mainInputField); });
        perform_action.onClick.AddListener(delegate { OnClickButton(perform_action); });

        argument_text = null;
        dragged_arg_object = null;
        sprite_arg = "<sprite name=\"argument_box\">";

        keepchilden.onValueChanged.AddListener(delegate { ChildToggle(keepchilden); });
        evaluation_type.onValueChanged.AddListener(delegate { InstantEvalToggle(evaluation_type); });
        instant_eval = false;
        keep_child_object = true;

        textbox_open = false;
        eval_finished = false;

        # region function arguments and types dictionary
        addition_dict = new Dictionary<int, string>()
        {
            {10, "graph"},
            {13, "graph"},
        };

        addition_order_dict = new Dictionary<int, int>()
        {
            {10, 0},
            {13, 1},
        };


        dummy_dict = new Dictionary<int, string>()
        {
            {7, "string"},
            {10, "string"},
        };

        dummy_order_dict = new Dictionary<int, int>()
        {
            {7, 0},
            {10, 1},
        };

        # endregion
    }

    private void Update()
    {
        //TodO:add_for_drag

        if (PenTouchInfo.PressedThisFrame)
        {
            if (!eval_finished) message_box.GetComponent<TextMeshProUGUI>().text = "";
            if (TMP_TextUtilities.IsIntersectingRectTransform(tmptextlabel.rectTransform, PenTouchInfo.penPosition, paintable.GetComponent<Paintable>().main_camera))
            {
                paintable.GetComponent<Paintable>().dragged_arg_textbox = transform.gameObject;
                if (!textbox_open && !eval_finished)
                {
                    if (input_option.activeSelf) input_option.SetActive(false);
                    else input_option.SetActive(true);
                }

                if (match_found && !eval_finished)
                {
                    var index = TMP_TextUtilities.FindIntersectingCharacter(tmptextlabel, PenTouchInfo.penPosition, paintable.GetComponent<Paintable>().main_camera, false);
                    Debug.Log("Found character at " + index.ToString());

                    if (!textbox_open)
                    {
                        //extra check for better UX
                        if (cur_dict.ContainsKey(index))
                            index = index;
                        else if(cur_dict.ContainsKey(index - 1))
                            index = index - 1;
                        else if (cur_dict.ContainsKey(index + 1))
                            index = index + 1;
                        else
                            index = -1;
                    }

                    if (index != -1 && !textbox_open)
                    {
                        if (cur_dict[index] == "string" || cur_dict[index] == "int")
                        {
                            Vector3 TouchStart = Camera.main.ScreenToWorldPoint(PenTouchInfo.penPosition);
                            Debug.Log(TouchStart.ToString());
                            //todo:not_being__correctly_mapped
                            argument_text = Instantiate(argument_text_box,
                                //TouchStart,
                                paintable.GetComponent<Paintable>().canvas_radial.transform.TransformPoint(TouchStart),
                                Quaternion.identity,
                                paintable.GetComponent<Paintable>().canvas_radial.transform/*transform*/);

                            argument_text.GetComponent<FunctionTextInputMenu>().setArgument(transform.gameObject, index);
                            textbox_open = true;
                        }                    
                    }
                }
            }
            //remove active textbox, if any
            else
            {
                
                if (textbox_open)
                {
                    int index = argument_text.GetComponent<FunctionTextInputMenu>().str_index;
                    string str_arg = argument_text.GetComponent<FunctionTextInputMenu>().str_arg;

                    if (str_arg.Length > 0)
                    {
                        cur_arg_Str[index] = str_arg;
                        string arg_Str = get_arguments_string();
                        text_label.GetComponent<TextMeshProUGUI>().text = mainInputField.text.ToUpper() + arg_Str;
                        //ToDo: store for calculation
                        //argument_objects[cur_order_dict[index]] = str_arg;
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
                }
            }
        }

        else if (PenTouchInfo.ReleasedThisFrame
            && (dragged_arg_object != null || 
            (paintable.GetComponent<Paintable>().dragged_arg_textbox != null && paintable.GetComponent<Paintable>().dragged_arg_textbox != transform.gameObject)))
        {
            if (TMP_TextUtilities.IsIntersectingRectTransform(tmptextlabel.rectTransform, PenTouchInfo.penPosition, paintable.GetComponent<Paintable>().main_camera))
            {
                var index = TMP_TextUtilities.FindIntersectingCharacter(tmptextlabel, PenTouchInfo.penPosition, paintable.GetComponent<Paintable>().main_camera, false);                

                if (dragged_arg_object == null && paintable.GetComponent<Paintable>().dragged_arg_textbox != null)
                {
                    dragged_arg_object = paintable.GetComponent<Paintable>().dragged_arg_textbox;
                }

                Debug.Log("Found draaged object" + dragged_arg_object.name + " character at " + index.ToString());

                //extra check for better UX
                if (cur_dict.ContainsKey(index))
                    index = index;
                else if (cur_dict.ContainsKey(index - 1))
                    index = index - 1;
                else if (cur_dict.ContainsKey(index + 1))
                    index = index + 1;
                else
                    index = -1;

                if (index != -1)
                {
                    Transform temp = null;
                    // whether graph or icon is expected as arg.type
                    if (cur_dict[index] == "graph")
                    {
                        if (dragged_arg_object.transform.parent.tag == "node_parent")
                        {
                            temp = dragged_arg_object.transform.parent.parent;
                            cur_arg_Str[index] = temp.GetComponent<GraphElementScript>().graph_name;
                            // argument obj referece update for calculation
                            argument_objects[cur_order_dict[index]] = temp.gameObject;
                        }
                    }
                    else if (cur_dict[index] == "iconic" && dragged_arg_object.tag == "iconic")
                    {
                        temp = dragged_arg_object.transform;
                        cur_arg_Str[index] = temp.GetComponent<iconicElementScript>().icon_number.ToString();
                        // argument obj referece update for calculation
                        argument_objects[cur_order_dict[index]] = temp.gameObject;
                    }
                    else if (cur_dict[index] == "string" || cur_dict[index] == "int")
                    {
                        // double check if the same object is not clicked
                        if (paintable.GetComponent<Paintable>().dragged_arg_textbox != null)
                        {
                            temp = paintable.GetComponent<Paintable>().dragged_arg_textbox.transform;
                            cur_arg_Str[index] = temp.GetComponent<FunctionMenuScript>().text_label.GetComponent<TextMeshProUGUI>().text;
                            // argument obj referece update for calculation
                            //TodO:add_for_int_arguments
                            argument_objects[cur_order_dict[index]] = temp.gameObject;
                        }
                    }
                    

                    if (temp != null)
                    {
                        // argument string update for display
                        string arg_Str = get_arguments_string();
                        text_label.GetComponent<TextMeshProUGUI>().text = mainInputField.text.ToUpper() + arg_Str;                        
                    }
                    else
                    {
                        message_box.GetComponent<TextMeshProUGUI>().text = "Invalid argument!";
                    }

                    // clearing the clicked gameobject insided if block for ensuring the previous information is not lost
                    // in other FunctionMenuScripts that are not clicked on
                    paintable.GetComponent<Paintable>().dragged_arg_textbox = null;
                }
                paintable.GetComponent<Paintable>().dragged_arg_textbox = null;
            }

            dragged_arg_object = null;
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
                cur_order_dict = addition_order_dict;
                cur_arg_Str = new Dictionary<int, string>();
                argument_objects = new GameObject[cur_dict.Count];

                output_type = "graph";

                foreach (int key in cur_dict.Keys)
                {
                    cur_arg_Str.Add(key, sprite_arg);
                }

                string argument_str = get_arguments_string();

                text_label.GetComponent<TextMeshProUGUI>().text = input.text.ToUpper() + argument_str;
                paintable.GetComponent<Paintable>().no_func_menu_open = false;
            }

            else if (input.text.ToLower().Equals("dummy"))
            {
                match_found = true;
                cur_dict = dummy_dict;
                cur_order_dict = dummy_order_dict;
                cur_arg_Str = new Dictionary<int, string>();
                argument_objects = new GameObject[cur_dict.Count];

                output_type = "scalar";

                foreach (int key in cur_dict.Keys)
                {
                    cur_arg_Str.Add(key, sprite_arg);
                }

                string argument_str = get_arguments_string();

                text_label.GetComponent<TextMeshProUGUI>().text = input.text.ToUpper() + argument_str;
                paintable.GetComponent<Paintable>().no_func_menu_open = false;
            }

        }
    }

    void OnClickButton(Button perform_action)
    {
        //Debug.Log("here");
        if (match_found)
        {            
            // ToDo: update string parsing, pass current func name too
            if (output_type != "scalar")
            {
                transform.parent.GetComponent<FunctionCaller>().GetGraphStrings(argument_objects);
                transform.parent.GetComponent<FunctionCaller>().Function_Caller(mainInputField.text.ToLower());
            }
            else
            {
                PostProcess("5");
            }
            
            input_option.SetActive(false);
            paintable.GetComponent<Paintable>().no_func_menu_open = true;            
        }
    }

    public void PostProcess(string output = null)
    {
        if (!keep_child_object)
        {
            foreach (GameObject child_graph in argument_objects)
            {
                if (child_graph.tag == "graph")
                {
                    child_graph.SetActive(false);
                }
            }
        }

        if (instant_eval)
        {
            // show results instantly
            transform.parent.GetComponent<MeshFilter>().sharedMesh.Clear();
            if (output_type == "graph") transform.parent.GetComponent<FunctionElementScript>().InstantiateGraph(output);
        }

        message_box.GetComponent<TextMeshProUGUI>().text = "<color=\"black\">" + text_label.GetComponent<TextMeshProUGUI>().text ;
        if (output_type == "scalar")   text_label.GetComponent<TextMeshProUGUI>().text = output;
        else text_label.transform.parent.gameObject.SetActive(false);
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
}
