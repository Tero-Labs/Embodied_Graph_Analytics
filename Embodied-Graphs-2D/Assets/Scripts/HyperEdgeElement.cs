using System.Collections;
using System.Collections.Generic;
using System;
using System.Linq;
using UnityEngine;

using UnityEngine.UI;
using UnityEngine.InputSystem;

public class HyperEdgeElement : MonoBehaviour
{
    public GameObject parent_node;
    
    private void Awake()
    {
       
    }


    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }

    public void UpdateEndpoints(Vector3 start, Vector3 end)
    {
        LineRenderer l = transform.GetComponent<LineRenderer>();
        l.material.color = Color.black;
        l.startWidth = 2f;
        l.endWidth = 2f;

        // set up the line renderer
        l.positionCount = 2;
        l.SetPosition(0, start);
        l.SetPosition(1, end);

        // set line renderer texture scale
        var linedist = Vector3.Distance(l.GetPosition(0), l.GetPosition(1));
        l.materials[0].mainTextureScale = new Vector2(linedist, 1);
    }

    public void UpdateSingleEndpoint(Vector3 start)
    {
        transform.position = start;
        LineRenderer l = transform.GetComponent<LineRenderer>();
        l.material.color = Color.black;
        l.startWidth = 2f;
        l.endWidth = 2f;

        // set up the line renderer
        l.positionCount = 2;
        l.SetPosition(0, start);

        // set line renderer texture scale
        var linedist = Vector3.Distance(l.GetPosition(0), l.GetPosition(1));
        l.materials[0].mainTextureScale = new Vector2(linedist, 1);
    }

    void OnDestroy()
    {
        // destroy the whole parent as well
        Destroy(transform.parent.gameObject);
    }
}