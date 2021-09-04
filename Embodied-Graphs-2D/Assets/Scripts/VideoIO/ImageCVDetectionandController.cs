using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;

// this script is basically a copy of iconicElementScript.cs, VideoPlayerChildrenAccess.cs and VideoController.cs
public class ImageCVDetectionandController : MonoBehaviour
{
    public GameObject quad;
    public Canvas canvas;
    public Button settings, delete;
    public GameObject settings_menu;
    public GameObject control_menu;

    public InputField mainInputField;
    public InputField WindowInputField;
    public Toggle node_radius, site_specific;
    public Toggle auto_track, manual_track;

    public float width, height;

    public float node_radius_val;
    public string graph_type;
    public bool auto_track_val;

    // prefabs
    public GameObject icon_prefab;
    public GameObject edge_prefab;
    public GameObject graph_prefab;
    public GameObject paintable;
    public GameObject graph_holder;

    public Sprite recognized_sprite;

    void Start()
    {
        settings.onClick.AddListener(delegate { SettingsMenu(); });
        delete.onClick.AddListener(delegate { Delete(); });
        mainInputField.onValueChanged.AddListener(delegate { LockInput(mainInputField); });

        node_radius.onValueChanged.AddListener(delegate { GraphType(); });
        site_specific.onValueChanged.AddListener(delegate { GraphType(); });

        auto_track.onValueChanged.AddListener(delegate { TrackType(auto_track); });
        manual_track.onValueChanged.AddListener(delegate { TrackType(manual_track); });

        // to setup initial values
        GraphType();
        TrackType(auto_track);

        width = quad.transform.localScale.x;
        height = quad.transform.localScale.y;
        
        //global_scope
        //control_menu.GetComponentInParent<Canvas>().worldCamera = Camera.main;

        UIlayout();
    }


    public void UIlayout()
    {
        //http://www.robotmonkeybrain.com/convert-unity-ui-screen-space-position-to-world-position/
        //https://forum.unity.com/threads/world-position-to-local-recttransform-position.445256/
        //https://forum.unity.com/threads/world-position-to-local-recttransform-position.445256/
        //https://stackoverflow.com/a/43736203       

        Vector3 temp_pos = new Vector3(quad.transform.position.x,
                                                    quad.transform.position.y - (height / 2) - 45,
                                                    0f);

        Vector3 screen_temp_pos = RectTransformUtility.WorldToScreenPoint(Camera.main, temp_pos);

        Vector2 rect_Try;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(control_menu.transform.parent.GetComponent<RectTransform>(), screen_temp_pos,
                                    null, out rect_Try);

        control_menu.GetComponent<RectTransform>().anchoredPosition = rect_Try;

        Vector3 temp_pos_2 = new Vector3(quad.transform.position.x + (width / 2) + 72,
                                                   quad.transform.position.y,
                                                   0f);

        Vector3 screen_temp_pos_2 = RectTransformUtility.WorldToScreenPoint(Camera.main, temp_pos_2);

        Vector2 rect_Try_2;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(settings_menu.transform.parent.GetComponent<RectTransform>(),
                    screen_temp_pos_2, null, out rect_Try_2);

        settings_menu.GetComponent<RectTransform>().anchoredPosition = rect_Try_2;
    }

    /*
    public void UIlayoutWorldSpace()
    {
        //http://www.robotmonkeybrain.com/convert-unity-ui-screen-space-position-to-world-position/
        //https://forum.unity.com/threads/world-position-to-local-recttransform-position.445256/
        //https://forum.unity.com/threads/world-position-to-local-recttransform-position.445256/
        //https://stackoverflow.com/a/43736203

        Vector3 temp_pos = new Vector3(quad.transform.position.x - (width / 2),
                                                    quad.transform.position.y - (height / 2) - 45,
                                                    -5f);

        Debug.Log("temp_pos:" + temp_pos.ToString());
               
        //control_menu.GetComponent<RectTransform>().anchoredPosition = temp_pos;
        control_menu.transform.position = temp_pos;

        Vector3 temp_pos_2 = new Vector3(quad.transform.position.x + (width / 2) + 72,
                                                   quad.transform.position.y,
                                                   0f);

        Vector3 screen_temp_pos_2 = RectTransformUtility.WorldToScreenPoint(Camera.main, temp_pos_2);

        Vector2 rect_Try_2;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(settings_menu.transform.parent.GetComponent<RectTransform>(),
                    screen_temp_pos_2, null, out rect_Try_2);

        settings_menu.GetComponent<RectTransform>().anchoredPosition = rect_Try_2;
    }
    */

    void LockInput(InputField input)
    {
        // if a function is getting evaluated now, we will not receive any further input
        /*if (paintable.GetComponent<Paintable>().no_func_menu_open)
            return;*/

        Paintable.click_on_inputfield = true;

        if (input.text.Length > 0)
        {
            float.TryParse(input.text, out node_radius_val);
        }
    }    

