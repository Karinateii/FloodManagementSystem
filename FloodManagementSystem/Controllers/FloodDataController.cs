using GlobalDisasterManagement.Data;
using GlobalDisasterManagement.Models;
using GlobalDisasterManagement.Models.Configuration;
using GlobalDisasterManagement.Services.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.ML;
using Microsoft.ML.Data;
using System.Globalization;
using System.Security.Claims;
using System.Net.Mail;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Options;

namespace GlobalDisasterManagement.Controllers
{
    [Authorize]
    public class FloodDataController : Controller
    {
        private readonly DisasterDbContext _context;
        private readonly UserManager<User> _userManager;
        private readonly IWebHostEnvironment _environment;
        private readonly EmailSettings _emailSettings;
        private readonly ILogger<FloodDataController> _logger;
        private readonly INotificationService _notificationService;

        public FloodDataController(
            DisasterDbContext context, 
            UserManager<User> userManager, 
            IWebHostEnvironment environment,
            IOptions<EmailSettings> emailSettings,
            ILogger<FloodDataController> logger,
            INotificationService notificationService)
        {
            _context = context;
            _userManager = userManager;
            _environment = environment;
            _emailSettings = emailSettings.Value;
            _logger = logger;
            _notificationService = notificationService;
        }

        public async Task<string> GetCurrentUserIdAsync(ClaimsPrincipal user)
        {
            var currentUser = await _userManager.GetUserAsync(user);
            return currentUser.Id;
        }

        [Authorize(Roles = "admin")]
        public async Task<IActionResult> Train(Guid id)
        {
            try
            {
                _logger.LogInformation("Starting model training for file {FileId}", id);
                
                var filePath = Path.Combine(_environment.WebRootPath, "Uploads", id + ".csv");
                
                if (!System.IO.File.Exists(filePath))
                {
                    _logger.LogWarning("Training file not found: {FilePath}", filePath);
                    TempData["Error"] = "Training file not found.";
                    return RedirectToAction("Index", "Admin");
                }

                var mlContext = new MLContext();
                var dataView = mlContext.Data.LoadFromTextFile<FloodDataModel>(filePath, separatorChar: ',', hasHeader: true);
                var featureColumns = mlContext.Transforms.Categorical.OneHotEncoding("CityEncoded", "City")
                    .Append(mlContext.Transforms.Conversion.ConvertType("MonthString", "Month", outputKind: DataKind.Single))
                    .Append(mlContext.Transforms.Conversion.ConvertType("MaxTempString", "MaxTemp", outputKind: DataKind.Single))
                    .Append(mlContext.Transforms.Conversion.ConvertType("MinTempString", "MinTemp", outputKind: DataKind.Single))
                    .Append(mlContext.Transforms.Conversion.ConvertType("MeanTempString", "MeanTemp", outputKind: DataKind.Single))
                    .Append(mlContext.Transforms.Conversion.ConvertType("PrecipitationString", "Precipitation", outputKind: DataKind.Single))
                    .Append(mlContext.Transforms.Conversion.ConvertType("PrecipCoverString", "PrecipCover", outputKind: DataKind.Single))
                    .Append(mlContext.Transforms.Concatenate("Features", "CityEncoded", "MonthString", "MaxTempString", "MinTempString", "MeanTempString", "PrecipitationString", "PrecipCoverString"))
                    .Append(mlContext.Transforms.NormalizeMinMax("MonthEncoded", "Month"))
                    .Append(mlContext.Transforms.Concatenate("FeaturesEncoded", "CityEncoded", "MonthEncoded"));

                var transformedDataView = featureColumns.Fit(dataView).Transform(dataView);
                var labelColumn = "FloodRisk";
                var dataSplit = mlContext.Data.TrainTestSplit(dataView, testFraction: 0.3);
                var trainingData = dataSplit.TrainSet;
                var testingData = dataSplit.TestSet;
                var trainer = mlContext.BinaryClassification.Trainers.FastTree(labelColumnName: labelColumn, featureColumnName: "FeaturesEncoded");
                var pipeline = featureColumns.Append(trainer);
                var model = pipeline.Fit(trainingData);
                var predictions = model.Transform(testingData);
                var metrics = mlContext.BinaryClassification.Evaluate(predictions, labelColumnName: labelColumn);
                ViewData["Accuracy"] = metrics.Accuracy;

                _logger.LogInformation("Model trained with accuracy: {Accuracy}", metrics.Accuracy);

                // Save the model to a file
                var modelPath = Path.Combine(_environment.WebRootPath, "Uploads", id + ".zip");
                if (System.IO.File.Exists(modelPath))
                {
                    System.IO.File.Delete(modelPath);
                }
                mlContext.Model.Save(model, trainingData.Schema, modelPath);

                _logger.LogInformation("Model saved successfully to {ModelPath}", modelPath);
                TempData["Success"] = $"Model trained successfully with {metrics.Accuracy:P2} accuracy.";

                return RedirectToAction("Index", "Admin");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error training model for file {FileId}", id);
                TempData["Error"] = "An error occurred while training the model.";
                return RedirectToAction("Index", "Admin");
            }
        }


