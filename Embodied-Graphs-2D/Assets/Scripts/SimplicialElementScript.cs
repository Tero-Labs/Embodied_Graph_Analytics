using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//https://forum.unity.com/threads/draw-polygon.54092/
public class SimplicialElementScript : MonoBehaviour
{
    public List<Vector3> theVertices;
    public List<GameObject> thenodes;
    public Material myMaterial;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    public void updatePolygon()
    {        
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
