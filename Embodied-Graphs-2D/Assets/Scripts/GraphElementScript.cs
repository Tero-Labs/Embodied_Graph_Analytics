using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System.Linq;
using TMPro;


public class GraphElementScript : MonoBehaviour
{
    public string graph_name;
    public string nodes_str;
    public string edges_str;
    public string hyper_edges_str;
    public string simplicial_str;
    public string abstraction_layer;
    public GameObject Objects_parent;

    public bool splined_edge_flag;

    public GameObject EdgeElement;
    public GameObject SimplicialEdgeElement;
    public GameObject hyperEdgeElement;
    public GameObject LabelElement;
    //public GameObject canvas;

    public bool graph_lock;
    public bool simplicial_lock;
    public bool hyper_edges_lock;
    public bool abstract_lock;

    public bool graph_drawn;
    public bool simplicial_drawn;
    public bool hyper_edges_drawn;
    public bool abstract_drawn;

    public Graph graph;

    public Dictionary<string, Transform> nodeMaps;

    // Start is called before the first frame update
    void Start()
    {
        abstraction_layer = "graph";
        //Objects_parent = GameObject.Find("Objects");

        graph_lock = true;
        simplicial_lock = true;
        hyper_edges_lock = true;
        abstract_lock = true;

        graph_drawn = false;
        simplicial_drawn = false;
        hyper_edges_drawn = false;
        //splined_edge_flag = false;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void DeleteChildren(Transform trans)
    {
        for (int i = trans.childCount - 1; i >= 0; --i)
        {
            var child = trans.GetChild(i).gameObject;
            Debug.Log("Deleting " + child.name);
            Destroy(child);
        }

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
        edges_init();
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
                if (child.GetComponent<SimplicialElementScript>() != null)
                {
                    simplicial_str += "{";
                    foreach (GameObject node in child.GetComponent<SimplicialElementScript>().thenodes)
                        simplicial_str += node.GetComponent<iconicElementScript>().icon_number.ToString() + ",";

                    simplicial_str = simplicial_str.Remove(simplicial_str.Length - 1) + "}";
                }
                else
                {
                    simplicial_str += "{" + child.GetComponent<EdgeElementScript>().edge_start.GetComponent<iconicElementScript>().icon_number.ToString() + "," + child.GetComponent<EdgeElementScript>().edge_end.GetComponent<iconicElementScript>().icon_number.ToString() + "}";
                }
            }
        }

