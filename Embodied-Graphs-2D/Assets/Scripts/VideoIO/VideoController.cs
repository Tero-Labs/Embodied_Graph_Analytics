using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using UnityEngine.Video;
using System.IO;
using System;
using TMPro;
using OpenCVForUnity.CoreModule;
using OpenCVForUnity.Features2dModule;
using OpenCVForUnity.UnityUtils;
using OpenCVForUnity.ImgprocModule;

public class VideoController : MonoBehaviour, IDragHandler, IPointerDownHandler
{
    [SerializeField]
    private VideoPlayer videoplayer;
    public Slider mainSlider;

    frames frames_annotation;
    all_route All_Route;

    public float width, height;    
    public int frequency;    
    public Vector3[] vec;
    long prev_frame;

    public GameObject icon_prefab;
    public GameObject image_icon_prefab;
    public GameObject edge_prefab;
    public GameObject graph_prefab;
    public GameObject paintable;
    public GameObject graph_holder;

    public Toggle none;
    // visual Variable UI
    public TMP_Dropdown visual_var_type;
    public InputField max_limit, min_limit;
    public TMP_InputField contour_obj_count;
    public FlexibleColorPicker color_picker_script;
    public Slider blobsizeSlider;

    public List<GameObject> all_icons;

    public float node_radius_val;
    public string graph_type;
    public bool auto_track;
    public bool copy_graph;

    // CV related variables
    Texture2D cur_texture;
    public int visual_var_val;
    public float min_visual_var, max_visual_var, contour_size;
    public int contour_cnt;

    // Start is called before the first frame update
    void Start()
    {
        graph_holder = null;
        node_radius_val = 20f;
        frequency = 1;

        visual_var_val = visual_var_type.value; 
        min_visual_var = 0f;
        max_visual_var = 60f;

        copy_graph = false;

        // visual variable setup
        visual_var_type.onValueChanged.AddListener(delegate { ChangeVisualVariable(visual_var_type); });
        max_limit.onValueChanged.AddListener(delegate { TrackVisualInputField(max_limit); });
        min_limit.onValueChanged.AddListener(delegate { TrackVisualInputField(min_limit); });
        contour_obj_count.onValueChanged.AddListener(delegate { TrackVisualInputField(contour_obj_count); });
        blobsizeSlider.onValueChanged.AddListener(delegate { SliderValueChanged(blobsizeSlider); });
    }

    void ChangeVisualVariable(TMP_Dropdown dropdown)
    {
        visual_var_val = dropdown.value;
        GraphCreation();
    }

    void TrackVisualInputField(InputField inputField)
    {
        Paintable.click_on_inputfield = true;

        if (inputField.name == "minInputField" && inputField.text.Length > 0)
        {
            float.TryParse(inputField.text, out min_visual_var);
            GraphCreation();
        }

        if (inputField.name == "maxInputField" && inputField.text.Length > 0)
        {
            float.TryParse(inputField.text, out max_visual_var);
            GraphCreation();
        }

        if (inputField.name == "contourInputField" && inputField.text.Length > 0)
        {
            int.TryParse(inputField.text, out contour_cnt);
            GraphCreation();
        }
    }

    void TrackVisualInputField(TMP_InputField inputField)
    {
        Paintable.click_on_inputfield = true;
        
        if (inputField.name == "contourInputField" && inputField.text.Length > 0)
        {
            int.TryParse(inputField.text, out contour_cnt);
            GraphCreation();
        }
    }
    
    void SliderValueChanged(Slider slider)
    {
        contour_size = slider.value;
        GraphCreation();
    }

    public void loadAnnotation(string filename)
    {
        Debug.Log("filename:" + filename);
        try
        {
            frames_annotation = JsonUtility.FromJson<frames>(File.ReadAllText(filename));
        }
        catch(Exception exc)
        {
            Debug.Log("It needs labelling by CV!");
            gameObject.AddComponent<ContourandRotatedRectDetection>();            
        }
        

        width = videoplayer.transform.localScale.x;
        height = videoplayer.transform.localScale.y;

        //temp_parent = Instantiate(graph_prefab);
    }

    public void loadRoutes(string filename)
    {
        Debug.Log("filename:" + filename);
        All_Route = JsonUtility.FromJson<all_route>(File.ReadAllText(filename));

    }

