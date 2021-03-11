using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class VideoPlayerChildrenAccess : MonoBehaviour
{
    public GameObject slider, PlayorPause, quad;
    public Canvas canvas;
    public Button settings, delete;
    public GameObject settings_menu;
    public GameObject control_menu;
    public GameObject paintable;

    public InputField mainInputField;
    public Toggle node_radius, site_specific;
    public Toggle auto_track, manual_track;

    // Start is called before the first frame update
    void Start()
    {
        settings.onClick.AddListener(delegate { SettingsMenu(); });
        delete.onClick.AddListener(delegate { Delete(); });
        mainInputField.onValueChanged.AddListener(delegate { LockInput(mainInputField); });

        node_radius.onValueChanged.AddListener(delegate { GraphType(node_radius); });
        site_specific.onValueChanged.AddListener(delegate { GraphType(site_specific); });

        auto_track.onValueChanged.AddListener(delegate { TrackType(auto_track); });
        manual_track.onValueChanged.AddListener(delegate { TrackType(manual_track); });

        // to setup initial values
        GraphType(node_radius);
        TrackType(auto_track);
    }

    void LockInput(InputField input)
    {
        // if a function is getting evaluated now, we will not receive any further input
        /*if (paintable.GetComponent<Paintable>().no_func_menu_open)
            return;*/
        if (input.text.Length > 0)
        {
            float result = slider.GetComponent<VideoController>().node_radius;
            float.TryParse(input.text, out result);
            slider.GetComponent<VideoController>().node_radius = result;
        }
    }

    void GraphType(Toggle toggle)
    {        
        if (site_specific.isOn) slider.GetComponent<VideoController>().graph_type = "SiteSpecific";
        else slider.GetComponent<VideoController>().graph_type = "NodeRadius";

        mainInputField.interactable = node_radius.isOn;
    }

    void TrackType(Toggle toggle)
    {
        slider.GetComponent<VideoController>().auto_track = auto_track.isOn;
    }

    void SettingsMenu()
    {
        settings_menu.SetActive(!settings_menu.activeSelf);
    }

    void Delete()
    {
        if (slider.GetComponent<VideoController>().graph_holder != null)
        {
            Destroy(slider.GetComponent<VideoController>().graph_holder);
        }

        Destroy(transform.gameObject);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void checkHitAndMove(Vector3 diff)
    {
        transform.position += diff;
        control_menu.transform.position += diff;
        settings_menu.transform.position += diff;
        slider.GetComponent<VideoController>().graph_holder.transform.position += diff;
    }

}
