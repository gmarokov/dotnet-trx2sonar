using System;
using System.IO;
using System.Linq;
using TrxToSonar.Model.Sonar;
using TrxToSonar.Model.Trx;
using File = TrxToSonar.Model.Sonar.File;

using System.Reflection.Metadata;
using System.Reflection.PortableExecutable;
using System.Text;
using System.Reflection.Metadata.Ecma335;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace TrxToSonar
{
    public static class Extensions
    {
        private static readonly string _pathSep = Path.DirectorySeparatorChar.ToString();

        public static UnitTest GetUnitTest(this UnitTestResult unitTestResult, TrxDocument trxDocument)
        {
            return trxDocument.TestDefinitions.FirstOrDefault(x => x.Id == unitTestResult.TestId);
        }

        public static File GetFile(this SonarDocument sonarDocument, string testFile)
        {
            return sonarDocument.Files.FirstOrDefault(x => x.Path == testFile);
        }
        public static string GetFilePathFromPDB(this UnitTest unitTest)
        {
            String path = Path.ChangeExtension(unitTest.Storage, "pdb");
            using FileStream fs = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);

            List<String> files = ReadPdbDocuments(path);

            foreach(String file in files.Distinct())
            {
                if (System.IO.File.Exists(file))
                {
                    // In case test used [Random(0, 3000, 50)]
                    // Method name is like RandomManyLoad(1732)
                    // remove (1732) from name
                    String methodName = unitTest.TestMethod.Name;
                    if (unitTest.TestMethod.Name.Contains("("))
                        methodName = unitTest.TestMethod.Name.Substring(0, unitTest.TestMethod.Name.IndexOf('('));

                    String classname = unitTest.TestMethod.ClassName.Substring(unitTest.TestMethod.ClassName.LastIndexOf('.') + 1);
                    String namespaceName = unitTest.TestMethod.ClassName.Substring(0, unitTest.TestMethod.ClassName.LastIndexOf('.'));
                    namespaceName.Replace(".", "\\.");
                    String code = System.IO.File.ReadAllText(file);
                    Regex namespaceRegex = new Regex("namespace[\\s\\t]+"+ namespaceName);
                    Regex classRegex = new Regex("public[\\s\\t]+class[\\s\\t]+"+ classname);
                    Regex methodRegex = new Regex(methodName + "\\(.*\\)[\\s\\n\\r\\t]*\\{");
                    if (namespaceRegex.IsMatch(code) && classRegex.IsMatch(code) && methodRegex.IsMatch(code))
                    {
                        return file;
                    }
                }
            }
            return "";
        }

        static string ReadDocumentPath(MetadataReader reader, Document doc)
        {
            BlobReader blob = reader.GetBlobReader(doc.Name);

            // Read path separator character
            char separator = (char) blob.ReadByte();
            StringBuilder sb = new StringBuilder(blob.Length * 2);

            // Read path segments
            while (true)
            {
                BlobHandle bh = blob.ReadBlobHandle();

                if (!bh.IsNil)
                {
                    byte[] nameBytes = reader.GetBlobBytes(bh);
                    sb.Append(Encoding.UTF8.GetString(nameBytes));
                }

                if (blob.Offset >= blob.Length)
                {
                    break;
                }

                sb.Append(separator);
            }

            return sb.ToString();
        }

        public static List<String> ReadPdbDocuments(string pdbPath)
        {
            List<String> files = new List<String>();
            // Open Portable PDB file
            using FileStream fs = new FileStream(pdbPath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
            using MetadataReaderProvider provider = MetadataReaderProvider.FromPortablePdbStream(fs);
            MetadataReader reader = provider.GetMetadataReader();
            // Display information about documents in each MethodDebugInformation table entry
            foreach (MethodDebugInformationHandle h in reader.MethodDebugInformation)
            {
                MethodDebugInformation mdi = reader.GetMethodDebugInformation(h);

                if (mdi.Document.IsNil)
                {
                    continue;
                }

                int token = MetadataTokens.GetToken(h);

                Document doc = reader.GetDocument(mdi.Document);
                files.Add(ReadDocumentPath(reader, doc));
            }
            return files;
        }

        public static string GetTestFile(this UnitTest unitTest, string solutionDirectory, bool useAbsolutePath, bool usePDBFile)
        {
            if (usePDBFile && System.IO.File.Exists(Path.ChangeExtension(unitTest.Storage, "pdb")))
            {
                return unitTest.GetFilePathFromPDB();

            }
            var className = unitTest?.TestMethod?.ClassName;

            if (string.IsNullOrEmpty(className))
            {
                return string.Empty;
            }

            var pathIndex = className.LastIndexOf(".", StringComparison.Ordinal);
            var pathIndexLast = unitTest.TestMethod.CodeBase.IndexOf($"{_pathSep}bin{_pathSep}", StringComparison.Ordinal);
            var path = unitTest.TestMethod.CodeBase.Substring(0, pathIndexLast);
            var pathIndexFirst = path.LastIndexOf(_pathSep, StringComparison.Ordinal);
            path = path.Substring(pathIndexFirst + 1);

            var filename = className.Substring(pathIndex + 1, className.Length - pathIndex - 1);

            var folders = className.Replace(path, "").Replace(filename, "").Split('.', StringSplitOptions.RemoveEmptyEntries);
            if (folders.Length > 0)
            {
                path += _pathSep;
                foreach (var folder in folders)
                {
                    path += folder + _pathSep;
                }
            }

            string result;
            if (!useAbsolutePath)
            {
                result = string.Format("{0}.cs", Path.Combine(path, filename));
            }
            else
            {
                result = string.Format("{0}.cs", Path.Combine(solutionDirectory, path, filename));
            }

            return result.Replace(" ", "\\\\ ");
        }
    }
}
