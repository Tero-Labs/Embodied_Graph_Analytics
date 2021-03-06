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

        LineRenderer l = transform.GetComponent<LineRenderer>();
        l.material.SetColor("_Color", transform.parent.GetComponent<HyperElementScript>().paintable.GetComponent<Paintable>().color_picker_script.color);

        l.startWidth = 1f;
        l.endWidth = 1f;
    }

    // Update is called once per frame
    void Update()
    {

    }

    // start: node. end: hypernode
    public void UpdateEndpoints(Vector3 start, Vector3 end)
    {
        start = parent_node.GetComponent<iconicElementScript>().edge_position;//.getclosestpoint(end);
        transform.position = start;

        Vector3 dir_vec = start - end;
        Vector2 unit_vec = new Vector2(-dir_vec.y, dir_vec.x);
        Debug.Log("before normalized:" + unit_vec.ToString());
        unit_vec.Normalize();
        Debug.Log("after normalized:" + unit_vec.ToString());

        Vector3 first_cpt = Vector3.Lerp(start, end, 0.3f);
        Vector3 second_pt = Vector3.Lerp(start, end, 0.6f);
        Vector3 third_pt = Vector3.Lerp(start, end, 0.75f);
        Vector3 fourth_pt = Vector3.Lerp(start, end, 0.9f);
        
        float approx_dist;

        approx_dist = Vector3.Distance(start, end) / UnityEngine.Random.Range(2f,4f);//3;

        if (spline_flag == 1)
        {
            Vector2 temp_vec = new Vector2(first_cpt.x, first_cpt.y) - (approx_dist * unit_vec);
            first_cpt = new Vector3(temp_vec.x, temp_vec.y, first_cpt.z);

            temp_vec = new Vector2(third_pt.x, third_pt.y) + ((approx_dist / 2) * unit_vec);
            third_pt = new Vector3(temp_vec.x, temp_vec.y, third_pt.z);
        }
        else
        {

            Vector2 temp_vec = new Vector2(first_cpt.x, first_cpt.y) + (approx_dist * unit_vec);
            first_cpt = new Vector3(temp_vec.x, temp_vec.y, first_cpt.z);

            temp_vec = new Vector2(third_pt.x, third_pt.y) - ((approx_dist / 2) * unit_vec);
            third_pt = new Vector3(temp_vec.x, temp_vec.y, third_pt.z);
        }


        LineRenderer l = transform.GetComponent<LineRenderer>();        

        # region spline

        GameObject spline = new GameObject("spline");
        spline.AddComponent<BezierSpline>();
        BezierSpline bs = spline.transform.GetComponent<BezierSpline>();

        // free handle mode and set the control point so that we get a eclipse like shape even with only two points! 
        bs.Initialize(4);
        bs[0].position = start;
        bs[0].handleMode = BezierPoint.HandleMode.Free;
        bs[0].followingControlPointPosition = first_cpt;

        bs[1].position = second_pt;
        bs[1].handleMode = BezierPoint.HandleMode.Free;
        bs[1].precedingControlPointPosition = first_cpt;
        bs[1].followingControlPointPosition = third_pt;

        bs[2].position = fourth_pt;
        bs[2].handleMode = BezierPoint.HandleMode.Free;
        bs[2].precedingControlPointPosition = third_pt;
        bs[2].followingControlPointPosition = end;

        bs[3].position = end;
        bs[3].handleMode = BezierPoint.HandleMode.Free;
        bs[3].precedingControlPointPosition = fourth_pt;

        int pts = 100;
        List<Vector3>  recorded_path = new List<Vector3>(pts);
        for (int i = 0; i < pts; i++)
        {
            recorded_path.Add(bs.GetPoint(Mathf.InverseLerp(0, pts-1, i)));
        }

        Destroy(spline);

        # endregion

        Debug.Log("my_spline:" + recorded_path.Count.ToString());

        l.positionCount = recorded_path.Count;
        l.SetPositions(recorded_path.ToArray());

    }

    public void UpdateSingleEndpoint(Vector3 start)
    {
        UpdateEndpoints(start, transform.parent.position);
    }

    public void UpdateEndpointsOld(Vector3 start, Vector3 end)
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


        List<Vector3> recorded_path = new List<Vector3>(10);
        for (int i = 0; i < 10; i++)
        {
            recorded_path.Add(bs.GetPoint(Mathf.InverseLerp(0, 9, i)));
        }

        Destroy(spline);

        Debug.Log("my_spline:" + recorded_path.Count.ToString());

        l.positionCount = recorded_path.Count;
        l.SetPositions(recorded_path.ToArray());

    }

    public void UpdateSingleEndpointOld(Vector3 start)
    {
        transform.position = start;
        UpdateEndpointsOld(start, transform.parent.position);
    }


    void OnDestroy()
    {
        // destroy the whole parent as well
        Destroy(transform.parent.gameObject);
    }
}