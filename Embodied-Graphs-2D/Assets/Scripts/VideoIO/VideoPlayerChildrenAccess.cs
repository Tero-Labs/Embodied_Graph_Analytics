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
}
