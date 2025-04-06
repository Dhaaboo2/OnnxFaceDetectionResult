using Microsoft.ML.OnnxRuntime;
using Microsoft.ML.OnnxRuntime.Tensors;
using OnnxFaceDetection.ML.DataModels;
using System.Data;
using System.Diagnostics.CodeAnalysis;
using System.Security.Cryptography;
using System.Windows.Forms;

namespace OnnxFaceDetection.GUIS
{
    public partial class OnnxFaceDetectionFrm : Form
    {

        [AllowNull]
        private OpenFileDialog _odl;
        [AllowNull]
        private InferenceSession _session;
        [AllowNull]
        private Bitmap loadedImage;
        public const int featuresPerBox = 5;
        private const float CONFIDENCE_THRESHOLD = 0.5f;  // Adjust as needed
        private const int INPUT_SIZE = 640;
        private const int ModelInputSize = 640;
        public OnnxFaceDetectionFrm()
        {
            InitializeComponent();

            _odl = new OpenFileDialog { Filter = "Image Files|*.jpg;*.png;*.bmp" };
            LoadModel();


        }

        private void LoadModel()
        {
            try
            {
                var _Prodir = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "../../../", "ML"));
                var _yolov8x = Path.Combine(_Prodir, "OnnxModels", "yolov8x-face-lindevs.onnx");
                _session = new InferenceSession(_yolov8x);
                MessageBox.Show("Model Loaded Successfully.");
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message.ToString());
            }

        }

        private void btnbrow_Click(object sender, EventArgs e)
        {
            try
            {
                if (_odl.ShowDialog() == DialogResult.OK)
                {
                    loadedImage = new Bitmap(_odl.FileName);
                    PB.Image = loadedImage;
                    //var image = Image.FromFile(_odl.FileName);
                    //PB.Image = DetectFaces(image);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message.ToString());
            }
        }
        private void btndet_Click(object sender, EventArgs e)
        {
            try
            {
                if (loadedImage == null)
                {
                    MessageBox.Show("Please load an image first.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }
                var resized = new Bitmap(loadedImage, new Size(ModelInputSize, ModelInputSize));
                var input = Preprocess(resized);

                using var results = _session.Run(new[] {
                NamedOnnxValue.CreateFromTensor("images", input)
            });

                var output = results.First().AsEnumerable<float>().ToArray();
                var detections = ParseDetections(output, loadedImage.Width, loadedImage.Height);

                Bitmap annotated = DrawDetections(loadedImage, detections);
                PB.Image = annotated;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message.ToString());
            }
        }

        private Tensor<float> Preprocess(Bitmap bitmap)
        {
            var input = new DenseTensor<float>(new[] { 1, 3, ModelInputSize, ModelInputSize });

            for (int y = 0; y < ModelInputSize; y++)
            {
                for (int x = 0; x < ModelInputSize; x++)
                {
                    var pixel = bitmap.GetPixel(x, y);
                    input[0, 0, y, x] = pixel.R / 255f;
                    input[0, 1, y, x] = pixel.G / 255f;
                    input[0, 2, y, x] = pixel.B / 255f;
                }
            }

            return input;
        }

        private List<RectangleF> ParseDetections(float[] output, int origW, int origH, float confThreshold = 0.5f)
        {
            List<RectangleF> boxes = new();
            int numDetections = 8400;
            //int numAttrs = 5;

            for (int i = 0; i < numDetections; i++)
            {
                float x_center = output[0 * numDetections + i];
                float y_center = output[1 * numDetections + i];
                float width = output[2 * numDetections + i];
                float height = output[3 * numDetections + i];
                float conf = output[4 * numDetections + i];

                if (conf < confThreshold)
                    continue;

                float scaleX = (float)origW / ModelInputSize;
                float scaleY = (float)origH / ModelInputSize;

                float x = (x_center - width / 2) * scaleX;
                float y = (y_center - height / 2) * scaleY;
                float w = width * scaleX;
                float h = height * scaleY;

                boxes.Add(new RectangleF(x, y, w, h));
            }

            return boxes;
        }

        private Bitmap DrawDetections(Bitmap original, List<RectangleF> boxes)
        {
            Bitmap result = new(original);
            using Graphics g = Graphics.FromImage(result);
            using Pen pen = new(Color.Red, 2);
            using Font font = new("Arial", 12);
            using Brush brush = new SolidBrush(Color.Yellow);

            foreach (var box in boxes)
            {
                g.DrawRectangle(pen, box.X, box.Y, box.Width, box.Height);
                g.DrawString("Face", font, brush, box.X, box.Y - 20);
            }

            return result;
        }



    }
}
