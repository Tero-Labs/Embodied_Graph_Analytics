using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using OpenCVForUnity.CoreModule;
using OpenCVForUnity.UnityUtils;
using OpenCVForUnity.ImgprocModule;

// this script is basically a copy of iconicElementScript.cs, VideoPlayerChildrenAccess.cs and VideoController.cs
public class ImageCVDetectionandController : MonoBehaviour
{
    public GameObject quad;
    public Canvas canvas;
    public Button settings, delete, copy;
    public GameObject settings_menu;
    public GameObject control_menu;

    public InputField mainInputField;
    public InputField WindowInputField;
    public Toggle node_radius, site_specific, none;
    public Toggle auto_track, manual_track;

    // visual Variable UI
    public TMP_Dropdown visual_var_type;
    public InputField max_limit, min_limit;
    public TMP_InputField contour_obj_count;
    public FlexibleColorPicker color_picker_script;
    public Slider blobsizeSlider;

    public float width, height;

    public float node_radius_val;
    public string graph_type;
    public bool auto_track_val;

    // prefabs
    public GameObject icon_prefab;
    public GameObject edge_prefab;
    public GameObject graph_prefab;
    public GameObject paintable;
    public GameObject graph_holder;

    public Sprite recognized_sprite;
    Texture2D SpriteTexture;

    //visual variable related things
    public List<RotatedRect> all_bounding_rects;
    public int visual_var_val;
    public float min_visual_var, max_visual_var, contour_size;
    public int contour_cnt;

    void Start()
    {
        settings.onClick.AddListener(delegate { SettingsMenu(); });
        delete.onClick.AddListener(delegate { Delete(); });
        copy.onClick.AddListener(delegate { Copy(); });
        mainInputField.onValueChanged.AddListener(delegate { LockInput(mainInputField); });

        none.onValueChanged.AddListener(delegate { GraphType(); });
        node_radius.onValueChanged.AddListener(delegate { GraphType(); });
        site_specific.onValueChanged.AddListener(delegate { GraphType(); });

        auto_track.onValueChanged.AddListener(delegate { TrackType(auto_track); });
        manual_track.onValueChanged.AddListener(delegate { TrackType(manual_track); });

        // visual variable setup
        visual_var_type.onValueChanged.AddListener(delegate { ChangeVisualVariable(visual_var_type); });
        max_limit.onValueChanged.AddListener(delegate { TrackVisualInputField(max_limit); });
        min_limit.onValueChanged.AddListener(delegate { TrackVisualInputField(min_limit); });
        contour_obj_count.onValueChanged.AddListener(delegate { TrackVisualInputField(contour_obj_count); });
        blobsizeSlider.onValueChanged.AddListener(delegate { SliderValueChanged(blobsizeSlider); });

        // to setup initial values
        GraphType();
        TrackType(auto_track);

        // moved to spritelaoder function
        //width = quad.transform.localScale.x;
        //height = quad.transform.localScale.y;

        //global_scope
        //control_menu.GetComponentInParent<Canvas>().worldCamera = Camera.main;
        visual_var_val = visual_var_type.value; 
        min_visual_var = -360f;
        max_visual_var = 360f;

        node_radius_val = 20f;
        contour_cnt = 7;
        UIlayout();
    }
    
    public void UIlayout()
    {
        //http://www.robotmonkeybrain.com/convert-unity-ui-screen-space-position-to-world-position/
        //https://forum.unity.com/threads/world-position-to-local-recttransform-position.445256/
        //https://forum.unity.com/threads/world-position-to-local-recttransform-position.445256/
        //https://stackoverflow.com/a/43736203       

        Vector3 temp_pos = new Vector3(quad.transform.position.x,
                                                    quad.transform.position.y - 45,
                                                    0f);

        Vector3 screen_temp_pos = RectTransformUtility.WorldToScreenPoint(Camera.main, temp_pos);

        Vector2 rect_Try;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(control_menu.transform.parent.GetComponent<RectTransform>(), screen_temp_pos,
                                    null, out rect_Try);

        control_menu.GetComponent<RectTransform>().anchoredPosition = rect_Try;

        Vector3 temp_pos_2 = new Vector3(quad.transform.position.x + width + 80,
                                                   quad.transform.position.y + (height/2),
                                                   0f);

        Vector3 screen_temp_pos_2 = RectTransformUtility.WorldToScreenPoint(Camera.main, temp_pos_2);

        Vector2 rect_Try_2;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(settings_menu.transform.parent.GetComponent<RectTransform>(),
                    screen_temp_pos_2, null, out rect_Try_2);

        settings_menu.GetComponent<RectTransform>().anchoredPosition = rect_Try_2;
    }

