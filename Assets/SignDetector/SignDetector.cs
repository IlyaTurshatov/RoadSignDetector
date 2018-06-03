using UnityEngine;
using System.Collections;
using System.Collections.Generic;

#if UNITY_5_3 || UNITY_5_3_OR_NEWER
using UnityEngine.SceneManagement;
#endif
using OpenCVForUnity;

public class SignDetector : MonoBehaviour
{

    CCircleDetectorModel model;

    // Use this for initialization
    void Start()
    {
        //if true, The error log of the Native side OpenCV will be displayed on the Unity Editor Console.
        Utils.setDebugMode(true);

        Texture2D imgTexture = Resources.Load("120km") as Texture2D;

        Mat imgMat = new Mat(imgTexture.height, imgTexture.width, CvType.CV_8UC4);
        Utils.texture2DToMat(imgTexture, imgMat);
        // Initial algo parameters
        model.image = imgMat;
        model.param1 = 150;
        model.param2 = 20;
        model.minRadius = 2;
        model.maxRadius = 80;

        model.lowerHue = 102;
        model.upperHue = 156;
        model.lowerSaturation = 91;
        model.upperSaturation = 90;
        model.lowerVariance = 62;
        model.upperVariance = 60;
        model.cannyTreshold = 128;

        ProcessImage();

        Utils.setDebugMode(false);
    }

    void ProcessImage()
    {
        Mat processedImg = FindCirclesAndDisplay(ref model);
        Texture2D texture = new Texture2D(processedImg.cols(), processedImg.rows(), TextureFormat.RGBA32, false);
        Utils.matToTexture2D(processedImg, texture);

        gameObject.GetComponent<Renderer>().material.mainTexture = texture;
    }


    Mat FindCirclesAndDisplay(ref CCircleDetectorModel pCircleDetectorModel)
    {
        Mat pSourceImage = pCircleDetectorModel.image;
        Mat imgHsv = new Mat(pSourceImage.size(), pSourceImage.type());

        Imgproc.cvtColor(pSourceImage, imgHsv, Imgproc.COLOR_RGB2HSV);
        List<Mat> vChannels = new List<Mat>(3);     // 3 channels
        Core.split(imgHsv, vChannels);

        int lowerHue = pCircleDetectorModel.lowerHue;
        int upperHue = pCircleDetectorModel.upperHue;
        int lowerSaturation = pCircleDetectorModel.lowerSaturation;
        int upperSaturation = pCircleDetectorModel.upperSaturation;
        int lowerVariance = pCircleDetectorModel.lowerVariance;
        int upperVariance = pCircleDetectorModel.upperVariance;
        int cannyTreshold = pCircleDetectorModel.cannyTreshold;

        Mat thresholdedHue = new Mat(vChannels[0].size(), vChannels[0].type());
        Mat thresholdedSat = new Mat(vChannels[1].size(), vChannels[1].type());
        Mat thresholdedVar = new Mat(vChannels[2].size(), vChannels[2].type());

        Core.inRange(vChannels[0], new Scalar(lowerHue), new Scalar(upperHue), thresholdedHue);
        Core.inRange(vChannels[1], new Scalar(lowerSaturation), new Scalar(upperSaturation), thresholdedSat);
        Core.inRange(vChannels[2], new Scalar(lowerVariance), new Scalar(upperVariance), thresholdedVar);

        Mat imgResult = new Mat(vChannels[0].size(), pSourceImage.type());
        {
            List<Mat> channels = new List<Mat>();

            channels.Add(thresholdedVar);
            channels.Add(thresholdedSat);
            channels.Add(thresholdedHue);
            /// Merge the three channels
            Core.merge(channels, imgResult);
        }

        ShowImage(imgResult, "ChannelsFilter", 2);

        Imgproc.medianBlur(imgResult, imgResult, 5);
        ShowImage(imgResult, "medianBlur", 3);


        Imgproc.threshold(imgResult, imgResult, 128, 255, Imgproc.THRESH_BINARY_INV);

        //List<Vector3> circles;   //  vector that stores sets of 3 values: x_{c}, y_{c}, r for each detected circle
        Mat circles = new Mat(imgResult.size(), imgResult.type());

        Imgproc.cvtColor(imgResult, imgResult, Imgproc.COLOR_HSV2RGB);
        Imgproc.cvtColor(imgResult, imgResult, Imgproc.COLOR_RGB2GRAY);


        Imgproc.HoughCircles(imgResult,
                     circles,
                     Imgproc.CV_HOUGH_GRADIENT,
                     1,
                     45,
                     pCircleDetectorModel.param1,
                     pCircleDetectorModel.param2,
                     pCircleDetectorModel.minRadius,
                     pCircleDetectorModel.maxRadius);

        ShowImage(circles, "Circles", 4);

        //pCircleDetectorModel.image = imgResult;
        return imgResult;
    }

    void ShowImage(Mat mat, string name, int quadNumber)
    {
        var g = GameObject.Find("Quad" + quadNumber);
        //g.name = name;
        Texture2D texture = new Texture2D(mat.cols(), mat.rows(), TextureFormat.RGBA32, false);
        Utils.matToTexture2D(mat, texture);
        g.GetComponent<Renderer>().material.color = Color.white;
        g.GetComponent<Renderer>().material.mainTexture = texture;
    }

    struct CCircleDetectorModel
    {
        public Mat image; // Image used to visualize

        public int param1;  //Upper threshold for the internal Canny edge detector
        public int param2;  //Threshold for center detection.
        public int minRadius;
        public int maxRadius;

        // HSV
        public int lowerHue;
        public int upperHue;
        public int lowerSaturation;
        public int upperSaturation;
        public int lowerVariance;
        public int upperVariance;

        public int cannyTreshold;
    };
}

