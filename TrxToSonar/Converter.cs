using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.Extensions.Logging;
using TrxToSonar.Model.Sonar;
using TrxToSonar.Model.Trx;
using File = TrxToSonar.Model.Sonar.File;

namespace TrxToSonar
{
    public class Converter : IConverter
    {
        private readonly ILogger _logger;
        private readonly XmlParser<SonarDocument> _sonarParser = new XmlParser<SonarDocument>();
        private readonly XmlParser<TrxDocument> _trxParser = new XmlParser<TrxDocument>();

        public Converter(ILogger<Converter> logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public bool Save(SonarDocument sonarDocument, string outputFilename)
        {
            return _sonarParser.Save(sonarDocument, outputFilename);
        }

        public SonarDocument Parse(string solutionDirectory, bool useAbsolutePath, bool usePDBFile)
        {
            if (string.IsNullOrEmpty(solutionDirectory) || !Directory.Exists(solutionDirectory))
            {
                return null;
            }

            IEnumerable<string> trxFiles = Directory.EnumerateFiles(solutionDirectory, "*.trx", SearchOption.AllDirectories);

            var sonarDocuments = new List<SonarDocument>();
            foreach (string trxFile in trxFiles)
            {
                _logger.LogInformation($"Parsing: {trxFile}");
                TrxDocument trxDocument = _trxParser.Deserialize(trxFile);
                SonarDocument sonarDocument = Convert(trxDocument, solutionDirectory, useAbsolutePath, usePDBFile);

                if (sonarDocument != null)
                {
                    sonarDocuments.Add(sonarDocument);
                }
            }

            return Merge(sonarDocuments);
        }

        private SonarDocument Merge(IReadOnlyList<SonarDocument> sonarDocuments)
        {
            _logger.LogInformation("Merge {0} file(s).", sonarDocuments.Count);
            if (sonarDocuments.Count == 1)
            {
                return sonarDocuments[0];
            }

            var result = new SonarDocument();
            foreach (SonarDocument sonarDocument in sonarDocuments)
            {
                foreach (File sonarFile in sonarDocument.Files)
                {
                    result.Files.Add(sonarFile);
                }
            }

            return result;
        }

        private SonarDocument Convert(TrxDocument trxDocument, string solutionDirectory, bool useAbsolutePath, bool usePDBFile)
        {
            var sonarDocument = new SonarDocument();

            foreach (UnitTestResult trxResult in trxDocument.Results)
            {
                UnitTest unitTest = trxResult.GetUnitTest(trxDocument);

                string testFile = unitTest.GetTestFile(solutionDirectory, useAbsolutePath, usePDBFile);

                File file = sonarDocument.GetFile(testFile);

                if (file == null)
                {
                    file = new File(testFile);
                    sonarDocument.Files.Add(file);
                }

                var testCase = new TestCase(trxResult.TestName, Utils.ToSonarDuration(trxResult.Duration));

                if (trxResult.Outcome != Outcome.Passed)
                {
                    if (trxResult.Outcome == Outcome.NotExecuted)
                    {
                        testCase.Skipped = new Skipped();
                        _logger.LogInformation($"Skipped: {trxResult.TestName}");
                    }
                    else
                    {
                        testCase.Failure = new Failure(
                            trxResult.Output?.ErrorInfo?.Message,
                            trxResult.Output?.ErrorInfo?.StackTrace);
                        _logger.LogInformation($"Failure: {trxResult.TestName}");
                    }
                }
                else
                {
                    _logger.LogInformation($"Passed: {trxResult.TestName}");
                }

                file.TestCases.Add(testCase);
            }

            return sonarDocument;
        }
    }
}
