using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class GraphSliderMenu : MonoBehaviour
{
    public string cur_layer;
    public GameObject parent;
    public Slider mainSlider;

    public TMP_Text tmptextlabel;

    // Start is called before the first frame update
    void Start()
    {
        
    }
    
    // Update is called once per frame
    void Update()
    {
        
    }

    public void setparent(GameObject obj)
    {
        this.parent = obj;
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
        tmptextlabel.text = cur_layer;
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

        tmptextlabel.text = cur_layer;
        parent.GetComponent<GraphElementScript>().StartConversion(cur_layer);
    }

   
}
