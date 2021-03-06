using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HyperElementScript : MonoBehaviour
{
    public List<Vector3> theVertices;
    public List<GameObject> thenodes;
    public GameObject edge;
    public GameObject paintable;
    public Sprite dot_sprite;

    // Start is called before the first frame update
    void Start()
    {
        //transform.GetComponent<BoxCollider>().size = transform.GetComponent<SpriteRenderer>().size;
    }

    public void addChildren()
    {
        for (int i = 0; i < theVertices.Count; i++)
        {
            GameObject temp = Instantiate(edge, theVertices[i], Quaternion.identity, transform);
            temp.name = "hyper_child_edge_" + i.ToString();
            temp.tag = "hyper_child_edge";
            temp.transform.parent = transform;
            // to track the connected node
            temp.GetComponent<HyperEdgeElement>().parent_node = thenodes[i];
            temp.GetComponent<HyperEdgeElement>().UpdateEndpoints(theVertices[i], transform.position);
            // set as the i-th child,  so that it is easy to track
            temp.transform.SetSiblingIndex(i);
            // show the black dot at the end
            SpriteRenderer sr = temp.AddComponent<SpriteRenderer>();
            sr.material.SetColor("_Color", paintable.GetComponent<Paintable>().color_picker_script.color);
            sr.sprite = dot_sprite;
        }
    }

    public void UpdateChildren()
    {
        for (int i = 0; i < theVertices.Count; i++)
        {
            GameObject temp = transform.GetChild(i).gameObject;
            temp.transform.position = thenodes[i].GetComponent<iconicElementScript>().edge_position;
            temp.GetComponent<HyperEdgeElement>().UpdateEndpoints(thenodes[i].GetComponent<iconicElementScript>().edge_position, transform.position);
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void OnDestroy()
    {
        Transform node_parent = transform.parent;
        if (node_parent.tag == "hyper_parent")
        {
            node_parent.parent.GetComponent<GraphElementScript>().hyperedges_init();
            //node_parent.parent.GetComponent<GraphElementScript>().hyperedges_as_Str();
        }
    }
}
