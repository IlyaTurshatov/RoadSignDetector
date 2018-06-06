using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;

#if UNITY_5_3 || UNITY_5_3_OR_NEWER
using UnityEngine.SceneManagement;
#endif
using OpenCVForUnity;

public class SignDetector : MonoBehaviour
{
    public GameObject imgPrefub;
    public Transform imgPanel;
    CCircleDetectorModel model;

    // Use this for initialization
    void Start()
    {
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
        model.lowerSaturation = 0;
        model.upperSaturation = 255;
        model.lowerVariance = 62;
        model.upperVariance = 60;
        model.cannyTreshold = 128;

        lowerHue = model.lowerHue;
        upperHue = model.upperHue;
        lowerSaturation = model.lowerSaturation;
        upperSaturation = model.upperSaturation;
        lowerVariance = model.lowerVariance;
        upperVariance = model.upperVariance;
        cannyTreshold = model.cannyTreshold;

        ProcessImage();
    }


    public void ProcessImage()
    {
        Mat processedImg = FindCirclesAndDisplay(ref model);
        Texture2D texture = new Texture2D(processedImg.cols(), processedImg.rows(), TextureFormat.RGBA32, false);
        Utils.matToTexture2D(processedImg, texture);

        //gameObject.GetComponent<Renderer>().material.mainTexture = texture;
    }

    public float lowerHue { get; set; }
    public float upperHue { get; set; }
    public float lowerSaturation { get; set; }
    public float upperSaturation { get; set; }
    public float lowerVariance { get; set; }
    public float upperVariance { get; set; }
    public float cannyTreshold { get; set; }


    Mat FindCirclesAndDisplay(ref CCircleDetectorModel pCircleDetectorModel)
    {
        Mat pSourceImage = pCircleDetectorModel.image;
        Mat imgHsv = new Mat(pSourceImage.size(), pSourceImage.type());

        Imgproc.cvtColor(pSourceImage, imgHsv, Imgproc.COLOR_RGB2HSV);
        List<Mat> vChannels = new List<Mat>(3);     // 3 channels
        Core.split(imgHsv, vChannels);

        Mat thresholdedHue = new Mat(vChannels[0].size(), vChannels[0].type());
        Mat thresholdedSat = new Mat(vChannels[1].size(), vChannels[1].type());
        Mat thresholdedVar = new Mat(vChannels[2].size(), vChannels[2].type());

        Core.inRange(vChannels[0], new Scalar(lowerHue), new Scalar(upperHue), thresholdedHue);
        Core.inRange(vChannels[1], new Scalar(lowerSaturation), new Scalar(upperSaturation), thresholdedSat);
        Core.inRange(vChannels[2], new Scalar(lowerVariance), new Scalar(upperVariance), thresholdedVar);


        ShowImage(thresholdedHue, "thresholdedHue");
        ShowImage(thresholdedSat, "thresholdedSat");
        ShowImage(thresholdedVar, "thresholdedVar");

        Mat imgResult = new Mat(vChannels[0].size(), pSourceImage.type());
        {
            List<Mat> channels = new List<Mat>();

            channels.Add(thresholdedVar);
            channels.Add(thresholdedSat);
            channels.Add(thresholdedHue);
            /// Merge the three channels
            Core.merge(channels, imgResult);
        }


        Imgproc.medianBlur(imgResult, imgResult, 5);
        Imgproc.threshold(imgResult, imgResult, 128, 255, Imgproc.THRESH_BINARY_INV);

        Mat circles = new Mat(imgResult.size(), imgResult.type());

        Imgproc.cvtColor(imgResult, imgResult, Imgproc.COLOR_HSV2RGB);

        Imgproc.cvtColor(imgResult, imgResult, Imgproc.COLOR_RGB2GRAY);

        ShowImage(imgResult, "GRAY");


        Imgproc.HoughCircles(imgResult,
                     circles,
                     Imgproc.CV_HOUGH_GRADIENT,
                     1,
                     45,
                     pCircleDetectorModel.param1,
                     pCircleDetectorModel.param2,
                     pCircleDetectorModel.minRadius,
                     pCircleDetectorModel.maxRadius);

        ShowImage(circles, "circles");

        //pCircleDetectorModel.image = imgResult;
        return imgResult;
    }

    void ShowImage(Mat mat, string name = "")
    {
        var g1 = GameObject.Instantiate(imgPrefub, imgPanel);
        Texture2D texture = new Texture2D(mat.cols(), mat.rows(), TextureFormat.RGBA32, false);
        Utils.matToTexture2D(mat, texture);
        var img = g1.GetComponent<Image>();
        var rect = new UnityEngine.Rect(0, 0, texture.width, texture.height);
        img.overrideSprite = Sprite.Create(texture, rect, new Vector2(0.5f, 0.5f));
        g1.GetComponentInChildren<UnityEngine.UI.Text>().text = name;
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

