﻿using OpenCvSharp;
using OpenCvSharp.Features2D;
using OpenCvSharp.XFeatures2D;
using SampleBase;

namespace SamplesLegacy
{
    /// <summary>
    /// SIFT and SURF sample
    /// http://www.prism.gatech.edu/~ahuaman3/docs/OpenCV_Docs/tutorials/nonfree_1/nonfree_1.html
    /// </summary>
    class SiftSurfSample : ConsoleTestBase
    {
        public override void RunTest()
        {
            using var src1 = new Mat(ImagePath.Match1, ImreadModes.Color);
            using var src2 = new Mat(ImagePath.Match2, ImreadModes.Color);

            MatchBySift(src1, src2);
            MatchBySurf(src1, src2);
        }

        private void MatchBySift(Mat src1, Mat src2)
        {
            using var gray1 = new Mat();
            using var gray2 = new Mat();
            Cv2.CvtColor(src1, gray1, ColorConversionCodes.BGR2GRAY);
            Cv2.CvtColor(src2, gray2, ColorConversionCodes.BGR2GRAY);

            // Detect the keypoints and generate their descriptors using SIFT
            using var sift = SIFT.Create();
            using var descriptors1 = new Mat<float>();
            using var descriptors2 = new Mat<float>();
            sift.DetectAndCompute(gray1, null, out var keypoints1, descriptors1);
            sift.DetectAndCompute(gray2, null, out var keypoints2, descriptors2);

            // Match descriptor vectors
            using var bfMatcher = new BFMatcher(NormTypes.L2, false);
            using var flannMatcher = new FlannBasedMatcher();
            DMatch[] bfMatches = bfMatcher.Match(descriptors1, descriptors2);
            DMatch[] flannMatches = flannMatcher.Match(descriptors1, descriptors2);

            // Draw matches
            using var bfView = new Mat();
            using var flannView = new Mat();
            Cv2.DrawMatches(gray1, keypoints1, gray2, keypoints2, bfMatches, bfView);
            Cv2.DrawMatches(gray1, keypoints1, gray2, keypoints2, flannMatches, flannView);

            using (new Window("SIFT matching (by BFMather)", bfView))
            using (new Window("SIFT matching (by FlannBasedMatcher)", flannView))
            {
                Cv2.WaitKey();
            }
        }

        private void MatchBySurf(Mat src1, Mat src2)
        {
            using var gray1 = new Mat();
            using var gray2 = new Mat();
            Cv2.CvtColor(src1, gray1, ColorConversionCodes.BGR2GRAY);
            Cv2.CvtColor(src2, gray2, ColorConversionCodes.BGR2GRAY);

            // Detect the keypoints and generate their descriptors using SURF
            using var surf = SURF.Create(200, 4, 2, true);
            using var descriptors1 = new Mat<float>();
            using var descriptors2 = new Mat<float>();
            surf.DetectAndCompute(gray1, null, out var keypoints1, descriptors1);
            surf.DetectAndCompute(gray2, null, out var keypoints2, descriptors2);

            // Match descriptor vectors 
            using var bfMatcher = new BFMatcher(NormTypes.L2, false);
            using var flannMatcher = new FlannBasedMatcher();
            DMatch[] bfMatches = bfMatcher.Match(descriptors1, descriptors2);
            DMatch[] flannMatches = flannMatcher.Match(descriptors1, descriptors2);

            // Draw matches
            using var bfView = new Mat();
            using var flannView = new Mat();
            Cv2.DrawMatches(gray1, keypoints1, gray2, keypoints2, bfMatches, bfView);
            Cv2.DrawMatches(gray1, keypoints1, gray2, keypoints2, flannMatches, flannView);

            using (new Window("SURF matching (by BFMather)", bfView))
            using (new Window("SURF matching (by FlannBasedMatcher)", flannView))
            {
                Cv2.WaitKey();
            }
        }

    }
}