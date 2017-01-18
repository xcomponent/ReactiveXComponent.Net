using System.IO;

namespace XCClientApiGenerator
{
    class Settings
    {
        public string SourceFileName { get; set; }
        public string XmlFileName { get; set; }

        public bool IsValid()
        {
            if (!string.IsNullOrEmpty(SourceFileName) && File.Exists(SourceFileName))
            {
                if (!string.IsNullOrEmpty(XmlFileName) && File.Exists(XmlFileName))
                {
                    return true;
                }
            }

            return false;
        }
    }
}
