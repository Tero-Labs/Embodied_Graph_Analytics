using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

using UnityEngine.SceneManagement;
using OpenCVForUnity.CoreModule;
using OpenCVForUnity.Features2dModule;
using OpenCVForUnity.UnityUtils;
using OpenCVForUnity.ImgprocModule;
using System.Linq;
using OpenCVForUnity.UnityUtils.Helper;
using System.IO;

public class ContourandRotatedRectDetection : MonoBehaviour
{
    public List<OpenCVForUnity.CoreModule.Rect> all_horizontal_rects;
    public List<float> all_intensities;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void FindAngles()
    {
        Utils.setDebugMode(true);


        Texture2D imgTexture = Resources.Load("matchshapes") as Texture2D;

        Mat imgMat = new Mat(imgTexture.height, imgTexture.width, CvType.CV_8UC1);

        Utils.texture2DToMat(imgTexture, imgMat);
        Debug.Log("imgMat.ToString() " + imgMat.ToString());

        // trying out contours
        List<MatOfPoint> contours = new List<MatOfPoint>();
        Mat hierarchy = new Mat();
        Imgproc.findContours(imgMat, contours, hierarchy, 1/*Imgproc.CV_RETR_LIST*/, 1/*Imgproc.CV_CHAIN_APPROX_NONE*/);
        Debug.Log("no of contours:" + contours.Count.ToString());
        // visualization purpose
        //Imgproc.drawContours(outImgMat, contours, -1, new Scalar(0, 255, 0), 2);

        // trying out rotatedrect
        // https://docs.opencv.org/3.4/de/d62/tutorial_bounding_rotated_ellipses.html
        foreach (MatOfPoint contour in contours)
        {
            // IMPORTANT: THE MatOfPoint CAN NOT BE DIRECTLY CASTED TO MatOfPoint2f HENCE WE GOT THE POINT ARRAY FROM MatOfPoint
            MatOfPoint2f cur_points = new MatOfPoint2f(contour.toArray());
            RotatedRect minRect = Imgproc.minAreaRect(cur_points);
            Debug.Log("angle of current rect:" + minRect.angle.ToString());

            // visualization purpose
            /*Point[] rect_points = new Point[4];
            minRect.points(rect_points);

            for (int j = 0; j < 4; j++)
            {
                Imgproc.line(outImgMat, rect_points[j], rect_points[(j + 1) % 4], new Scalar(255, 0, 0));
            }*/

        }


        Utils.setDebugMode(false);
    }

    /// <summary>
    /// Morphs the ops.
    /// </summary>
    /// <param name="thresh">Thresh.</param>
    private void morphOps(Mat thresh)
    {
        //create structuring element that will be used to "dilate" and "erode" image.
        //the element chosen here is a 3px by 3px rectangle
        Mat erodeElement = Imgproc.getStructuringElement(Imgproc.MORPH_RECT, new Size(8, 8));
        //dilate with larger element so make sure object is nicely visible
        Mat dilateElement = Imgproc.getStructuringElement(Imgproc.MORPH_RECT, new Size(8, 8));


        // https://www.opencv-srf.com/2010/09/object-detection-using-color-seperation.html
        //morphological closing (fill small holes in the foreground)
        Imgproc.dilate(thresh, thresh, dilateElement);
        Imgproc.erode(thresh, thresh, erodeElement);

        //morphological opening (remove small objects from the foreground)
        //Imgproc.erode(thresh, thresh, erodeElement);
        //Imgproc.dilate(thresh, thresh, dilateElement);
    }

    public List<RotatedRect> FindResultFromImageTexture(Texture2D imgTexture, int contour_count = 7, int visual_var = 0)
    {
        Utils.setDebugMode(false);

        List<RotatedRect> all_bounding_rects = new List<RotatedRect>();
        if (Paintable.visual_variable_dict[visual_var] == "brightness") all_intensities = new List<float>();

        Mat imgMat = new Mat(imgTexture.height, imgTexture.width, CvType.CV_8UC1);

        Utils.texture2DToMat(imgTexture, imgMat);

        if (Paintable.visual_variable_dict[visual_var] == "color")
        {
            Debug.Log("mat channel count: " + imgMat.channels().ToString());
            Imgproc.cvtColor(imgMat, imgMat, Imgproc.COLOR_BGR2HSV);
            Debug.Log("imgMat.ToString() " + imgMat.ToString());

            // Mat imgThresholded = new Mat(imgTexture.height, imgTexture.width, CvType.CV_8UC1);
            Core.inRange(imgMat, new Scalar(40, 100, 100), new Scalar(50, 255, 255), imgMat); //Threshold the image
            morphOps(imgMat);

            Texture2D temp_texture = new Texture2D(imgMat.cols(), imgMat.rows(), TextureFormat.RGBA32, false);
            Utils.matToTexture2D(imgMat, temp_texture);
            File.WriteAllBytes("F:/tero_lab_stuff/" + "dump.png", temp_texture.EncodeToPNG());
        }
            
        // Debug.Log("imgMat.ToString() " + imgMat.ToString());
        

        List<MatOfPoint> contours = new List<MatOfPoint>();
        Mat hierarchy = new Mat();
        Imgproc.findContours(imgMat, contours, hierarchy, 1/*Imgproc.CV_RETR_LIST*/, 1/*Imgproc.CV_CHAIN_APPROX_NONE*/);
        // Debug.Log("no of contours (from texture):" + contours.Count.ToString());
        // visualization purpose
        //Imgproc.drawContours(imgMat, contours, -1, new Scalar(0, 255, 0), 2);
        // sorting by length
        contours = contours.OrderByDescending(x => x.toList().Count).ToList();

        // trying out rotatedrect
        // https://docs.opencv.org/3.4/de/d62/tutorial_bounding_rotated_ellipses.html
        int num_iter = 0;
        foreach (MatOfPoint contour in contours)
        {
            // IMPORTANT: THE MatOfPoint CAN NOT BE DIRECTLY CASTED TO MatOfPoint2f HENCE WE GOT THE POINT ARRAY FROM MatOfPoint
            Point[] points = contour.toArray();
            MatOfPoint2f cur_points = new MatOfPoint2f(points);
            RotatedRect minRect = Imgproc.minAreaRect(cur_points);
            // Debug.Log("angle of current rect (from texture):" + minRect.angle.ToString());

            all_bounding_rects.Add(minRect);

            // visualization purpose
            /*Point[] rect_points = new Point[4];
            minRect.points(rect_points);

            for (int j = 0; j < 4; j++)
            {
                Imgproc.line(imgMat, rect_points[j], rect_points[(j + 1) % 4], new Scalar(255, 0, 0));
            }*/

            if (Paintable.visual_variable_dict[visual_var] == "brightness")
            {
                all_intensities.Add(FindIntensity(points, imgTexture));
            }

            num_iter++;
            if (num_iter == contour_count)
                break;

        }


        Utils.setDebugMode(false);

        return all_bounding_rects;
    }