        [Authorize(Roles = "admin")]
        public async Task<IActionResult> Predict(Guid id, int cityid, Guid modelid, string year)
        {
            try
            {
                _logger.LogInformation("Starting prediction for city {CityId} using model {ModelId}", cityid, modelid);

                var city = await _context.Cities.FindAsync(cityid);
                var modelName = await _context.CsvFiles.FindAsync(modelid);
                
                if (city == null || modelName == null)
                {
                    _logger.LogWarning("City or model not found. CityId: {CityId}, ModelId: {ModelId}", cityid, modelid);
                    TempData["Error"] = "City or model not found.";
                    return RedirectToAction("Index", "Admin");
                }

                var modelPath = Path.Combine(_environment.WebRootPath, "Uploads", modelid + ".zip");
                var filePredpath = Path.Combine(_environment.WebRootPath, "Uploads", id.ToString() + ".csv");

                if (!System.IO.File.Exists(modelPath) || !System.IO.File.Exists(filePredpath))
                {
                    _logger.LogWarning("Required files not found. Model: {ModelPath}, Data: {DataPath}", modelPath, filePredpath);
                    TempData["Error"] = "Required files not found.";
                    return RedirectToAction("Index", "Admin");
                }

                var mlContext = new MLContext();
                using var modelStream = new FileStream(modelPath, FileMode.Open, FileAccess.Read, FileShare.Read);
                var trainedModel = mlContext.Model.Load(modelStream, out var schema);

                var input = mlContext.Data.LoadFromTextFile<FloodDataModel>(filePredpath, separatorChar: ',', hasHeader: true);
                var predictionEngine = mlContext.Model.CreatePredictionEngine<FloodDataModel, FloodPredictionResult>(trainedModel);
                var predictions = mlContext.Data.CreateEnumerable<FloodDataModel>(input, reuseRowObject: false)
                                        .Select(inp => predictionEngine.Predict(inp))
                                        .ToList();

                var cityPredictions = new List<CityFloodPrediction>();
                var monthCount = 1;
                
                foreach (var item in predictions)
                {
                    var ps = new CityFloodPrediction
                    {
                        FileId = id,
                        ModelId = modelid,
                        ModelFileName = modelName.Name,
                        CityId = cityid,
                        City = city.Name,
                        Year = year,
                        Month = CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(monthCount),
                        Prediction = item.Prediction.ToString()
                    };
                    monthCount++;
                    cityPredictions.Add(ps);
                }

                // Bulk insert for better performance
                await _context.CityPredictions.AddRangeAsync(cityPredictions);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Created {Count} predictions for city {CityName}", cityPredictions.Count, city.Name);

                // Send email notifications for high-risk predictions
                var highRiskPredictions = cityPredictions.Where(p => p.Prediction == true.ToString()).ToList();
                foreach (var prediction in highRiskPredictions)
                {
                    CheckStatusAndSendEmail(prediction);
                    
                    // Send real-time notification via SignalR
                    await _notificationService.SendFloodAlertToCity(cityid, prediction);
                }

                TempData["Success"] = $"Predictions generated successfully for {city.Name}.";
                return RedirectToAction("Index", "Admin");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating predictions for city {CityId}", cityid);
                TempData["Error"] = "An error occurred while generating predictions.";
                return RedirectToAction("Index", "Admin");
            }
        }


        public async Task<IActionResult> Predictions()
        {
            try
            {
                var userId = await GetCurrentUserIdAsync(User);
                var user = await _context.Users.FindAsync(userId);
                
                if (user == null)
                {
                    _logger.LogWarning("User not found: {UserId}", userId);
                    return RedirectToAction("Index", "Home");
                }

                var predictionsForUser = _context.CityPredictions
                    .Where(x => x.CityId == user.CityId)
                    .OrderByDescending(x => x.Year)
                    .ThenBy(x => x.Month)
                    .ToList();

                return View(predictionsForUser);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving predictions for user");
                TempData["Error"] = "An error occurred while loading predictions.";
                return RedirectToAction("Index", "Home");
            }
        }
        
        private void CheckStatusAndSendEmail(CityFloodPrediction entity)
        {
            if (entity.Prediction == true.ToString())
            {
                var users = _context.Users.Where(x => x.CityId == entity.CityId).ToList();
                
                if (string.IsNullOrEmpty(_emailSettings.SenderEmail) || string.IsNullOrEmpty(_emailSettings.SenderPassword))
                {
                    _logger.LogWarning("Email settings not configured. Skipping email notification.");
                    return;
                }

                try
                {
                    using var mail = new MailMessage();
                    using var smtpServer = new SmtpClient(_emailSettings.SmtpServer);

                    mail.From = new MailAddress(_emailSettings.SenderEmail);
                    foreach (var user in users)
                    {
                        if (!string.IsNullOrEmpty(user.Email))
                        {
                            mail.To.Add(user.Email);
                        }
                    }

                    if (mail.To.Count == 0)
                    {
                        _logger.LogWarning("No valid email addresses found for city {CityId}", entity.CityId);
                        return;
                    }

                    mail.Subject = "Flood Risk Alert in Your City";
                    mail.Body = $"Dear Resident,\n\nYour city, {entity.City}, is predicted to have a high flood risk in {entity.Month} {entity.Year}.\n\nPlease take necessary precautions.\n\nBest regards,\nLagos Flood Detection System";

                    smtpServer.Port = _emailSettings.SmtpPort;
                    smtpServer.Credentials = new System.Net.NetworkCredential(_emailSettings.SenderEmail, _emailSettings.SenderPassword);
                    smtpServer.EnableSsl = _emailSettings.EnableSsl;

                    smtpServer.Send(mail);
                    _logger.LogInformation("Flood alert email sent to {Count} users for city {City}", mail.To.Count, entity.City);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to send flood alert email for city {City}", entity.City);
                }
            }
        }
    }
}
