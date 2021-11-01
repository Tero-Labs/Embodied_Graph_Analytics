using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

using UnityEngine.SceneManagement;
using OpenCVForUnity.CoreModule;
using OpenCVForUnity.Features2dModule;
using OpenCVForUnity.UnityUtils;
using OpenCVForUnity.ImgprocModule;
using OpenCVForUnity.DnnModule;
using OpenCVForUnity.ImgcodecsModule;
using System.Linq;
using OpenCVForUnity.UnityUtils.Helper;
using System.IO;

public class ContourandRotatedRectDetection : MonoBehaviour
{
    public List<OpenCVForUnity.CoreModule.Rect> all_horizontal_rects;
    public List<float> all_intensities;

    static int inpWidth = 416;
    static int inpHeight = 416;
    static float scale = 1f / 255f;
    static Scalar mean = new Scalar (0, 0, 0);
    static bool swapRB = false;
    static float confThreshold = 0.24f;
    static float nmsThreshold = 0.24f;

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

    public List<RotatedRect> FindResultFromImageTexture(Texture2D imgTexture, int contour_count = 7, bool copy_graph = false, int visual_var = 0, int blob_size = 0, float max_visual_var = 0, float min_visual_var = 0)
    {     
        Utils.setDebugMode(true);
        // Debug.Log("Texture format: " + imgTexture.format.ToString());
        List<RotatedRect> all_bounding_rects = new List<RotatedRect>();
        if (copy_graph) all_horizontal_rects = new List<OpenCVForUnity.CoreModule.Rect>();
        if (Paintable.visual_variable_dict[visual_var] == "brightness") all_intensities = new List<float>();

        Mat imgMat = new Mat(imgTexture.height, imgTexture.width, CvType.CV_8UC3);
        Utils.texture2DToMat(imgTexture, imgMat);

        if (Paintable.visual_variable_dict[visual_var] == "category")
        {
            return GetYoloPreds(imgMat);
        }

        if (Paintable.visual_variable_dict[visual_var] == "color")
        {
            Debug.Log("mat channel count: " + imgMat.channels().ToString());
            Imgproc.cvtColor(imgMat, imgMat, Imgproc.COLOR_RGB2GRAY/*COLOR_BGR2HSV*/);
            Debug.Log("mat channel count after COLOR_BGR2HSV: " + imgMat.channels().ToString());            

            Core.inRange(imgMat, /*new Scalar(25, 50, 50)*/new Scalar(76, 135, 141), /*new Scalar(75, 255, 255)*/new Scalar(104, 178, 153), imgMat); //Threshold the image
            morphOps(imgMat);                        
            Debug.Log("mat channel count after masking: " + imgMat.channels().ToString());                       
        }
        else
            Imgproc.cvtColor(imgMat, imgMat, Imgproc.COLOR_RGB2GRAY);

        /*Texture2D temp_texture = new Texture2D(imgMat.cols(), imgMat.rows(), TextureFormat.RGB24, false);
        Utils.matToTexture2D(imgMat, temp_texture, flip: false, flipAfter: true);
        File.WriteAllBytes("F:/tero_lab_stuff/" + "contour_input.jpg", temp_texture.EncodeToJPG());*/

        List<MatOfPoint> contours = new List<MatOfPoint>();
        Mat hierarchy = new Mat();        
        Imgproc.findContours(imgMat, contours, hierarchy, 1/*Imgproc.CV_RETR_LIST*/, 1/*Imgproc.CV_CHAIN_APPROX_NONE*/);
        // Debug.Log("no of contours (from texture):" + contours.Count.ToString());
        // visualization purpose
        //Imgproc.drawContours(imgMat, contours, -1, new Scalar(0, 255, 0), 2);
        // sorting by length
        contours = contours.OrderByDescending(x => x.toList().Count).ToList();
                
        // scaling with current target
        if (blob_size > 0)
        {
            blob_size = (int)Mathf.Lerp(contours[contours.Count - 1].toList().Count, contours[0].toList().Count, Mathf.InverseLerp(1, 50, blob_size));
        }

        // trying out rotatedrect
        // https://docs.opencv.org/3.4/de/d62/tutorial_bounding_rotated_ellipses.html
        int num_iter = 0;
        foreach (MatOfPoint contour in contours)
        {
            // IMPORTANT: THE MatOfPoint CAN NOT BE DIRECTLY CASTED TO MatOfPoint2f HENCE WE GOT THE POINT ARRAY FROM MatOfPoint
            Point[] points = contour.toArray();
            // if less than predefined value- break and discard it
            if (points.Length < blob_size) break;

            MatOfPoint2f cur_points = new MatOfPoint2f(points);
            RotatedRect minRect = Imgproc.minAreaRect(cur_points);

            // Debug.Log("angle of current rect (from texture):" + minRect.angle.ToString());

            // size
            if (Paintable.visual_variable_dict[visual_var] == "size")
            {
                float value = ((float)minRect.size.width + (float)minRect.size.height) / 2;
                if (value < min_visual_var || value > max_visual_var)
                    continue;
            }                
            // angle
            else if (Paintable.visual_variable_dict[visual_var] == "angle")
            {
                if ((float)minRect.angle < min_visual_var || (float)minRect.angle > max_visual_var)
                    continue;
            }
            // brightness
            else if (Paintable.visual_variable_dict[visual_var] == "brightness")
            {
                float value = FindIntensity(points, imgTexture);
                if (value < min_visual_var || value > max_visual_var)
                    continue;
                all_intensities.Add(value);
            }                

            all_bounding_rects.Add(minRect);

            if (copy_graph) all_horizontal_rects.Add(Imgproc.boundingRect(cur_points));

            // visualization purpose
            /*Point[] rect_points = new Point[4];
            minRect.points(rect_points);

            for (int j = 0; j < 4; j++)
            {
                Imgproc.line(imgMat, rect_points[j], rect_points[(j + 1) % 4], new Scalar(255, 0, 0));
            }*/            

            num_iter++;
            if (num_iter == contour_count)
                break;

        }


        Utils.setDebugMode(false);

        return all_bounding_rects;
    }

