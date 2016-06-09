using System;
using System.IO;

namespace ResxGenerator
{
    public class Program
    {
        private readonly string[] _resxFiles = new[]
        {
            @"..\..\src\Microsoft.AspNetCore.OData\CommonWebApiResources.resx",
            @"..\..\src\Microsoft.AspNetCore.OData\SRResources.resx"
        };

        public void Main()
        {
            foreach (var resxFile in _resxFiles)
            {
                var fileName = Path.GetFileName(resxFile);

                if (!File.Exists(resxFile))
                {
                    Console.Error.WriteLine("Resource file '{0}' not found. Skipping...", fileName);
                    continue;
                }

                var fileNameNoExt = Path.GetFileNameWithoutExtension(resxFile);
                Console.WriteLine("Generating '{0}.Designer.cs' for '{1}'...", fileNameNoExt, fileName);
                Generator.GenerateResx(resxFile);
            }
        }
    }
}
