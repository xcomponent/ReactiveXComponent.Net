using System.IO;
using System.Text;



namespace XCClientApiGenerator
{
    class ApiGenerator
    {
        static void Main(string[] args)
        {
            var settings = ArgumentsParser.Parse(args);
            if (settings.IsValid())
            {
                string[] arg = args[1].Split('=');
                var myOrderProcessingApi = new XCClientSender.XCClientSender(arg[1]);
                StringBuilder csFileBuilder = new StringBuilder();
                csFileBuilder.AppendLine(myOrderProcessingApi.TransformText());
                File.WriteAllText(settings.SourceFileName, csFileBuilder.ToString());
            }
        }   
    }
}
