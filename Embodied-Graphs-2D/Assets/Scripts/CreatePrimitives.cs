using System.Collections;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.EnhancedTouch;
using TMPro;
/*using uPIe;
using PDollarGestureRecognizer;

using Symbolism;
using Jobberwocky.GeometryAlgorithms.Source.API;
using Jobberwocky.GeometryAlgorithms.Source.Core;
using Jobberwocky.GeometryAlgorithms.Source.Parameters;*/

public class CreatePrimitives : MonoBehaviour
{

    // Length, area, distance units
    public static float unitScale = 0.025f;

    public GameObject PenLine;
    public GameObject SetLine;
    public GameObject EdgeLinePrefab;
    public GameObject FunctionLine;
    public GameObject StaticPenLine;
    public GameObject ParameterUIField;
    public GameObject EdgeUIField;

    /*public GameObject radial_menu;  // prefab
    public GameObject set_radial_menu;
    public GameObject pen_radial_menu;
    public GameObject function_radial_menu;
    public GameObject set_timer_submenu;
    public Slider abstraction_slider; // prefab
    public GameObject popup_radial;
    public Canvas canvas_radial;*/

    public static GameObject iconicElementButton;
    public static GameObject pan_button;
    /*
    public static GameObject select_button;
    public static GameObject edge_button;
    public static GameObject function_button;
    public static GameObject eraser_button;
    public static GameObject staticpen_button;*/

    // Prefabs
    public GameObject IconicElement;

    public GameObject paintable_object;

    // background materials
    public Material default_background_material, grid_material;

    // line, set, function materials
    public Material set_line_material, function_line_material;

    //private VectorLine tempLine;
    //public List<VectorLine> all_lines = new List<VectorLine>();

    // predictive stroke
    // private List<Gesture> trainingSet = new List<Gesture>();
    // private List<Point> predictive_points = new List<Point>();
    private float predictive_stroke_up_time = 0f;
    private int strokeID = 0;
    public Sprite[] commonShapes;

    //public int totalLines = 0;

    // Start is called before the first frame update
    void Awake()
    {
        iconicElementButton = GameObject.Find("IconicPen");
        pan_button = GameObject.Find("Pan");
        /*
        select_button = GameObject.Find("Select");
        edge_button = GameObject.Find("Edge_draw");
        function_button = GameObject.Find("FunctionButton");
        eraser_button = GameObject.Find("Eraser");
        staticpen_button = GameObject.Find("StaticPen");*/

        paintable_object = GameObject.FindGameObjectWithTag("paintable_canvas_object");

        // Predictive stroke
        // Load pre-made gestures
        /*TextAsset[] gesturesXml = Resources.LoadAll<TextAsset>("GestureSet/BasicShapeTemplates/");
        foreach (TextAsset gestureXml in gesturesXml)
            trainingSet.Add(GestureIO.ReadGestureFromXML(gestureXml.text));

        commonShapes = Resources.LoadAll<Sprite>("Shapes/CommonShapes");*/
    }


    // ======================== PenLine Free-form ============================

    // Assumes templine has been initialized in pointer.start and pointer.moved
    public GameObject FinishPenLine(GameObject templine, Mesh combinedMesh = null)
    {
        
        // compute centroid and bounds
        templine.GetComponent<iconicElementScript>().computeCentroid();
        templine.GetComponent<iconicElementScript>().computeBounds();

        // set line renderer, width
        templine.GetComponent<LineRenderer>().widthCurve = templine.GetComponent<iconicElementScript>().widthcurve;

        templine.GetComponent<LineRenderer>().numCapVertices = 15;
        templine.GetComponent<LineRenderer>().numCornerVertices = 15;

        templine.GetComponent<LineRenderer>().positionCount = templine.GetComponent<iconicElementScript>().points.Count;
        templine.GetComponent<LineRenderer>().SetPositions(templine.GetComponent<iconicElementScript>().points.ToArray());

        // attached with the parent, so no need of initialization any further
        /*GameObject meshObj = new GameObject("_meshobj");
        meshObj.AddComponent<MeshFilter>();
        meshObj.AddComponent<MeshRenderer>();*/

        var lineRenderer = templine.GetComponent<LineRenderer>();
        var meshFilter = templine.GetComponent<MeshFilter>();
               

        // If a combined mesh is not passed, use the line renderer mesh (default case)
        if (combinedMesh == null)
        {
            Mesh mesh = new Mesh();
            lineRenderer.BakeMesh(mesh, true);
            meshFilter.sharedMesh = mesh;
            //meshObj.GetComponent<MeshFilter>().sharedMesh = mesh;
            templine.GetComponent<iconicElementScript>()._mesh = mesh;
        }
        else
        {
            meshFilter.sharedMesh = combinedMesh;
            //meshObj.GetComponent<MeshFilter>().sharedMesh = combinedMesh;
            templine.GetComponent<iconicElementScript>()._mesh = combinedMesh;
        }        

        //meshObj.GetComponent<MeshRenderer>().sharedMaterial = templine.GetComponent<iconicElementScript>().icon_elem_material;
        templine.GetComponent<MeshRenderer>().sharedMaterial = templine.GetComponent<iconicElementScript>().icon_elem_material;


        // get rid of the line renderer?
        Destroy(templine.GetComponent<LineRenderer>());

        // disable trail renderer, no longer needed
        templine.GetComponent<TrailRenderer>().enabled = false;

        // add a collider
        
        templine.AddComponent<BoxCollider>();

        //Debug.Log("box collider before: " + templine.GetComponent<BoxCollider>().center.ToString());
        templine.GetComponent<BoxCollider>().size = templine.GetComponent<MeshFilter>().sharedMesh.bounds.size;
        //templine.GetComponent<BoxCollider>().center = new Vector3(0, 0, 0);
        templine.GetComponent<BoxCollider>().center = templine.GetComponent<MeshFilter>().sharedMesh.bounds.center;
        templine.GetComponent<iconicElementScript>().edge_position = templine.GetComponent<MeshFilter>().sharedMesh.bounds.center;
        //Debug.Log("box collider after: " + templine.GetComponent<BoxCollider>().center.ToString());

        // set collider trigger
        templine.GetComponent<BoxCollider>().isTrigger = true;

        // disable the collider because we are in the pen mode right now. Pan mode enables back all colliders.
        templine.GetComponent<BoxCollider>().enabled = false;

        // set transform position
        //templine.transform.position = templine.GetComponent<MeshFilter>().sharedMesh.bounds.center;
        templine.transform.position = new Vector3(0, 0, 0); //meshObj.transform.position;
        //templine.transform.position = meshObj.GetComponent<MeshFilter>().sharedMesh.bounds.center;

        // update the previous_position variable for templine for checkMove()
        templine.GetComponent<iconicElementScript>().previous_position = templine.transform.position;   

        // Save the area of the bounding box 
        templine.GetComponent<iconicElementScript>().attribute.Area =
            templine.GetComponent<MeshFilter>().sharedMesh.bounds.size.x *
            templine.GetComponent<MeshFilter>().sharedMesh.bounds.size.y * unitScale * unitScale;

        // set current_attribute of penLine
        templine.GetComponent<iconicElementScript>().current_attribute =
            templine.GetComponent<iconicElementScript>().attribute.Length;
        
        return templine;
    }

