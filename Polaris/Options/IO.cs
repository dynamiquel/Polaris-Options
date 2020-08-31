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
using System.Collections.Generic;
using Polaris.IO;

namespace Polaris.Options
{
    internal static class IO
    {
        internal static void Write(IDictionary<string, dynamic> options, IDictionary<string, string> fileLocations)
        {
            foreach (var volume in options)
            {
                if (volume.Value == null) 
                    return;
                
                Yaml.Write(fileLocations[volume.Key], volume.Value);
            }
        }

        internal static bool Read(string filePath, out IDictionary<string, dynamic> options)
        {
            // If file exists, attempt to deserialise it.
            if (File.Exists(filePath))
            {
                try
                {
                    var loadedOptions = Yaml.Read<Dictionary<string, dynamic>>(filePath);
                    
                    // Fix: If file is string.Empty, deserialisers assume it's null.
                    // We don't want null, we want an empty Dictionary<string, dynamic>.
                    if (loadedOptions != null)
                    {
                        options = loadedOptions;
                        return true;
                    }
                }
                catch (Exception e)
                {
                    UnityEngine.Debug.Log($"Could not read YAML at '{filePath}'. Reason:{e}.");
                }
            }
            
            // If file could not be deserialised or was null, create a new dictionary.
            options = null;
            return false;
        }
    }
}