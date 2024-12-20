﻿using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;
using System.IO;
using LiquidTestReports.Tests.Shared;
using System.Reflection;
using System.Linq;
using System;
using System.Collections.Generic;

namespace LiquidTestReports.Tests
{
    /// <summary>
    /// Runs integration tests using markdown implementation
    /// </summary>
    [Collection("Logger Integration Tests")]
    public class LoggerIntegrationTests
    {
        private const string coreLogger = "liquid.custom";
        private const string markdownLogger = "liquid.md";
        private static readonly string[] frameworks = { "net6.0", "net7.0", "net8.0" };
        private readonly ITestOutputHelper _testOutputHelper;
        public string NUnitTestReport { get; }
        public string MSTestTestReport { get; }
        public string XUnitTestReport { get; }

        public LoggerIntegrationTests(ITestOutputHelper testOutputHelper)
        {
            _testOutputHelper = testOutputHelper;
        }

        [Theory]
        [InlineData(null, markdownLogger, "xUnit_Md", "../../../../SampleProject/SampleProject.Tests.xUnit")]
        [InlineData(null, markdownLogger, "MSTest_Md", "../../../../SampleProject/SampleProject.Tests.MSTest")]
        [InlineData(null, markdownLogger, "NUnit_Md", "../../../../SampleProject/SampleProject.Tests.NUnit")]
        public async Task Run_Markdown_GeneratesTestReport(string template, string logger, string file, string project)
        {
            var testPath = Path.GetFullPath(project);
            var resultsPath = Path.Combine(testPath, "TestResults");
            var reportOutput = Path.Combine(resultsPath, file);
            var expectedResultsTable = File.ReadAllText("Resources/ExpectedMdTable.txt");
            TryClean(reportOutput);

            await RunTestSamples.RunTest(testOutputHelper: _testOutputHelper,
                testProjectPath: testPath,
                logger: logger,
                templateName: template,
                logFilePrefix: reportOutput);

            var files = Directory.GetFiles(resultsPath, $"*{file}*");
            Assert.Contains(files, f => frameworks.Any(fw => f.Contains(fw)));
            Assert.All(files, f => Assert.Contains(expectedResultsTable, File.ReadAllText(f)));
        }

        [Theory]
        [InlineData(@"Resources/TemplateExample.txt", coreLogger, "xUnit_Text", "../../../../SampleProject/SampleProject.Tests.xUnit")]
        [InlineData(@"Resources/TemplateExample.txt", coreLogger, "MSTest_Text", "../../../../SampleProject/SampleProject.Tests.MSTest")]
        [InlineData(@"Resources/TemplateExample.txt", coreLogger, "NUnit_Text", "../../../../SampleProject/SampleProject.Tests.NUnit")]
        public async Task Run_Core_GeneratesTestReport(string template, string logger, string file, string project)
        {
            var testPath = Path.GetFullPath(project);
            var resultsPath = Path.Combine(testPath, "TestResults");
            var reportOutput = Path.Combine(resultsPath, file);
            var expectedText = File.ReadAllText("Resources/ExpectedTextOutput.txt");
            TryClean(reportOutput);

            await RunTestSamples.RunTest(testOutputHelper: _testOutputHelper,
                testProjectPath: testPath,
                logger: logger,
                templateName: template,
                logFilePrefix: reportOutput);

            var files = Directory.GetFiles(resultsPath, $"*{file}*");

            Assert.Contains(files, f => frameworks.Any(fw => f.Contains(fw)));
            Assert.All(files, f => Assert.Equal(expectedText, File.ReadAllText(f)));
        }

        private static void TryClean(string path)
        {
            var id = new DirectoryInfo(path);
            if (id.Exists)
            {
                id.Delete(true);
            }
        }
    }
}