    // Assumes templine has been initialized in pointer.start and pointer.moved
    public GameObject FinishFunctionLine(GameObject templine, Mesh combinedMesh = null)
    {

        // compute centroid and bounds
        templine.GetComponent<FunctionElementScript>().computeCentroid();
        templine.GetComponent<FunctionElementScript>().computeBounds();

        // set line renderer, width
        templine.GetComponent<LineRenderer>().widthCurve = templine.GetComponent<FunctionElementScript>().widthcurve;

        templine.GetComponent<LineRenderer>().numCapVertices = 15;
        templine.GetComponent<LineRenderer>().numCornerVertices = 15;

        templine.GetComponent<LineRenderer>().positionCount = templine.GetComponent<FunctionElementScript>().points.Count;
        templine.GetComponent<LineRenderer>().SetPositions(templine.GetComponent<FunctionElementScript>().points.ToArray());

        var lineRenderer = templine.GetComponent<LineRenderer>();
        var meshFilter = templine.GetComponent<MeshFilter>();


        // If a combined mesh is not passed, use the line renderer mesh (default case)
        if (combinedMesh == null)
        {
            Mesh mesh = new Mesh();
            lineRenderer.BakeMesh(mesh, true);
            meshFilter.sharedMesh = mesh;
            templine.GetComponent<FunctionElementScript>()._mesh = mesh;
        }
        else
        {
            meshFilter.sharedMesh = combinedMesh;
            templine.GetComponent<FunctionElementScript>()._mesh = combinedMesh;
        }

        templine.GetComponent<MeshRenderer>().sharedMaterial = templine.GetComponent<FunctionElementScript>().icon_elem_material;
        
        //Destroy(templine.GetComponent<LineRenderer>());

        // disable trail renderer, no longer needed
        templine.GetComponent<TrailRenderer>().enabled = false;
        templine.GetComponent<LineRenderer>().enabled = false;

        // add a collider
       /* templine.AddComponent<BoxCollider>();
        templine.GetComponent<BoxCollider>().size = templine.GetComponent<MeshFilter>().sharedMesh.bounds.size;
        templine.GetComponent<BoxCollider>().center = templine.GetComponent<MeshFilter>().sharedMesh.bounds.center;*/

        templine.GetComponent<FunctionElementScript>().edge_position = templine.GetComponent<MeshFilter>().sharedMesh.bounds.center;

        // set collider trigger
        //templine.GetComponent<BoxCollider>().isTrigger = true;

        // disable the collider because we are in the pen mode right now. Pan mode enables back all colliders.
        //templine.GetComponent<BoxCollider>().enabled = false;

        // set transform position
        templine.transform.position = new Vector3(0, 0, 0); //meshObj.transform.position;

        // update the previous_position variable for templine for checkMove()
        templine.GetComponent<FunctionElementScript>().previous_position = templine.transform.position;

        // Save the area of the bounding box 
        templine.GetComponent<FunctionElementScript>().attribute.Area =
            templine.GetComponent<MeshFilter>().sharedMesh.bounds.size.x *
            templine.GetComponent<MeshFilter>().sharedMesh.bounds.size.y * unitScale * unitScale;

        // set current_attribute of penLine
        templine.GetComponent<FunctionElementScript>().current_attribute = templine.GetComponent<FunctionElementScript>().attribute.Length;

        return templine;
    }


