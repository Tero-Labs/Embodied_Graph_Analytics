using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class topoLabelScript : MonoBehaviour
{
    public GameObject parent;
    public TMP_Text tmptextlabel;
    bool draggable;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (PenTouchInfo.PressedThisFrame)
        {
            if (TMP_TextUtilities.IsIntersectingRectTransform
                    (tmptextlabel.rectTransform, PenTouchInfo.penPosition, Camera.main))
            {
                //Debug.Log("success");
                draggable = true;
                if (parent != null) Paintable.dragged_arg_textbox = parent;
                else if (transform.parent.tag == "iconic")
                    Paintable.dragged_arg_textbox = transform.parent.gameObject;

            }
        }

        else if (PenTouchInfo.ReleasedThisFrame && draggable)
        {
            draggable = false;
        }
    }
}