    // Update is called once per frame
    void Update()
    {        

        if (videoplayer.frameCount > 0)
        {
            mainSlider.value = (float)videoplayer.frame / (float)videoplayer.frameCount;
        }

        if (prev_frame!= videoplayer.frame && videoplayer.frame > 0)
        {
            /*if (videoplayer.frame % frequency == 0 || videoplayer.frame == 5)
            {*/
            prev_frame = videoplayer.frame;
            //Debug.Log("current_frame: " + videoplayer.frame.ToString());

            GraphCreation();

            if (videoplayer.frame % VideoPlayerChildrenAccess.time_slice == 0)
                graph_holder.GetComponent<GraphElementScript>().RequestRecalculationonValueChange(videoplayer.transform.gameObject);
                        
        }
    }

    public void Copy()
    {
        Vector3 target_pos = new Vector3(width, 0f, 0f);
        GameObject cp = Instantiate(graph_holder, graph_holder.transform.position + target_pos, Quaternion.identity, graph_holder.transform.parent.transform);
        cp.GetComponent<GraphElementScript>().video_graph = false;
        cp.GetComponent<GraphElementScript>().checkHitAndMove(target_pos);
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

            // when the seeker is updated, reconstruct the frame once again
            if (graph_holder != null) Destroy(graph_holder);
            GraphCreation();
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

    //  DumpRenderTexture(videoplayer.targetTexture, "Assets/Resources/" + "dump.png");
    //  https://gist.github.com/AlexanderDzhoganov/d795b897005389071e2a
    public Texture2D DumpRenderTexture(/*RenderTexture rt, string pngOutPath*/)
    {
        RenderTexture rt = videoplayer.targetTexture;
        var oldRT = RenderTexture.active;
        var tex = new Texture2D(rt.width, rt.height);        

        RenderTexture.active = rt;
        tex.ReadPixels(new UnityEngine.Rect(0, 0, rt.width, rt.height), 0, 0);
        tex.Apply();               

        //File.WriteAllBytes("Assets/Resources/" + "dump.png", tex_small.EncodeToPNG());
        RenderTexture.active = oldRT;

        //Destroy(tex);
        return tex;
    }

    public Texture2D CropRenderTexture(Texture2D InputTex, int bottom_x, int bottom_y, int width, int height)
    {
        Debug.Log(" bottom_x: " + bottom_x.ToString() + " bottom_y: " + bottom_y.ToString() + " width: " + width.ToString() + " height: " + height.ToString());
        var tex_small = new Texture2D((int)width, (int)height);
        tex_small.SetPixels(InputTex.GetPixels((int)bottom_x, (int)bottom_y, (int)width, (int)height));
        tex_small.Apply();
        return tex_small;
    }

    public void GraphCreation()
    {
        if (graph_holder != null)
        {
            /*Destroy(temp_parent);
            temp_parent = null;*/

            // when the nodes are moving we destroy all the nodes, edges and redraw them in each frame
            // or if the video is being tracked by openCV
            if ((frames_annotation == null && (videoplayer.frame % frequency == 0 || videoplayer.frame == 5)) || (frames_annotation != null && frames_annotation.node_type != "static"))
            {
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

        }

        else
        {
            graph_holder = Instantiate(graph_prefab);
            graph_holder.transform.parent = paintable.GetComponent<Paintable>().Objects_parent.transform;
            graph_holder.GetComponent<GraphElementScript>().video_graph = true;
            graph_holder.GetComponent<GraphElementScript>().abstraction_layer = "graph";
            graph_holder.GetComponent<GraphElementScript>().paintable = paintable;
            Paintable.graph_count++;
            graph_holder.name = "graph_" + Paintable.graph_count.ToString();
            graph_holder.GetComponent<GraphElementScript>().graph_name = "G" + Paintable.graph_count.ToString();
        }

        graph_holder.SetActive(true);

        if (graph_holder.transform.GetChild(0).childCount == 0)
        {

            all_icons = new List<GameObject>();
            List<tracked_object> all_objects = new List<tracked_object>();
            List<RotatedRect> all_rects = new List<RotatedRect>();

            if (frames_annotation != null)
            {
                if (frames_annotation.node_type != "static")
                    all_objects = frames_annotation.all_frame[(int)videoplayer.frame].objects;
                else
                    all_objects = frames_annotation.all_frame[0].objects;
            }
            else if (videoplayer.frame % frequency == 0 || videoplayer.frame == 5)
            {
                //if (cur_texture == null)
                cur_texture = DumpRenderTexture(/*"Assets/Resources/dump" + videoplayer.frame.ToString() + ".png"*/);
                all_rects = transform.GetComponent<ContourandRotatedRectDetection>().FindResultFromVideoTexture(cur_texture, copy_graph: copy_graph);
                // instead of creating different conditions for visual variables, I treated them similarly as proximity graph
                if (none.isOn) node_radius_val = float.MaxValue;
            }


            // because, we do not want to increase total icon numbers in each frame, which will be ambigious
            int num = Paintable.totalLines;
            List<Vector3> graph_points = new List<Vector3>();

            graph_holder.GetComponent<GraphElementScript>().nodeMaps = new Dictionary<string, Transform>();
            graph_holder.GetComponent<GraphElementScript>().graph = new Graph();
            graph_holder.GetComponent<GraphElementScript>().graph.nodes = new List<int>();

            if (frames_annotation != null)
            {
                foreach (tracked_object cur_obj in all_objects)
                {
                    Vector3 edge_pos = Vector3.zero;
                    GameObject temp = Instantiate(icon_prefab, Vector3.zero, Quaternion.identity, graph_holder.transform.GetChild(0));

                    num++;
                    temp.tag = "iconic";
                    temp.name = "iconic_" + /*num*/cur_obj.name; // setting the id with labelled value so that we can track over time 
                    temp.GetComponent<iconicElementScript>().icon_number = /*num*/cur_obj.id;
                    temp.GetComponent<iconicElementScript>().video_icon = true;
                    all_icons.Add(temp);

                    graph_holder.GetComponent<GraphElementScript>().graph.nodes.Add(/*num*/cur_obj.id);
                    graph_holder.GetComponent<GraphElementScript>().nodeMaps.Add(/*num*/cur_obj.id.ToString(), temp.transform);

                    temp.GetComponent<TrailRenderer>().enabled = false;
                    temp.GetComponent<MeshRenderer>().enabled = false;

                    List<Vector3> points = new List<Vector3>();

                    foreach (bounds first_obj in cur_obj.bounds)
                    {
                        float lerped_x = Mathf.Lerp(videoplayer.transform.position.x - (width / 2), videoplayer.transform.position.x + (width / 2), Mathf.InverseLerp(1, frames_annotation.width, first_obj.x));
                        float lerped_y = Mathf.Lerp(videoplayer.transform.position.y + (height / 2), videoplayer.transform.position.y - (height / 2), Mathf.InverseLerp(1, frames_annotation.height, first_obj.y));

                        Vector3 pos_vec = new Vector3(lerped_x, lerped_y, -40f);
                        edge_pos += pos_vec;

                        points.Add(pos_vec);
                    }

                    /*float bound_box_avg_size = (Vector3.Distance(points[0], points[1]) + Vector3.Distance(points[1], points[2])) / 2;
                    temp.GetComponent<iconicElementScript>().visual_variable = bound_box_avg_size;*/
                    //Debug.Log("size: " + bound_box_avg_size.ToString());


                    // connect the end and statr position as well
                    /*LineRenderer l = temp.GetComponent<LineRenderer>();
                    l.material.color = Color.black;
                    l.startWidth = 2f;
                    l.endWidth = 2f;
                    l.loop = true;
                    // if we don't manually change the position count, it only takes the first two positions
                    l.positionCount = points.Count;
                    l.SetPositions(points.ToArray());*/

                    // getting the center of the bounding box
                    edge_pos = edge_pos / points.Count;
                    temp.GetComponent<iconicElementScript>().edge_position = edge_pos;
                    temp.GetComponent<iconicElementScript>().bounds_center = edge_pos;

                    // to save compute resource, we store all points in graph instead
                    temp.GetComponent<iconicElementScript>().points = points;
                    // will be used for community detection
                    graph_points.AddRange(points);

                    temp.GetComponent<iconicElementScript>().centroid = edge_pos;

                    // again we optimize by approxmating the radius calculation
                    if (points.Count > 1)
                        temp.GetComponent<iconicElementScript>().radius = Vector3.Distance(points[0], points[1]) / 2;
                    else
                        temp.GetComponent<iconicElementScript>().radius = 20f;

                    float rad = temp.GetComponent<iconicElementScript>().radius;
                    Vector3 size = new Vector3(rad, rad, rad);

                    // size
                    if (visual_var_val == 1)
                        temp.GetComponent<iconicElementScript>().visual_variable = rad;
                    // angle
                    else if (visual_var_val == 2)
                        temp.GetComponent<iconicElementScript>().visual_variable = 0f;

                    BoxCollider box_cl = temp.AddComponent<BoxCollider>();
                    box_cl.center = edge_pos;//Vector3.zero;
                    box_cl.size = size * 10;
                }
            }
            else if (videoplayer.frame % frequency == 0 || videoplayer.frame == 5)
            {
                int cur_rect_iter = 0;
                foreach (RotatedRect cur_obj in all_rects)
                {
                    Vector3 edge_pos = Vector3.zero;
                    GameObject temp = Instantiate(icon_prefab, Vector3.zero, Quaternion.identity, graph_holder.transform.GetChild(0));

                    num++;
                    temp.tag = "iconic";
                    temp.GetComponent<iconicElementScript>().icon_number = num;
                    temp.GetComponent<iconicElementScript>().video_icon = true;


                    graph_holder.GetComponent<GraphElementScript>().graph.nodes.Add(num);
                    graph_holder.GetComponent<GraphElementScript>().nodeMaps.Add(num.ToString(), temp.transform);

                    try
                    {
                        temp.GetComponent<TrailRenderer>().enabled = false;
                        temp.GetComponent<MeshRenderer>().enabled = false;
                    }
                    catch (Exception exc)
                    {
                        //Debug.Log("no renderer attached");
                    }

                    List<Vector3> points = new List<Vector3>();

                    float lerped_x = Mathf.Lerp(videoplayer.transform.position.x - (width / 2), videoplayer.transform.position.x + (width / 2), Mathf.InverseLerp(1, cur_texture.width, (float)cur_obj.center.x));
                    float lerped_y = Mathf.Lerp(videoplayer.transform.position.y + (height / 2), videoplayer.transform.position.y - (height / 2), Mathf.InverseLerp(1, cur_texture.height, (float)cur_obj.center.y));

                    Vector3 pos_vec = new Vector3(lerped_x, lerped_y, -40f);
                    edge_pos += pos_vec;
                    points.Add(pos_vec);

                    /*temp.GetComponent<iconicElementScript>().image_icon = true;
                    int bottom_x, bottom_y, rect_width, rect_height;
                    bottom_x = transform.GetComponent<ContourandRotatedRectDetection>().all_horizontal_rects[cur_rect_iter].x;
                    bottom_y = transform.GetComponent<ContourandRotatedRectDetection>().all_horizontal_rects[cur_rect_iter].y; 
                    rect_width = transform.GetComponent<ContourandRotatedRectDetection>().all_horizontal_rects[cur_rect_iter].width;
                    rect_height = transform.GetComponent<ContourandRotatedRectDetection>().all_horizontal_rects[cur_rect_iter].height;

                    lerped_x = Mathf.Lerp(videoplayer.transform.position.x - (width / 2), videoplayer.transform.position.x + (width / 2), 
                        Mathf.InverseLerp(1, cur_texture.width, bottom_x));
                    lerped_y = Mathf.Lerp(videoplayer.transform.position.y + (height / 2), videoplayer.transform.position.y - (height / 2), 
                        Mathf.InverseLerp(1, cur_texture.height, bottom_y + rect_height));                                               

                    Texture2D target_Tex = CropRenderTexture(cur_texture, bottom_x, bottom_y, rect_width, rect_height);

                    temp.GetComponent<iconicElementScript>().LoadNewSprite("FilePath", input_texture: target_Tex);
                    // ToDo: not correctly positioned
                    temp.transform.position = new Vector3(lerped_x, lerped_y, -40f); //edge_pos; 
                    */

                    all_icons.Add(temp);

                    // size
                    if (visual_var_val == 1)
                        temp.GetComponent<iconicElementScript>().visual_variable = ((float)cur_obj.size.width + (float)cur_obj.size.height) / 2;
                    // angle
                    else if (visual_var_val == 2)
                        temp.GetComponent<iconicElementScript>().visual_variable = (float)cur_obj.angle;

                    // getting the center of the bounding box
                    edge_pos = edge_pos / points.Count;
                    temp.GetComponent<iconicElementScript>().edge_position = edge_pos;
                    temp.GetComponent<iconicElementScript>().bounds_center = edge_pos;

                    // to save compute resource, we store all points in graph instead
                    temp.GetComponent<iconicElementScript>().points = points;
                    // will be used for community detection
                    graph_points.AddRange(points);

                    temp.GetComponent<iconicElementScript>().centroid = edge_pos;

                    // again we optimize by approxmating the radius calculation
                    if (points.Count > 1)
                        temp.GetComponent<iconicElementScript>().radius = Vector3.Distance(points[0], points[1]) / 2;
                    else
                        temp.GetComponent<iconicElementScript>().radius = 20f;

                    float rad = temp.GetComponent<iconicElementScript>().radius;
                    Vector3 size = new Vector3(rad, rad, rad);

                    BoxCollider box_cl = temp.AddComponent<BoxCollider>();
                    box_cl.center = edge_pos;//Vector3.zero;
                    box_cl.size = size * 10;

                    cur_rect_iter++;
                }

            }

            Debug.Log("all_icons: " + all_icons.Count.ToString());

            graph_holder.GetComponent<GraphElementScript>().points = graph_points;
            graph_holder.GetComponent<GraphElementScript>().graph.edges = new List<Edge>();

        }

        // create graph based on node radius
        // this also handles the visual variables!
        if (graph_type == "NodeRadius")
        {
            for (int i = 0; i < all_icons.Count; i++)
            {
                // the node can not make edges if it is greater than the max or smaller than the min in visual variables  
                if (frames_annotation == null)
                {
                    if (visual_var_val != 0 &&
                        (all_icons[i].GetComponent<iconicElementScript>().visual_variable < min_visual_var || all_icons[i].GetComponent<iconicElementScript>().visual_variable > max_visual_var))
                    {
                        //Debug.Log("discarding for edge creation");
                        continue;
                    }
                }

                //Debug.Log("passed for edge creation" + all_icons[i].GetComponent<iconicElementScript>().visual_variable.ToString());

                for (int j = (i + 1); j < all_icons.Count; j++)
                {
                    if (visual_var_val != 0 &&
                        (all_icons[j].GetComponent<iconicElementScript>().visual_variable < min_visual_var || all_icons[j].GetComponent<iconicElementScript>().visual_variable > max_visual_var))
                    {
                        //Debug.Log("discarding for edge creation");
                        continue;
                    }

                    if (Vector3.Distance(all_icons[i].GetComponent<iconicElementScript>().edge_position,
                        all_icons[j].GetComponent<iconicElementScript>().edge_position) < node_radius_val)
                    {
                        GameObject temp = Instantiate(edge_prefab, Vector3.zero, Quaternion.identity, graph_holder.transform.GetChild(1));

                        temp.tag = "edge";
                        temp.GetComponent<EdgeElementScript>().edge_start = all_icons[i];
                        temp.GetComponent<EdgeElementScript>().edge_end = all_icons[j];
                        temp.GetComponent<EdgeElementScript>().video = true;


                        //temp.GetComponent<EdgeElementScript>().addDot();
                        //temp.GetComponent<EdgeElementScript>().updateEndPoint();
                        temp.GetComponent<EdgeElementScript>().addEndPoint(true);

                        Edge edge = new Edge();
                        edge.edge_start = all_icons[i].GetComponent<iconicElementScript>().icon_number;
                        edge.edge_end = all_icons[j].GetComponent<iconicElementScript>().icon_number;
                        edge.weight = 1;

                        graph_holder.GetComponent<GraphElementScript>().graph.edges.Add(edge);
                    }
                }
            }
        }
        // create site specific edges  
        else
        {
            List<Edge> all_edges = All_Route.edge_list[(int)videoplayer.frame].edges;

            foreach (Edge cur_edge in all_edges)
            {
                GameObject temp = Instantiate(edge_prefab, Vector3.zero, Quaternion.identity, graph_holder.transform.GetChild(1));

                temp.tag = "edge";
                temp.GetComponent<EdgeElementScript>().edge_start =
                    graph_holder.GetComponent<GraphElementScript>().nodeMaps[cur_edge.edge_start.ToString()].gameObject;

                temp.GetComponent<EdgeElementScript>().edge_end =
                    graph_holder.GetComponent<GraphElementScript>().nodeMaps[cur_edge.edge_end.ToString()].gameObject;

                temp.GetComponent<EdgeElementScript>().video = true;

                //temp.GetComponent<EdgeElementScript>().addDot();
                //temp.GetComponent<EdgeElementScript>().updateEndPoint();
                temp.GetComponent<EdgeElementScript>().addEndPoint(true);

                graph_holder.GetComponent<GraphElementScript>().graph.edges.Add(cur_edge);
            }
        }

    }
}
