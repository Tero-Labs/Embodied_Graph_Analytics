using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class GraphSliderMenu : MonoBehaviour
{
    public string cur_layer;
    public GameObject graph_parent;
    public Slider mainSlider;

    public Toggle graphtoggle;
    public Toggle simplicialtoggle;
    public Toggle hypergraphtoggle;
    public Toggle abstracttoggle;

    public InputField mainInputField;

    // Start is called before the first frame update
    void Start()
    {
        mainInputField.onValueChanged.AddListener(delegate { LockInput(mainInputField); });

    }

    // Checks if there is anything entered into the input field.
    void LockInput(InputField input)
    {
        if (input.text.Length > 0)
        {
            graph_parent.GetComponent<GraphElementScript>().graph_name = input.text;
            // graph name update
            if (graph_parent.transform.childCount > 4 && graph_parent.transform.GetChild(4).gameObject.activeSelf)
            {
                graph_parent.transform.GetChild(4).GetChild(0).GetComponent<TextMeshProUGUI>().text = input.text;
            }
            //ToDo:show_name_in_a_text
            //menu_parent.name = input.text;
            //Debug.Log("name" + input.text + "has been updated for"+ menu_parent.name);
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void setparent(GameObject obj)
    {
        this.graph_parent = obj;
        graphtoggle.GetComponent<ToggleButton>().graph_parent = obj;
        simplicialtoggle.GetComponent<ToggleButton>().graph_parent = obj;
        hypergraphtoggle.GetComponent<ToggleButton>().graph_parent = obj;
        abstracttoggle.GetComponent<ToggleButton>().graph_parent = obj;
    }

    public void UpdateLayer(string layer)
    {
        if (layer == "graph")
        {
            mainSlider.value = 1;
            cur_layer = "graph";
        }
        else if (layer == "simplicial")
        {
            mainSlider.value = 2;
            cur_layer = "simplicial";
        }
        else if (layer == "hypergraph")
        {
            mainSlider.value = 3;
            cur_layer = "hypergraph";
        }
        else
        {
            mainSlider.value = 4;
            cur_layer = "abstract";
        }
    }

    public void OnSliderValueChanged(float value)
    {
        if (value == 1)
        {
            cur_layer = "graph";
        }            
        else if (value == 2)
        {
            cur_layer = "simplicial";
        }            
        else if (value == 3)
        {
            cur_layer = "hypergraph";
        }
        else
        {
            cur_layer = "abstract";
        }

        graph_parent.GetComponent<GraphElementScript>().StartConversion(cur_layer);
    }

   
}
