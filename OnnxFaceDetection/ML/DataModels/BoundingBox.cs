using System.Diagnostics.CodeAnalysis;

namespace OnnxFaceDetection.ML.DataModels;

public class BoundingBoxDimensions
{
    public float X { get; set; }
    public float Y { get; set; }
    public float Height { get; set; }
    public float Width { get; set; }
}

public class BoundingBox
{
    [AllowNull]
    public BoundingBoxDimensions Dimensions { get; set; }

    [AllowNull]
    public string Label { get; set; }

    public float Confidence { get; set; }

    public RectangleF Rect
    {
        get { return new RectangleF(Dimensions.X, Dimensions.Y, Dimensions.Width, Dimensions.Height); }
    }

    public Color BoxColor { get; set; }

    public string Description => $"{Label} ({(Confidence * 100).ToString("0")}%)";

    private static readonly Color[] classColors = new Color[]
    {
        Color.Khaki, Color.Fuchsia, Color.Silver, Color.RoyalBlue,
        Color.Green, Color.DarkOrange, Color.Purple, Color.Gold,
        Color.Red, Color.Aquamarine, Color.Lime, Color.AliceBlue,
        Color.Sienna, Color.Orchid, Color.Tan, Color.LightPink,
        Color.Yellow, Color.HotPink, Color.OliveDrab, Color.SandyBrown,
        Color.DarkTurquoise
    };

    public static Color GetColor(int index) => index < classColors.Length ? classColors[index] : classColors[index % classColors.Length];
}

public class BoundingBoxDimensions2
{
    public float X { get; set; }
    public float Y { get; set; }
    public float Width { get; set; }
    public float Height { get; set; }
}

public class BoundingBox2
{
    [AllowNull]
    public BoundingBoxDimensions2 Dimensions { get; set; }
    public float Confidence { get; set; }
    public Color BoxColor { get; set; } = Color.Green;

    public RectangleF Rect => new RectangleF(Dimensions.X, Dimensions.Y, Dimensions.Width, Dimensions.Height);
}
