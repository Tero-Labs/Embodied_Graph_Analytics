using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GraphElementScript : MonoBehaviour
{
    public string nodes_str;
    public string edges_str;
    public string hyper_edges_str;
    public string simplicial_str;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void edges_as_Str()
    {
        edges_str = "";

        Transform Prev_edge_parent = transform.GetChild(1);
        Transform[] allChildrenedge = Prev_edge_parent.GetComponentsInChildren<Transform>();

        foreach (Transform child in allChildrenedge)
        {
            if (child.tag == "edge")
            {
                edges_str += "{" + child.GetComponent<EdgeElementScript>().edge_start.GetComponent<iconicElementScript>().icon_number.ToString() + "," + child.GetComponent<EdgeElementScript>().edge_end.GetComponent<iconicElementScript>().icon_number.ToString() + "}";
            }
        }

        //Debug.Log("edges_str" + " : " + edges_str);
    }

    public void simplicial_as_Str()
    {
        simplicial_str = "";

        Transform Prev_simp_parent = transform.GetChild(2);
        Transform[] allChildrensimpedge = Prev_simp_parent.GetComponentsInChildren<Transform>();

        foreach (Transform child in allChildrensimpedge)
        {
            if (child.tag == "simplicial")
            {
                simplicial_str += "{";
                foreach (GameObject node in child.GetComponent<SimplicialElementScript>().thenodes)
                    simplicial_str += node.GetComponent<iconicElementScript>().icon_number.ToString() + ",";

                simplicial_str = simplicial_str.Remove(simplicial_str.Length - 1) + "}";
            }
        }

        //Debug.Log("simplicial_str" + " : " + simplicial_str);
    }

    public void hyperedges_as_Str()
    {
        hyper_edges_str = "";

        Transform Prev_hyper_parent = transform.GetChild(3);
        Transform[] allChildrenhyperedge = Prev_hyper_parent.GetComponentsInChildren<Transform>();

        foreach (Transform child in allChildrenhyperedge)
        {
            if (child.tag == "hyper")
            {
                hyper_edges_str += "{";
                foreach (GameObject node in child.GetComponent<HyperElementScript>().thenodes)
                    hyper_edges_str += node.GetComponent<iconicElementScript>().icon_number.ToString() + ",";

                hyper_edges_str = hyper_edges_str.Remove(hyper_edges_str.Length - 1) + "}";                
            }
        }

        //Debug.Log("hyper_edges_str" + " : " + hyper_edges_str);
    }

    // express the graph as string, so that we can pass to the python server
    public void Graph_as_Str()
    {
        nodes_str = "{";

        Transform Prev_node_parent = transform.GetChild(0);
        Transform[] allChildrennode = Prev_node_parent.GetComponentsInChildren<Transform>();

        
        foreach (Transform child in allChildrennode)
        {
            if (child.tag == "iconic")
            {
                nodes_str += child.GetComponent<iconicElementScript>().icon_number.ToString() + ",";
            }            
        }

        
        nodes_str = nodes_str.Remove(nodes_str.Length - 1) + "}";
        //Debug.Log("nodes_str" + " : " + nodes_str);

        edges_as_Str();
        simplicial_as_Str();
        hyperedges_as_Str();

        //transform.GetComponent<HelloClient>().Abstraction_conversion(nodes_str+"-"+edges_str, "graph_to_hypergraph");
        //transform.GetComponent<HelloClient>().Abstraction_conversion(nodes_str + "-" + simplicial_str, "simplicial_to_graph");
        transform.GetComponent<HelloClient>().Abstraction_conversion(nodes_str + "-" + hyper_edges_str, "hypergraph_to_graph");
    }
}