    // Assumes templine has been initialized in pointer.start and pointer.moved
    public GameObject FinishEdgeLine(GameObject templine, Mesh combinedMesh = null)
    {
        var lineRenderer = templine.GetComponent<LineRenderer>();
        var meshFilter = templine.GetComponent<MeshFilter>();

        Mesh mesh = new Mesh();
        lineRenderer.BakeMesh(mesh, true);

        // If a combined mesh is not passed, use the line renderer mesh (default case)
        if (combinedMesh == null)
        {
            meshFilter.sharedMesh = mesh;
        }
        else
        {
            meshFilter.sharedMesh = combinedMesh;
        }

        templine.GetComponent<iconicElementScript>()._mesh = mesh;

        templine.GetComponent<MeshRenderer>().sharedMaterial = templine.GetComponent<iconicElementScript>().icon_elem_material;

        // get rid of the line renderer?
        //Destroy(templine.GetComponent<LineRenderer>());
        templine.GetComponent<LineRenderer>().enabled = false;

        // add a collider
        templine.AddComponent<BoxCollider>();

        templine.GetComponent<BoxCollider>().size = templine.GetComponent<MeshFilter>().sharedMesh.bounds.size;
        templine.GetComponent<BoxCollider>().center = templine.GetComponent<MeshFilter>().sharedMesh.bounds.center;

        // set collider trigger
        templine.GetComponent<BoxCollider>().isTrigger = true;

        // disable the collider because we are in the pen mode right now. Pan mode enables back all colliders.
        templine.GetComponent<BoxCollider>().enabled = false;

        // set transform position
        templine.transform.position = new Vector3(0, 0, 0); //meshObj.transform.position;

        // update the previous_position variable for templine for checkMove()
        templine.GetComponent<iconicElementScript>().previous_position = templine.transform.position;

        // Save the area of the bounding box 
        templine.GetComponent<iconicElementScript>().attribute.Area =
            templine.GetComponent<MeshFilter>().sharedMesh.bounds.size.x *
            templine.GetComponent<MeshFilter>().sharedMesh.bounds.size.y * unitScale * unitScale;

        // set current_attribute of penLine
        templine.GetComponent<iconicElementScript>().current_attribute =
            templine.GetComponent<iconicElementScript>().attribute.Length;

        return templine;
    }

    // ASSUMES: 1. points are in global space (not local)
    /*public GameObject CreatePenLine(List<Vector3> points, Color color, float widthMultiplier = 1f, AnimationCurve widthCurve = null)
    {
        GameObject templine;

        // CODE FROM POINTER.START

        paintable_object.GetComponent<Paintable>().totalLines++;

        templine = Instantiate(PenLine, this.gameObject.transform);
        templine.GetComponent<TrailRenderer>().material.color = Color.black;

        templine.name = "penLine_" + paintable_object.GetComponent<Paintable>().totalLines.ToString();
        templine.tag = "penline";

        templine.transform.GetChild(1).GetComponent<MeshRenderer>().enabled = true;
        templine.transform.GetChild(1).GetComponent<BoxCollider2D>().enabled = false;

        // color and width
        templine.GetComponent<TrailRenderer>().material.color = color;
        templine.GetComponent<iconicElementScript>().icon_elem_material.color = color;
        templine.GetComponent<TrailRenderer>().widthMultiplier = widthMultiplier;
        templine.GetComponent<LineRenderer>().widthMultiplier = widthMultiplier;

        // disable the argument_label button, currently it's at the 3rd index
        // templine.transform.GetChild(2).gameObject.SetActive(false);

        // CODE FROM POINTER.MOVE
        templine.GetComponent<iconicElementScript>().calculateLengthAttributeFromPoints();

        // if no width curve exists then assign a generic width
        if (widthCurve == null)
        {
            templine.GetComponent<iconicElementScript>().widthcurve.AddKey(0f, 1f);
            templine.GetComponent<iconicElementScript>().widthcurve.AddKey(1f, 1f);
        }
        else
        {
            templine.GetComponent<iconicElementScript>().widthcurve = widthCurve;
        }

        templine.GetComponent<iconicElementScript>().points = points;

        // FINISH THE TEMPLINE
        templine = FinishPenLine(templine);

        return templine;
    }*/

