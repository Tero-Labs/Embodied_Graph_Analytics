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
    // Start is called before the first frame update
    void Start()
    {
        settings.onClick.AddListener(delegate { SettingsMenu(); });
        delete.onClick.AddListener(delegate { Delete(); });
    }

    void SettingsMenu()
    {
        settings_menu.SetActive(!settings_menu.activeSelf);
    }

    void Delete()
    {
        if (slider.GetComponent<VideoController>().temp_parent != null)
        {
            Destroy(slider.GetComponent<VideoController>().temp_parent);
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
        slider.GetComponent<VideoController>().temp_parent.transform.position += diff;
    }

}