    public void GraphType()
    {
        if (site_specific.isOn) graph_type = "SiteSpecific";
        else graph_type = "NodeRadius";

        mainInputField.interactable = node_radius.isOn;
    }

    void TrackType(Toggle toggle)
    {
        auto_track_val = auto_track.isOn;
    }

    void SettingsMenu()
    {
        settings_menu.SetActive(!settings_menu.activeSelf);
    }

    void Delete()
    {
        if (graph_holder != null)
        {
            Destroy(graph_holder);
        }

        Destroy(transform.gameObject);
    }

    // Update is called once per frame
    void Update()
    {
        if (mainInputField != null && mainInputField.isFocused)
        {
            Paintable.click_on_inputfield = true;
            return;
        }
    }

    public void checkHitAndMove(Vector3 diff)
    {
        transform.position += diff;
        UIlayout();
        /*control_menu.transform.position += diff;
        settings_menu.transform.position += diff;*/

        if (graph_holder != null)
        {
            graph_holder.transform.position += diff;
            graph_holder.GetComponent<GraphElementScript>().checkHitAndMove(diff);
        }

    }

    // for image loading and rendering, copied from .cs

    // load a new image and convert to sprite 
    // https://forum.unity.com/threads/generating-sprites-dynamically-from-png-or-jpeg-files-in-c.343735/ (the solution by Freznosis#5)        
    public Sprite LoadNewSprite(string FilePath, float PixelsPerUnit = 100.0f, SpriteMeshType spriteType = SpriteMeshType.Tight)
    {

        // Load a PNG or JPG image from disk to a Texture2D, assign this texture to a new sprite and return its reference
        // testing CV 
        gameObject.AddComponent<ContourandRotatedRectDetection>();


        Texture2D SpriteTexture = LoadTexture(FilePath);
        Sprite NewSprite = Sprite.Create(SpriteTexture, new Rect(0, 0, SpriteTexture.width, SpriteTexture.height), new Vector2(0, 0), PixelsPerUnit, 0, spriteType);
        recognized_sprite = NewSprite;
        SpriteToRender();

        gameObject.GetComponent<ContourandRotatedRectDetection>().FindAnglesFromTexture(SpriteTexture);

        return NewSprite;
    }

    public Sprite LoadSprite(Texture2D SpriteTexture, /*float x, float y, float width, float height,*/ float PixelsPerUnit = 100.0f, SpriteMeshType spriteType = SpriteMeshType.Tight)
    {

        // Load a PNG or JPG image from disk to a Texture2D, assign this texture to a new sprite and return its reference

        Sprite NewSprite = Sprite.Create(SpriteTexture, new Rect(0, 0, SpriteTexture.width, SpriteTexture.height),//(x, y, width, height), 
            new Vector2(0, 0), PixelsPerUnit, 0, spriteType);
        recognized_sprite = NewSprite;
        SpriteToRender();
        return NewSprite;
    }

    public Texture2D LoadTexture(string FilePath)
    {

        // Load a PNG or JPG file from disk to a Texture2D
        // Returns null if load fails

        Texture2D Tex2D;
        byte[] FileData;

        if (File.Exists(FilePath))
        {
            FileData = File.ReadAllBytes(FilePath);
            Tex2D = new Texture2D(2, 2);           // Create new "empty" texture
            if (Tex2D.LoadImage(FileData))           // Load the imagedata into the texture (size is set automatically)
                return Tex2D;                 // If data = readable -> return texture
        }
        return null;                     // Return null if load failed
    }

    public void SpriteToRender()
    {

        // rescaling, otherwise the projected mesh is too small
        transform.localScale = new Vector3(30f, 30f, 1f);

        //transform.gameObject.AddComponent<BoxCollider>();
        transform.gameObject.AddComponent<SpriteRenderer>();
        transform.GetComponent<SpriteRenderer>().sprite = recognized_sprite;

        //Debug.Log("box collider before: " + templine.GetComponent<BoxCollider>().center.ToString());
        /*transform.GetComponent<BoxCollider>().size = transform.GetComponent<SpriteRenderer>().sprite.bounds.size;
        transform.GetComponent<BoxCollider>().center = new Vector3(transform.GetComponent<SpriteRenderer>().sprite.bounds.center.x,
                            transform.GetComponent<SpriteRenderer>().sprite.bounds.center.y,
                            -5f);

        radius = transform.GetComponent<SpriteRenderer>().bounds.extents.magnitude;

        // set collider trigger
        transform.GetComponent<BoxCollider>().isTrigger = true;
        transform.GetComponent<BoxCollider>().enabled = true;

        edge_position = transform.GetComponent<SpriteRenderer>().bounds.extents;
        edge_position.z = -5f;

        bounds_center = edge_position;*/

    }



}
