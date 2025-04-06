using System.Diagnostics.CodeAnalysis;

namespace OnnxFaceDetection.ML.DataModels;

public class FacePredictions
{
    [AllowNull]
    public float[] FaceType { get; set; }
}