    // LOAD A SPRITE AS A PENLINE OBJECT
    /*public GameObject CreatePenLine(string sprite_filename)
    {
        GameObject templine;

        // CODE FROM POINTER.START

        paintable_object.GetComponent<Paintable>().totalLines++;

        templine = Instantiate(PenLine, GameObject.FindGameObjectWithTag("paintable_canvas_object").transform);
        templine.GetComponent<TrailRenderer>().material.color = Color.black;

        templine.name = "penLine_" + paintable_object.GetComponent<Paintable>().totalLines.ToString();
        templine.tag = "penline";

        templine.GetComponent<iconicElementScript>().isThisAnImage = true;
        //Sprite sprite = Resources.Load<Sprite>("ExampleImages/" + sprite_filename.Replace(".png", ""));
        Sprite sprite = IMG2Sprite.instance.LoadNewSprite(sprite_filename, 2f);
        templine.GetComponent<iconicElementScript>().sprite = sprite;
        templine.GetComponent<iconicElementScript>().sprite_filename = sprite_filename;

        templine.transform.GetChild(1).GetComponent<MeshRenderer>().enabled = true;
        templine.transform.GetChild(1).GetComponent<BoxCollider2D>().enabled = false;

        // color and width

        // disable the argument_label button, currently it's at the 3rd index
        templine.transform.GetChild(2).gameObject.SetActive(false);

        // FIND CONVEX HULL
        Vector3[] vertices = new Vector3[sprite.vertices.Length];
        for (int i = 0; i < vertices.Length; i++)
        {
            vertices[i].x = sprite.vertices[i].x;
            vertices[i].y = sprite.vertices[i].y;
            vertices[i].z = -40f;
        }

        // Compute convex hull. A concavity value less than 30 may produce tighter shape, but point in polygon test starts failing at that point.
        var hullAPI = new HullAPI();
        var hull = hullAPI.Hull2D(new Hull2DParameters() { Points = vertices, Concavity = 30 });

        vertices = hull.vertices;

        // sort vertices according to angle

        // Store angles between texture's center and vertex position vector
        float[] angles = new float[vertices.Length];

        // Calculate the angle between each vertex and the texture's /center
        for (int i = 0; i < vertices.Length; i++)
        {
            angles[i] = Paintable_Script.AngleBetweenVectors(vertices[i], templine.transform.position);
            //vertexArray[i] -= texCenter;   // Offset vertex about texture center
        }

        // Sort angles into ascending order, use to put vertices in clockwise order
        Array.Sort(angles, vertices);

        templine.GetComponent<penLine_script>().points = vertices.ToList();

        // CODE FROM POINTER.MOVE
        templine.GetComponent<penLine_script>().calculateLengthAttributeFromPoints();

        templine.GetComponent<penLine_script>().widthcurve.AddKey(0f, 1f);
        templine.GetComponent<penLine_script>().widthcurve.AddKey(1f, 1f);

        // FINISH THE TEMPLINE

        // set the gesture/legible layer fields: no prediction was done. Set \ space as default.
        templine.GetComponent<penLine_script>().isPredictionDone = true;
        templine.GetComponent<penLine_script>().gestureTemplate = "legrectangle";

        // this one is needed for the penline script functions
        templine.GetComponent<penLine_script>().paintable_object =
            GameObject.FindGameObjectWithTag("paintable_canvas_object");

        // compute centroid and bounds
        templine.GetComponent<penLine_script>().computeCentroid();
        templine.GetComponent<penLine_script>().computeBounds();

        templine.GetComponent<LineRenderer>().enabled = false;
        templine.GetComponent<TrailRenderer>().enabled = false;

        GameObject meshObj = new GameObject("_meshobj");
        meshObj.AddComponent<SpriteRenderer>();

        meshObj.GetComponent<SpriteRenderer>().sprite = sprite;

        // add a collider
        templine.AddComponent<BoxCollider>();

        templine.GetComponent<BoxCollider>().size = meshObj.GetComponent<SpriteRenderer>().sprite.bounds.size;
        templine.GetComponent<BoxCollider>().center = new Vector3(0, 0, 0);
        //Debug.Log("box collider after: " + templine.GetComponent<BoxCollider>().center.ToString());

        // set collider trigger
        templine.GetComponent<BoxCollider>().isTrigger = true;

        // enable the collider and enable pan mode
        templine.GetComponent<BoxCollider>().enabled = true;
        Paintable_Script.pan_button.GetComponent<AllButtonsBehavior>().whenSelected();

        // set transform position
        templine.transform.position = new Vector3(0, 0, -40);

        // update the previous_position variable for templine for checkMove()
        templine.GetComponent<penLine_script>().previous_position = templine.transform.position;

        meshObj.transform.SetParent(templine.transform);

        // set sibling index. I want this to be Child 0, and the param_text component child 1.
        meshObj.transform.SetAsFirstSibling();

        // Save the area of the bounding box 
        templine.GetComponent<penLine_script>().attribute.Area =
            meshObj.GetComponent<SpriteRenderer>().sprite.bounds.size.x *
            meshObj.GetComponent<SpriteRenderer>().sprite.bounds.size.y * unitScale * unitScale;

        // set current_attribute of penLine
        templine.GetComponent<penLine_script>().current_attribute =
            templine.GetComponent<penLine_script>().attribute.Length;

        // set the _name field of penline
        templine.GetComponent<penLine_script>()._name = "line";
        templine.GetComponent<penLine_script>().symbol_name = new Symbol("line");

        // now transform all points in the line script to local positions
        templine.GetComponent<penLine_script>().fromGlobalToLocalPoints();

        // set up the text labels for transformation GUI (param_text)
        templine.transform.GetChild(1).localScale = new Vector3(4, 4, 1);
        templine.transform.GetChild(2).localScale = new Vector3(4, 4, 1);
        templine.transform.GetChild(4).GetChild(0).localScale = new Vector3(4, 4, 1);

        // set the box collider as the size of the rect transform
        templine.transform.GetChild(4).GetChild(0).GetComponent<BoxCollider2D>().size =
            templine.transform.GetChild(4).GetChild(0).GetComponent<RectTransform>().sizeDelta;

        templine.transform.GetChild(4).GetChild(0).GetComponent<MeshRenderer>().enabled = false;
        templine.transform.GetChild(4).GetChild(0).GetComponent<BoxCollider2D>().enabled = false;

        templine.transform.GetChild(2).GetComponent<MeshRenderer>().enabled = false;
        templine.transform.GetChild(2).GetComponent<BoxCollider2D>().enabled = false;

        templine.transform.GetChild(1).GetComponent<MeshRenderer>().enabled = false;
        templine.transform.GetChild(1).GetComponent<BoxCollider2D>().enabled = false;
        // turn off the length (temporary display) text
        templine.transform.GetChild(2).GetComponent<TextMeshPro>().text = "0.";

        // Set up the argument_label button and text.
        // get the highest point on the pen line, in local coordinates
        templine.transform.GetChild(3).gameObject.SetActive(true);

        templine.GetComponent<penLine_script>().computeBounds();
        templine.GetComponent<penLine_script>().computeCentroid();

        templine.transform.GetChild(3).transform.localPosition =
            templine.GetComponent<penLine_script>().points[
                HighestMeshPointIndex(templine.GetComponent<penLine_script>().points)
                ];
        //new Vector3(templine.GetComponent<penLine_script>().centroid.x,
        //templine.GetComponent<penLine_script>().maxy, 0);   // z is already -40, hence putting 0 here in local.

        templine.transform.GetChild(3).GetComponent<RectTransform>().sizeDelta = new Vector2(20, 20);

        templine.transform.GetChild(3).GetChild(0).GetComponent<TextMeshProUGUI>().text = "";

        templine.transform.GetChild(3).gameObject.SetActive(false); // turn it off now. Needed only for double function.

        // Don't check for membership in this case. Leave it be.

        return templine;
    }*/

