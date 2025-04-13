using System.Drawing;

namespace ONNXFaceLib.ML.DataModels
{
    public class FaceDetection
    {
        public RectangleF Box { get; set; }
        public float Confidence { get; set; }
    }
}
