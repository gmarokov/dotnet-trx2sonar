using System;
using System.IO;
using System.Linq;
using TrxToSonar.Model.Sonar;
using TrxToSonar.Model.Trx;
using File = TrxToSonar.Model.Sonar.File;

namespace TrxToSonar
{
    public static class Extensions
    {
        private static readonly string[] SearchPatternFormats =
        {
            "{0}.cs",
            "{0}Test.cs",
            "{0}Tests.cs",
            "{0}.vb",
            "{0}Test.vb",
            "{0}Tests.vb",
            "{0}*"
        };

        private static readonly string TestProjectSignature = Path.Combine("Tests", "bin");

        public static UnitTest GetUnitTest(this UnitTestResult unitTestResult, TrxDocument trxDocument)
        {
            return trxDocument.TestDefinitions.FirstOrDefault(x => x.Id == unitTestResult.TestId);
        }

        public static File GetFile(this SonarDocument sonarDocument, string testFile)
        {
            return sonarDocument.Files.FirstOrDefault(x => x.Path == testFile);
        }

        public static string GetTestFile(this UnitTest unitTest, string solutionDirectory, bool useAbsolutePath)
        {
            string fullClassName = unitTest?.TestMethod?.ClassName;

            if (string.IsNullOrEmpty(fullClassName))
            {
                throw new NullReferenceException("Class name was not provided");
            }

            string className = fullClassName.Split(".", StringSplitOptions.RemoveEmptyEntries)[^1];

            int indexOfSignature = unitTest.TestMethod.CodeBase.IndexOf(TestProjectSignature, StringComparison.OrdinalIgnoreCase);
            string projectDirectory = unitTest.TestMethod.CodeBase.Substring(0, indexOfSignature + 6);

            string file =
                SearchPatternFormats.Select(format => string.Format(format, className))
                    .Select(searchPattern => Directory.GetFiles(projectDirectory, searchPattern, SearchOption.AllDirectories))
                    .Where(files => files.Length > 0)
                    .Select(files => files[0])
                    .FirstOrDefault();

            if (string.IsNullOrEmpty(file))
            {
                throw new FileNotFoundException($"Cannot find file with class {className}. Check that file has the same name as the class.");
            }

            if (!useAbsolutePath)
            {
                file = file.Substring(solutionDirectory.Length + 1);
            }

            return file;
        }
    }
}
