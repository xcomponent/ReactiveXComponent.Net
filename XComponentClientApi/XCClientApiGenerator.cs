using System;
using System.IO;
using System.Text;

namespace XComponentClientApi
{
    public static class XCClientApiGenerator
    {
        public static void ExposeApi(string xcApiFileName, string csFileName)
        {
            var settings = new Settings();
            if (xcApiFileName.Length <= 1) return;
            settings.XmlFileName = GetFullPath(xcApiFileName);
            if (csFileName.Length <= 1) return;
            settings.CSFileName = GetFullPath(csFileName).Replace(@"\bin\Debug","");

            if (!settings.IsValid()) return;
            var myOrderProcessingApi = new XCClientApi(xcApiFileName);
            StringBuilder csFileBuilder = new StringBuilder();
            csFileBuilder.AppendLine(myOrderProcessingApi.TransformText());
            File.WriteAllText(settings.CSFileName, csFileBuilder.ToString());
        }

        private static string GetFullPath(string fileName)
        {
            if (Path.IsPathRooted(fileName)) return fileName;
            var absolutePath = Path.Combine(Directory.GetCurrentDirectory(), fileName);
            return Path.GetFullPath((new Uri(absolutePath)).LocalPath);
        }
    }
}