        //Debug.Log("simplicial_str" + " : " + simplicial_str);
        simplicial_init();
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
        hyperedges_init();
    }

    // express the graph as string, so that we can pass to the python server
    public void Graph_as_Str()
    {
        nodes_str = "{";

        Transform Prev_node_parent = transform.GetChild(0);
        Transform[] allChildrennode = Prev_node_parent.GetComponentsInChildren<Transform>();

        nodeMaps = new Dictionary<string, Transform>();
        int icon_count = 0;
        foreach (Transform child in allChildrennode)
        {
            if (child.tag == "iconic")
            {
                icon_count++;
                nodes_str += child.GetComponent<iconicElementScript>().icon_number.ToString() + ",";
                nodeMaps.Add(child.GetComponent<iconicElementScript>().icon_number.ToString(), child);
            }            
        }

        if (icon_count == 0)
            Destroy(transform.gameObject);
                
        nodes_str = nodes_str.Remove(nodes_str.Length - 1) + "}";
        //Debug.Log("nodes_str" + " : " + nodes_str);

        edges_as_Str();
        simplicial_as_Str();
        hyperedges_as_Str();

        Graph_init();
    }


    public void edges_init()
    {
        graph.edges = new List<Edge>();

        Transform Prev_edge_parent = transform.GetChild(1);
        Transform[] allChildrenedge = Prev_edge_parent.GetComponentsInChildren<Transform>();

        foreach (Transform child in allChildrenedge)
        {
            if (child.tag == "edge")
            {
                Edge edge = new Edge();
                edge.edge_start = child.GetComponent<EdgeElementScript>().edge_start.GetComponent<iconicElementScript>().icon_number;
                edge.edge_end = child.GetComponent<EdgeElementScript>().edge_end.GetComponent<iconicElementScript>().icon_number;
                edge.weight = child.GetComponent<EdgeElementScript>().edge_weight;

                graph.edges.Add(edge);
            }
        }

    }

    public void simplicial_init()
    {
        graph.simplicials = new List<HyperOrSimplicialEdge>();

        Transform Prev_simp_parent = transform.GetChild(2);
        Transform[] allChildrensimpedge = Prev_simp_parent.GetComponentsInChildren<Transform>();

        foreach (Transform child in allChildrensimpedge)
        {
            if (child.tag == "simplicial")
            {
                HyperOrSimplicialEdge simplicial = new HyperOrSimplicialEdge();
                simplicial.nodes = new List<int>();

                if (child.GetComponent<SimplicialElementScript>() != null)
                {
                    foreach (GameObject node in child.GetComponent<SimplicialElementScript>().thenodes)
                        simplicial.nodes.Add(node.GetComponent<iconicElementScript>().icon_number);                    
                }
                else
                {
                    simplicial.nodes.Add(child.GetComponent<EdgeElementScript>().edge_start.GetComponent<iconicElementScript>().icon_number);
                    simplicial.nodes.Add(child.GetComponent<EdgeElementScript>().edge_end.GetComponent<iconicElementScript>().icon_number);                    
                }

                graph.simplicials.Add(simplicial);
            }
        }
    }

    public void hyperedges_init()
    {
        graph.hyperedges = new List<HyperOrSimplicialEdge>();

        Transform Prev_hyper_parent = transform.GetChild(3);
        Transform[] allChildrenhyperedge = Prev_hyper_parent.GetComponentsInChildren<Transform>();

        foreach (Transform child in allChildrenhyperedge)
        {
            if (child.tag == "hyper")
            {
                HyperOrSimplicialEdge hyperedge = new HyperOrSimplicialEdge();
                hyperedge.nodes = new List<int>();

                foreach (GameObject node in child.GetComponent<HyperElementScript>().thenodes)
                    hyperedge.nodes.Add(node.GetComponent<iconicElementScript>().icon_number);

                graph.hyperedges.Add(hyperedge);
            }
        }        
    }

    // express the graph as string, so that we can pass to the python server
    public void Graph_init()
    {
        graph = new Graph();
        graph.nodes = new List<int>();

        Transform Prev_node_parent = transform.GetChild(0);
        Transform[] allChildrennode = Prev_node_parent.GetComponentsInChildren<Transform>();

        nodeMaps = new Dictionary<string, Transform>();
        int icon_count = 0;
        foreach (Transform child in allChildrennode)
        {
            if (child.tag == "iconic")
            {
                icon_count++;
                graph.nodes.Add(child.GetComponent<iconicElementScript>().icon_number);
                nodeMaps.Add(child.GetComponent<iconicElementScript>().icon_number.ToString(), child);
            }
        }

        if (icon_count == 0)
            Destroy(transform.gameObject);

        edges_init();
        simplicial_init();
        hyperedges_init();
    }

    public void GetGraphJson()
    {
        Graphs graphs = new Graphs();
        graphs.graphs = new List<Graph>();

        graphs.graphs.Add(graph);

        Debug.Log(JsonUtility.ToJson(graphs));
        File.WriteAllText("Assets/Resources/" + "data.json", JsonUtility.ToJson(graphs));
    }


    public void StartConversion(string target_layer)
    {
        
        // already at the target abstraction layer, so no change is needed
        if (abstraction_layer == target_layer)
            return;

        if (target_layer == "abstract")
        {
            // already label present
            if (transform.childCount > 4)
            {
                transform.GetChild(4).gameObject.SetActive(true);
                transform.GetChild(4).GetChild(0).GetComponent<TextMeshProUGUI>().text = graph_name;
            } 
            else
            {
                GameObject label = Instantiate(LabelElement, transform.GetChild(0).GetChild(0).GetComponent<iconicElementScript>().edge_position, Quaternion.identity, transform);
                label.transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = graph_name;
                label.transform.SetSiblingIndex(4);
            }
                
            abstraction_layer = target_layer;
            for (int i = 0; i < 4; i++)
            {
                transform.GetChild(i).gameObject.SetActive(false);
            }

            return;
        }

        if (abstraction_layer == "abstract")
        {
            // ToDo: need discussion
            // if a label present
            if (transform.childCount > 4)
            {
                transform.GetChild(4).gameObject.SetActive(false);
            }


            transform.GetChild(0).gameObject.SetActive(true);
            if (target_layer == "graph")
            {
                transform.GetChild(1).gameObject.SetActive(true);
            }
            else if(target_layer == "simplicial")
            {
                transform.GetChild(2).gameObject.SetActive(true);
            }
            else if(target_layer == "hypergraph")
            {
                transform.GetChild(3).gameObject.SetActive(true);
            }
            abstraction_layer = target_layer;
            return;
        }

        Debug.Log("abstraction_layer: " + abstraction_layer + ", target_layer:" + target_layer);

        //Graph_as_Str();
        Graph_init();
        GetGraphJson();

        bool flag = false;

        if (abstraction_layer == "graph")
        {
            flag = transform.GetComponent<HelloClient>().Abstraction_conversion(nodes_str + "-" + edges_str, abstraction_layer + "_to_" + target_layer);
        }
        else if (abstraction_layer == "simplicial")
        {
            flag = transform.GetComponent<HelloClient>().Abstraction_conversion(nodes_str + "-" + simplicial_str, abstraction_layer + "_to_" + target_layer);
        }
        else
        {
            flag = transform.GetComponent<HelloClient>().Abstraction_conversion(nodes_str + "-" + hyper_edges_str, abstraction_layer + "_to_" + target_layer);
        }

        // set the current layer
        if (flag)
            abstraction_layer = target_layer;
    }

    public void showconversion(string serverUpdate, string command)
    {
        // TodO: if an edge is already present, just SetActive instead of creating new, also update edge positions when icons are moving
        // as a work around, i have deleted when i am switching to another layer, will change when it is locked
        Debug.Log("triggered for " + command + " while current layer is: " + abstraction_layer);

        // check if the current command matches with the last conversion operation
        if ((command.Split('_'))[2] == abstraction_layer)
        {
            // string parsing, edges are separated by hyphen and each node of an edge is separated by a comma 
            string[] newedges = serverUpdate.Split('-');

            if ((command.Split('_'))[0] == "simplicial")
            {
                //DeleteChildren(transform.GetChild(2));
                transform.GetChild(2).gameObject.SetActive(false);

            }
            else if ((command.Split('_'))[0] == "hypergraph")
            {
                //DeleteChildren(transform.GetChild(3));
                transform.GetChild(3).gameObject.SetActive(false);
            }
            else if ((command.Split('_'))[0] == "graph")
            {
                //DeleteChildren(transform.GetChild(1));
                transform.GetChild(1).gameObject.SetActive(false);
            }

            // the follwings are basically copy of paintable script
            if (abstraction_layer == "graph")
            {
                transform.GetChild(1).gameObject.SetActive(true);
                if (graph_lock == false)
                {
                    DeleteChildren(transform.GetChild(1));
                }
                // force initial conversion
                else if (graph_drawn)
                {
                    return;
                }
                else if (graph_drawn == false)
                {
                    graph_drawn = true;
                }

                foreach (string edge in newedges)
                {
                    string[] nodes_of_edge = edge.Split(',');

                    // simplicial may have subsets of length 1, will skip those
                    if (nodes_of_edge.Length != 2)
                        continue;                                       

                    EdgeCreation("edge", nodes_of_edge, 1);
                }
            }

            else if (abstraction_layer == "simplicial")
            {
                transform.GetChild(2).gameObject.SetActive(true);

                if (simplicial_lock == false)
                {
                    DeleteChildren(transform.GetChild(2));
                }
                // force initial conversion
                else if (simplicial_drawn)
                {
                    return;
                }
                else if (simplicial_drawn == false)
                {
                    simplicial_drawn = true;
                }

                foreach (string edge in newedges)
                {
                    string[] nodes_of_edge = edge.Split(',');

                    // simplicial may have subsets of length 1, will skip those
                    if (nodes_of_edge.Length > 2)
                    {
                        SimplicialCreation(nodes_of_edge);
                    }                        
                    // if length 2, that is same as a normal graph edge 
                    else if (nodes_of_edge.Length == 2)
                    {
                        EdgeCreation("simplicial", nodes_of_edge, 2); 
                    }                    
                }
            }

            else if (abstraction_layer == "hypergraph")
            {
                transform.GetChild(3).gameObject.SetActive(true);

                if (hyper_edges_lock == false)
                {
                    DeleteChildren(transform.GetChild(3));
                }
                // force initial conversion
                else if (hyper_edges_drawn)
                {
                    return;
                }
                else if (hyper_edges_drawn == false)
                {
                    hyper_edges_drawn = true;
                }

                foreach (string edge in newedges)
                {
                    string[] nodes_of_edge = edge.Split(',');

                    // simplicial may have subsets of length 1, will skip those
                    if (nodes_of_edge.Length > 2)
                    {
                        HyperGraphCreation(nodes_of_edge);
                    }
                    // if length 2, that is same as a normal graph edge 
                    else if (nodes_of_edge.Length == 2)
                    {
                        HyperGraphCreation(nodes_of_edge); //EdgeCreation("hyper", nodes_of_edge, 3);
                    }
                }
            }

        }
    }

    void EdgeCreation(string tag, string[] nodes_of_edge, int idx)
    {
        List<GameObject> temp_nodes = new List<GameObject>();
        foreach (string node in nodes_of_edge)
        {
            if (nodeMaps.ContainsKey(node))
            {
                temp_nodes.Add(nodeMaps[node].gameObject);
            }
        }

        GameObject edgeline = Instantiate(EdgeElement, temp_nodes[0].GetComponent<iconicElementScript>().edge_position, Quaternion.identity, transform.GetChild(idx));
        edgeline.name = "edge_1";
        edgeline.tag = tag;

        edgeline.GetComponent<EdgeElementScript>().edge_start = temp_nodes[0];
        edgeline.GetComponent<EdgeElementScript>().edge_end = temp_nodes[1];

        edgeline.GetComponent<LineRenderer>().SetPosition(0, temp_nodes[0].GetComponent<iconicElementScript>().edge_position);
        edgeline.GetComponent<LineRenderer>().SetPosition(1, temp_nodes[1].GetComponent<iconicElementScript>().edge_position);

        var edgepoints = new List<Vector3>() { edgeline.GetComponent<LineRenderer>().GetPosition(0), edgeline.GetComponent<LineRenderer>().GetPosition(1) };

        edgeline.GetComponent<EdgeCollider2D>().points = edgepoints.Select(x =>
        {
            var pos = edgeline.GetComponent<EdgeCollider2D>().transform.InverseTransformPoint(x);
            return new Vector2(pos.x, pos.y);
        }).ToArray();

        edgeline.GetComponent<EdgeCollider2D>().edgeRadius = 10;

        // set line renderer texture scale
        var linedist = Vector3.Distance(edgeline.GetComponent<LineRenderer>().GetPosition(0), edgeline.GetComponent<LineRenderer>().GetPosition(1));
        edgeline.GetComponent<LineRenderer>().materials[0].mainTextureScale = new Vector2(linedist, 1);
        edgeline.GetComponent<EdgeElementScript>().addDot();
    }

    void SimplicialCreation(string[] nodes_of_edge)
    {
        GameObject simplicialline = Instantiate(SimplicialEdgeElement, transform.position, Quaternion.identity, transform.GetChild(2));
        simplicialline.name = "simplicial_1";
        simplicialline.tag = "simplicial";

        foreach (string node in nodes_of_edge)
        {
            if (nodeMaps.ContainsKey(node))
            {
                //Debug.Log(node + " : " + nodeMaps[node].name);
                simplicialline.GetComponent<SimplicialElementScript>().theVertices.Add(nodeMaps[node].GetComponent<iconicElementScript>().edge_position);
                simplicialline.GetComponent<SimplicialElementScript>().thenodes.Add(nodeMaps[node].gameObject);
            }
        }

        simplicialline.GetComponent<SimplicialElementScript>().updatePolygon();
    }

    void HyperGraphCreation(string[] nodes_of_edge)
    {
        GameObject hyperline = Instantiate(hyperEdgeElement, transform.position, Quaternion.identity, transform.GetChild(3));
        hyperline.name = "hyper_1";
        hyperline.tag = "hyper";
        Vector3 vec = new Vector3(0, 0, 0);
        foreach (string node in nodes_of_edge)
        {
            if (nodeMaps.ContainsKey(node))
            {
                //Debug.Log(node + " : " + nodeMaps[node].name);
                hyperline.GetComponent<HyperElementScript>().theVertices.Add(nodeMaps[node].GetComponent<iconicElementScript>().edge_position);
                hyperline.GetComponent<HyperElementScript>().thenodes.Add(nodeMaps[node].gameObject);

                vec.x += nodeMaps[node].GetComponent<iconicElementScript>().edge_position.x;
                vec.y += nodeMaps[node].GetComponent<iconicElementScript>().edge_position.y;
                vec.z += nodeMaps[node].GetComponent<iconicElementScript>().edge_position.z;
            }
        }
                
        vec.x /= hyperline.GetComponent<HyperElementScript>().theVertices.Count;
        vec.y /= hyperline.GetComponent<HyperElementScript>().theVertices.Count;
        vec.z /= hyperline.GetComponent<HyperElementScript>().theVertices.Count;
        hyperline.transform.position = vec;
        hyperline.GetComponent<HyperElementScript>().addChildren();
    }

    public void checkHitAndMove(Vector3 diff)
    {
        Transform[] allChildrennode = transform.GetChild(0).GetComponentsInChildren<Transform>();
        
        foreach (Transform child in allChildrennode)
        {
            if (child.tag == "iconic")
                child.GetComponent<iconicElementScript>().edge_position += diff;
        }

        Transform[] allChildrenedge = transform.GetChild(1).GetComponentsInChildren<Transform>();
        foreach (Transform child in allChildrenedge)
        {
            if (child.tag == "edge")
            {
                if (splined_edge_flag)
                    child.GetComponent<EdgeElementScript>().updateSplineEndPoint();
                else
                    child.GetComponent<EdgeElementScript>().updateEndPoint();
            }
                
        }

        /*transform.GetChild(2);
        transform.GetChild(3);*/

    }

}
