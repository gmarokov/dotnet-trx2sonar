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

            string testProjectSignature = Path.Combine("Tests", "bin");
            int indexOfSignature = unitTest.TestMethod.CodeBase.IndexOf(testProjectSignature, StringComparison.Ordinal);
            string projectDirectory = unitTest.TestMethod.CodeBase.Substring(0, indexOfSignature + 6);

            string[] files = Directory.GetFiles(projectDirectory, $"{className}.cs", SearchOption.AllDirectories);

            if (files.Length == 0)
            {
                throw new FileNotFoundException($"Cannot find file with class {className}. Check that file has the same name as the class.");
            }

            string result = files[0];

            if (!useAbsolutePath)
            {
                result = result.Substring(solutionDirectory.Length + 1);
            }

            return result;
        }
    }
}
