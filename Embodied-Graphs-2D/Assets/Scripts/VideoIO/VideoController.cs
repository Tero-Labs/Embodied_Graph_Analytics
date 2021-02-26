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

    // Start is called before the first frame update
    void Start()
    {

    }

    public void loadAnnotation(string filename)
    {
        Debug.Log("filename:" + filename);
        frames_annotation = JsonUtility.FromJson<frames>(File.ReadAllText(filename));
        Debug.Log(JsonUtility.ToJson(frames_annotation.all_frame[0].objects[0]));        
    }

    // Update is called once per frame
    void Update()
    {
        if (videoplayer.frameCount > 0)
        {
            mainSlider.value = (float)videoplayer.frame / (float)videoplayer.frameCount;
        }

        if (videoplayer.frame %5 == 0)
        {
            Debug.Log("current_frame: " + videoplayer.frame.ToString());

            frame cur_frame = frames_annotation.all_frame[(int)videoplayer.frame];
            bounds first_obj = cur_frame.objects[0].bounds[0];

            Debug.Log("transformed_pt: " + videoplayer.transform.InverseTransformPoint(new Vector3(first_obj.x, first_obj.y, -5f)));
            //Debug.Log(JsonUtility.ToJson(frames_annotation.all_frame[(int)videoplayer.frame]));
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
