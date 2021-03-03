using System.Collections;
using System.Collections.Generic;
using System;
using System.Linq;
using UnityEngine;
using BezierSolution;
using UnityEngine.UI;
using UnityEngine.InputSystem;

public class HyperEdgeElement : MonoBehaviour
{
    public GameObject parent_node;
    public int spline_flag;
    
    private void Awake()
    {
       
    }


    // Start is called before the first frame update
    void Start()
    {
        float temp = UnityEngine.Random.Range(1f, 2f);
        print("rand:" + temp.ToString());
        if (temp > 1.5f)
            spline_flag = 1;            
        else
            spline_flag = 2;
        //spline_flag = Mathf.RoundToInt(UnityEngine.Random.Range(1f, 2f));
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

        GameObject spline = new GameObject("spline");
        spline.AddComponent<BezierSpline>();
        BezierSpline bs = spline.transform.GetComponent<BezierSpline>();

        // free handle mode and set the control point so that we get a eclipse like shape even with only two points! 
        bs.Initialize(2);
        bs[0].position = start;
        bs[0].handleMode = BezierPoint.HandleMode.Free;

        float temp_x = (start.x + end.x) / 2;
        float temp_y = (start.y + end.y) / 2;

        if (spline_flag == 1)
        {
            temp_y = temp_y - Math.Abs(start.x - end.x) / 3;
        }
        else
        {
            temp_y = temp_y + Math.Abs(start.x - end.x) / 3;
        }        

        bs[0].followingControlPointPosition = new Vector3(temp_x, temp_y, start.z);

        bs[1].position = end;
        bs[1].handleMode = BezierPoint.HandleMode.Free;
        bs[1].precedingControlPointPosition = new Vector3(temp_x, temp_y, start.z);


        List<Vector3>  recorded_path = new List<Vector3>(10);
        for (int i = 0; i < 10; i++)
        {
            recorded_path.Add(bs.GetPoint(Mathf.InverseLerp(0, 9, i)));
        }

        Destroy(spline);

        Debug.Log("my_spline:" + recorded_path.Count.ToString());

        l.positionCount = recorded_path.Count;
        l.SetPositions(recorded_path.ToArray());

    }

    public void UpdateSingleEndpoint(Vector3 start)
    {
        transform.position = start;
        UpdateEndpoints(start, transform.parent.position);
    }



    public void UpdateEndpointsOld(Vector3 start, Vector3 end)
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

    public void UpdateSingleEndpointOld(Vector3 start)
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