    public List<RotatedRect> FindResultFromVideoTexture(Texture2D vidTexture, int contour_count = 7, bool copy_graph = false, int visual_var = 0, int blob_size = 0)
    {
        Utils.setDebugMode(false);

        List<RotatedRect> all_bounding_rects = new List<RotatedRect>();
        if (copy_graph) all_horizontal_rects = new List<OpenCVForUnity.CoreModule.Rect>();
        if (Paintable.visual_variable_dict[visual_var] == "brightness") all_intensities = new List<float>();

        Mat vidMat = new Mat(vidTexture.height, vidTexture.width, CvType.CV_8UC3);
        Utils.texture2DToMat(vidTexture, vidMat);               

        //Mat gray_image = new Mat(vidTexture.height, vidTexture.width, CvType.CV_8UC1);
        Imgproc.cvtColor(vidMat, vidMat, Imgproc.COLOR_RGB2GRAY);
        // Debug.Log("vidMat.ToString() " + vidMat.ToString());

        if (Paintable.visual_variable_dict[visual_var] == "color")
        {            
            Core.inRange(vidMat, /*new Scalar(25, 50, 50)*/new Scalar(76, 135, 141), /*new Scalar(75, 255, 255)*/new Scalar(104, 178, 153), vidMat); //Threshold the image
            morphOps(vidMat);
        }


        // trying out contours
        List<MatOfPoint> contours = new List<MatOfPoint>();
        Mat hierarchy = new Mat();
        Imgproc.findContours(vidMat, contours, hierarchy, 1/*Imgproc.CV_RETR_LIST*/, 1/*Imgproc.CV_CHAIN_APPROX_NONE*/);
        // Debug.Log("no of contours (from texture):" + contours.Count.ToString());
        // visualization purpose
        //Imgproc.drawContours(vidMat, contours, -1, new Scalar(0, 255, 0), 2);
        // sorting by length
        contours = contours.OrderByDescending(x => x.toList().Count).ToList();

        // scaling with current target
        if (blob_size > 0)
        {
            blob_size = (int)Mathf.Lerp(contours[contours.Count - 1].toList().Count, contours[0].toList().Count, Mathf.InverseLerp(1, 50, blob_size));
        }

        int num_iter = 0;
        // trying out rotatedrect
        // https://docs.opencv.org/3.4/de/d62/tutorial_bounding_rotated_ellipses.html
        foreach (MatOfPoint contour in contours)
        {
            // IMPORTANT: THE MatOfPoint CAN NOT BE DIRECTLY CASTED TO MatOfPoint2f HENCE WE GOT THE POINT ARRAY FROM MatOfPoint
            Point[] points = contour.toArray();
            // if less than predefined value- break and discard it
            if (points.Length < blob_size) break;

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

    public List<RotatedRect> GetYoloPreds(Mat img)
    {
        // Create a 4D blob from a frame.
        Size inpSize = new Size(inpWidth > 0 ? inpWidth : img.cols(),
                            inpHeight > 0 ? inpHeight : img.rows());
        Mat blob = Dnn.blobFromImage(img, scale, inpSize, mean, swapRB, false);


        // Run a model.
        Paintable.net.setInput(blob);

        if (Paintable.net.getLayer(new DictValue(0)).outputNameToIndex("im_info") != -1)
        {  // Faster-RCNN or R-FCN
            Imgproc.resize(img, img, inpSize);
            Mat imInfo = new Mat(1, 3, CvType.CV_32FC1);
            imInfo.put(0, 0, new float[] {
                    (float)inpSize.height,
                    (float)inpSize.width,
                    1.6f
                });
            Paintable.net.setInput(imInfo, "im_info");
        }


        TickMeter tm = new TickMeter();
        tm.start();

        List<Mat> outs = new List<Mat>();
        Paintable.net.forward(outs, Paintable.outBlobNames);

        tm.stop();
        Debug.Log("Inference time, ms: " + tm.getTimeMilli());

        postprocess(img, outs, Paintable.net);

        for (int i = 0; i < outs.Count; i++)
        {
            outs[i].Dispose();
        }

        blob.Dispose();
        Paintable.net.Dispose();
        Utils.setDebugMode(false);

        List<RotatedRect> all_bounding_rects = new List<RotatedRect>();
        return all_bounding_rects;
    }

    private void postprocess(Mat frame, List<Mat> outs, Net net)
    {
        string outLayerType = Paintable.outBlobTypes[0];

        List<int> classIdsList = new List<int>();
        List<float> confidencesList = new List<float>();
        List<OpenCVForUnity.CoreModule.Rect> boxesList = new List<OpenCVForUnity.CoreModule.Rect>();
        if (net.getLayer(new DictValue(0)).outputNameToIndex("im_info") != -1)
        {  // Faster-RCNN or R-FCN
            // Network produces output blob with a shape 1x1xNx7 where N is a number of
            // detections and an every detection is a vector of values
            // [batchId, classId, confidence, left, top, right, bottom]

            if (outs.Count == 1)
            {

                outs[0] = outs[0].reshape(1, (int)outs[0].total() / 7);

                //                    Debug.Log ("outs[i].ToString() " + outs [0].ToString ());

                float[] data = new float[7];

                for (int i = 0; i < outs[0].rows(); i++)
                {

                    outs[0].get(i, 0, data);

                    float confidence = data[2];

                    if (confidence > confThreshold)
                    {
                        int class_id = (int)(data[1]);


                        int left = (int)(data[3] * frame.cols());
                        int top = (int)(data[4] * frame.rows());
                        int right = (int)(data[5] * frame.cols());
                        int bottom = (int)(data[6] * frame.rows());
                        int width = right - left + 1;
                        int height = bottom - top + 1;


                        classIdsList.Add((int)(class_id) - 0);
                        confidencesList.Add((float)confidence);
                        boxesList.Add(new OpenCVForUnity.CoreModule.Rect(left, top, width, height));
                    }
                }
            }
        }
        else if (outLayerType == "DetectionOutput")
        {
            // Network produces output blob with a shape 1x1xNx7 where N is a number of
            // detections and an every detection is a vector of values
            // [batchId, classId, confidence, left, top, right, bottom]

            if (outs.Count == 1)
            {

                outs[0] = outs[0].reshape(1, (int)outs[0].total() / 7);

                //                    Debug.Log ("outs[i].ToString() " + outs [0].ToString ());

                float[] data = new float[7];

                for (int i = 0; i < outs[0].rows(); i++)
                {

                    outs[0].get(i, 0, data);

                    float confidence = data[2];

                    if (confidence > confThreshold)
                    {
                        int class_id = (int)(data[1]);


                        int left = (int)(data[3] * frame.cols());
                        int top = (int)(data[4] * frame.rows());
                        int right = (int)(data[5] * frame.cols());
                        int bottom = (int)(data[6] * frame.rows());
                        int width = right - left + 1;
                        int height = bottom - top + 1;


                        classIdsList.Add((int)(class_id) - 0);
                        confidencesList.Add((float)confidence);
                        boxesList.Add(new OpenCVForUnity.CoreModule.Rect(left, top, width, height));
                    }
                }
            }
        }
        else if (outLayerType == "Region")
        {
            for (int i = 0; i < outs.Count; ++i)
            {
                // Network produces output blob with a shape NxC where N is a number of
                // detected objects and C is a number of classes + 4 where the first 4
                // numbers are [center_x, center_y, width, height]

                //                        Debug.Log ("outs[i].ToString() "+outs[i].ToString());

                float[] positionData = new float[5];
                float[] confidenceData = new float[outs[i].cols() - 5];

                for (int p = 0; p < outs[i].rows(); p++)
                {



                    outs[i].get(p, 0, positionData);

                    outs[i].get(p, 5, confidenceData);

                    int maxIdx = confidenceData.Select((val, idx) => new { V = val, I = idx }).Aggregate((max, working) => (max.V > working.V) ? max : working).I;
                    float confidence = confidenceData[maxIdx];

                    if (confidence > confThreshold)
                    {

                        int centerX = (int)(positionData[0] * frame.cols());
                        int centerY = (int)(positionData[1] * frame.rows());
                        int width = (int)(positionData[2] * frame.cols());
                        int height = (int)(positionData[3] * frame.rows());
                        int left = centerX - width / 2;
                        int top = centerY - height / 2;

                        classIdsList.Add(maxIdx);
                        confidencesList.Add((float)confidence);
                        boxesList.Add(new OpenCVForUnity.CoreModule.Rect(left, top, width, height));

                    }
                }
            }
        }
        else
        {
            Debug.Log("Unknown output layer type: " + outLayerType);
        }


        MatOfRect boxes = new MatOfRect();
        boxes.fromList(boxesList);

        MatOfFloat confidences = new MatOfFloat();
        confidences.fromList(confidencesList);


        MatOfInt indices = new MatOfInt();
        Dnn.NMSBoxes(boxes, confidences, confThreshold, nmsThreshold, indices);

        for (int i = 0; i < indices.total(); ++i)
        {
            int idx = (int)indices.get(i, 0)[0];
            OpenCVForUnity.CoreModule.Rect box = boxesList[idx];

            Debug.Log("Current Pred Result:" + Paintable.getPredClassName(classIdsList[idx], confidencesList[idx]));

            /*drawPred(classIdsList[idx], confidencesList[idx], box.x, box.y,
                box.x + box.width, box.y + box.height, frame);*/
        }

        indices.Dispose();
        boxes.Dispose();
        confidences.Dispose();

    }

}
