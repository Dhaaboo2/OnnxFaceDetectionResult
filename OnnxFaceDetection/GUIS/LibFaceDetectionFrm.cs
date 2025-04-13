using System.Diagnostics.CodeAnalysis;
using ONNXFaceLib.ML.DataModels;

namespace OnnxFaceDetection.GUIS
{
    public partial class LibFaceDetectionFrm : Form
    {
        [AllowNull]
        private OpenFileDialog _odl;
        [AllowNull]
        private Bitmap loadedImage;
        public LibFaceDetectionFrm()
        {
            InitializeComponent();
            _odl = new OpenFileDialog { Filter = "Image Files|*.jpg;*.png;*.bmp" };
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
                var faceDetector = new FaceDetector();
                var faces = faceDetector.Detect(loadedImage);
                var displayImage = new Bitmap(loadedImage);

                using (Graphics g = Graphics.FromImage(displayImage))
                {
                    Pen pen = new Pen(Color.Red, 2);

                    foreach (var face in faces)
                    {
                        g.DrawRectangle(pen, face.X, face.Y, face.Width, face.Height);
                    }
                }
                PB.Image = displayImage;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message.ToString());
            }
        }
    }
}
