using Microsoft.ML.OnnxRuntime.Tensors;
using Microsoft.ML.OnnxRuntime;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Diagnostics.CodeAnalysis;
using ONNXFaceLib.Properties;

namespace ONNXFaceLib.ML.DataModels
{
    public class FaceDetector
    {
        [AllowNull]
        private readonly InferenceSession _session;

        public float _ConfidenceThreshold { get; set; } 
        public float _NmsThreshold { get; set; }

        public FaceDetector(float ConfidenceThreshold = 0.5f, float NmsThreshold= 0.4f,string ModelPath= null)
        {
            try
            {
                _ConfidenceThreshold = ConfidenceThreshold;
                _NmsThreshold = NmsThreshold;

                _session = new InferenceSession(ModelPath);
                
            }
            catch (Exception ex)
            {
               Console.WriteLine($"[FaceDetector] Error loading model: {ex.Message}");
            }
        }

        public List<RectangleF> Detect(Bitmap image)
        {

                var resized = new Bitmap(image, new Size(640, 640));
                var input = PreprocessImage(resized);

                var inputs = new List<NamedOnnxValue>
                {
                    NamedOnnxValue.CreateFromTensor("images", input)
                };

                using var results = _session.Run(inputs);
                var output = results.First().AsTensor<float>().ToArray();

                var rawBoxes = Parse(output, image.Width, image.Height);
                var finalBoxes = ApplyNms(rawBoxes, _NmsThreshold);

               
                return finalBoxes;
          
        }

        private DenseTensor<float> PreprocessImage(Bitmap bmp)
        {
            var tensor = new DenseTensor<float>(new[] { 1, 3, 640, 640 });

            for (int y = 0; y < 640; y++)
                for (int x = 0; x < 640; x++)
                {
                    var color = bmp.GetPixel(x, y);
                    tensor[0, 0, y, x] = color.R / 255f;
                    tensor[0, 1, y, x] = color.G / 255f;
                    tensor[0, 2, y, x] = color.B / 255f;
                }

            return tensor;
        }

        private List<Detection> Parse(float[] output, int origW, int origH)
        {
            var boxes = new List<Detection>();
            int num = 8400;

            for (int i = 0; i < num; i++)
            {
                float conf = output[4 * num + i];
                if (conf < _ConfidenceThreshold) continue;

                float x = (output[0 * num + i] - output[2 * num + i] / 2) * origW / 640;
                float y = (output[1 * num + i] - output[3 * num + i] / 2) * origH / 640;
                float w = output[2 * num + i] * origW / 640;
                float h = output[3 * num + i] * origH / 640;

                boxes.Add(new Detection
                {
                    Box = new RectangleF(x, y, w, h),
                    Confidence = conf
                });
            }

            //Debug.WriteLine($"[FaceDetector] Parsed {boxes.Count} raw detections above threshold.");
            return boxes;
        }

        private List<RectangleF> ApplyNms(List<Detection> detections, float threshold)
        {
            var results = new List<RectangleF>();

            var sorted = detections.OrderByDescending(d => d.Confidence).ToList();

            while (sorted.Count > 0)
            {
                var best = sorted[0];
                results.Add(best.Box);
                sorted.RemoveAt(0);

                sorted = sorted.Where(d => IoU(best.Box, d.Box) < threshold).ToList();
            }

           // Debug.WriteLine($"[FaceDetector] {results.Count} boxes remaining after NMS.");
            return results;
        }

        private float IoU(RectangleF a, RectangleF b)
        {
            float x1 = Math.Max(a.Left, b.Left);
            float y1 = Math.Max(a.Top, b.Top);
            float x2 = Math.Min(a.Right, b.Right);
            float y2 = Math.Min(a.Bottom, b.Bottom);

            float interArea = Math.Max(0, x2 - x1) * Math.Max(0, y2 - y1);
            float unionArea = a.Width * a.Height + b.Width * b.Height - interArea;

            return unionArea == 0 ? 0 : interArea / unionArea;
        }

        private class Detection
        {
            public RectangleF Box { get; set; }
            public float Confidence { get; set; }
        }
    }
}
