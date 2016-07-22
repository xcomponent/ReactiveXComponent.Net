using System.IO;

namespace XComponentClientApi
{
    internal class Settings
    {
        public string CSFileName { get; set; }
        public string XmlFileName { get; set; }

        public bool IsValid()
        {
            if (string.IsNullOrEmpty(CSFileName) || !File.Exists(CSFileName)) return false;
            return !string.IsNullOrEmpty(XmlFileName) && File.Exists(XmlFileName);
        }
    }
}
