using Moq;
using ProjectMS.Controllers;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using Xunit;

namespace ProjectMS.Tests
{
    public class ProjectMSControllerTests
    {
        [Fact]
        public void Generate_ReturnsOkResult_WithExpectedResponse()
        {
            // Arrange
            var mockService = new Mock<IProjectService>();
            mockService.Setup(service => service.Generate(It.IsAny<int>(), It.IsAny<List<SchemaField>>()))
                       .Returns(new JObject { ["Report_Entry"] = new JArray() });

            var controller = new ProjectMSController(mockService.Object);

            var schema = new List<SchemaField>
            {
                new SchemaField { fieldName = "name", fieldType = "string" },
                new SchemaField { fieldName = "age", fieldType = "integer" }
            };

            // Act
            var result = controller.Generate(5, schema);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var returnValue = Assert.IsType<JObject>(okResult.Value);
            Assert.NotNull(returnValue["Report_Entry"]);
        }

        [Fact]
        public void Generate_ReturnsBadRequest_OnException()
        {
            // Arrange
            var mockService = new Mock<IProjectService>();
            mockService.Setup(service => service.Generate(It.IsAny<int>(), It.IsAny<List<SchemaField>>()))
                       .Throws(new System.Exception("Test exception"));

            var controller = new ProjectMSController(mockService.Object);

            var schema = new List<SchemaField>
            {
                new SchemaField { fieldName = "name", fieldType = "string" },
                new SchemaField { fieldName = "age", fieldType = "integer" }
            };

            // Act
            var result = controller.Generate(5, schema);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal("Error: Test exception", badRequestResult.Value);
        }
    }
}
