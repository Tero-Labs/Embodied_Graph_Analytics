using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GraphSliderMenu : MonoBehaviour
{
    public string cur_layer;
    public GameObject graph_parent;
    public Slider mainSlider;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void UpdateLayer(string layer)
    {
        if (layer == "graph")
        {
            mainSlider.value = 0.25f;
            cur_layer = "graph";
        }
        else if (layer == "simplicial")
        {
            mainSlider.value = 0.50f;
            cur_layer = "simplicial";
        }
        else if (layer == "hypergraph")
        {
            mainSlider.value = 0.75f;
            cur_layer = "hypergraph";
        }
        else
        {
            mainSlider.value = 1f;
            cur_layer = "abstract";
        }
    }

    public void OnSliderValueChanged(float value)
    {
        if (value < 0.25f)
        {
            cur_layer = "graph";
        }            
        else if (value < 0.50f)
        {
            cur_layer = "simplicial";
        }            
        else if (value < 0.75f)
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
