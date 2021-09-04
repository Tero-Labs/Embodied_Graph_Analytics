﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

using UnityEngine.SceneManagement;
using OpenCVForUnity.CoreModule;
using OpenCVForUnity.Features2dModule;
using OpenCVForUnity.UnityUtils;
using OpenCVForUnity.ImgprocModule;

public class ContourandRotatedRectDetection : MonoBehaviour
{
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

    public void FindAnglesFromTexture(Texture2D imgTexture)
    {
        Utils.setDebugMode(true);

        Mat imgMat = new Mat(imgTexture.height, imgTexture.width, CvType.CV_8UC1);

        Utils.texture2DToMat(imgTexture, imgMat);
        Debug.Log("imgMat.ToString() " + imgMat.ToString());

        // trying out contours
        List<MatOfPoint> contours = new List<MatOfPoint>();
        Mat hierarchy = new Mat();
        Imgproc.findContours(imgMat, contours, hierarchy, 1/*Imgproc.CV_RETR_LIST*/, 1/*Imgproc.CV_CHAIN_APPROX_NONE*/);
        Debug.Log("no of contours (from texture):" + contours.Count.ToString());
        // visualization purpose
        //Imgproc.drawContours(outImgMat, contours, -1, new Scalar(0, 255, 0), 2);

        // trying out rotatedrect
        // https://docs.opencv.org/3.4/de/d62/tutorial_bounding_rotated_ellipses.html
        foreach (MatOfPoint contour in contours)
        {
            // IMPORTANT: THE MatOfPoint CAN NOT BE DIRECTLY CASTED TO MatOfPoint2f HENCE WE GOT THE POINT ARRAY FROM MatOfPoint
            MatOfPoint2f cur_points = new MatOfPoint2f(contour.toArray());
            RotatedRect minRect = Imgproc.minAreaRect(cur_points);
            Debug.Log("angle of current rect (from texture):" + minRect.angle.ToString());

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

}