using System.Collections;
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
    GameObject temp_parent;

    // Start is called before the first frame update
    void Start()
    {

    }

    public void loadAnnotation(string filename)
    {
        Debug.Log("filename:" + filename);
        frames_annotation = JsonUtility.FromJson<frames>(File.ReadAllText(filename));
        Debug.Log(JsonUtility.ToJson(frames_annotation.all_frame[0].objects[0]));

        width = videoplayer.transform.localScale.x;
        height = videoplayer.transform.localScale.y;
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
            if (temp_parent != null) Destroy(temp_parent);
            temp_parent = new GameObject();

            List<GameObject> all_icons = new List<GameObject>();
            List<tracked_object> all_objects = frames_annotation.all_frame[(int)videoplayer.frame].objects;
            Vector3 edge_pos = Vector3.zero;

            foreach (tracked_object cur_obj in all_objects)
            {
                GameObject temp = Instantiate(icon_prefab, temp_parent.transform);
                all_icons.Add(temp);

                temp.GetComponent<TrailRenderer>().enabled = false;
                temp.GetComponent<MeshRenderer>().enabled = false;

                LineRenderer l = temp.GetComponent<LineRenderer>();
                l.material.color = Color.black;
                l.startWidth = 2f;
                l.endWidth = 2f;

                List<Vector3> points = new List<Vector3>();
                
                foreach (bounds first_obj in cur_obj.bounds)
                {
                    float lerped_x = Mathf.Lerp(videoplayer.transform.position.x - (width / 2), videoplayer.transform.position.x + (width / 2), Mathf.InverseLerp(1, 853, first_obj.x));
                    float lerped_y = Mathf.Lerp(videoplayer.transform.position.y + (height / 2), videoplayer.transform.position.y - (height / 2), Mathf.InverseLerp(1, 480, first_obj.y));

                    Vector3 pos_vec = new Vector3(lerped_x, lerped_y, -5f);
                    edge_pos += pos_vec;

                    points.Add(pos_vec);
                    //Debug.Log("transformed_pt_lerped: " + pos_vec);
                    //Debug.Log("transformed_pt: " + videoplayer.transform.InverseTransformPoint(new Vector3(first_obj.x, first_obj.y, -5f)));
                    //Debug.Log(JsonUtility.ToJson(frames_annotation.all_frame[(int)videoplayer.frame]));
                }

                // connect the end and statr position as well
                l.loop = true;
                // if we don't manually change the position count, it only takes the first two positions
                l.positionCount = points.Count;
                l.SetPositions(points.ToArray());
                //temp.transform.localScale = new Vector3(2f, 2f, 2f);

                edge_pos = edge_pos / points.Count;
                temp.GetComponent<iconicElementScript>().edge_position = edge_pos;


            }

            //videoplayer.Pause();
            Debug.Log("all_icons: " + all_icons.Count.ToString());

            // create graph based on node radius
            // tODo: try updated algorithm
            for (int i = 0; i < all_icons.Count; i++ )
            {
                for (int j = (i+1); j < all_icons.Count; j++)
                {
                    if (Vector3.Distance(all_icons[i].GetComponent<iconicElementScript>().edge_position, 
                        all_icons[j].GetComponent<iconicElementScript>().edge_position) < 10f)
                    {
                        GameObject temp = Instantiate(edge_prefab, all_icons[i].GetComponent<iconicElementScript>().edge_position, Quaternion.identity, temp_parent.transform);
                        temp.GetComponent<EdgeElementScript>().edge_start = all_icons[i];
                        temp.GetComponent<EdgeElementScript>().edge_end = all_icons[j];
                        temp.GetComponent<EdgeElementScript>().addDot();
                        temp.GetComponent<EdgeElementScript>().updateEndPoint();
                    }
                }
            }
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
