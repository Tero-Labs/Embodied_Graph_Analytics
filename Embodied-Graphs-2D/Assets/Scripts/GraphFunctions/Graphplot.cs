using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ChartAndGraph;
using UnityEngine.UI;
using TMPro;


public class Graphplot : MonoBehaviour
{
    public GameObject paintable;
    public Camera main_camera;

    public GraphChart Graph;
    public int TotalPoints = 5;
    float lastTime = 0f;
    float lastX = 0f;

    public Button show_graph;
    public InputField mainInputField;

    public TMP_Text arg_1;
    public TMP_Text arg_2;
    public TMP_Text arg_3;
    public GameObject arg_1_obj;
    public GameObject arg_2_obj;
    public GameObject arg_3_obj;
    public GameObject first_arg;
    public GameObject second_arg;

    public bool graph_flag;

    void Start()
    {
        main_camera = Camera.main;
        show_graph.onClick.AddListener(delegate { OnGraphShow(); });
    }

    void OnGraphShow()
    {
        graph_flag = !graph_flag;
        Graph.gameObject.SetActive(graph_flag);

        if (graph_flag)
        {
            string command = mainInputField.text;
            var args = command.Split('/');

            if (args[0] == "arg_1")
            {
                first_arg = arg_1_obj;
            }
            else if (args[0] == "arg_2")
            {
                first_arg = arg_2_obj;
            }
            else if (args[0] == "arg_3")
            {
                first_arg = arg_3_obj;
            }
            else
            {
                var temp_stat = Instantiate(paintable.GetComponent<Paintable>().status_label_obj, transform.parent);
                temp_stat.GetComponent<Status_label_text>().ChangeLabel("not enough argument");
                return;
            }

            if (args[1] == "arg_1")
            {
                second_arg = arg_1_obj;
            }
            else if (args[1] == "arg_2")
            {
                second_arg = arg_2_obj;
            }
            else if (args[1] == "arg_3")
            {
                second_arg = arg_3_obj;
            }
            else
            {
                var temp_stat = Instantiate(paintable.GetComponent<Paintable>().status_label_obj, transform.parent);
                temp_stat.GetComponent<Status_label_text>().ChangeLabel("not enough argument");
                return;
            }

            DataInit();
        }
            
    }

    void DataInit()
    {
        if (Graph == null) // the ChartGraph info is obtained via the inspector
            return;
        float x = 3f * TotalPoints;
        Graph.DataSource.StartBatch(); // calling StartBatch allows changing the graph data without redrawing the graph for every change
        Graph.DataSource.ClearCategory("Player 1"); // clear the "Player 1" category. this category is defined using the GraphChart inspector


        for (int i = 0; i < TotalPoints; i++)  //add random points to the graph
        {
            // x: time, y: distance
            Graph.DataSource.AddPointToCategory("Player 1", System.DateTime.Now - System.TimeSpan.FromSeconds(x), 0); 

            // each time we call AddPointToCategory 
            x -= Random.value * 3f;
            lastX = x;
        }

        Graph.DataSource.EndBatch(); // finally we call EndBatch , this will cause the GraphChart to redraw itself
    }

    void Update()
    {
        
        // only work in graph analysis operation        
        checkDragOrPress();

        if (Graph.gameObject.activeSelf == false) return;

        if (arg_1_obj == null || arg_2_obj == null) return;

        float time = Time.time;
        if (lastTime + 2f < time)
        {
            lastTime = time;
            lastX += Random.value * 3f;
            Graph.DataSource.AddPointToCategoryRealtime("Player 1", System.DateTime.Now, 
                Vector3.Distance(first_arg.GetComponent<iconicElementScript>().edge_position, second_arg.GetComponent<iconicElementScript>().edge_position), 
                1f);
        }

    }

    void checkDragOrPress()
    {
        if (paintable.GetComponent<Paintable>().eraser_button.GetComponent<AllButtonsBehaviors>().selected)
        {
            if (PenTouchInfo.ReleasedThisFrame)
            {
                if (TMP_TextUtilities.IsIntersectingRectTransform(arg_1.rectTransform, PenTouchInfo.penPosition, null))
                {
                    paintable.GetComponent<Paintable>().no_analysis_menu_open = false;
                    Destroy(transform.gameObject);
                }
            }
        }

        if (!paintable.GetComponent<Paintable>().AnalysisPen_button.GetComponent<AllButtonsBehaviors>().selected) return;

        if (PenTouchInfo.ReleasedThisFrame)
        {
            if (paintable.GetComponent<Paintable>().dragged_arg_textbox == null)
            {
                return;
            }

            Debug.Log("checking_drag");

            var dragged_arg_object = paintable.GetComponent<Paintable>().dragged_arg_textbox;

            if (TMP_TextUtilities.IsIntersectingRectTransform(arg_1.rectTransform, PenTouchInfo.penPosition, null))
            {
                Debug.Log("found_drag_with_arg_1");
                if (dragged_arg_object.tag == "iconic")
                {
                    arg_1.GetComponent<TextMeshProUGUI>().text =
                        dragged_arg_object.transform.GetComponent<iconicElementScript>().icon_number.ToString();
                    arg_1_obj = dragged_arg_object;
                }

                else
                    arg_1.GetComponent<TextMeshProUGUI>().text = "topo_label_or_func_label";
            }

            else if (TMP_TextUtilities.IsIntersectingRectTransform(arg_2.rectTransform, PenTouchInfo.penPosition, null))
            {
                Debug.Log("found_drag_with_arg_1");
                if (dragged_arg_object.tag == "iconic")
                {
                    arg_2.GetComponent<TextMeshProUGUI>().text =
                        dragged_arg_object.transform.GetComponent<iconicElementScript>().icon_number.ToString();
                    arg_2_obj = dragged_arg_object;
                }

                else
                    arg_2.GetComponent<TextMeshProUGUI>().text = "topo_label_or_func_label";
            }

            else if (TMP_TextUtilities.IsIntersectingRectTransform(arg_3.rectTransform, PenTouchInfo.penPosition, null))
            {
                Debug.Log("found_drag_with_arg_1");
                if (dragged_arg_object.tag == "iconic")
                {
                    arg_3.GetComponent<TextMeshProUGUI>().text =
                        dragged_arg_object.transform.GetComponent<iconicElementScript>().icon_number.ToString();
                    arg_3_obj = dragged_arg_object;
                }

                else
                    arg_3.GetComponent<TextMeshProUGUI>().text = "topo_label_or_func_label";
            }
            else
                Debug.Log("dafaq");
        }
    }
}
