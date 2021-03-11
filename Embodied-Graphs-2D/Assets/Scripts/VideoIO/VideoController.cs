﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using UnityEngine.Video;
using System.IO;

public class VideoController : MonoBehaviour, IDragHandler, IPointerDownHandler
{
    [SerializeField]
    private VideoPlayer videoplayer;
    public Slider mainSlider;

    frames frames_annotation;
    public float width, height;    
    public Vector3[] vec;
    long prev_frame;

    public GameObject icon_prefab;
    public GameObject edge_prefab;
    public GameObject graph_prefab;
    public GameObject paintable;
    public GameObject graph_holder;

    public float node_radius;
    public string graph_type;
    public bool auto_track;

    // Start is called before the first frame update
    void Start()
    {
        graph_holder = null;
        node_radius = 20f;
    }

    public void loadAnnotation(string filename)
    {
        Debug.Log("filename:" + filename);
        frames_annotation = JsonUtility.FromJson<frames>(File.ReadAllText(filename));
        Debug.Log(JsonUtility.ToJson(frames_annotation.all_frame[0].objects[0]));

        width = videoplayer.transform.localScale.x;
        height = videoplayer.transform.localScale.y;

        //temp_parent = Instantiate(graph_prefab);
    }

    // Update is called once per frame
    void Update()
    {
        if (videoplayer.frameCount > 0)
        {
            mainSlider.value = (float)videoplayer.frame / (float)videoplayer.frameCount;
        }

        if (videoplayer.frame %5 == 0 && prev_frame!= videoplayer.frame)
        {
            prev_frame = videoplayer.frame;
            Debug.Log("current_frame: " + videoplayer.frame.ToString());
            if (graph_holder != null)
            {
                /*Destroy(temp_parent);
                temp_parent = null;*/

                GameObject nodepar = graph_holder.transform.GetChild(0).gameObject;
                GameObject edgepar = graph_holder.transform.GetChild(1).gameObject;
                GameObject simplicialpar = graph_holder.transform.GetChild(2).gameObject;
                GameObject hyperpar = graph_holder.transform.GetChild(3).gameObject;

                Destroy(nodepar);
                Destroy(edgepar);
                Destroy(simplicialpar);
                Destroy(hyperpar);

                GameObject tempnodeparent = new GameObject("node_parent_1");
                tempnodeparent.tag = "node_parent";
                tempnodeparent.transform.parent = graph_holder.transform;
                tempnodeparent.transform.SetSiblingIndex(0);

                GameObject tempedgeparent = new GameObject("edge_parent_1");
                tempedgeparent.tag = "edge_parent";
                tempedgeparent.transform.parent = graph_holder.transform;
                tempedgeparent.transform.SetSiblingIndex(1);

                GameObject tempsimplicialparent = new GameObject("simplicial_parent_1");
                tempsimplicialparent.tag = "simplicial_parent";
                tempsimplicialparent.transform.parent = graph_holder.transform;
                tempsimplicialparent.transform.SetSiblingIndex(2);

                GameObject temphyperparent = new GameObject("hyper_parent_1");
                temphyperparent.tag = "hyper_parent";
                temphyperparent.transform.parent = graph_holder.transform;
                temphyperparent.transform.SetSiblingIndex(3);
            }
            else
            {
                graph_holder = Instantiate(graph_prefab);
                graph_holder.GetComponent<GraphElementScript>().abstraction_layer = "graph";
                graph_holder.GetComponent<GraphElementScript>().paintable = paintable;
                paintable.GetComponent<Paintable>().graph_count++;
                graph_holder.name = "graph_" + paintable.GetComponent<Paintable>().graph_count.ToString();
                graph_holder.GetComponent<GraphElementScript>().graph_name = "G" + paintable.GetComponent<Paintable>().graph_count.ToString();
            }

            //temp_parent = Instantiate(graph_prefab);            

            List<GameObject> all_icons = new List<GameObject>();
            List<tracked_object> all_objects = frames_annotation.all_frame[(int)videoplayer.frame].objects;
            // because, we do not want to increase total icon numbers in each frame, which will be ambigious
            int num = paintable.GetComponent<Paintable>().totalLines;
            List<Vector3> graph_points = new List<Vector3>();

            foreach (tracked_object cur_obj in all_objects)
            {
                Vector3 edge_pos = Vector3.zero;                
                GameObject temp = Instantiate(icon_prefab, Vector3.zero, Quaternion.identity, graph_holder.transform.GetChild(0));

                num++;                
                temp.tag = "iconic";
                temp.name = "iconic_" + num.ToString();
                temp.GetComponent<iconicElementScript>().icon_number = num;
                temp.GetComponent<iconicElementScript>().video_icon = true;
                all_icons.Add(temp);

                temp.GetComponent<TrailRenderer>().enabled = false;
                temp.GetComponent<MeshRenderer>().enabled = false;
                
                List<Vector3> points = new List<Vector3>();
                
                foreach (bounds first_obj in cur_obj.bounds)
                {
                    float lerped_x = Mathf.Lerp(videoplayer.transform.position.x - (width / 2), videoplayer.transform.position.x + (width / 2), Mathf.InverseLerp(1, 853, first_obj.x));
                    float lerped_y = Mathf.Lerp(videoplayer.transform.position.y + (height / 2), videoplayer.transform.position.y - (height / 2), Mathf.InverseLerp(1, 480, first_obj.y));

                    Vector3 pos_vec = new Vector3(lerped_x, lerped_y, -5f);
                    edge_pos += pos_vec;

                    points.Add(pos_vec);
                }


                // connect the end and statr position as well
                /*LineRenderer l = temp.GetComponent<LineRenderer>();
                l.material.color = Color.black;
                l.startWidth = 2f;
                l.endWidth = 2f;
                l.loop = true;
                // if we don't manually change the position count, it only takes the first two positions
                l.positionCount = points.Count;
                l.SetPositions(points.ToArray());*/

                edge_pos = edge_pos / points.Count;
                temp.GetComponent<iconicElementScript>().edge_position = edge_pos;
                temp.GetComponent<iconicElementScript>().bounds_center = edge_pos;

                // to save compute resource, we store all points in graph instead
                temp.GetComponent<iconicElementScript>().points = points;
                graph_points.AddRange(points);
                
                temp.GetComponent<iconicElementScript>().centroid = edge_pos;

                // again we optimize by approxmating the radius calculation
                temp.GetComponent<iconicElementScript>().radius = Vector3.Distance(points[0], points[1]) / 2;
                float rad = temp.GetComponent<iconicElementScript>().radius;
                Vector3 size = new Vector3(rad, rad, rad);

                BoxCollider box_cl = temp.AddComponent<BoxCollider>();
                box_cl.center = edge_pos;//Vector3.zero;
                box_cl.size = size;
            }

            //videoplayer.Pause();
            Debug.Log("all_icons: " + all_icons.Count.ToString());

            graph_holder.GetComponent<GraphElementScript>().points = graph_points;

            // create graph based on node radius
            // tODo: try updated algorithm
            for (int i = 0; i < all_icons.Count; i++ )
            {
                for (int j = (i+1); j < all_icons.Count; j++)
                {
                    if (Vector3.Distance(all_icons[i].GetComponent<iconicElementScript>().edge_position, 
                        all_icons[j].GetComponent<iconicElementScript>().edge_position) < node_radius)
                    {
                        GameObject temp = Instantiate(edge_prefab, Vector3.zero, Quaternion.identity, graph_holder.transform.GetChild(1));

                        temp.tag = "edge";
                        temp.GetComponent<EdgeElementScript>().edge_start = all_icons[i];
                        temp.GetComponent<EdgeElementScript>().edge_end = all_icons[j];

                        //temp.GetComponent<EdgeElementScript>().addDot();
                        //temp.GetComponent<EdgeElementScript>().updateEndPoint();
                        temp.GetComponent<EdgeElementScript>().addEndPoint(true);
                    }
                }
            }

            graph_holder.GetComponent<GraphElementScript>().Graph_init();
        }
    }
       

    public void OnSliderValueChanged(PointerEventData eventData)
    {
        Vector2 localpoint;
        if (RectTransformUtility.ScreenPointToLocalPointInRectangle
            (mainSlider.GetComponent<RectTransform>(), eventData.position, null, out localpoint))
        {
            float value = Mathf.InverseLerp(mainSlider.GetComponent<RectTransform>().rect.xMin, mainSlider.GetComponent<RectTransform>().rect.xMax, localpoint.x) ;
            var frame = videoplayer.frameCount * value;
            videoplayer.frame = (long)frame;
            mainSlider.value = (float)value;
        }
        
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        OnSliderValueChanged(eventData);
        //throw new System.NotImplementedException();
    }

    public void OnDrag(PointerEventData eventData)
    {
        OnSliderValueChanged(eventData);
        //throw new System.NotImplementedException();
    }
}
