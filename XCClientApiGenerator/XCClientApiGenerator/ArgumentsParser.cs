using System;
using System.IO;

namespace XCClientApiGenerator
{

    static class ArgumentsParser
    {

        private const string CSFile = "csfile=";
        private const string XMLFile = "xmlfile=";

        static public Settings Parse(string[] args)
        {
            Settings settings = new Settings();

            foreach (string arg in args)
            {
                if (arg.ToLower().StartsWith(CSFile))
                {
                    string[] csFile = arg.Split('=');
                    if (csFile.Length > 1)
                    {
                        settings.SourceFileName = GetFullPath(csFile[1]);
                        Console.WriteLine("cs file=" + settings.SourceFileName);
                    }
                }
                else if (arg.ToLower().StartsWith(XMLFile))
                {
                    string[] xmlFile = arg.Split('=');
                    if (xmlFile.Length > 1)
                    {
                        settings.XmlFileName = GetFullPath(xmlFile[1]);
                        Console.WriteLine("xml file=" + settings.XmlFileName);

                    }
                }
            }

            return settings;

        }

        static string GetFullPath(string fileName)
        {
            if (!Path.IsPathRooted(fileName))
            {
                var absolutePath = Path.Combine(Directory.GetCurrentDirectory(), fileName);
                return Path.GetFullPath((new Uri(absolutePath)).LocalPath);
            }

            return fileName;
        }
    }
}
