using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI; // Required when Using UI elements.

public class EdgeMenuScript : MonoBehaviour
{
    public InputField mainInputField;

    public GameObject menu_parent;
    public int weight;
    public Toggle toggle;
    public Button delete;


    // Start is called before the first frame update
    void Start()
    {
        //https://docs.unity3d.com/2018.4/Documentation/ScriptReference/UI.InputField-onEndEdit.html
        //Adds a listener that invokes the "LockInput" method when the player finishes editing the main input field.
        //Passes the main input field into the method when "LockInput" is invoked
        mainInputField.onValueChanged.AddListener(delegate { LockInput(mainInputField); });
        toggle.onValueChanged.AddListener(delegate { DirEdge(toggle); });
        delete.onClick.AddListener(delegate { Delete(delete); });

        StartCoroutine(SetupParent());
    }

    IEnumerator SetupParent()
    {
        mainInputField.text = menu_parent.GetComponent<EdgeElementScript>().edge_weight.ToString();
        toggle.isOn = menu_parent.GetComponent<EdgeElementScript>().directed_edge;
        yield return null;
    }

    // Checks if there is anything entered into the input field.
    void LockInput(InputField input)
    {
        Paintable.click_on_inputfield = true;

        if (input.text.Length > 0)
        {            
            weight = int.Parse(input.text);
            menu_parent.GetComponent<EdgeElementScript>().edge_weight = weight;
            menu_parent.transform.parent.parent.GetComponent<GraphElementScript>().edges_init();
            Debug.Log("weight" + input.text + "has been updated for"+ menu_parent.name);
        }
    }

    void DirEdge(Toggle toggle)
    {
        for (int i = 0; i < menu_parent.transform.childCount; i++)
            Destroy(menu_parent.transform.GetChild(i).gameObject);

        menu_parent.GetComponent<EdgeElementScript>().directed_edge = toggle.isOn;
        menu_parent.GetComponent<EdgeElementScript>().addDot();
    }

    void Delete(Button delete)
    {
        StartCoroutine(Delete());
    }

    IEnumerator Delete()
    {
        GameObject graph = menu_parent.transform.parent.parent.gameObject;
        Destroy(menu_parent);

        yield return null;

        graph.GetComponent<GraphElementScript>().edges_init();

        Destroy(transform.gameObject);
    }

    // Update is called once per frame
    void Update()
    {
        if (mainInputField != null && mainInputField.isFocused)
            Paintable.click_on_inputfield = true;
    }

}
