using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using OpenCvSharp;
using System.Web.Mvc;
using OpenCvSharp.CPlusPlus;
using XEuropeApp;
using Point = OpenCvSharp.CPlusPlus.Point;

namespace WEB_API.Controllers
{
    public class HomeController : Controller
    {
        private Mat _mat;
        private Image _img;
        private const int NO_OF_TILES = 2;

        private Mat rgba, gray, mMarkerImage;
        private List<Vec4i> hierarchy;
        private Mat[] components;
        private MarkerDetector markerDetector;
        private bool isMarkerDetected = false;
        private System.Windows.Rect markerPosition;
        private string path;
        private string _result;
        //private VideoCapture _videoCapture;
        public ActionResult Index()
        {
            path = Server.MapPath("/SampleImage/");
            _img = Image.FromFile(path + "1-1-1-24.png");
            var fileStream = Converters.imageToByteArray(_img);
            var baseStrig = System.Convert.ToBase64String(fileStream);
            var tesdt = baseStrig.ToLower();
            var image = Converters.byteArrayToImage(fileStream);
            image.Save(path + "temp.png", ImageFormat.Png);
            //_img.Save(path + "test.png");
            var marker = new DtouchMarker();
            //_videoCapture = new VideoCapture(CaptureDevice.MIL);
            markerDetector = new MarkerDetector(new HIPreference());
            processFrame(marker, path + "temp.png");

            if (isMarkerDetected)
            {
                ViewBag.Title = "Talált!";
                ViewBag.Message = marker.getCodeKey();
                ViewBag.Logo = path + @"markered_match.png";

            }
            else
            {
                ViewBag.Title = "Nem talált :(";
                
            }
            if (System.IO.File.Exists(path + "temp.png"))
            {
                System.IO.File.Delete(path + "temp.png");
            }
            return View();
        }
        private void setMarkerDetected(bool detected)
        {
            isMarkerDetected = detected;
        }
        private void processFrame(DtouchMarker marker, String filename = "")
        {

            //Get original image.
            rgba = new Mat(filename, LoadMode.Color);
            //Get gray scale image.
            gray = new Mat(filename, LoadMode.GrayScale);
            //Get image segment to detect marker.
            markerPosition = calculateImageSegmentArea(gray);
            Mat imgSegmentMat = cloneMarkerImageSegment(gray);
            //apply threshold.)
            Mat thresholdedImgMat = new Mat(imgSegmentMat.Size(), imgSegmentMat.Type());
                
                //imgSegmentMat.Threshold(0, 255, ThresholdType.Binary | ThresholdType.Otsu);

            applyThresholdOnImage(imgSegmentMat, thresholdedImgMat);
            //thresholdedImgMat.ToBitmap().Save(path + "thresholded2.png");

            imgSegmentMat.Release();
            //find markers.
            bool markerFound = findMarkers(thresholdedImgMat, marker);
            thresholdedImgMat.Release();
            //Marker detected.
            if (markerFound)
            {
                setMarkerDetected(true);
                //if marker is found then copy the marker image segment.
                mMarkerImage = cloneMarkerImageSegment(rgba);

                //display rect with indication that a marker is identified.
                displayRectOnImageSegment(rgba, true);
                //display marker image
                displayMarkerImage(mMarkerImage, rgba);
                //mMarkerImage.ToBitmap().Save(path + "markered_match.png");
            }
            else
            {
                displayRectOnImageSegment(rgba, false);
            }
        }
        private void displayMarkerImage(Mat srcImgMat, Mat destImageMat)
        {
            //find location of image segment to be replaced in the destination image.
            var rect = calculateImageSegmentArea(destImageMat);
            Mat destSubmat = destImageMat.SubMat((int) rect.Y, (int) rect.Y + (int) rect.Height, (int) rect.X,
                (int) rect.X + (int) rect.Width);
            //copy image.
            srcImgMat.CopyTo(destSubmat);
        }
        private bool findMarkers(Mat imgMat, DtouchMarker marker)
        {
            bool markerFound = false;
            Mat contourImg = imgMat.Clone();
            Mat[] compArray = new Mat[0];
            //components = new List<MatOfPoint>();
            HierarchyIndex[] hier;
            Point[][] comp;
            //Mat[] contoursMat;
            //Cv2.FindContours(contourImg, out components, hierarchy, ContourRetrieval.Tree, ContourChain.ApproxNone);
            contourImg.FindContours(out comp, out hier,ContourRetrieval.Tree, ContourChain.ApproxNone);
            
            hierarchy = new List<Vec4i>();
            hier.ToList().ForEach(x => hierarchy.Add(x.ToVec4i()));
            
            
            contourImg.Release();

            List<int> code = new List<int>();
            
            for (int i = 0; i < comp.Count(); i++)
            {
                //clean this list.
                code.Clear();
                if (markerDetector.verifyRoot(i, hierarchy , code))
                {
                    //if marker found.
                    //code.Sort();
                    marker.setCode(code);
                    //marker.setComponent(mComponents.get(i));
                    marker.setComponentIndex(i);
                    markerFound = true;
                    break;
                }
            }

            ;
            //hier.();

            components = null;
            hierarchy.Clear();
            hierarchy = null;
            return markerFound;
        }

