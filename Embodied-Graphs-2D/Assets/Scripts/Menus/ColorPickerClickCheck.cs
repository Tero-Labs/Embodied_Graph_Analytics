using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class ColorPickerClickCheck : MonoBehaviour
{
    public bool pointer;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (EventSystem.current.IsPointerOverGameObject(0))
        {
            pointer = true;
            //Debug.Log("currently over color picker");
        }
        else
        {
            pointer = false;
        }
    }
}
