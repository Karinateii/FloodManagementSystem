using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using GlobalDisasterManagement.Controllers.API;

namespace GlobalDisasterManagement.Tests.Controllers
{
    public class PredictionsApiControllerTests
    {
        private readonly Mock<IUnitOfWork> _mockUnitOfWork;
        private readonly Mock<ILogger<PredictionsApiController>> _mockLogger;
        private readonly PredictionsApiController _controller;

        public PredictionsApiControllerTests()
        {
            _mockUnitOfWork = new Mock<IUnitOfWork>();
            _mockLogger = new Mock<ILogger<PredictionsApiController>>();
            _controller = new PredictionsApiController(_mockUnitOfWork.Object, _mockLogger.Object);
        }

        [Fact]
        public async Task GetAllPredictions_ShouldReturnOkWithPredictions()
        {
            // Arrange
            var predictions = new List<CityFloodPrediction>
            {
                new CityFloodPrediction 
                { 
                    Id = 1, 
                    CityId = 1, 
                    City = "Lagos Island", 
                    Year = "2024",
                    Month = "January",
                    Prediction = "True"
                },
                new CityFloodPrediction 
                { 
                    Id = 2, 
                    CityId = 2, 
                    City = "Ikeja", 
                    Year = "2024",
                    Month = "January",
                    Prediction = "False"
                }
            };

            _mockUnitOfWork.Setup(u => u.CityPredictions.GetAllAsync())
                .ReturnsAsync(predictions);

            // Act
            var result = await _controller.GetAllPredictions();

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var returnedPredictions = Assert.IsAssignableFrom<IEnumerable<CityFloodPrediction>>(okResult.Value);
            Assert.Equal(2, returnedPredictions.Count());
        }

        [Fact]
        public async Task GetPredictionById_WithValidId_ShouldReturnOk()
        {
            // Arrange
            var prediction = new CityFloodPrediction
            {
                Id = 1,
                CityId = 1,
                City = "Lagos Island",
                Year = "2024",
                Month = "January",
                Prediction = "True"
            };

            _mockUnitOfWork.Setup(u => u.CityPredictions.GetByIdAsync(1))
                .ReturnsAsync(prediction);

            // Act
            var result = await _controller.GetPredictionById(1);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var returnedPrediction = Assert.IsType<CityFloodPrediction>(okResult.Value);
            Assert.Equal("Lagos Island", returnedPrediction.City);
        }

        [Fact]
        public async Task GetPredictionById_WithInvalidId_ShouldReturnNotFound()
        {
            // Arrange
            _mockUnitOfWork.Setup(u => u.CityPredictions.GetByIdAsync(999))
                .ReturnsAsync((CityFloodPrediction?)null);

            // Act
            var result = await _controller.GetPredictionById(999);

            // Assert
            Assert.IsType<NotFoundObjectResult>(result.Result);
        }

        [Fact]
        public async Task GetPredictionsByCity_WithValidCityId_ShouldReturnPredictions()
        {
            // Arrange
            int cityId = 1;
            var predictions = new List<CityFloodPrediction>
            {
                new CityFloodPrediction { Id = 1, CityId = 1, City = "Lagos Island", Prediction = "True" },
                new CityFloodPrediction { Id = 2, CityId = 1, City = "Lagos Island", Prediction = "False" }
            };

            _mockUnitOfWork.Setup(u => u.CityPredictions.FindAsync(It.IsAny<System.Linq.Expressions.Expression<Func<CityFloodPrediction, bool>>>()))
                .ReturnsAsync(predictions);

            // Act
            var result = await _controller.GetPredictionsByCity(cityId);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var returnedPredictions = Assert.IsAssignableFrom<IEnumerable<CityFloodPrediction>>(okResult.Value);
            Assert.Equal(2, returnedPredictions.Count());
        }

        [Fact]
        public async Task GetHighRiskPredictions_ShouldReturnOnlyHighRiskPredictions()
        {
            // Arrange
            var predictions = new List<CityFloodPrediction>
            {
                new CityFloodPrediction { Id = 1, Prediction = "True", City = "City1" },
                new CityFloodPrediction { Id = 2, Prediction = "False", City = "City2" },
                new CityFloodPrediction { Id = 3, Prediction = "True", City = "City3" }
            };

            _mockUnitOfWork.Setup(u => u.CityPredictions.FindAsync(It.IsAny<System.Linq.Expressions.Expression<Func<CityFloodPrediction, bool>>>()))
                .ReturnsAsync((System.Linq.Expressions.Expression<Func<CityFloodPrediction, bool>> predicate) =>
                    predictions.Where(predicate.Compile()).ToList());

            // Act
            var result = await _controller.GetHighRiskPredictions();

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var highRiskPredictions = Assert.IsAssignableFrom<IEnumerable<CityFloodPrediction>>(okResult.Value);
            Assert.Equal(2, highRiskPredictions.Count());
            Assert.All(highRiskPredictions, p => Assert.Equal("True", p.Prediction));
        }

        [Theory]
        [InlineData(2024)]
        [InlineData(2023)]
        public async Task GetPredictionsByYear_WithValidYear_ShouldReturnPredictions(int year)
        {
            // Arrange
            var predictions = new List<CityFloodPrediction>
            {
                new CityFloodPrediction { Id = 1, Year = year.ToString(), City = "City1" },
                new CityFloodPrediction { Id = 2, Year = year.ToString(), City = "City2" }
            };

            _mockUnitOfWork.Setup(u => u.CityPredictions.FindAsync(It.IsAny<System.Linq.Expressions.Expression<Func<CityFloodPrediction, bool>>>()))
                .ReturnsAsync(predictions);

            // Act
            var result = await _controller.GetPredictionsByYear(year.ToString());

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var returnedPredictions = Assert.IsAssignableFrom<IEnumerable<CityFloodPrediction>>(okResult.Value);
            Assert.Equal(2, returnedPredictions.Count());
        }
    }
}
