using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FunctionCaller : MonoBehaviour
{
    private HelloRequester _helloRequester;
    private string graphs_as_string;

    private void Start()
    {

    }

    public void GetGraphStrings(GameObject[] selected_graphs)
    {
        graphs_as_string = "";
        for (int i = 0; i < selected_graphs.Length; i++)
        {
            selected_graphs[i].GetComponent<GraphElementScript>().Graph_as_Str();
            graphs_as_string = graphs_as_string + selected_graphs[i].GetComponent<GraphElementScript>().nodes_str + "-" + 
                selected_graphs[i].GetComponent<GraphElementScript>().edges_str + "-" +
                selected_graphs[i].GetComponent<GraphElementScript>().simplicial_str + "-" +
                selected_graphs[i].GetComponent<GraphElementScript>().hyper_edges_str + "+" ;
        }
        graphs_as_string = graphs_as_string.Remove(graphs_as_string.Length - 1);

        Debug.Log("combined_string: " + graphs_as_string);
    }

    private void OnDestroy()
    {
        //_helloRequester.Stop();
    }

    public bool Function_Caller(string function_name)
    {
        if (_helloRequester != null)
        {
            //bool flag = true;
            // housekeeping so that an already running thread does not throw netmq exception 
            if (_helloRequester.isalive())
            {
                return false;
            }
            //Debug.Log("checking is alive: " + _helloRequester.isalive().ToString() + " flag: " + flag.ToString());
        }

        transform.GetComponent<StartServer>().ExecuteCommand(function_name);
        _helloRequester = new HelloRequester();
        _helloRequester.graph_as_str = graphs_as_string; //"{8,9,7,10}-{8,9}{9,7}{8,7}+{8,9,7,10}-{8,9}{9,7}{8,7}";
        _helloRequester.command = function_name;
        _helloRequester.Start();
        return true;
    }

    void Update()
    {
        if (_helloRequester != null)
        {
            if (_helloRequester.serverUpdateCame)
            {
                _helloRequester.serverUpdateCame = false;
                //show_your_output_as_you_want
                transform.GetComponent<FunctionElementScript>().InstantiateGraph(_helloRequester.serverUpdate);
            }
        }

    }
}
