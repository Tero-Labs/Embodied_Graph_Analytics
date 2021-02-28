using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class VideoPlayerChildrenAccess : MonoBehaviour
{
    public GameObject slider, PlayorPause, quad;
    public Canvas canvas;
    public Button settings;
    public GameObject settings_menu;
    // Start is called before the first frame update
    void Start()
    {
        settings.onClick.AddListener(delegate { SettingsMenu(); });
    }

    void SettingsMenu()
    {
        settings_menu.SetActive(!settings_menu.activeSelf);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
