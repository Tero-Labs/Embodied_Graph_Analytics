﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Michsky.UI.ModernUIPack;

public class DropDownMenu : MonoBehaviour
{
    public GameObject toggle_parent;
    public GameObject details_dropdown;
    public bool nodes_visibility;

    // Start is called before the first frame update
    void Start()
    {
        StartCoroutine("ToggleSetup");
        nodes_visibility = true;
    }

    IEnumerator ToggleSetup()
    {
        yield return null;

        Toggle toggle_1 = toggle_parent.transform.GetChild(0).GetComponent<Toggle>();
        toggle_1.onValueChanged.AddListener(delegate { ShowNodes(toggle_1); });

        Toggle toggle_2 = toggle_parent.transform.GetChild(1).GetComponent<Toggle>();
        toggle_2.onValueChanged.AddListener(delegate { ShowEdges(toggle_2); });

        Toggle toggle_3 = toggle_parent.transform.GetChild(2).GetComponent<Toggle>();
        toggle_3.onValueChanged.AddListener(delegate { ShowFunctionLasso(toggle_3); });

        Toggle toggle_4 = toggle_parent.transform.GetChild(3).GetComponent<Toggle>();
        toggle_4.onValueChanged.AddListener(delegate { ShowFunctionAnchor(toggle_4); });

        Toggle toggle_5 = toggle_parent.transform.GetChild(4).GetComponent<Toggle>();
        toggle_5.onValueChanged.AddListener(delegate { ShowVideoPlayer(toggle_5); });


    }

    public void applyDetails()
    {        
        bool state = details_dropdown.GetComponent<DropdownMultiSelect>().transform.GetChild(1).GetChild(0).GetChild(1).GetChild(0).GetChild(0).GetComponent<Toggle>().isOn;

        if (state != nodes_visibility)
        {
            nodes_visibility = state;
            GameObject[] node_par = GameObject.FindGameObjectsWithTag("graph");

            foreach (GameObject vp in node_par)
            {
                vp.transform.GetChild(0).gameObject.SetActive(nodes_visibility);
            }
        }
    }


    // Update is called once per frame
    void Update()
    {
        // applyDetails();
    }

    public void ShowNodes(Toggle toggle)
    {
        //StartCoroutine(ShowNodesResult(toggle));
        bool visibility = toggle.isOn;
        
        GameObject[] graphs = GameObject.FindGameObjectsWithTag("graph");

        foreach (GameObject graph in graphs)
        {
            //graph.transform.GetChild(0).gameObject.SetActive(visibility);
            graph.GetComponent<GraphElementScript>().ShowNodes(toggle);
        }
    }
    
    public void ShowEdges(Toggle toggle)
    {
        GameObject[] graphs = GameObject.FindGameObjectsWithTag("graph");

        foreach (GameObject graph in graphs)
        {
            graph.GetComponent<GraphElementScript>().ShowEdges(toggle);
            /*for (int i = 1; i < 4; i++)
            {
                graph.transform.GetChild(i).gameObject.SetActive(toggle.isOn);
            }*/
        }
    }

    public void ShowFunctionLasso(Toggle toggle)
    {
        GameObject[] functions = GameObject.FindGameObjectsWithTag("function");

        foreach (GameObject cur_function in functions)
        {
            cur_function.GetComponent<MeshRenderer>().enabled = toggle.isOn;
        }
    }

    public void ShowFunctionAnchor(Toggle toggle)
    {
        GameObject[] functions = GameObject.FindGameObjectsWithTag("function");

        foreach (GameObject cur_function in functions)
        {
            if (cur_function.transform.childCount == 0) continue; 
            
            cur_function.transform.GetChild(0).GetChild(0).gameObject.SetActive(toggle.isOn);
        }
    }

    public void ShowVideoPlayer(Toggle toggle)
    {
        GameObject[] vps = GameObject.FindGameObjectsWithTag("video_player");

        foreach (GameObject cur_vp in vps)
        {
            cur_vp.GetComponent<MeshRenderer>().enabled = toggle.isOn;
        }
    }

}