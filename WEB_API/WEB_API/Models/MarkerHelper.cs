using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using OpenCvSharp;
using OpenCvSharp.CPlusPlus;
using OpenCvSharp.Extensions;
using XEuropeApp;
using Point = OpenCvSharp.CPlusPlus.Point;

namespace WEB_API.Models
{
    public class MarkerHelper
    {
        private Mat _mat;
        private Image _img;
        private const int NO_OF_TILES = 2;
        private int _sizeModifier;
        private Mat  _gray;
        private List<Vec4i> _hierarchy;
        private MarkerDetector markerDetector;
        private bool _isMarkerDetected = false;
        private System.Windows.Rect _markerPosition;
        private string fileName;
        private string _result;

        public MarkerHelper(MarkerDetector detector, int sizeM = 2)
        {
            markerDetector = detector;
            _sizeModifier = sizeM;
        }

        public bool IsMarkerDetected
        {
            get { return _isMarkerDetected; }
        }


        private void SetMarkerDetected(bool detected)
        {
            _isMarkerDetected = detected;
        }

        public void ProcessFrame(DtouchMarker marker, String filename = "")
        {
            //fileName = System.Web.Hosting.HostingEnvironment.MapPath("/SampleImage/");
            //Get gray scale image.
            _gray = new Mat(filename, LoadMode.GrayScale);
            var color = new Mat(filename, LoadMode.Color);
            //Get image segment to detect marker.
            _markerPosition = calculateImageSegmentArea(_gray);
            var imgSegmentMat = CloneMarkerImageSegment(_gray);
            //var colorMat = CloneMarkerImageSegment(color);
            //apply threshold.)
            var thresholdedImgMat = new Mat(imgSegmentMat.Size(), imgSegmentMat.Type());



            applyThresholdOnImage(imgSegmentMat, thresholdedImgMat);

            //colorMat.ToBitmap().Save(fileName + "thresholdedColor2.png");

            imgSegmentMat.Release();
            //find markers.
            var markerFound = FindMarkers(thresholdedImgMat, marker);
            thresholdedImgMat.Release();
            //Marker detected.
            SetMarkerDetected(markerFound);

           
        }

        private bool FindMarkers(Mat imgMat, DtouchMarker marker)
        {
            var markerFound = false;
            var contourImg = imgMat.Clone();

            HierarchyIndex[] hier;
            Point[][] comp;

            contourImg.FindContours(out comp, out hier, ContourRetrieval.Tree, ContourChain.ApproxNone);

            _hierarchy = new List<Vec4i>();
            hier.ToList().ForEach(x => _hierarchy.Add(x.ToVec4i()));


            contourImg.Release();

            var code = new List<int>();

            for (var i = 0; i < comp.Count(); i++)
            {
                //clean this list.
                code.Clear();
                if (markerDetector.verifyRoot(i, _hierarchy, code))
                {
                    //if marker found.
                    marker.setCode(code);
                    marker.setComponentIndex(i);
                    markerFound = true;
                    break;
                }
            }

            //_components = null;
            _hierarchy.Clear();
            _hierarchy = null;
            return markerFound;
        }



        private System.Windows.Rect calculateImageSegmentArea(Mat imgMat)
        {
            var width = imgMat.Cols;
            var height = imgMat.Rows;
            var aspectRatio = (double)width / (double)height;

            var imgWidth = width / _sizeModifier;
            var imgHeight = height /  _sizeModifier;

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
            var x = (width - imgWidth) / 2;
            var y = (height - imgHeight) / 2;

            return new System.Windows.Rect(x, y, imgWidth, imgHeight);
        }

        private Mat CloneMarkerImageSegment(Mat imgMat)
        {
            var rect = calculateImageSegmentArea(imgMat);
            var calculatedImg = imgMat.SubMat((int)rect.Y, (int)rect.Y + (int)rect.Height, (int)rect.X,
                (int)rect.X + (int)rect.Width);

            return calculatedImg.Clone();
        }

        private static List<double> applyThresholdOnImage(Mat srcImgMat, Mat outputImgMat)
        {
            double localThreshold;
            int startRow, endRow, startCol, endCol;

            var tileWidth = (int)srcImgMat.Size().Height / NO_OF_TILES;
            var tileHeight = (int)srcImgMat.Size().Width / NO_OF_TILES;



            var localThresholds = new List<double>();

            //Split image into tiles and apply threshold on each image tile separately.  	
            //process image tiles other than the last one.
            for (var tileRowCount = 0; tileRowCount < NO_OF_TILES; tileRowCount++)
            {
                startRow = tileRowCount * tileWidth;

                if (tileRowCount < NO_OF_TILES - 1)
                    endRow = (tileRowCount + 1) * tileWidth;
                else
                    endRow = (int)srcImgMat.Size().Height;

                for (var tileColCount = 0; tileColCount < NO_OF_TILES; tileColCount++)
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
                    var tileThreshold = new Mat();
                    var tileMat = srcImgMat.SubMat(startRow, endRow, startCol, endCol);

                    localThreshold = OpenCvSharp.CPlusPlus.Cv2.Threshold(tileMat, tileThreshold, 0, 255,
                        ThresholdType.Binary | ThresholdType.Otsu);

                    var copyMat = outputImgMat.SubMat(startRow, endRow, startCol, endCol);
                    tileThreshold.CopyTo(copyMat);
                    tileThreshold.Release();
                    localThresholds.Add(localThreshold);
                }
            }

            return localThresholds;
        }
    }
}