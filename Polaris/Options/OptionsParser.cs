//  This file is part of Polaris-Options - A user options system for games.
//  https://github.com/dynamiquel/Polaris-Options
//  Copyright (c) 2020 dynamiquel and contributors

//  MIT License
//  Permission is hereby granted, free of charge, to any person obtaining a copy
//  of this software and associated documentation files (the "Software"), to deal
//  in the Software without restriction, including without limitation the rights
//  to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
//  copies of the Software, and to permit persons to whom the Software is
//  furnished to do so, subject to the following conditions:

//  The above copyright notice and this permission notice shall be included in all
//  copies or substantial portions of the Software.

//  THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
//  IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
//  FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
//  AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
//  LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
//  OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
//  SOFTWARE.

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
                
                Yaml.WriteAsync(fileLocations[volume.Key], volume.Value);
            }
        }

        public static bool ReadFromYaml(string filePath, out IDictionary<string, dynamic> options)
        {
            options = new Dictionary<string, dynamic>();
            
            try
            {
                options = Task.Run(async() => await Yaml.ReadAsync<Dictionary<string, dynamic>>(filePath)).Result;
                return true;
            }
            catch (Exception e)
            {
                UnityEngine.Debug.Log($"Could not read YAML at '{filePath}'. Reason:{e}.");
                return false;
            }
        }
    }
}