    /*
    public void UIlayoutWorldSpace()
    {
        //http://www.robotmonkeybrain.com/convert-unity-ui-screen-space-position-to-world-position/
        //https://forum.unity.com/threads/world-position-to-local-recttransform-position.445256/
        //https://forum.unity.com/threads/world-position-to-local-recttransform-position.445256/
        //https://stackoverflow.com/a/43736203

        Vector3 temp_pos = new Vector3(quad.transform.position.x - (width / 2),
                                                    quad.transform.position.y - (height / 2) - 45,
                                                    -5f);

        Debug.Log("temp_pos:" + temp_pos.ToString());
               
        //control_menu.GetComponent<RectTransform>().anchoredPosition = temp_pos;
        control_menu.transform.position = temp_pos;

        Vector3 temp_pos_2 = new Vector3(quad.transform.position.x + (width / 2) + 72,
                                                   quad.transform.position.y,
                                                   0f);

        Vector3 screen_temp_pos_2 = RectTransformUtility.WorldToScreenPoint(Camera.main, temp_pos_2);

        Vector2 rect_Try_2;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(settings_menu.transform.parent.GetComponent<RectTransform>(),
                    screen_temp_pos_2, null, out rect_Try_2);

        settings_menu.GetComponent<RectTransform>().anchoredPosition = rect_Try_2;
    }
    */

    void ChangeVisualVariable(TMP_Dropdown dropdown)
    {
        visual_var_val = dropdown.value;
        all_bounding_rects = gameObject.GetComponent<ContourandRotatedRectDetection>().FindResultFromImageTexture(SpriteTexture, contour_count: contour_cnt, visual_var: visual_var_val);

        StartCoroutine(GraphCreation());
    }

    void TrackVisualInputField(InputField inputField)
    {
        Paintable.click_on_inputfield = true;

        if (inputField.name == "minInputField" && inputField.text.Length > 0)
        {
            float.TryParse(inputField.text, out min_visual_var);
            StartCoroutine(GraphCreation());
        }

        if (inputField.name == "maxInputField" && inputField.text.Length > 0)
        {
            float.TryParse(inputField.text, out max_visual_var);
            StartCoroutine(GraphCreation());
        }

        if (inputField.name == "contourInputField" && inputField.text.Length > 0)
        {
            int.TryParse(inputField.text, out contour_cnt);
            StartCoroutine(GraphCreation());
        }
    }

    void TrackVisualInputField(TMP_InputField inputField)
    {
        Paintable.click_on_inputfield = true;

        if (inputField.name == "contourInputField" && inputField.text.Length > 0)
        {
            int.TryParse(inputField.text, out contour_cnt);
            all_bounding_rects = gameObject.GetComponent<ContourandRotatedRectDetection>().FindResultFromImageTexture(SpriteTexture, contour_count: contour_cnt, visual_var: visual_var_val);
            StartCoroutine(GraphCreation());
        }
    }

    void SliderValueChanged(Slider slider)
    {
        contour_size = slider.value;
        StartCoroutine(GraphCreation());
    }

    void LockInput(InputField input)
    {
        // if a function is getting evaluated now, we will not receive any further input
        /*if (paintable.GetComponent<Paintable>().no_func_menu_open)
            return;*/

        Paintable.click_on_inputfield = true;

        if (input.text.Length > 0)
        {
            float.TryParse(input.text, out node_radius_val);
            StartCoroutine(GraphCreation());
        }
    }    

    public void GraphType()
    {
        if (site_specific.isOn) graph_type = "SiteSpecific";
        else graph_type = "NodeRadius";

        mainInputField.interactable = node_radius.isOn;

        // instead of creating different conditions for visual variables, I treated them similarly as proximity graph
        if (none.isOn) node_radius_val = float.MaxValue;

        StartCoroutine(GraphCreation());
    }

    void TrackType(Toggle toggle)
    {
        auto_track_val = auto_track.isOn;
    }

    void SettingsMenu()
    {
        settings_menu.SetActive(!settings_menu.activeSelf);
    }

    void Delete()
    {
        if (graph_holder != null)
        {
            Destroy(graph_holder);
        }

        Destroy(transform.gameObject);
    }

    // Update is called once per frame
    void Update()
    {
        if (mainInputField != null && mainInputField.isFocused)
        {
            Paintable.click_on_inputfield = true;
            return;
        }
    }

    public void checkHitAndMove(Vector3 diff)
    {
        transform.position += diff;
        UIlayout();
        /*control_menu.transform.position += diff;
        settings_menu.transform.position += diff;*/

        if (graph_holder != null)
        {
            graph_holder.transform.position += diff;
            graph_holder.GetComponent<GraphElementScript>().checkHitAndMove(diff);
        }

    }

