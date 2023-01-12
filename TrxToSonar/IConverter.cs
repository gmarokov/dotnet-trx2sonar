using TrxToSonar.Model.Sonar;

namespace TrxToSonar
{
    public interface IConverter
    {
        SonarDocument Parse(string solutionDirectory, bool useAbsolutePath, bool usePDBFile);

        bool Save(SonarDocument sonarDocument, string outputFilename);
    }
}
