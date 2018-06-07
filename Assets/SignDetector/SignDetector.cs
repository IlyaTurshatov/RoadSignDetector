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
    public GameObject largeImage;


    CCircleDetectorModel model;


    // Use this for initialization
    void Start()
    {
        Texture2D imgTexture = Resources.Load("2") as Texture2D;
        Mat imgMat = new Mat(imgTexture.height, imgTexture.width, CvType.CV_8UC4);
        Utils.texture2DToMat(imgTexture, imgMat);
        // Initial algo parameters
        model.image = imgMat;
        model.param1 = 120;
        model.param2 = 14;
        model.minRadius = 2;
        model.maxRadius = 80;

        model.lowerHue = 0;
        model.upperHue = 17;
        model.lowerSaturation = 51;
        model.upperSaturation = 255;
        model.lowerVariance = 62;
        model.upperVariance = 255;
        model.cannyTreshold = 128;

        lowerHue = model.lowerHue;
        upperHue = model.upperHue;
        lowerSaturation = model.lowerSaturation;
        upperSaturation = model.upperSaturation;
        lowerVariance = model.lowerVariance;
        upperVariance = model.upperVariance;
        cannyTreshold = model.cannyTreshold;

        lowerThreshold = 128;
        upperThreshold = 255;

        ProcessImage();
    }


    public void ProcessImage()
    {
        for (int i = 0; i < imgPanel.childCount; i++)
            Destroy(imgPanel.GetChild(i).gameObject);
        FindCirclesAndDisplay(ref model);
    }

    public float lowerHue { get; set; }
    public float upperHue { get; set; }
    public float lowerSaturation { get; set; }
    public float upperSaturation { get; set; }
    public float lowerVariance { get; set; }
    public float upperVariance { get; set; }
    public float lowerThreshold { get; set; }
    public float upperThreshold { get; set; }
    public float cannyTreshold { get; set; }


    void FindCirclesAndDisplay(ref CCircleDetectorModel pCircleDetectorModel)
    {
        Mat pSourceImage = pCircleDetectorModel.image;
        ShowImage(pSourceImage, "pSourceImage");

        Mat imgHsv = new Mat(pSourceImage.size(), pSourceImage.type());
        Imgproc.cvtColor(pSourceImage, imgHsv, Imgproc.COLOR_RGB2HSV);

        List<Mat> vChannels = new List<Mat>(3);     // 3 channels
        Core.split(imgHsv, vChannels);

        Core.inRange(vChannels[0], new Scalar(lowerHue), new Scalar(upperHue), vChannels[0]);
        Core.inRange(vChannels[1], new Scalar(lowerSaturation), new Scalar(upperSaturation), vChannels[1]);
        Core.inRange(vChannels[2], new Scalar(lowerVariance), new Scalar(upperVariance), vChannels[2]);

        Mat filter = vChannels[0] & vChannels[1] & vChannels[2];
        ShowImage(filter, "Filter");


        //Imgproc.dilate(filter, filter, new Mat());
        //Imgproc.dilate(filter, filter, new Mat());

        //Imgproc.erode(filter, filter, new Mat());
        //Imgproc.dilate(filter, filter, new Mat());

        //ShowImage(filter, "Dilate");

        Mat imgResult = new Mat(pSourceImage.size(), pSourceImage.type());
        pSourceImage.copyTo(imgResult, filter);
        ShowImage(imgResult, "Filtered");

        //Imgproc.medianBlur(imgResult, imgResult, 3);
        //ShowImage(imgResult, "Blurred");

        Imgproc.threshold(imgResult, imgResult, lowerThreshold, upperThreshold, Imgproc.THRESH_BINARY_INV);
        ShowImage(imgResult, "Threshold");

        Imgproc.cvtColor(imgResult, imgResult, Imgproc.COLOR_RGB2GRAY);
        ShowImage(imgResult, "GRAY");

        Mat circles = new Mat();
        Imgproc.HoughCircles(imgResult,
                            circles,
                            Imgproc.CV_HOUGH_GRADIENT,
                            1,
                            45,
                            pCircleDetectorModel.param1,
                            pCircleDetectorModel.param2,
                            pCircleDetectorModel.minRadius,
                            pCircleDetectorModel.maxRadius);
        Debug.Log("Number of circles:" + circles.cols());
        Point pt = new Point();
        for (int i = 0; i < circles.cols(); i++)
        {
            double[] data = circles.get(0, i);
            pt.x = data[0];
            pt.y = data[1];
            double rho = data[2];

            Imgproc.circle(pSourceImage, pt, (int)rho, new Scalar(255, 0, 0, 255), 3);
        }

        ShowImage(pSourceImage, "circles");
    }

    void ShowImage(Mat mat, string name = "")
    {
        var g = Instantiate(imgPrefub, imgPanel);
        Texture2D texture = new Texture2D(mat.cols(), mat.rows(), TextureFormat.RGBA32, false);
        Utils.matToTexture2D(mat, texture);
        var img = g.GetComponent<Image>();
        var rect = new UnityEngine.Rect(0, 0, texture.width, texture.height);
        img.overrideSprite = Sprite.Create(texture, rect, new Vector2(0.5f, 0.5f));
        g.GetComponentInChildren<UnityEngine.UI.Text>().text = name;
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

