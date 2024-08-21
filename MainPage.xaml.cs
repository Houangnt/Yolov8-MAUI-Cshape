using Microsoft.Maui.Controls;
using Microsoft.Maui.Storage;
using Compunet.YoloV8;
using Compunet.YoloV8.Data;
using System.IO;
using System.Threading.Tasks;

namespace Scratch_Detection
{
    public partial class MainPage : ContentPage
    {
        private string selectedImagePath;
        private YoloV8Predictor yoloPredictor;

        public MainPage()
        {
            InitializeComponent();
            InitializeYoloPredictor();
        }

        private void InitializeYoloPredictor()
        {
            try
            {
                // Initialize YOLOv8 predictor with your model path
                yoloPredictor = YoloV8Predictor.Create("C:\\Users\\Engineer\\source\\repos\\Scratch-Detection\\Asset\\model\\scratch_medium.onnx");
            }
            catch (Exception ex)
            {
                // Handle initialization error
                DisplayAlert("Error", $"Failed to initialize model: {ex.Message}", "OK");
            }
        }

        private async void OnSelectImageClicked(object sender, EventArgs e)
        {
            try
            {
                var fileResult = await FilePicker.PickAsync();
                if (fileResult != null)
                {
                    selectedImagePath = fileResult.FullPath;
                    OriginalImage.Source = ImageSource.FromFile(selectedImagePath);
                    ProcessedImage.Source = null; // Clear processed image
                    ResultsLabel.Text = "Detection detail will be shown here."; // Clear previous results
                }
            }
            catch (Exception ex)
            {
                await DisplayAlert("Error", $"Failed to select image: {ex.Message}", "OK");
            }
        }

        private async void OnDetectClicked(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(selectedImagePath))
            {
                await DisplayAlert("Error", "Please select an image first.", "OK");
                return;
            }

            try
            {
                var resultPath = Path.Combine(Path.GetDirectoryName(selectedImagePath), "result_" + Path.GetFileName(selectedImagePath));

                var detectionResult = await PredictAndSaveAsync(selectedImagePath, resultPath);

                if (detectionResult != null)
                {
                    ProcessedImage.Source = ImageSource.FromFile(resultPath);
                    await ShowDetectionResultsAsync(detectionResult); // Show detection results
                }
                else
                {
                    await DisplayAlert("Error", "Detection failed.", "OK");
                }
            }
            catch (Exception ex)
            {
                await DisplayAlert("Error", $"Detection error: {ex.Message}", "OK");
            }
        }

        private async Task<DetectionResult> PredictAndSaveAsync(string imagePath, string resultPath)
        {
            try
            {
                // Predict and save result
                var result = await yoloPredictor.PredictAndSaveAsync(imagePath, resultPath);

                // Attempt to cast to DetectionResult
                var detectionResult = result as DetectionResult;

                if (detectionResult != null)
                {
                    return detectionResult;
                }
                else
                {
                    await DisplayAlert("Error", "Prediction result is not of type DetectionResult.", "OK");
                    return null;
                }
            }
            catch (Exception ex)
            {
                // Handle prediction error
                await DisplayAlert("Error", $"Prediction error: {ex.Message}", "OK");
                return null;
            }
        }


        private async Task ShowDetectionResultsAsync(DetectionResult result)
        {
            try
            {
                if (result.Boxes.Length != 0)
                {
                    int errorNumber = 1;
                    var resultsText = "";

                    foreach (var box in result.Boxes)
                    {
                        var confidence = box.Confidence;
                        var rectangle = box.Bounds;

                        // Extract Width and Height
                        int width = rectangle.Width;
                        int height = rectangle.Height;

                        // Calculate the area of the bounding box
                        int area = width * height;

                        resultsText += $"Error number {errorNumber} have: Confidence: {confidence}, Area: {area} pixels\n";
                        errorNumber++;
                    }
                    ResultsLabel.Text = resultsText;
                }
                else
                {
                    ResultsLabel.Text = "Not found error in surface.";
                }
            }
            catch (Exception ex)
            {
                // Handle detection result error
                await DisplayAlert("Error", $"Failed to show detection results: {ex.Message}", "OK");
            }
        }
    }

}
