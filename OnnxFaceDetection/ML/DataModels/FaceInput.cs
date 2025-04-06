using System.Diagnostics.CodeAnalysis;

namespace OnnxFaceDetection.ML.DataModels;

public class FaceInput
{
    [AllowNull]
    public Bitmap ImageSource { get; set; }
}
