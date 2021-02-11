using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class GraphLabelScript : MonoBehaviour
{
    public TMP_Text tmptextlabel;
    bool draggable_now;
    private Vector3 touchDelta = new Vector3();
    public Image img;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (!(transform.parent.GetComponent<GraphElementScript>().paintable.GetComponent<Paintable>().panZoomLocked))
        {
            checkHitAndMove();
        }
    }

    public void checkHitAndMove()
    {

        if (PenTouchInfo.PressedThisFrame)
        {
            if (TMP_TextUtilities.IsIntersectingRectTransform(tmptextlabel.rectTransform, PenTouchInfo.penPosition, 
                transform.parent.GetComponent<GraphElementScript>().paintable.GetComponent<Paintable>().main_camera))
            {
                Debug.Log("started drag");

                draggable_now = true;
                Vector3 vec = Vector3.zero;
                RectTransformUtility.ScreenPointToWorldPointInRectangle(tmptextlabel.rectTransform, PenTouchInfo.penPosition,
                    transform.parent.GetComponent<GraphElementScript>().paintable.GetComponent<Paintable>().main_camera, out vec);

                // enforce the same z coordinate as the rest of the points in the parent set object
                vec.z = -5f;

                touchDelta = transform.GetChild(0).position - vec;
                // change anchor color
                img.color = Color.gray;
            }

            else
            {
                return;
            }
        }

        else if (PenTouchInfo.PressedNow)
        {

            if (TMP_TextUtilities.IsIntersectingRectTransform(tmptextlabel.rectTransform, PenTouchInfo.penPosition,
                transform.parent.GetComponent<GraphElementScript>().paintable.GetComponent<Paintable>().main_camera))
            {
                //Debug.Log(transform.name);

                draggable_now = true;
                Vector3 vec = Vector3.zero;
                RectTransformUtility.ScreenPointToWorldPointInRectangle(tmptextlabel.rectTransform, PenTouchInfo.penPosition,
                    transform.parent.GetComponent<GraphElementScript>().paintable.GetComponent<Paintable>().main_camera, out vec);

                // enforce the same z coordinate as the rest of the points in the parent set object
                vec.z = -5f;
                Vector3 diff = vec - transform.GetChild(0).position + touchDelta;
                diff.z = 0;

                // don't move right away, move if a threshold has been crossed
                // 5 seems to work well in higher zoom levels and for my finger
                //if (Vector3.Distance(transform.position, vec) > 5)
                // update the function position. 
                transform.position += diff;

                transform.parent.GetComponent<GraphElementScript>().checkHitAndMove(diff);

            }

        }

        else if (PenTouchInfo.ReleasedThisFrame && draggable_now)//(Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Ended && draggable_now)
        {
            draggable_now = false;

            touchDelta = new Vector3(); // reset touchDelta
            img.color = new Color32(125, 255, 165, 255);            
        }

    }
}

