using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;


namespace ProjectMS.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProjectMSController : ControllerBase
    {
        private readonly IProjectService _projectService;
        public ProjectMSController(IProjectService projectService)
        {
            _projectService = projectService;
        }


        [HttpPost("generate")]
        public IActionResult Generate([FromQuery] int count, [FromBody] List<SchemaField> schema)
        {
            try
            {
                var pro = _projectService.Generate(count, schema);
                return Ok(pro);
            }
            catch (Exception ex)
            {
                return BadRequest($"Error: {ex.Message}");
            }
        }
    }
    public interface IProjectService
    {
        JObject Generate(int count, List<SchemaField> schema);
    }

    public class ProjectService : IProjectService
    {
        private static readonly Random random = new Random();

        public JObject Generate(int count, List<SchemaField> schema)
        {
            var forecasts = new JArray();

            for (int i = 0; i < count; i++)
            {
                var forecast = ProjectData(schema);
                forecasts.Add(forecast);
            }

            var response = new JObject();
            response["Report_Entry"] = forecasts;

            return response;
        }



        private JObject ProjectData(List<SchemaField> schema)
        {
            var forecast = new JObject();

            foreach (var field in schema)
            {
                var fieldName = field.fieldName;
                var fieldType = field.fieldType;
                List<string> enumValues = null;

                if (fieldType.ToLower() == "enum")
                {
                    if (field.enumValues == null || !field.enumValues.Any())
                        throw new ArgumentException($"Enum values are required for field: {fieldName}");

                    enumValues = field.enumValues;
                }

                var value = GenerateValue(fieldType, enumValues, field.schema);
                forecast[fieldName] = value;
            }

            return forecast;
        }


        private JToken GenerateValue(string type, List<string> enumValues = null, List<SchemaField> schema = null)
        {
            switch (type.ToLower())
            {
                case "string":
                    return GenerateRandomString();
                case "integer":
                    return GenerateRandomInteger();
                case "guid":
                    return GenerateRandomGuid();
                case "enum":
                    if (enumValues == null || !enumValues.Any())
                        throw new ArgumentException("Enum values are required.");
                    return JToken.FromObject(GenerateRandomEnum(enumValues));
                case "object":
                    if (schema == null || !schema.Any())
                        throw new ArgumentException("Schema is required for object type.");
                    return ProjectData(schema);
                case "array":
                    if (schema == null || !schema.Any())
                        throw new ArgumentException("Schema is required for array type.");
                    return GenerateRandomArray(schema);
                case "duration":
                    return GenerateRandomDuration();
                case "datetime":
                    return DateTime.Now.ToString("yyyy-MM-ddTHH:mm:ss.fffK");
                case "date":
                    return DateTime.Now.ToString("yyyy-MM-dd");
                default:
                    throw new ArgumentException($"Unsupported field type: {type}");
            }

        }

        public string GenerateRandomGuid()
        {
            return Guid.NewGuid().ToString("N");
        }
        private string GenerateRandomDuration()
        {
            Random random = new Random();
            return random.Next(30, 300).ToString(); 
        }
        private string GenerateRandomString()
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
            return new string(Enumerable.Repeat(chars, 8)
              .Select(s => s[random.Next(s.Length)]).ToArray());
        }

        private int GenerateRandomInteger()
        {
            return random.Next(1, 100);
        }
        private string GenerateRandomEnum(List<string> enumValues)
        {
            return enumValues[random.Next(enumValues.Count)];
        }
        private JArray GenerateRandomArray(List<SchemaField> schema)
        {
            var array = new JArray();
            int arraySize = random.Next(1, 5);

            for (int i = 0; i < arraySize; i++)
            {
                array.Add(ProjectData(schema));
            }

            return array;
        }
    }

    public class SchemaField
    {
        public string fieldName { get; set; }
        public string fieldType { get; set; }
        public List<string>? enumValues { get; set; }
        public List<SchemaField>? schema { get; set; }
    }
}