    // ASSUMES: 1. points are in global space (not local)
    public GameObject CreatePenLine(GameObject[] penlines)
    {
        GameObject templine;

        List<GameObject> pen_meshobjs = new List<GameObject>();

        List<Vector3> points = new List<Vector3>();

        Color newColor;

        int totalLength = 0, totalArea = 0;

        for (int k = 0; k < penlines.Length; k++)
        {
            if (penlines[k].GetComponent<MeshFilter>().sharedMesh != null)
        {                   
            pen_meshobjs.Add(penlines[k].transform.gameObject);

            points.AddRange(penlines[k].GetComponent<iconicElementScript>().points);

            totalLength += (int)penlines[k].GetComponent<iconicElementScript>().attribute.Length;
        }
        }

        // stop if no penline mesh objs found
        if (pen_meshobjs.Count == 0) return null;
        //else Debug.Log("total penline objects "+ pen_meshobjs.Count.ToString());

        newColor = penlines[0].GetComponent<iconicElementScript>().icon_elem_material.color;

        CombineInstance[] combine = new CombineInstance[pen_meshobjs.Count];

        for (int i = 0; i < pen_meshobjs.Count; i++)
        {
            combine[i].mesh = pen_meshobjs[i].GetComponent<MeshFilter>().sharedMesh;
            combine[i].transform = pen_meshobjs[i].GetComponent<MeshFilter>().transform.localToWorldMatrix;
            //meshFilters[i].gameObject.SetActive(false);
        }

        Mesh cmesh = new Mesh();
        cmesh.CombineMeshes(combine);

        paintable_object.GetComponent<Paintable>().totalLines++;

        templine = Instantiate(paintable_object.GetComponent<Paintable>().IconicElement, paintable_object.GetComponent<Paintable>().Objects_parent.transform);
        templine.GetComponent<TrailRenderer>().material.color = Color.black;

        
        templine.name = "iconic_" + paintable_object.GetComponent<Paintable>().totalLines.ToString();
        templine.tag = "iconic";


        templine.transform.GetComponent<MeshRenderer>().enabled = true;
        if (templine.transform.GetComponent<BoxCollider2D>() != null)
            templine.transform.GetComponent<BoxCollider2D>().enabled = false;

        // color and width
        templine.GetComponent<TrailRenderer>().material.color = newColor;
        templine.GetComponent<iconicElementScript>().icon_elem_material.color = newColor;
        templine.GetComponent<TrailRenderer>().widthMultiplier = 1f;
        templine.GetComponent<LineRenderer>().widthMultiplier = 1f;

        
        // CODE FROM POINTER.MOVE
        templine.GetComponent<iconicElementScript>().attribute.Length = totalLength;
        //templine.GetComponent<penLine_script>().calculateLengthAttributeFromPoints();

        // if no width curve exists then assign a generic width cuve (won't get used for final mesh render anyway)
        templine.GetComponent<iconicElementScript>().widthcurve.AddKey(0f, 1f);
        templine.GetComponent<iconicElementScript>().widthcurve.AddKey(1f, 1f);

        templine.GetComponent<iconicElementScript>().points = points;

        // FINISH THE TEMPLINE, PASS COMBINED MESH
        templine = FinishPenLine(templine, cmesh);

        for (int i = 0; i < pen_meshobjs.Count; i++)
            Destroy(pen_meshobjs[i]);

        //paintable_object.GetComponent<Paintable>().templine = templine;
        return templine;
    }

