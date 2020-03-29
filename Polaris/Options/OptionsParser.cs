using System;
using System.Collections;
using System.Collections.Generic;
using Polaris.IO;

namespace Polaris.Options
{
    public static class OptionsParser
    {
        public static void ExportToYaml(IDictionary<string, dynamic> options, IDictionary<string, string> fileLocations)
        {
            foreach (var volume in options)
            {
                if (volume.Value == null) 
                    return;
                
                Yaml.Write(fileLocations[volume.Key], volume.Value);
            }
        }

        public static bool ReadFromYaml(string filePath, out IDictionary<string, dynamic> options)
        {
            options = new Dictionary<string, dynamic>();
            
            if (Yaml.TryRead(filePath, out options, true))
            {
                return true;
            }
            
            return false;
        }
    }
}