    // for image loading and rendering, copied from .cs

    // load a new image and convert to sprite 
    // https://forum.unity.com/threads/generating-sprites-dynamically-from-png-or-jpeg-files-in-c.343735/ (the solution by Freznosis#5)        
    public Sprite LoadNewSprite(string FilePath, float PixelsPerUnit = 100.0f, SpriteMeshType spriteType = SpriteMeshType.Tight)
    {

        // Load a PNG or JPG image from disk to a Texture2D, assign this texture to a new sprite and return its reference
        // testing CV 
        gameObject.AddComponent<ContourandRotatedRectDetection>();

        SpriteTexture = LoadTexture(FilePath);
        Sprite NewSprite = Sprite.Create(SpriteTexture, new UnityEngine.Rect(0, 0, SpriteTexture.width, SpriteTexture.height), new Vector2(0, 0), PixelsPerUnit, 0, spriteType);
        recognized_sprite = NewSprite;
        SpriteToRender();

        all_bounding_rects = gameObject.GetComponent<ContourandRotatedRectDetection>().FindResultFromImageTexture(SpriteTexture);

        StartCoroutine(GraphCreation());

        return NewSprite;
    }

    public Sprite LoadSprite(Texture2D SpriteTexture, /*float x, float y, float width, float height,*/ float PixelsPerUnit = 100.0f, SpriteMeshType spriteType = SpriteMeshType.Tight)
    {

        // Load a PNG or JPG image from disk to a Texture2D, assign this texture to a new sprite and return its reference
        Debug.Log("Texture width: " + SpriteTexture.width.ToString() + ", height: " + SpriteTexture.height.ToString());
        Sprite NewSprite = Sprite.Create(SpriteTexture, new UnityEngine.Rect(0, 0, SpriteTexture.width, SpriteTexture.height),//(x, y, width, height), 
            new Vector2(0, 0), PixelsPerUnit, 0, spriteType);
        recognized_sprite = NewSprite;
        SpriteToRender();
        return NewSprite;
    }

    public Texture2D LoadTexture(string FilePath)
    {

        // Load a PNG or JPG file from disk to a Texture2D
        // Returns null if load fails

        Texture2D Tex2D;
        byte[] FileData;

        if (File.Exists(FilePath))
        {
            FileData = File.ReadAllBytes(FilePath);
            Tex2D = new Texture2D(2, 2);           // Create new "empty" texture
            if (Tex2D.LoadImage(FileData))           // Load the imagedata into the texture (size is set automatically)
                return Tex2D;                 // If data = readable -> return texture
        }
        return null;                     // Return null if load failed
    }

    public void SpriteToRender()
    {

        // rescaling, otherwise the projected mesh is too small
        quad.transform.localScale = new Vector3(30f, 30f, 1f);

        quad.AddComponent<BoxCollider>();
        quad.AddComponent<SpriteRenderer>();
        quad.GetComponent<SpriteRenderer>().sprite = recognized_sprite;

        //Debug.Log("box collider before: " + templine.GetComponent<BoxCollider>().center.ToString());
        quad.GetComponent<BoxCollider>().size = quad.GetComponent<SpriteRenderer>().sprite.bounds.size;
        quad.GetComponent<BoxCollider>().center = new Vector3(quad.GetComponent<SpriteRenderer>().sprite.bounds.center.x,
                            quad.GetComponent<SpriteRenderer>().sprite.bounds.center.y,
                            -5f);

        width = quad.transform.localScale.x * quad.GetComponent<BoxCollider>().size.x;
        height = quad.transform.localScale.y * quad.GetComponent<BoxCollider>().size.y;

        //radius = transform.GetComponent<SpriteRenderer>().bounds.extents.magnitude;

        // set collider trigger
        quad.GetComponent<BoxCollider>().isTrigger = true;
        quad.GetComponent<BoxCollider>().enabled = true;

        /*edge_position = transform.GetComponent<SpriteRenderer>().bounds.extents;
        edge_position.z = -5f;

        bounds_center = edge_position;*/

    }

    public IEnumerator GraphCreation()
    {
        yield return null;

        if (graph_holder != null)
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
            graph_holder.GetComponent<GraphElementScript>().parent_video_or_image = transform.gameObject;
        }

        graph_holder.SetActive(true);


        List<GameObject> all_icons = new List<GameObject>();

        // because, we do not want to increase total icon numbers in each frame, which will be ambigious
        int num = Paintable.totalLines;
        List<Vector3> graph_points = new List<Vector3>();