    // ASSUMES: 1. points are in global space
    /*public GameObject CreateSet(List<Vector3> points)
    {
        GameObject templine;

        // ==============CODE FROM POINTER.START=================
        paintable_object.GetComponent<Paintable_Script>().totalLines++;

        templine = Instantiate(SetLine, this.gameObject.transform);

        templine.name = "Set_" + paintable_object.GetComponent<Paintable_Script>().totalLines.ToString();
        templine.tag = "set";

        // disable the argument_label button
        templine.transform.GetChild(0).GetChild(2).gameObject.SetActive(false);

        //==========================================================

        // explicitly set the points
        templine.GetComponent<setLine_script>().points = points;

        // =================CODE FROM POINTER.END==================

        // set the gesture/legible layer fields: set previous_gestureType to initiate change in updateLegibleLayer().
        templine.GetComponent<setLine_script>().previous_gestureType = "";
        templine.GetComponent<setLine_script>().updateGestureType = true;

        // this one is needed for the setline script functions
        templine.GetComponent<setLine_script>().paintable_object =
            GameObject.FindGameObjectWithTag("paintable_canvas_object");

        //templine.GetComponent<LineRenderer>().useWorldSpace = false;
        templine.GetComponent<LineRenderer>().positionCount = templine.GetComponent<setLine_script>().points.Count;
        templine.GetComponent<LineRenderer>().SetPositions(templine.GetComponent<setLine_script>().points.ToArray());

        // compute centroid and bounds
        templine.GetComponent<setLine_script>().computeCentroid();
        templine.GetComponent<setLine_script>().computeBounds();

        //Debug.Log(templine.GetComponent<LineRenderer>().positionCount);
        templine.GetComponent<LineRenderer>().Simplify(1.2f);
        Vector3[] pts = new Vector3[templine.GetComponent<LineRenderer>().positionCount];
        templine.GetComponent<LineRenderer>().GetPositions(pts);
        templine.GetComponent<setLine_script>().points = new List<Vector3>(pts);
        //Debug.Log(templine.GetComponent<LineRenderer>().positionCount);

        // set transform position
        templine.transform.position = new Vector3(templine.GetComponent<setLine_script>().centroid.x,
            templine.GetComponent<setLine_script>().centroid.y, 0);

        // now transform all points in the line script to local positions
        templine.GetComponent<setLine_script>().fromGlobalToLocalPoints();

        //Debug.Log("transform position: " + templine.transform.position.ToString());

        // bake into mesh: create a new child object. Direct mesh addition in the Set messes up the transform of the mesh
        // for some reason.
        GameObject meshObj = new GameObject("_meshobj");
        meshObj.AddComponent<MeshFilter>();
        //meshObj.AddComponent<BoxCollider>();
        meshObj.AddComponent<MeshRenderer>();
        meshObj.transform.SetParent(templine.transform);

        var lineRenderer = templine.GetComponent<LineRenderer>();
        var meshFilter = meshObj.GetComponent<MeshFilter>();
        Mesh mesh = new Mesh();
        lineRenderer.BakeMesh(mesh, true);
        meshFilter.sharedMesh = mesh;

        var meshRenderer = meshObj.GetComponent<MeshRenderer>();
        //meshRenderer.sharedMaterial = templine.GetComponent<Material>();
        meshRenderer.sharedMaterial = set_line_material;

        // get rid of the line renderer?
        templine.GetComponent<LineRenderer>().enabled = false;

        // set anchor position now, after gameobject transform. Order matters.
        templine.transform.GetChild(0).position =
            templine.transform.TransformPoint(
                templine.transform.GetComponent<setLine_script>().points[
                    templine.GetComponent<setLine_script>().LowestMeshPointIndex()]
            );
        templine.transform.GetChild(0).position += new Vector3(0, 0, -40f); // set z separately

        // remember the anchor offset
        templine.GetComponent<setLine_script>().calculateAnchorOffset();

        // set the properties of the draggable anchor
        templine.transform.GetChild(0).Rotate(new Vector3(90, 0, 0));

        templine.transform.GetChild(0).localScale = new Vector3(100f, 10f, 100f) / 2f;

        // disable the anchor collider for now for free drawing of next objects. Pan mode enables the collider back.
        templine.transform.GetChild(0).GetComponent<BoxCollider>().enabled = false;

        // create a material instance
        templine.transform.GetChild(0).GetComponent<MeshRenderer>().sharedMaterial = templine.transform.GetChild(0).GetComponent<MeshRenderer>().materials[0];

        // The following takes care of some details based on where the anchor is, to move the text ahead and make it visible
        templine.transform.GetChild(0).Find("_text").transform.localPosition = new Vector3(0f, -1.5f, 0f);
        templine.transform.GetChild(0).Find("_text").GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Center;

        // set up the TeX legible layer text box
        templine.transform.GetChild(0).Find("_TeX").transform.position = templine.transform.position;
        //templine.GetComponent<setLine_script>().centroid;

        // Fit the sizeDelta within bounding box
        float bxdiff = templine.transform.GetChild(1).GetComponent<MeshFilter>().sharedMesh.bounds.max.x -
            templine.transform.GetChild(1).GetComponent<MeshFilter>().sharedMesh.bounds.min.x;
        float bydiff = templine.transform.GetChild(1).GetComponent<MeshFilter>().sharedMesh.bounds.max.y -
            templine.transform.GetChild(1).GetComponent<MeshFilter>().sharedMesh.bounds.min.y;
        float range1param = Mathf.InverseLerp(30f, 300f, bxdiff);
        //float range1param = Mathf.Lerp(30f, 300f, bxdiff);
        float sd = Mathf.LerpUnclamped(1, 5, range1param); // possible values for sizeDelta = [1, 5].
        templine.transform.GetChild(0).Find("_TeX").GetComponent<RectTransform>().sizeDelta = new Vector2(sd, sd);

        // turn off the text component until abstraction slider asks for the legible layer
        templine.transform.GetChild(0).Find("_TeX").GetComponent<TEXDraw>().enabled = false;

        // disable trail renderer, no longer needed
        templine.GetComponent<TrailRenderer>().enabled = false;

        // Set up the argument_label button and text.
        templine.transform.GetChild(0).GetChild(2).gameObject.SetActive(false);

        templine.transform.GetChild(0).GetChild(2).GetComponent<RectTransform>().sizeDelta = new Vector2(0.5f, 0.5f);
        templine.transform.GetChild(0).GetChild(2).GetChild(0).GetComponent<RectTransform>().sizeDelta = new Vector2(0.5f, 0.5f);   // TMP text
        templine.transform.GetChild(0).GetChild(2).GetChild(0).GetComponent<TextMeshProUGUI>().text =
            templine.GetComponent<setLine_script>().this_set_name;

        // set the argument label position at the highest (max y) of points. Use global position (TransformPoint)
        templine.transform.GetChild(0).GetChild(2).transform.position =
            templine.transform.TransformPoint(
                templine.transform.GetComponent<setLine_script>().points[
            templine.GetComponent<setLine_script>().HighestMeshPointIndex()]
            );

        templine.transform.GetChild(0).GetChild(2).gameObject.SetActive(true);

        // check what pencil lines were included when drawing the set, and make them children of this set
        // find all game objects under Paintable canvas
        // the pen lines should be made a child after the transform position of the parent is calculated.
        // otherwise, there might be unwanted offset.

        //GameObject[] penLines = GameObject.FindGameObjectsWithTag("penline");

        // transform.childcount changes dynamically as we change the children's parent to a set,
        // so don't use it in an increasing for-loop: it ends up leaving out alternate elements
        for (int i = transform.childCount - 1; i > -1; i--)
        //for(int i = 0; i < penLines.Length; i++)
        {
            // find all pen lines
            if (transform.GetChild(i).name.Contains("penLine_"))
            {
                // check if the lines are inside the drawn set polygon -- in respective local coordinates
                if (templine.GetComponent<setLine_script>().isInsidePolygon(
                    templine.GetComponent<setLine_script>().transform.InverseTransformPoint(
                    transform.GetChild(i).transform.position)
                    ))
                {
                    transform.GetChild(i).SetParent(templine.transform);
                    //Debug.Log("parent found");

                }
            }
        }

        // update the feature and text on the anchor
        templine.GetComponent<setLine_script>().updateFeature();

        return templine;
    }
    */

