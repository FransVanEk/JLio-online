using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using JLio;
using JLio.Extensions.ETL;
using JLio.Extensions.Math;
using JLio.Extensions.Text;
using JLio.Extensions.TimeDate;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace SampleValidator
{
    public class Sample
    {
        public string Title { get; set; }
        public string Description { get; set; }
        public List<string> Tags { get; set; }
        public SampleModel Model { get; set; }
    }

    public class SampleModel
    {
        public string Name { get; set; }
        public string ScriptText { get; set; }
        public List<JsonObjectViewModel> InputObjects { get; set; }
        public List<JsonObjectViewModel> OutputObjects { get; set; }
    }

    public class JsonObjectViewModel
    {
        public string Name { get; set; }
        public string JsonString { get; set; }
    }

    public class ValidationResult
    {
        public string SampleName { get; set; }
        public string FilePath { get; set; }
        public bool Success { get; set; }
        public string ErrorMessage { get; set; }
        public string ExpectedOutput { get; set; }
        public string ActualOutput { get; set; }
        public List<string> Issues { get; set; } = new List<string>();
    }

    public class SampleValidator
    {
        public static async Task<List<ValidationResult>> ValidateAllSamples(string samplesDirectory)
        {
            var results = new List<ValidationResult>();
            var sampleFiles = Directory.GetFiles(samplesDirectory, "Sample-*.json", SearchOption.AllDirectories);
            
            Console.WriteLine($"Found {sampleFiles.Length} sample files to validate...");

            foreach (var filePath in sampleFiles)
            {
                var result = await ValidateSample(filePath);
                results.Add(result);
                
                if (result.Success)
                {
                    Console.WriteLine($"? {result.SampleName}");
                }
                else
                {
                    Console.WriteLine($"? {result.SampleName}: {result.ErrorMessage}");
                }
            }

            return results;
        }

        public static async Task<ValidationResult> ValidateSample(string filePath)
        {
            var result = new ValidationResult
            {
                FilePath = filePath,
                Success = false
            };

            try
            {
                // Read and parse sample file
                var jsonContent = await File.ReadAllTextAsync(filePath);
                var sample = JsonConvert.DeserializeObject<Sample>(jsonContent);
                
                if (sample?.Model == null)
                {
                    result.ErrorMessage = "Invalid sample format - missing model";
                    return result;
                }

                result.SampleName = sample.Model.Name;

                // Parse script
                var parseOptions = ParseOptions.CreateDefault()
                    .RegisterMath()
                    .RegisterText() 
                    .RegisterETL()
                    .RegisterTimeDate();

                JLioScript script;
                try
                {
                    script = JLioConvert.Parse(sample.Model.ScriptText, parseOptions);
                }
                catch (Exception ex)
                {
                    result.ErrorMessage = $"Script parse error: {ex.Message}";
                    return result;
                }

                // Prepare input data
                var inputData = new Dictionary<string, JToken>();
                if (sample.Model.InputObjects != null)
                {
                    foreach (var inputObj in sample.Model.InputObjects)
                    {
                        try
                        {
                            var token = JToken.Parse(inputObj.JsonString);
                            inputData[inputObj.Name] = token;
                        }
                        catch (Exception ex)
                        {
                            result.ErrorMessage = $"Input object parse error ({inputObj.Name}): {ex.Message}";
                            return result;
                        }
                    }
                }

                // Convert to execution data format
                JToken executionData;
                if (inputData.Count == 0)
                {
                    executionData = new JObject();
                }
                else if (inputData.Count == 1)
                {
                    // Single input object - use its content directly
                    executionData = inputData.Values.First();
                }
                else
                {
                    // Multiple input objects - create container
                    var container = new JObject();
                    foreach (var kvp in inputData)
                    {
                        container[kvp.Key] = kvp.Value;
                    }
                    executionData = container;
                }

                // Execute script
                var context = ExecutionContext.CreateDefault();
                JToken actualResult;
                try
                {
                    var executionResult = script.Execute(executionData, context);
                    if (!executionResult.Success)
                    {
                        result.ErrorMessage = $"Script execution failed: {context.Logger.LogText}";
                        return result;
                    }
                    actualResult = executionResult.Data;
                }
                catch (Exception ex)
                {
                    result.ErrorMessage = $"Script execution error: {ex.Message}";
                    return result;
                }

                // Prepare expected output for comparison
                var expectedOutputData = new Dictionary<string, JToken>();
                if (sample.Model.OutputObjects != null)
                {
                    foreach (var outputObj in sample.Model.OutputObjects)
                    {
                        try
                        {
                            var token = JToken.Parse(outputObj.JsonString);
                            expectedOutputData[outputObj.Name] = token;
                        }
                        catch (Exception ex)
                        {
                            result.ErrorMessage = $"Expected output parse error ({outputObj.Name}): {ex.Message}";
                            return result;
                        }
                    }
                }

                // Convert expected output to comparable format
                JToken expectedResult;
                if (expectedOutputData.Count == 0)
                {
                    expectedResult = new JObject();
                }
                else if (expectedOutputData.Count == 1)
                {
                    expectedResult = expectedOutputData.Values.First();
                }
                else
                {
                    var container = new JObject();
                    foreach (var kvp in expectedOutputData)
                    {
                        container[kvp.Key] = kvp.Value;
                    }
                    expectedResult = container;
                }

                // Store formatted outputs for debugging
                result.ExpectedOutput = expectedResult.ToString(Formatting.Indented);
                result.ActualOutput = actualResult.ToString(Formatting.Indented);

                // Compare results
                if (JToken.DeepEquals(actualResult, expectedResult))
                {
                    result.Success = true;
                }
                else
                {
                    result.ErrorMessage = "Output mismatch - actual result differs from expected output";
                    
                    // Add specific differences
                    var differences = FindDifferences(expectedResult, actualResult);
                    result.Issues.AddRange(differences);
                }

            }
            catch (Exception ex)
            {
                result.ErrorMessage = $"Validation error: {ex.Message}";
            }

            return result;
        }

        private static List<string> FindDifferences(JToken expected, JToken actual)
        {
            var differences = new List<string>();
            
            if (expected.Type != actual.Type)
            {
                differences.Add($"Type mismatch: expected {expected.Type}, got {actual.Type}");
                return differences;
            }

            if (expected is JObject expectedObj && actual is JObject actualObj)
            {
                // Check for missing properties in actual
                foreach (var prop in expectedObj.Properties())
                {
                    if (actualObj[prop.Name] == null)
                    {
                        differences.Add($"Missing property: {prop.Name}");
                    }
                    else
                    {
                        var subDiffs = FindDifferences(prop.Value, actualObj[prop.Name]);
                        differences.AddRange(subDiffs.Select(d => $"{prop.Name}.{d}"));
                    }
                }

                // Check for extra properties in actual
                foreach (var prop in actualObj.Properties())
                {
                    if (expectedObj[prop.Name] == null)
                    {
                        differences.Add($"Extra property: {prop.Name}");
                    }
                }
            }
            else if (expected is JArray expectedArray && actual is JArray actualArray)
            {
                if (expectedArray.Count != actualArray.Count)
                {
                    differences.Add($"Array length mismatch: expected {expectedArray.Count}, got {actualArray.Count}");
                }
                
                for (int i = 0; i < Math.Min(expectedArray.Count, actualArray.Count); i++)
                {
                    var subDiffs = FindDifferences(expectedArray[i], actualArray[i]);
                    differences.AddRange(subDiffs.Select(d => $"[{i}].{d}"));
                }
            }
            else
            {
                if (!JToken.DeepEquals(expected, actual))
                {
                    differences.Add($"Value mismatch: expected '{expected}', got '{actual}'");
                }
            }

            return differences;
        }

        public static void PrintSummary(List<ValidationResult> results)
        {
            var totalSamples = results.Count;
            var successCount = results.Count(r => r.Success);
            var failureCount = totalSamples - successCount;

            Console.WriteLine();
            Console.WriteLine("=== VALIDATION SUMMARY ===");
            Console.WriteLine($"Total samples: {totalSamples}");
            Console.WriteLine($"Successful: {successCount}");
            Console.WriteLine($"Failed: {failureCount}");
            
            if (failureCount > 0)
            {
                Console.WriteLine();
                Console.WriteLine("=== FAILED SAMPLES ===");
                foreach (var failure in results.Where(r => !r.Success))
                {
                    Console.WriteLine($"? {failure.SampleName}");
                    Console.WriteLine($"   File: {failure.FilePath}");
                    Console.WriteLine($"   Error: {failure.ErrorMessage}");
                    
                    if (failure.Issues.Any())
                    {
                        Console.WriteLine("   Issues:");
                        foreach (var issue in failure.Issues)
                        {
                            Console.WriteLine($"     - {issue}");
                        }
                    }
                    Console.WriteLine();
                }
            }
        }
    }

    class Program
    {
        static async Task Main(string[] args)
        {
            try
            {
                var samplesDirectory = Path.Combine(Directory.GetCurrentDirectory(), "Client", "wwwroot", "samples");
                
                if (!Directory.Exists(samplesDirectory))
                {
                    Console.WriteLine($"Samples directory not found: {samplesDirectory}");
                    return;
                }

                Console.WriteLine($"Validating samples in: {samplesDirectory}");
                
                var results = await SampleValidator.ValidateAllSamples(samplesDirectory);
                
                SampleValidator.PrintSummary(results);

                // Export detailed results to JSON for further analysis
                var detailedResults = results.Where(r => !r.Success).ToList();
                if (detailedResults.Any())
                {
                    var jsonResults = JsonConvert.SerializeObject(detailedResults, Formatting.Indented);
                    await File.WriteAllTextAsync("failed_samples.json", jsonResults);
                    Console.WriteLine("Detailed failure information exported to: failed_samples.json");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
            }
        }
    }
}