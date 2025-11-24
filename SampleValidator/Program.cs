using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace SampleValidator
{
    public class Sample
    {
        public string? Title { get; set; }
        public string? Description { get; set; }
        public List<string>? Tags { get; set; }
        public SampleModel? Model { get; set; }
    }

    public class SampleModel
    {
        public string? Name { get; set; }
        public string? ScriptText { get; set; }
        public List<JsonObjectViewModel>? InputObjects { get; set; }
        public List<JsonObjectViewModel>? OutputObjects { get; set; }
    }

    public class JsonObjectViewModel
    {
        public string? Name { get; set; }
        public string? JsonString { get; set; }
    }

    public class ValidationResult
    {
        public string SampleName { get; set; } = "";
        public string FilePath { get; set; } = "";
        public bool Success { get; set; }
        public string ErrorMessage { get; set; } = "";
        public string ExpectedOutput { get; set; } = "";
        public string ActualOutput { get; set; } = "";
        public List<string> StructuralIssues { get; set; } = new List<string>();
        public double ValidationTimeMs { get; set; }
    }

    public class SampleValidator
    {
        public static async Task<List<ValidationResult>> ValidateAllSamples(string samplesDirectory)
        {
            var results = new List<ValidationResult>();
            var sampleFiles = Directory.GetFiles(samplesDirectory, "Sample-*.json", SearchOption.AllDirectories)
                                     .OrderBy(f => f)
                                     .ToArray();
            
            Console.WriteLine($"Found {sampleFiles.Length} sample files to validate...\n");

            int processedCount = 0;
            foreach (var filePath in sampleFiles)
            {
                processedCount++;
                var result = await ValidateSample(filePath);
                results.Add(result);
                
                // Progress indicator every 20 samples
                if (processedCount % 20 == 0 || processedCount == sampleFiles.Length)
                {
                    Console.WriteLine($"Progress: {processedCount}/{sampleFiles.Length} samples processed");
                }
                
                // Show result
                if (result.Success)
                {
                    Console.WriteLine($"? {result.SampleName} ({result.ValidationTimeMs:F1}ms)");
                }
                else
                {
                    Console.WriteLine($"? {result.SampleName}: {result.ErrorMessage}");
                    if (result.StructuralIssues.Any())
                    {
                        foreach (var issue in result.StructuralIssues.Take(3))
                        {
                            Console.WriteLine($"   - {issue}");
                        }
                    }
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

            var stopwatch = System.Diagnostics.Stopwatch.StartNew();

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

                result.SampleName = sample.Model.Name ?? Path.GetFileNameWithoutExtension(filePath);

                // Validate basic structure
                var structuralIssues = ValidateStructure(sample);
                result.StructuralIssues.AddRange(structuralIssues);

                if (structuralIssues.Any(i => i.Contains("Critical")))
                {
                    result.ErrorMessage = "Critical structural issues prevent validation";
                    return result;
                }

                // Validate script JSON syntax
                try
                {
                    if (!string.IsNullOrEmpty(sample.Model.ScriptText))
                    {
                        JsonConvert.DeserializeObject(sample.Model.ScriptText);
                    }
                }
                catch (Exception ex)
                {
                    result.ErrorMessage = $"Script JSON parse error: {ex.Message}";
                    return result;
                }

                // Validate input/output JSON syntax
                if (sample.Model.InputObjects != null)
                {
                    foreach (var inputObj in sample.Model.InputObjects)
                    {
                        try
                        {
                            if (!string.IsNullOrEmpty(inputObj.JsonString))
                            {
                                JToken.Parse(inputObj.JsonString);
                            }
                        }
                        catch (Exception ex)
                        {
                            result.ErrorMessage = $"Input JSON parse error ({inputObj.Name}): {ex.Message}";
                            return result;
                        }
                    }
                }

                if (sample.Model.OutputObjects != null)
                {
                    foreach (var outputObj in sample.Model.OutputObjects)
                    {
                        try
                        {
                            if (!string.IsNullOrEmpty(outputObj.JsonString))
                            {
                                JToken.Parse(outputObj.JsonString);
                            }
                        }
                        catch (Exception ex)
                        {
                            result.ErrorMessage = $"Output JSON parse error ({outputObj.Name}): {ex.Message}";
                            return result;
                        }
                    }
                }

                // For now, consider validation successful if structure and JSON syntax are valid
                // Later we can add actual script execution when we resolve the JLio API issues
                if (structuralIssues.Any())
                {
                    result.ErrorMessage = $"Structural issues found: {structuralIssues.Count} issues";
                }
                else
                {
                    result.Success = true;
                }

                stopwatch.Stop();
                result.ValidationTimeMs = stopwatch.Elapsed.TotalMilliseconds;

            }
            catch (Exception ex)
            {
                result.ErrorMessage = $"Validation error: {ex.Message}";
                stopwatch.Stop();
                result.ValidationTimeMs = stopwatch.Elapsed.TotalMilliseconds;
            }

            return result;
        }

        private static List<string> ValidateStructure(Sample sample)
        {
            var issues = new List<string>();

            // Check basic properties
            if (string.IsNullOrEmpty(sample.Title))
                issues.Add("Missing title");

            if (string.IsNullOrEmpty(sample.Description))
                issues.Add("Missing description");

            if (sample.Tags == null)
                issues.Add("Missing tags array");

            if (sample.Model == null)
            {
                issues.Add("Critical: Missing model");
                return issues;
            }

            if (string.IsNullOrEmpty(sample.Model.Name))
                issues.Add("Missing model name");

            if (string.IsNullOrEmpty(sample.Model.ScriptText))
                issues.Add("Critical: Missing script text");

            // Validate input/output structure
            if (sample.Model.InputObjects != null)
            {
                foreach (var inputObj in sample.Model.InputObjects)
                {
                    if (string.IsNullOrEmpty(inputObj.Name))
                        issues.Add("Input object missing name");

                    if (string.IsNullOrEmpty(inputObj.JsonString))
                        issues.Add($"Input object '{inputObj.Name}' missing JSON content");
                }
            }

            if (sample.Model.OutputObjects != null)
            {
                foreach (var outputObj in sample.Model.OutputObjects)
                {
                    if (string.IsNullOrEmpty(outputObj.Name))
                        issues.Add("Output object missing name");

                    if (string.IsNullOrEmpty(outputObj.JsonString))
                        issues.Add($"Output object '{outputObj.Name}' missing JSON content");
                }
            }

            // Validate path references (this checks our earlier fix)
            var pathIssues = ValidatePathReferences(sample);
            issues.AddRange(pathIssues);

            return issues;
        }

        private static List<string> ValidatePathReferences(Sample sample)
        {
            var issues = new List<string>();

            if (sample.Model?.ScriptText == null || sample.Model.InputObjects == null)
                return issues;

            try
            {
                var script = JsonConvert.DeserializeObject<JArray>(sample.Model.ScriptText);
                if (script == null) return issues;

                // Extract path references from script
                var pathRefs = new HashSet<string>();
                foreach (var cmd in script)
                {
                    if (cmd is JObject cmdObj)
                    {
                        foreach (var pathProp in new[] { "path", "targetPath", "leftPath", "rightPath" })
                        {
                            var path = cmdObj[pathProp]?.ToString();
                            if (!string.IsNullOrEmpty(path) && path != "$")
                            {
                                if (System.Text.RegularExpressions.Regex.IsMatch(path, @"^\$\.([^.\[\]]+)"))
                                {
                                    var match = System.Text.RegularExpressions.Regex.Match(path, @"^\$\.([^.\[\]]+)");
                                    if (match.Success)
                                    {
                                        pathRefs.Add(match.Groups[1].Value);
                                    }
                                }
                            }
                        }
                    }
                }

                // Get available object names
                var objectNames = sample.Model.InputObjects
                    .Where(o => !string.IsNullOrEmpty(o.Name))
                    .Select(o => o.Name!)
                    .ToHashSet();

                if (sample.Model.OutputObjects != null)
                {
                    foreach (var outputObj in sample.Model.OutputObjects.Where(o => !string.IsNullOrEmpty(o.Name)))
                    {
                        objectNames.Add(outputObj.Name!);
                    }
                }

                // Get properties from input JSON
                var availableProperties = new HashSet<string>();
                foreach (var inputObj in sample.Model.InputObjects)
                {
                    try
                    {
                        if (!string.IsNullOrEmpty(inputObj.JsonString))
                        {
                            var json = JToken.Parse(inputObj.JsonString);
                            if (json is JObject obj)
                            {
                                foreach (var prop in obj.Properties())
                                {
                                    availableProperties.Add(prop.Name);
                                }
                            }
                        }
                    }
                    catch
                    {
                        // JSON parsing will be caught elsewhere
                    }
                }

                // Check for path references that don't match the pattern $.{objectName}.{property}
                foreach (var pathRef in pathRefs)
                {
                    if (!objectNames.Contains(pathRef) && !availableProperties.Contains(pathRef))
                    {
                        issues.Add($"Path reference '{pathRef}' doesn't match available object names or properties");
                    }
                }
            }
            catch
            {
                // Script parsing issues will be caught elsewhere
            }

            return issues;
        }

        public static void PrintSummary(List<ValidationResult> results)
        {
            var totalSamples = results.Count;
            var successCount = results.Count(r => r.Success);
            var failureCount = totalSamples - successCount;
            var totalValidationTime = results.Sum(r => r.ValidationTimeMs);
            var avgValidationTime = totalValidationTime / results.Count;

            Console.WriteLine("\n" + new string('=', 70));
            Console.WriteLine("SAMPLE VALIDATION SUMMARY");
            Console.WriteLine(new string('=', 70));
            Console.WriteLine($"?? Total samples validated: {totalSamples}");
            Console.WriteLine($"? Successful: {successCount} ({(double)successCount/totalSamples*100:F1}%)");
            Console.WriteLine($"? Failed: {failureCount} ({(double)failureCount/totalSamples*100:F1}%)");
            Console.WriteLine($"??  Total validation time: {totalValidationTime:F1}ms");
            Console.WriteLine($"? Average validation time: {avgValidationTime:F1}ms");
            
            if (failureCount > 0)
            {
                Console.WriteLine($"\n{new string('-', 50)}");
                Console.WriteLine("? FAILED SAMPLES BREAKDOWN");
                Console.WriteLine(new string('-', 50));
                
                // Group failures by error type
                var failureGroups = results.Where(r => !r.Success)
                                          .GroupBy(r => GetErrorCategory(r.ErrorMessage))
                                          .OrderByDescending(g => g.Count());

                foreach (var group in failureGroups)
                {
                    Console.WriteLine($"\n?? {group.Key} ({group.Count()} samples):");
                    foreach (var failure in group.Take(5)) // Show first 5 in each category
                    {
                        Console.WriteLine($"   • {failure.SampleName}: {failure.ErrorMessage}");
                        if (failure.StructuralIssues.Any())
                        {
                            var topIssues = failure.StructuralIssues.Take(2);
                            foreach (var issue in topIssues)
                            {
                                Console.WriteLine($"     - {issue}");
                            }
                        }
                    }
                    if (group.Count() > 5)
                    {
                        Console.WriteLine($"     ... and {group.Count() - 5} more samples");
                    }
                }

                // Show summary of structural issues
                var allStructuralIssues = results.SelectMany(r => r.StructuralIssues).ToList();
                if (allStructuralIssues.Any())
                {
                    Console.WriteLine($"\n?? MOST COMMON STRUCTURAL ISSUES:");
                    var issueGroups = allStructuralIssues.GroupBy(i => i)
                                                       .OrderByDescending(g => g.Count())
                                                       .Take(5);
                    foreach (var issueGroup in issueGroups)
                    {
                        Console.WriteLine($"   • {issueGroup.Key}: {issueGroup.Count()} occurrences");
                    }
                }
            }
            
            Console.WriteLine($"\n{new string('=', 70)}");
            
            if (successCount == totalSamples)
            {
                Console.WriteLine("?? ALL SAMPLES PASSED VALIDATION!");
            }
            else if (successCount > totalSamples * 0.9)
            {
                Console.WriteLine("? Most samples are healthy - only minor issues to resolve");
            }
            else if (successCount > totalSamples * 0.7)
            {
                Console.WriteLine("??  Some samples need attention");
            }
            else
            {
                Console.WriteLine("?? Many samples require fixes");
            }
        }

        private static string GetErrorCategory(string errorMessage)
        {
            if (string.IsNullOrEmpty(errorMessage))
                return "Unknown Error";
                
            if (errorMessage.Contains("Script JSON parse"))
                return "Script JSON Errors";
            if (errorMessage.Contains("Input JSON parse"))
                return "Input JSON Errors"; 
            if (errorMessage.Contains("Output JSON parse"))
                return "Output JSON Errors";
            if (errorMessage.Contains("Structural issues"))
                return "Structural Issues";
            if (errorMessage.Contains("Critical structural"))
                return "Critical Structure Problems";
                
            return "Other Errors";
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
                    Console.WriteLine($"? Samples directory not found: {samplesDirectory}");
                    Console.WriteLine("Make sure you're running from the solution root directory.");
                    Environment.Exit(1);
                }

                Console.WriteLine("?? JLio Sample Structure Validator");
                Console.WriteLine($"?? Validating samples in: {samplesDirectory}");
                Console.WriteLine($"? Started at: {DateTime.Now:HH:mm:ss}");
                Console.WriteLine();
                
                var results = await SampleValidator.ValidateAllSamples(samplesDirectory);
                
                SampleValidator.PrintSummary(results);

                // Export detailed results for failed samples
                var failedSamples = results.Where(r => !r.Success).ToList();
                if (failedSamples.Any())
                {
                    var detailedResults = failedSamples.Select(r => new
                    {
                        r.SampleName,
                        r.FilePath,
                        r.ErrorMessage,
                        r.StructuralIssues
                    }).ToList();

                    var jsonResults = JsonConvert.SerializeObject(detailedResults, Formatting.Indented);
                    var reportPath = "sample_validation_report.json";
                    await File.WriteAllTextAsync(reportPath, jsonResults);
                    Console.WriteLine($"?? Detailed validation report exported to: {reportPath}");
                }

                Console.WriteLine($"? Validation complete at {DateTime.Now:HH:mm:ss}");
                
                // Exit with appropriate code
                Environment.Exit(failedSamples.Any() ? 1 : 0);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"? Fatal error: {ex.Message}");
                Console.WriteLine($"Stack trace: {ex.StackTrace}");
                Environment.Exit(1);
            }
        }
    }
}