    // ASSUMES: 1. points are in global space
    /*public GameObject CreateFunction(List<Vector3> points)
    {
        GameObject templine;

        // ==============CODE FROM POINTER.START=================
        paintable_object.GetComponent<Paintable_Script>().totalLines++;

        templine = Instantiate(FunctionLine, this.gameObject.transform);

        templine.name = "Function_" + paintable_object.GetComponent<Paintable_Script>().totalLines.ToString();
        templine.tag = "function";

        // disable the argument_label button
        templine.transform.GetChild(0).GetChild(2).gameObject.SetActive(false);

        //==========================================================

        // explicitly set the points
        templine.GetComponent<functionLine_script>().points = points;

        // =================CODE FROM POINTER.END==================

        // set the gesture/legible layer fields: set previous_gestureType to initiate change in updateLegibleLayer().
        templine.GetComponent<functionLine_script>().previous_gestureType = "";
        templine.GetComponent<functionLine_script>().updateGestureType = true;

        // this one is needed for the setline script functions
        templine.GetComponent<functionLine_script>().paintable_object =
            GameObject.FindGameObjectWithTag("paintable_canvas_object");

        //templine.GetComponent<LineRenderer>().useWorldSpace = false;
        templine.GetComponent<LineRenderer>().positionCount = templine.GetComponent<functionLine_script>().points.Count;
        templine.GetComponent<LineRenderer>().SetPositions(templine.GetComponent<functionLine_script>().points.ToArray());

        // compute centroid and bounds
        templine.GetComponent<functionLine_script>().computeCentroid();
        templine.GetComponent<functionLine_script>().computeBounds();

        //Debug.Log(templine.GetComponent<LineRenderer>().positionCount);
        templine.GetComponent<LineRenderer>().Simplify(1.2f);
        Vector3[] pts = new Vector3[templine.GetComponent<LineRenderer>().positionCount];
        templine.GetComponent<LineRenderer>().GetPositions(pts);
        templine.GetComponent<functionLine_script>().points = new List<Vector3>(pts);
        //Debug.Log(templine.GetComponent<LineRenderer>().positionCount);

        // set transform position
        templine.transform.position = new Vector3(templine.GetComponent<functionLine_script>().centroid.x,
            templine.GetComponent<functionLine_script>().centroid.y, 0);

        // now transform all points in the line script to local positions
        templine.GetComponent<functionLine_script>().fromGlobalToLocalPoints();

        //Debug.Log("transform position: " + templine.transform.position.ToString());

        // bake into mesh: create a new child object. Direct mesh addition in the Set messes up the transform of the mesh
        // for some reason.
        GameObject meshObj = new GameObject("_meshobj");
        meshObj.AddComponent<MeshFilter>();
        //meshObj.AddComponent<BoxCollider>();
        meshObj.AddComponent<MeshRenderer>();
        meshObj.transform.SetParent(templine.transform);

        var lineRenderer = templine.GetComponent<LineRenderer>();
        var meshFilter = meshObj.GetComponent<MeshFilter>();
        Mesh mesh = new Mesh();
        lineRenderer.BakeMesh(mesh, true);
        meshFilter.sharedMesh = mesh;

        var meshRenderer = meshObj.GetComponent<MeshRenderer>();
        //meshRenderer.sharedMaterial = templine.GetComponent<Material>();
        meshRenderer.sharedMaterial = function_line_material;

        // get rid of the line renderer?
        templine.GetComponent<LineRenderer>().enabled = false;

        // set anchor position now, after gameobject transform. Order matters.
        templine.transform.GetChild(0).position =
            templine.transform.TransformPoint(
                templine.transform.GetComponent<functionLine_script>().points[
                    templine.GetComponent<functionLine_script>().LowestMeshPointIndex()]
            );
        templine.transform.GetChild(0).position += new Vector3(0, 0, -40f); // set z separately

        // remember the anchor offset
        templine.GetComponent<functionLine_script>().calculateAnchorOffset();

        // set the properties of the draggable anchor -- children objects and components are affected too.
        templine.transform.GetChild(0).Rotate(new Vector3(90, 0, 0));
        // scale it so it's a tapered in the horizontal direction (to accommodate the text)
        templine.transform.GetChild(0).localScale = new Vector3(200f, 10f, 100f) / 2f; // prev. value: new Vector3(100f, 10f, 100f) / 2f

        // turn off the anchor box collider for now. Pan mode enables it back.
        templine.transform.GetChild(0).GetComponent<BoxCollider>().enabled = false;

        // create a material instance
        templine.transform.GetChild(0).GetComponent<MeshRenderer>().sharedMaterial = templine.transform.GetChild(0).GetComponent<MeshRenderer>().materials[0];

        //	------- WHY DOESN'T THIS ROTATION WORK FROM INSIDE THE SCRIPT???? --------
        //templine.transform.GetChild(0).GetChild(0).transform.Rotate(new Vector3(90, 0, 0));
        //templine.transform.GetChild(0).Find("_text").transform.Rotate(new Vector3(90, 0, 0));

        // The following takes care of some details based on where the anchor is, to move the text ahead and make it visible
        templine.transform.GetChild(0).Find("_anchor_TeX").transform.localPosition = new Vector3(0f, -1.5f, 0f);
        //templine.transform.GetChild(0).Find("_text").GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Center;
        // undo the horizontal tapered scaling for the text rect transform
        templine.transform.GetChild(0).Find("_anchor_TeX").transform.localScale = new Vector3(0.005f, 0.01f, 0.005f);

        // set text container size wrt anchor
        //templine.transform.GetChild(0).Find("_text").GetComponent<TextMeshProUGUI>().GetComponent<TextContainer>().width = 

        // set up the TeX legible layer text box
        templine.transform.GetChild(0).Find("_TeX").transform.position =
            templine.GetComponent<functionLine_script>().centroid;

        // Fit the sizeDelta within bounding box

        float bxdiff = templine.transform.GetChild(1).GetComponent<MeshFilter>().sharedMesh.bounds.max.x -
            templine.transform.GetChild(1).GetComponent<MeshFilter>().sharedMesh.bounds.min.x;
        float bydiff = templine.transform.GetChild(1).GetComponent<MeshFilter>().sharedMesh.bounds.max.y -
            templine.transform.GetChild(1).GetComponent<MeshFilter>().sharedMesh.bounds.min.y;
        float range1param = Mathf.InverseLerp(30f, 300f, bxdiff);
        //float range1param = Mathf.Lerp(30f, 300f, bxdiff);
        float sd = Mathf.LerpUnclamped(5, 20, range1param); // possible values for sizeDelta = [1, 5].
        templine.transform.GetChild(0).Find("_TeX").GetComponent<RectTransform>().sizeDelta = new Vector2(sd, sd);

        // turn off the text component until abstraction slider asks for the legible layer
        templine.transform.GetChild(0).Find("_TeX").GetComponent<TEXDraw>().enabled = false;

        // disable trail renderer, no longer needed
        templine.GetComponent<TrailRenderer>().enabled = false;

        // Set up the argument_label button and text.
        templine.transform.GetChild(0).GetChild(2).gameObject.SetActive(true);

        templine.transform.GetChild(0).GetChild(2).transform.localScale = new Vector3(0.3f, 0.6f, 1f);  // undo the scale of anchor, and make it smaller too.
        templine.transform.GetChild(0).GetChild(2).GetComponent<RectTransform>().sizeDelta = new Vector2(0.6f, 0.6f);
        templine.transform.GetChild(0).GetChild(2).GetChild(0).GetComponent<RectTransform>().sizeDelta = new Vector2(0.3f, 0.3f);   // TMP text
        templine.transform.GetChild(0).GetChild(2).GetChild(0).GetComponent<TextMeshProUGUI>().text =
            templine.GetComponent<functionLine_script>().currentOperator();

        templine.transform.GetChild(0).GetChild(2).position =
            templine.transform.TransformPoint(
                templine.transform.GetComponent<functionLine_script>().points[
                    templine.GetComponent<functionLine_script>().HighestMeshPointIndex()]
            );
        templine.transform.GetChild(0).GetChild(2).position += new Vector3(0, 0, -40f); // set z separately

        // set up the _top_TeX label with category filters
        templine.transform.GetChild(0).GetChild(3).position =
            templine.transform.TransformPoint(
                templine.transform.GetComponent<functionLine_script>().points[
                    templine.GetComponent<functionLine_script>().HighestMeshPointIndex()]);

        templine.transform.GetChild(0).GetChild(3).localPosition += new Vector3(0, 0, -0.3f);   // move slightly upwards locally, z is y b/c of anchor rotation
        templine.transform.GetChild(0).GetChild(3).localScale = new Vector3(0.005f, 0.01f, 1); // undo the scale of anchor, and make it smaller too.

        templine.transform.GetChild(0).GetChild(3).GetComponent<TEXDraw>().text = "(), " + templine.name + ", \\legbox";

        // check what pencil lines were included when drawing the set, and make them children of this set
        // find all game objects under Paintable canvas
        // the pen lines should be made a child after the transform position of the parent is calculated.
        // otherwise, there might be unwanted offset.

        // transform.childcount changes dynamically as we change the children's parent to a set,
        // so don't use it in an increasing for-loop: it ends up leaving out alternate elements
        for (int i = transform.childCount - 1; i > -1; i--)
        {
            // find all sets/containers
            if (transform.GetChild(i).tag == "set")
            {
                // check if they are inside the drawn function polygon
                if (templine.GetComponent<functionLine_script>().isInsidePolygon(
                    templine.GetComponent<functionLine_script>().transform.InverseTransformPoint(
                    transform.GetChild(i).GetChild(0).transform.position)   // check if the anchor falls inside the function lasso. NOT the default transform.position.
                    ))
                {
                    transform.GetChild(i).SetParent(templine.transform);
                    //Debug.Log("parent found");
                }
            }

            // find all functions
            else if (transform.GetChild(i).tag == "function")
            {
                // check if they are inside the drawn function polygon
                if (templine.GetComponent<functionLine_script>().isInsidePolygon(
                    templine.GetComponent<functionLine_script>().transform.InverseTransformPoint(
                    transform.GetChild(i).GetChild(0).transform.position)   // check if the anchor falls inside the function lasso. NOT the default transform.position.
                    ))
                {
                    transform.GetChild(i).SetParent(templine.transform);
                    //Debug.Log("parent found");
                }
            }

            // find all penlines
            else if (transform.GetChild(i).tag == "penline")
            {
                // check if they are inside the drawn function polygon
                if (templine.GetComponent<functionLine_script>().isInsidePolygon(
                    templine.GetComponent<functionLine_script>().transform.InverseTransformPoint(
                    transform.GetChild(i).GetComponent<penLine_script>().transform.position)
                    ))
                {
                    transform.GetChild(i).SetParent(templine.transform);
                    //Debug.Log("parent found");
                }
            }
        }

        // update the feature and text on the anchor
        templine.GetComponent<functionLine_script>().updateFeature();

        return templine;
    }*/

    public int HighestMeshPointIndex(List<Vector3> points)
    {
        // which local point is the highest? Return its index.
        // find maxy from all the points
        float mxy = -100000f;
        int ind = -1;
        for (int i = 0; i < points.Count; i++)
        {
            if (points[i].y > mxy)
            {
                mxy = points[i].y;
                ind = i;
            }
        }
        return ind;
    }

    public int LowestMeshPointIndex(List<Vector3> points)
    {
        // which local point is the highest? Return its index.
        // find maxy from all the points
        float mny = 100000f;
        int ind = -1;
        for (int i = 0; i < points.Count; i++)
        {
            if (points[i].y < mny)
            {
                mny = points[i].y;
                ind = i;
            }
        }
        return ind;
    }
}
