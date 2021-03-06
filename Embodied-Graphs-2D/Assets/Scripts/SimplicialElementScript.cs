using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//https://forum.unity.com/threads/draw-polygon.54092/
public class SimplicialElementScript : MonoBehaviour
{
    public List<Vector3> theVertices;
    public List<GameObject> thenodes;
    public Material myMaterial;
    public GameObject dot_prefab;
    public GameObject paintable;

    // for directed edge
    public bool directed_edge = false;
    public Sprite directed_edge_sprite;

    // Start is called before the first frame update
    void Start()
    {
        Material new_material = new Material(transform.GetComponent<MeshRenderer>().material);
        new_material.SetColor("_Color", paintable.GetComponent<Paintable>().color_picker_script.color);
        transform.GetComponent<MeshRenderer>().material = new_material;
    }

    // the simplex will start from corner, not center
    public void UpdateVertices()
    {
        for (int i = 0; i < thenodes.Count; i++)
        {
            int j = (i + 1) % thenodes.Count;
            theVertices[i] = thenodes[i].GetComponent<iconicElementScript>().getclosestpoint(thenodes[j].GetComponent<iconicElementScript>().edge_position);

        }
    }

    public void addDot()
    {
        int child_count = transform.childCount;

        if (child_count == 0)
        {
            for (int x = 0; x < theVertices.Count; x++)
            {
                GameObject temp = Instantiate(dot_prefab, theVertices[x], Quaternion.identity, transform);
                temp.name = "dot_child";
                temp.transform.parent = transform;
                temp.transform.SetSiblingIndex(x);

                Color mat_color = transform.GetComponent<MeshRenderer>().material.color;
                mat_color = new Color(Mathf.Clamp(mat_color.r - 0.1f, 0f, 1f),
                    Mathf.Clamp(mat_color.g - 0.1f, 0f, 1f),
                    Mathf.Clamp(mat_color.b - 0.1f, 0f, 1f),
                    mat_color.a);

                LineRenderer lr = temp.GetComponent<LineRenderer>();
                lr.enabled = true;
                lr.material.SetColor("_Color", mat_color);

                if (directed_edge && x == 1)
                    temp.GetComponent<SpriteRenderer>().sprite = directed_edge_sprite;
            }
        }
        else
        {
            for (int x = 0; x < theVertices.Count; x++)
            {
                Transform temp = transform.GetChild(x);
                temp.position = theVertices[x];
            }
        }

        updateLine();
    }

    public void updateLine()
    {
        for (int x = 0; x < theVertices.Count; x++)
        {
            Transform temp = transform.GetChild(x);
            LineRenderer lr = temp.GetComponent<LineRenderer>();

            int y = (x + 1) % theVertices.Count;
            lr.SetPosition(0, theVertices[x]);
            lr.SetPosition(1, theVertices[y]);

            temp.position = theVertices[x];
        }
    }

    public void updatePolygon()
    {
        //UpdateVertices();

        //New mesh and game object
        GameObject myObject = this.transform.gameObject;
            
        //Components
        var MF = myObject.GetComponent<MeshFilter>();
        var MR = myObject.GetComponent<MeshRenderer>();
        var BC = myObject.GetComponent<BoxCollider>();

        //Create mesh
        var mesh = CreateMesh();
        //Assign materials
        MR.material = myMaterial;
        //Assign mesh to game object
        MF.mesh = mesh;

        BC.size = MF.mesh.bounds.size;
        BC.center = MF.mesh.bounds.center;
        //Debug.Log("box collider after: " + templine.GetComponent<BoxCollider>().center.ToString());

        // set collider trigger
        BC.isTrigger = true;
        BC.enabled = false;

        addDot();
    }

    Mesh CreateMesh()
    {
        int x; //Counter

        //Create a new mesh
        Mesh mesh = new Mesh();

        //Vertices
        Vector3[] vertex = new Vector3[theVertices.Count];

        for (x = 0; x < theVertices.Count; x++)
        {
            vertex[x] = theVertices[x];
        }

        //UVs
        var uvs = new Vector2[vertex.Length];

        for (x = 0; x < vertex.Length; x++)
        {
            if ((x % 2) == 0)
            {
                uvs[x] = new Vector2(0, 0);
            }
            else
            {
                uvs[x] = new Vector2(1, 1);
            }
        }

        //Triangles
        var tris = new int[3 * (vertex.Length - 2)];    //3 verts per triangle * num triangles
        int C1, C2, C3;        
        C1 = 0;
        C2 = 1;
        C3 = 2;

        for (x = 0; x < tris.Length; x += 3)
        {
            tris[x] = C1;
            tris[x + 1] = C2;
            tris[x + 2] = C3;

            C2++;
            C3++;
        } 

        //Assign data to mesh
        mesh.vertices = vertex;
        mesh.uv = uvs;
        mesh.triangles = tris;

        //Recalculations
        mesh.RecalculateNormals();
        mesh.RecalculateBounds();
        mesh.Optimize();

        //Name the mesh
        mesh.name = "MyMesh";

        //Return the mesh
        return mesh;
    }


    // Update is called once per frame
    void Update()
    {
        
    }

    void OnDestroy()
    {
        Transform node_parent = transform.parent;
        if (node_parent.tag == "simplicial_parent")
        {
            node_parent.parent.GetComponent<GraphElementScript>().simplicial_init();
            //node_parent.parent.GetComponent<GraphElementScript>().simplicial_as_Str();
        }
    }
}
