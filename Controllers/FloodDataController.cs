using Azure.Core;
using NewLagosFloodDetectionSystem.Data;
using NewLagosFloodDetectionSystem.Models;
using NewLagosFloodDetectionSystem.Models.DTO;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.ML;
using Microsoft.ML.Transforms;
using Microsoft.ML.Data;
using System.Data;
using System.Globalization;
using System.Security.Claims;
using System.Net.Mail;
using System.Xml;
using System.IO;
using Microsoft.AspNetCore.Authorization;

namespace NewLagosFloodDetectionSystem.Controllers
{
  //  [Authorize(Roles = "admin")]
    public class FloodDataController : Controller
    {
        private readonly FloodDbContext _context;
        private readonly UserManager<User> _userManager;
        private readonly IWebHostEnvironment _environment;

        public FloodDataController(FloodDbContext context, UserManager<User> userManager, IWebHostEnvironment environment)
        {
            _context = context;
            _userManager = userManager;
            _environment = environment;
        }

        public async Task<string> GetCurrentUserIdAsync(ClaimsPrincipal user)
        {
            var currentUser = await _userManager.GetUserAsync(user);
            return currentUser.Id;
        }

        public async Task<IActionResult> Train(Guid id)
        {
            var filePath = Path.Combine(_environment.WebRootPath, "Uploads", id + ".csv");
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

            // Save the model to a file
            var modelPath = Path.Combine(_environment.WebRootPath, "Uploads", id + ".zip");
            if (System.IO.File.Exists(modelPath))
            {
                System.IO.File.Delete(modelPath);
            }
            mlContext.Model.Save(model, trainingData.Schema, modelPath);

            return RedirectToAction("Index", "Admin");
        }


        public async Task<IActionResult> Predict(Guid id, int cityid, Guid modelid, string year)
        {
            var city = _context.Cities.Find(cityid);
            var modelName = _context.CsvFiles.Find(modelid);
            var mlContext = new MLContext();
            var modelPath = Path.Combine(_environment.WebRootPath, "Uploads", modelid + ".zip");
            var filePredpath = Path.Combine(_environment.WebRootPath, "Uploads", id.ToString() + ".csv");
            using var modelStream = new FileStream(modelPath, FileMode.Open, FileAccess.Read, FileShare.Read);
            var trainedModel = mlContext.Model.Load(modelStream, out var schema);


            var input = mlContext.Data.LoadFromTextFile<FloodDataModel>(filePredpath, separatorChar: ',', hasHeader: true);
            var predictionEngine = mlContext.Model.CreatePredictionEngine<FloodDataModel, FloodPredictionResult>(trainedModel);
            var predictions = mlContext.Data.CreateEnumerable<FloodDataModel>(input, reuseRowObject: false)
                                    .Select(inp => predictionEngine.Predict(inp));

                var monthCount = 1;
                foreach(var item in predictions)
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
                    await _context.CityPredictions.AddAsync(ps);
                    await _context.SaveChangesAsync();
                    var userId = await GetCurrentUserIdAsync(User);
                    var user = _context.Users.Find(userId);
                    var predictionsForUser = _context.CityPredictions.Where(x => x.CityId == user.CityId).ToList();

                }
                var predictionforMail = _context.CityPredictions.ToList();
                foreach(var p in predictionforMail)
                {
                    if((_context.Users.FirstOrDefault(x => x.CityId ==  p.CityId) != null))
                    {
                        CheckStatusAndSendEmail(p);
                    }
                }
            return RedirectToAction("Index", "Admin");           

        }


        public async Task<IActionResult> Predictions()
        {
            var userId = await GetCurrentUserIdAsync(User);
            var user = _context.Users.Find(userId);
            var predictionsForUser = _context.CityPredictions.Where(x => x.CityId == user.CityId).ToList();
            return View(predictionsForUser);
        }
        private void CheckStatusAndSendEmail(CityFloodPrediction entity)
        {
            if (entity.Prediction == true.ToString())
            {
                var users = _context.Users.Where(x => x.CityId == entity.CityId).ToList();
                // send email to user
                MailMessage mail = new MailMessage();
                SmtpClient SmtpServer = new SmtpClient("smtp.gmail.com");

                mail.From = new MailAddress("##");
                foreach(var user in users)
                {
                    mail.To.Add(user.Email);
                    mail.Subject = "Flood Risk In Your City";
                    mail.Body = $"Dear {user.UserName}, Your city, {entity.City} is predicted to have a high flood risk in {entity.Month}.";
                }

                SmtpServer.Port = 587;
                SmtpServer.Credentials = new System.Net.NetworkCredential("#", "###");
                SmtpServer.EnableSsl = true;

                SmtpServer.Send(mail);
            }
        }

    }
}
