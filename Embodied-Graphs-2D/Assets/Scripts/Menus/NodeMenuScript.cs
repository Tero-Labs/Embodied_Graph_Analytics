using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI; // Required when Using UI elements.

public class NodeMenuScript : MonoBehaviour
{
    public InputField mainInputField;
    public GameObject menu_parent;
    public int weight;
    // Start is called before the first frame update
    void Start()
    {
        //https://docs.unity3d.com/2018.4/Documentation/ScriptReference/UI.InputField-onEndEdit.html
        //Adds a listener that invokes the "LockInput" method when the player finishes editing the main input field.
        //Passes the main input field into the method when "LockInput" is invoked
        mainInputField.onValueChanged.AddListener(delegate { LockInput(mainInputField); });
    }

    // Checks if there is anything entered into the input field.
    void LockInput(InputField input)
    {
        if (input.text.Length > 0)
        {
            menu_parent.GetComponent<iconicElementScript>().icon_name = input.text;
            menu_parent.name = input.text;
            //Debug.Log("name" + input.text + "has been updated for"+ menu_parent.name);
        }
    }

    
}