        private void displayRectOnImageSegment(Mat imgMat, bool markerFound)
        {
            var color = new Color();
            color = markerFound ? Color.FromArgb(255, 0, 255, 0) : Color.FromArgb(255, 255, 0, 0);
            System.Windows.Rect rect = calculateImageSegmentArea(imgMat);

            // OpenCV-s függvény, vélhetőleg ez helyezi el a képernyőn a négyzetet. Nekünk lehet más megoldásunk szerintem.
            //Core.rectangle(imgMat, rect.tl(), rect.br(), color, 3, Core.LINE_AA, 0);
        }

        private System.Windows.Rect calculateImageSegmentArea(Mat imgMat)
        {
            int width = imgMat.Cols;
            int height = imgMat.Rows;
            double aspectRatio = (double)width / (double)height;

            int imgWidth = width;
            int imgHeight = height;

            //Size of width and height varies of input image can vary. Make the image segment width and height equal in order to make
            //the image segment square.

            //if width is more than the height.
            if (aspectRatio > 1)
            {
                //if total height is greater than imgWidth.
                if (height > imgWidth)
                    imgHeight = imgWidth;
            }
            else if (aspectRatio < 1)
            { //height is more than the width.
                //if total width is greater than the imgHeight.
                if (width > imgHeight)
                    imgWidth = imgHeight;
            }

            //find the centre position in the source image.
            int x = (width - imgWidth) / 2;
            int y = (height - imgHeight) / 2;

            return new System.Windows.Rect(x, y, imgWidth, imgHeight);
        }

        private Mat cloneMarkerImageSegment(Mat imgMat)
        {
            System.Windows.Rect rect = calculateImageSegmentArea(imgMat);
            Mat calculatedImg = imgMat.SubMat((int) rect.Y, (int) rect.Y + (int) rect.Height, (int) rect.X,
                (int) rect.X + (int) rect.Width);
                
                /*new Mat(imgMat,
                new CvSlice((int)rect.Y, (int)(rect.Y + rect.Height)),
                new CvSlice((int)rect.X, (int)(rect.X + rect.Width)));*/
            return calculatedImg.Clone();
        }

        private List<double> applyThresholdOnImage(Mat srcImgMat, Mat outputImgMat)
        {
            double localThreshold;
            int startRow, endRow, startCol, endCol;

            int tileWidth = (int)srcImgMat.Size().Height / NO_OF_TILES;
            int tileHeight = (int)srcImgMat.Size().Width / NO_OF_TILES;

            

            var localThresholds = new List<double>();

            //Split image into tiles and apply threshold on each image tile separately.  	
            //process image tiles other than the last one.
            for (int tileRowCount = 0; tileRowCount < NO_OF_TILES; tileRowCount++)
            {
                startRow = tileRowCount * tileWidth;

                if (tileRowCount < NO_OF_TILES - 1)
                    endRow = (tileRowCount + 1) * tileWidth;
                else
                    endRow = (int)srcImgMat.Size().Height;

                for (int tileColCount = 0; tileColCount < NO_OF_TILES; tileColCount++)
                {
                    startCol = tileColCount * tileHeight;
                    if (tileColCount < NO_OF_TILES - 1)
                    {
                        endCol = (tileColCount + 1) * tileHeight;
                    }
                    else
                    {
                        endCol = (int)srcImgMat.Size().Width;
                    }
                    Mat tileThreshold = new Mat();
                    Mat tileMat = srcImgMat.SubMat(startRow, endRow, startCol, endCol);
                    //Mat tileMat = new Mat(tileThreshold, new CvSlice(startRow, endRow), new CvSlice(startCol, endCol));

                    localThreshold = OpenCvSharp.CPlusPlus.Cv2.Threshold(tileMat, tileThreshold, 0, 255,
                        ThresholdType.Binary | ThresholdType.Otsu); 
                        
                        //NativeMethods.imgproc_threshold(tileMat.CvPtr, tileThreshold.CvPtr, 0, 255,
                        //CvConst.CV_THRESH_BINARY | CvConst.CV_THRESH_OTSU);


                    //localThreshold = Imgproc.threshold(tileMat, tileThreshold, 0, 255, Imgproc.THRESH_BINARY | Imgproc.THRESH_OTSU);
                    //Mat copyMat = new Mat(outputImgMat, new CvSlice(startRow, endRow), new CvSlice(startCol, endCol));
                    Mat copyMat = outputImgMat.SubMat(startRow, endRow, startCol, endCol);
                    tileThreshold.CopyTo(copyMat);
                    tileThreshold.Release();
                    localThresholds.Add(localThreshold);
                }
            }

            return localThresholds;
        }
        /*
        public WriteableBitmap ToGrayScale(WriteableBitmap bitmapImage)
        {

            for (var y = 0; y < bitmapImage.PixelHeight; y++)
            {
                for (var x = 0; x < bitmapImage.PixelWidth; x++)
                {
                    var pixelLocation = bitmapImage.PixelWidth * y + x;
                    var pixel = bitmapImage.Pixels[pixelLocation];
                    var pixelbytes = BitConverter.GetBytes(pixel);
                    var bwPixel = (byte)(.299 * pixelbytes[2] + .587 * pixelbytes[1] + .114 * pixelbytes[0]);
                    pixelbytes[0] = bwPixel;
                    pixelbytes[1] = bwPixel;
                    pixelbytes[2] = bwPixel;
                    bitmapImage.Pixels[pixelLocation] = BitConverter.ToInt32(pixelbytes, 0);
                }
            }

            return bitmapImage;
        }
        */
    }
}
