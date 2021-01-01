using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;

public class FunctionMenuScript : MonoBehaviour
{
    
    public InputField mainInputField;
    public GameObject text_label;
    public GameObject input_option;
    public bool match_found;
    public GameObject paintable;
    public bool red_flag;
    public TMP_Text tmptextlabel;


    // Start is called before the first frame update
    void Start()
    {
        //https://docs.unity3d.com/2018.4/Documentation/ScriptReference/UI.InputField-onEndEdit.html
        //Adds a listener that invokes the "LockInput" method when the player finishes editing the main input field.
        //Passes the main input field into the method when "LockInput" is invoked
        mainInputField.onValueChanged.AddListener(delegate { LockInput(mainInputField); });
    }

    private void Update()
    {

        if (PenTouchInfo.PressedThisFrame && TMP_TextUtilities.IsIntersectingRectTransform(tmptextlabel.rectTransform, PenTouchInfo.penPosition, paintable.GetComponent<Paintable>().main_camera))
        {
            input_option.SetActive(true);
            var index = TMP_TextUtilities.FindIntersectingCharacter(tmptextlabel, PenTouchInfo.penPosition, paintable.GetComponent<Paintable>().main_camera, false);
            if (index != -1) Debug.Log("Found intersecting character at " + index.ToString());
        }
                    
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

            // check if a valid function has been mapped, then inform paintable they can now instantiate drawing functions
            if (input.text.ToLower().Equals("addition"))
            {
                match_found = true;
                string argument_str = "( ";

                foreach (GameObject each_graph in transform.parent.GetComponent<FunctionElementScript>().grapharray)
                {
                    argument_str += each_graph.GetComponent<GraphElementScript>().graph_name + ", ";
                }

                argument_str = argument_str.Remove(argument_str.Length - 2) + " )";

                text_label.GetComponent<TextMeshProUGUI>().text = input.text.ToUpper() + argument_str;
                //paintable.GetComponent<Paintable>().no_func_menu_open = true;
                transform.parent.GetComponent<FunctionCaller>().GetGraphStrings(transform.parent.GetComponent<FunctionElementScript>().grapharray);
                transform.parent.GetComponent<FunctionCaller>().Function_Caller(input.text.ToLower());
                input_option.SetActive(false);
                paintable.GetComponent<Paintable>().no_func_menu_open = true;
            }

        }
    }
}