        graph_holder.GetComponent<GraphElementScript>().nodeMaps = new Dictionary<string, Transform>();
        graph_holder.GetComponent<GraphElementScript>().graph = new Graph();
        graph_holder.GetComponent<GraphElementScript>().graph.nodes = new List<int>();

        int cur_rect_iter = 0;
        foreach (RotatedRect cur_rect in all_bounding_rects)
        {
            Vector3 edge_pos = Vector3.zero;
            GameObject temp = Instantiate(icon_prefab, Vector3.zero, Quaternion.identity, graph_holder.transform.GetChild(0));

            num++;
            temp.tag = "iconic";
            temp.name = "iconic_" + num;
            temp.GetComponent<iconicElementScript>().icon_number = num;
            temp.GetComponent<iconicElementScript>().video_icon = true;
            all_icons.Add(temp);

            graph_holder.GetComponent<GraphElementScript>().graph.nodes.Add(num);
            graph_holder.GetComponent<GraphElementScript>().nodeMaps.Add(num.ToString(), temp.transform);

            temp.GetComponent<TrailRenderer>().enabled = false;
            temp.GetComponent<LineRenderer>().enabled = false;
            temp.GetComponent<MeshRenderer>().enabled = false;

            List<Vector3> points = new List<Vector3>();

            Point[] rect_points = new Point[4];
            cur_rect.points(rect_points);

            foreach (Point cur_pnt in rect_points)
            {
                // a little different than the videoplayer; because the transform position is set differently for sprite
                float lerped_x = Mathf.Lerp(quad.transform.position.x /*- (width / 2)*/, quad.transform.position.x + (width /*/ 2*/), Mathf.InverseLerp(1, SpriteTexture.width, (float)cur_pnt.x));
                float lerped_y = Mathf.Lerp(quad.transform.position.y + (height /*/ 2*/), quad.transform.position.y /*- (height / ) 2*/, Mathf.InverseLerp(1, SpriteTexture.height, (float)cur_pnt.y));

                Vector3 pos_vec = new Vector3(lerped_x, lerped_y, -40f);
                edge_pos += pos_vec;
                points.Add(pos_vec);
            }
                        
            // size
            if (Paintable.visual_variable_dict[visual_var_val] == "size")
                temp.GetComponent<iconicElementScript>().visual_variable = ((float)cur_rect.size.width + (float)cur_rect.size.height) / 2;
            // angle
            else if (Paintable.visual_variable_dict[visual_var_val] == "angle")
                temp.GetComponent<iconicElementScript>().visual_variable = (float)cur_rect.angle;
            // brightness
            else if (Paintable.visual_variable_dict[visual_var_val] == "brightness")
                temp.GetComponent<iconicElementScript>().visual_variable = 
                    transform.GetComponent<ContourandRotatedRectDetection>().all_intensities[cur_rect_iter];

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

            BoxCollider box_cl = temp.AddComponent<BoxCollider>();
            box_cl.center = edge_pos;//Vector3.zero;
            box_cl.size = size * 5;

            cur_rect_iter++;
        }

        // Debug.Log("all_icons: " + all_icons.Count.ToString());
        graph_holder.GetComponent<GraphElementScript>().points = graph_points;
        graph_holder.GetComponent<GraphElementScript>().graph.edges = new List<Edge>();      

        // create graph based on node radius
        if (graph_type == "NodeRadius")
        {
            for (int i = 0; i < all_icons.Count; i++)
            {
                // the node can not make edges if it is greater than the max or smaller than the min in visual variables  
                if (visual_var_val != 0 &&
                            (all_icons[i].GetComponent<iconicElementScript>().visual_variable < min_visual_var || all_icons[i].GetComponent<iconicElementScript>().visual_variable > max_visual_var))
                {
                    //Debug.Log("discarding for edge creation");
                    continue;
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
                
    }

    public GameObject Copy()
    {
        Vector3 target_pos = new Vector3(width + 20f, 0f, 0f);
        GameObject cp = Instantiate(graph_holder, graph_holder.transform.position + target_pos, Quaternion.identity, graph_holder.transform.parent.transform);
        cp.GetComponent<GraphElementScript>().video_graph = false;
        cp.GetComponent<GraphElementScript>().checkHitAndMove(target_pos);
        Paintable.graph_count++;
        cp.name = "graph_" + Paintable.graph_count.ToString();
        cp.GetComponent<GraphElementScript>().graph_name = "G" + Paintable.graph_count.ToString();

        // to make them moveable
        Transform node_parent = cp.transform.GetChild(0);
        for (int i = 0; i < node_parent.childCount; i++)
        {
            node_parent.GetChild(i).GetComponent<iconicElementScript>().video_icon = false;
        }

        return cp;
    }
}