    public List<RotatedRect> FindResultFromVideoTexture(Texture2D vidTexture, int contour_count = 7, bool copy_graph = false, int visual_var = 0)
    {
        Utils.setDebugMode(false);

        List<RotatedRect> all_bounding_rects = new List<RotatedRect>();
        if (copy_graph) all_horizontal_rects = new List<OpenCVForUnity.CoreModule.Rect>();
        if (Paintable.visual_variable_dict[visual_var] == "brightness") all_intensities = new List<float>();

        Mat vidMat = new Mat(vidTexture.height, vidTexture.width, CvType.CV_8UC1);

        Utils.texture2DToMat(vidTexture, vidMat);

        //Mat gray_image = new Mat(vidTexture.height, vidTexture.width, CvType.CV_8UC1);
        Imgproc.cvtColor(vidMat, vidMat, Imgproc.COLOR_BGR2HSV);
        // Debug.Log("vidMat.ToString() " + vidMat.ToString());

        // trying out contours
        List<MatOfPoint> contours = new List<MatOfPoint>();
        Mat hierarchy = new Mat();
        Imgproc.findContours(vidMat, contours, hierarchy, 1/*Imgproc.CV_RETR_LIST*/, 1/*Imgproc.CV_CHAIN_APPROX_NONE*/);
        // Debug.Log("no of contours (from texture):" + contours.Count.ToString());
        // visualization purpose
        //Imgproc.drawContours(vidMat, contours, -1, new Scalar(0, 255, 0), 2);
        // sorting by length
        contours = contours.OrderByDescending(x => x.toList().Count).ToList();

        int num_iter = 0;
        // trying out rotatedrect
        // https://docs.opencv.org/3.4/de/d62/tutorial_bounding_rotated_ellipses.html
        foreach (MatOfPoint contour in contours)
        {
            // IMPORTANT: THE MatOfPoint CAN NOT BE DIRECTLY CASTED TO MatOfPoint2f HENCE WE GOT THE POINT ARRAY FROM MatOfPoint
            Point[] points = contour.toArray();
            MatOfPoint2f cur_points = new MatOfPoint2f(points);
            RotatedRect minRect = Imgproc.minAreaRect(cur_points);
            // Debug.Log("angle of current rect (from texture):" + minRect.angle.ToString());

            all_bounding_rects.Add(minRect);

            if (copy_graph) all_horizontal_rects.Add(Imgproc.boundingRect(cur_points));

            // visualization purpose
            /*Point[] rect_points = new Point[4];
            minRect.points(rect_points);

            for (int j = 0; j < 4; j++)
            {
                Imgproc.line(vidMat, rect_points[j], rect_points[(j + 1) % 4], new Scalar(255, 0, 0));
            }*/

            if (Paintable.visual_variable_dict[visual_var] == "brightness")
            {
                all_intensities.Add(FindIntensity(points, vidTexture));
            }

            num_iter++;
            if (num_iter == contour_count)
                break;
        }


        Utils.setDebugMode(false);
        // Debug.Log("no of min rects (from texture):" + all_bounding_rects.Count.ToString());
        return all_bounding_rects;
    }

    public float FindIntensity(Point[] points, Texture2D imgTexture)
    {
        float avg_intensity = 0f;

        /*
        MatOfPoint2f cur_points = new MatOfPoint2f(imgMat);
        int cn = imgMat.channels();
        long pixel_values = imgMat.dataAddr();
        byte[] yuv = new byte[(int)(imgMat.total() * imgMat.channels())];
        imgMat.get(0, 0, yuv);
        Texture2D tex2D = new Texture2D(imgMat.width(), imgMat.height());
        Utils.matToTexture2D(imgMat, tex2D);*/

        int iter = 0;
        foreach (Point pnt in points)
        {
            float h, s, v;
            Color.RGBToHSV(imgTexture.GetPixel((int)pnt.x, (int)pnt.y), out h, out s, out v);
            avg_intensity += v;
            iter++;
        }

        return avg_intensity/iter;
    }
}
