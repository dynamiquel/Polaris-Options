﻿//  This file is part of Polaris-Options - A user options system for games.
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
using System.IO;
using System.Linq;
using System.Text;
using Newtonsoft.Json.Linq;

namespace Polaris.Options
{
    public static class OptionsManager
    {
        private static string _defaultFileLocation =
            Path.Combine(UnityEngine.Application.persistentDataPath, "Options.yml");
        public static string DefaultFileLocation
        {
            get => _defaultFileLocation;
            // Ensures only a valid path can be set as the default file location.
            set
            {
                if (Path.IsPathRooted(value))
                    _defaultFileLocation = value;
            }
        }
        public static bool EnableDefaultFile { get; set; } = true;
        public static bool AutoLoadDefaultFile { get; set; } = true;

        /// <summary>
        /// Invoked when an Option has been set or deleted.
        /// </summary>
        public static event Action OnOptionsChanged;
        /// <summary>
        /// Invoked when a Volume has been loaded or refreshed.<br></br><br></br>
        /// string: the name of the Volume.<br></br>
        /// bool: false is the Volume did not exist or was invalid (aka. it's empty).
        /// </summary>
        public static event Action<string, bool> OnVolumeLoaded;

        private static readonly IDictionary<string, string> FileLocations = new Dictionary<string, string>();
        private static readonly IDictionary<string, dynamic> Volumes = new Dictionary<string, dynamic>();

        static OptionsManager()
        {
            if (AutoLoadDefaultFile && EnableDefaultFile)
                Load(DefaultFileLocation);
        }

        #region Get Value
        
        /// <summary>
        /// Returns the object found as a dynamic object. If no object is found, 0 is returned.
        /// </summary>
        /// <param name="key">The key of the option to get.</param>
        /// <returns></returns>
        public static dynamic GetValue(string key)
        {
            var success = GetObjectFromKey(key, out dynamic result, out string finalKey);
            return success == OptionFound.Yes ? result[finalKey] : 0;
        }

        /// <summary>
        /// Returns the object found as a dynamic object. If no object is found, the defaultValue is set and returned.
        /// </summary>
        /// <param name="key">The key of the option to get.</param>
        /// <param name="defaultValue">The value to set and use if the option couldn't be found.</param>
        /// <returns></returns>
        public static dynamic GetValue(string key, dynamic defaultValue)
        {
            var success = GetObjectFromKey(key, out dynamic result, out string finalKey);

            if (success == OptionFound.Yes)
                return result[finalKey];

            SetValue(key, defaultValue);
            return GetValue(key);
        }

        /// <summary>
        /// Attempts to return the object found as a dynamic object. If no object is found, false is returned.
        /// </summary>
        /// <param name="key">The key of the option to get.</param>
        /// <param name="result">The object retrieved.</param>
        /// <returns></returns>
        public static bool TryGetValue(string key, out dynamic result)
        {
            result = null;

            var success = GetObjectFromKey(key, out var result2, out string finalKey);

            if (success == OptionFound.Yes)
            {
                result = result2[finalKey];
                return true;
            }

            return false;
        }

        /// <summary>
        /// Returns the object found as a T object. If no object is found, the default value for T is returned.
        /// </summary>
        /// <param name="key">The key of the option to get.</param>
        /// <returns></returns>
        public static T GetValue<T>(string key)
        {
            var success = GetObjectFromKey(key, out dynamic result, out string finalKey);

            if (success == OptionFound.Yes)
            {
                try
                {
                    dynamic option;
                    if (typeof(T) == typeof(string))
                        option = result[finalKey].ToString();
                    else
                        option = (T)result[finalKey];
                    
                    return option;
                }
                catch (Exception e)
                {
                    UnityEngine.Debug.Log(
                        $"Polaris Options: The value '{key}' could not be retrieved as it is the wrong type.\nMore details: {e}");
                }
            }

            return default;
        }

        /// <summary>
        /// Returns the object found as a T object. If no object is found, the defaultValue is set and returned.
        /// </summary>
        /// <param name="key">The key of the option to get.</param>
        /// <param name="defaultValue">The value to set and use if the option couldn't be found.</param>
        /// <returns></returns>
        public static T GetValue<T>(string key, T defaultValue)
        {
            var success = GetObjectFromKey(key, out dynamic result, out string finalKey);

            if (success == OptionFound.Yes)
            {
                try
                {
                    dynamic option;
                    if (typeof(T) == typeof(string))
                        option = result[finalKey].ToString();
                    else
                        option = (T)result[finalKey];
                    
                    return option;
                }
                catch (Exception e)
                {
                    UnityEngine.Debug.Log(
                        $"Polaris Options: The value '{key}' could not be retrieved as it is the wrong type.\nMore details: {e}");
                }
            }
            else
            {
                if (SetValue(key, defaultValue) == SetValueOutput.SetCreated)
                    return GetValue(key);
            }

            return default;
        }

        /// <summary>
        /// Attempts to return the object found as a T object. If no object is found, false is returned.
        /// </summary>
        /// <param name="key">The key of the option to get.</param>
        /// <returns></returns>
        public static bool TryGetValue<T>(string key, out T result)
        {
            result = default;

            var success = GetObjectFromKey(key, out var result2, out string finalKey);

            if (success == OptionFound.Yes)
            {
                try
                {
                    result = (T)result2[finalKey];
                    return true;
                }
                catch
                {
                    return false;
                }
            }

            return false;
        }

        // I have no idea why this is here...
        public static bool TryGetValue<T>(string key, out T result, T defaultValue)
        {
            result = default;

            var success = GetObjectFromKey(key, out var result2, out string finalKey);

            if (success == OptionFound.Yes)
            {
                try
                {
                    result = (T)result2[finalKey];
                    return true;
                }
                catch
                {
                    return false;
                }
            }
            
            if (SetValue(key, defaultValue) == SetValueOutput.SetCreated)
                    return TryGetValue(key, out result);

            return default;
        }
        
        #endregion
        
        /// <summary>
        /// Sets the value in the given key to the given value.
        /// <example>
        /// <code>SetValue("Master Volume", 0.8f);</code>
        /// </example>
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key">The key of the option to set.</param>
        /// <param name="value">The object to set the value to.</param>
        /// <returns>
        /// An integer of either:
        /// <list type="bullet">
        /// <item><description>0 = Unable to set value</description></item>
        /// <item><description>1 = Value set; key created</description></item>
        /// <item><description>2 = Value set; key replaced</description></item>
        /// </list>
        /// </returns>
        public static SetValueOutput SetValue<T>(string key, T value)
        {
            var success = GetObjectFromKey(key, out dynamic result, out string finalKey, true);
            
            if (success == OptionFound.OnlyDictionaryFound || success == OptionFound.Yes)
            {
                try
                {
                    result[finalKey] = value;
                    
                    OnOptionsChanged?.Invoke();
                    return success == OptionFound.Yes ? SetValueOutput.SetReplaced : SetValueOutput.SetCreated;
                }
                catch (Exception e)
                {
                    // Value at key was not set.
                    UnityEngine.Debug.LogError($"Polaris Options: Could not set value at '{key}'.\nMore details: {e}");
                    return SetValueOutput.Failed;
                }
            }
            
            // Value at key was not set.
            UnityEngine.Debug.LogError($"Polaris Options: Could not set value at '{key}'.");
            return SetValueOutput.Failed;
        }

        /// <summary>
        /// Deletes the option with the given key.
        /// <example>
        /// <code>Delete("Audio:Master Volume");</code>
        /// </example>
        /// </summary>
        /// <param name="key">The key of the option to delete.</param>
        /// <returns>
        /// True if the key was successfully deleted, otherwise false.
        /// </returns>
        public static bool Delete(string key) =>
            Delete(new[] { key }) == DeleteOutput.All;

        /// <summary>
        /// Deletes the options with the given keys.
        /// <example>
        /// <code>
        /// string[] keys = { "Audio:Master Volume", "Device Name" }
        /// Delete(keys);
        /// </code>
        /// </example>
        /// </summary>
        /// <param name="keys">The keys of the options to delete.</param>
        /// <returns>
        /// An integer of either:
        /// <list type="bullet">
        /// <item><description>0 = No keys were deleted</description></item>
        /// <item><description>1 = Some keys were deleted</description></item>
        /// <item><description>2 = All keys were deleted</description></item>
        /// </list>
        /// </returns>
        public static DeleteOutput Delete(IEnumerable<string> keys)
        {
            IDictionary<string, bool> deletes = new Dictionary<string, bool>();

            foreach (var key in keys)
            {
                var success = GetObjectFromKey(key, out dynamic result, out string finalKey);

                if (success <= 0) 
                    continue;
                
                try
                {
                    UnityEngine.Debug.Log($"Polaris Options: Deleting key '{finalKey}'.");

                    if (finalKey == "*")
                        result.Clear();
                    else
                        result.Remove(finalKey);

                    deletes[key] = true;
                }
                catch (Exception e)
                {
                    UnityEngine.Debug.LogError($"Polaris Options: Could not delete '{key}'.\nMore details: {e}");
                    deletes[key] = false;
                }
            }

            // If all items are true, return 1.
            // If all items are false, return 0.
            // If some items are false, return 2.
            if (deletes.Any(o => true))
            {
                OnOptionsChanged?.Invoke();
                return deletes.All(o => true) ? DeleteOutput.All : DeleteOutput.Some;
            }

            return DeleteOutput.None;
        }

        /// <summary>
        /// Deletes all the options (identical to: Delete("*");)
        /// <example>
        /// <code>DeleteAll();</code>
        /// </example>
        /// </summary>
        /// <returns>
        /// True if the operation was successful, otherwise false.
        /// </returns>
        public static bool DeleteAll(string[] exemptions = null) =>
            Delete("*");

        /// <summary>
        /// Saves all the options to the disk.
        /// <example>
        /// <code>Save();</code>
        /// </example>
        /// </summary>
        /// <returns>
        /// True if the options saved, otherwise false.
        /// </returns>
        public static bool Save()
        {
            IO.Write(Volumes, FileLocations);
            return true;
        }

        /// <summary>
        /// Reloads the options files that have previously been loaded.
        /// <example>
        /// <code>Refresh();</code>
        /// </example>
        /// </summary>
        public static void Refresh()
        {
            // Deletes all options and loads them all from the file, again.

            // Creates a copy of the FileLocations dictionary since the FileLocations dictionary will be altered
            // throughout the loop (exception error).
            var fileLocationsClone = new Dictionary<string, string>();

            foreach (var volume in FileLocations)
                fileLocationsClone[volume.Key] = volume.Value;

            // Reload every volume.
            foreach (var volume in fileLocationsClone)
            {
                try
                {
                    string fileLocation = FileLocations[volume.Key];
                    Load(fileLocation);
                }
                catch (Exception e)
                {
                    UnityEngine.Debug.LogError(string.Format("Polaris Options: Unable to refresh volume '{0}'.", volume.Key));
                }
            }
        }
        
        public static bool Load()
        {
            if (EnableDefaultFile)
                return Load(DefaultFileLocation);
            
            UnityEngine.Debug.LogError("Polaris Options: EnableDefaultFile must be set to true to use Load().");
            return false;
        }

        /// <summary>
        /// Loads the options file from the given path.
        /// <example>
        /// <code>
        /// Load("C:/Users/Me/Documents/GameOptions.yml", true);
        /// </code>
        /// </example>
        /// </summary>
        /// <param name="filePath">The path of the options file to load.</param>
        /// <param name="createIfNotPresent">When set to true, creates an empty file at the given path if one doesn't already exist.</param>
        public static bool Load(string filePath, bool createIfNotPresent = true)
        {
            string fileName = Path.GetFileNameWithoutExtension(filePath);

            // Reads the file and parses it into Options, then return true.
            // Return false if file couldn't be loaded.
            bool success = IO.Read(filePath, out var result);

            IDictionary<string, dynamic> volume = success ? result : new Dictionary<string, dynamic>();
                
            try
            {
                FileLocations[fileName] = filePath;
                Volumes[fileName] = volume;
                
                OnVolumeLoaded?.Invoke(fileName, success);
                UnityEngine.Debug.Log($"Polaris Options: Added '{fileName}'.");
                
                return true;
            }
            catch (Exception e)
            {
                UnityEngine.Debug.LogError(
                    $"Polaris Options: Could not add file '{filePath}' to dictionary.\nMore details: {e}");
                return false;
            }
        }

        /// <summary>
        /// Checks whether the options contains the given key.
        /// <example>
        /// <code>bool result = ContainsKey("Master Volume");</code>
        /// </example>
        /// </summary>
        /// <param name="key">The key of the option you want to check exists.</param>
        /// <returns>
        /// True if the options does contain the given key, otherwise false.
        /// </returns>
        public static bool ContainsKey(string key)
        {
            var success = GetObjectFromKey(key, out dynamic result, out string finalKey);

            return success == OptionFound.Yes;
        }

        public new static string ToString()
        {
            var sb = new StringBuilder();

            for (var i = 0; i < Volumes.Count; i++)
                sb.AppendLine(Volumes.ElementAt(i).Value.ToString());

            return sb.ToString();
        }

        /// <summary>
        /// Parses the given key to an array of strings.
        /// <example>
        /// <code>string[] keys = SplitString("Video:Resolution:X");</code>
        /// </example>
        /// </summary>
        /// <param name="path">The key path of the option.</param>
        /// <returns>
        /// The splitted key in a string array,
        /// </returns>
        private static string[] SplitPath(string path)
        {
            string directory = Path.GetFileNameWithoutExtension(DefaultFileLocation);

            var tempCmpPath = new string[] { directory, path };

            if (path.Contains('/'))
            {
                var tempDir = path.Split('/');
                
                if (tempDir.Length > 2)
                    UnityEngine.Debug.Log("Polaris Options: Invalid path, contains more than one volume.");
                else
                {
                    directory = tempDir[0];
                    tempCmpPath = tempDir;
                }

            }

            var keyPath = tempCmpPath[1].Split(':');
            var cmpPath = new List<string>();
            cmpPath.Add(directory);
            cmpPath.AddRange(keyPath);

            return cmpPath.ToArray();
        }

        private static OptionFound GetObjectFromKey(string key, out dynamic result, bool createNonExistentPaths = false)
        {
            return GetObjectFromKey(key, out result, out string finalKey, createNonExistentPaths);
        }

        /// <summary>
        /// Attempts to find the dictionary that contains the desired key path.
        /// <example>
        /// <code>
        /// string[] keys = { "Audio:Master Volume", "Device Name" }
        /// Delete(keys);</code>
        /// </example>
        /// </summary>
        /// <param name="key">The key path of the options to find.</param>
        /// <param name="result">The dictionary that contains the key.</param>
        /// <param name="createNonExistentPaths">If true, all required dictionaries for the option to exist will be created.</param>
        /// <returns>
        /// <list type="bullet">
        /// <item><description>0 = The option could not be found; the dictionary before it could.</description></item>
        /// <item><description>1 = The option could be found.</description></item>
        /// <item><description>-1 = Neither the option or the dictionary before it could be found.</description></item>
        /// </list>
        /// </returns>
        private static OptionFound GetObjectFromKey(string key, out dynamic result, out string finalKey, bool createNonExistentPaths = false)
        {
            result = null;
            finalKey = string.Empty;

            try
            {
                string[] keyPath = SplitPath(key);

                result = Volumes;
                finalKey = keyPath[keyPath.Length - 1];
                
                for (var i = 0; i < keyPath.Length - 1; i++)
                {
                    if (!result.ContainsKey(keyPath[i]))
                        if (createNonExistentPaths)
                            result[keyPath[i]] = new Dictionary<string, dynamic>();
                        else
                            return OptionFound.No;

                    result = result[keyPath[i]];
                }

                // Fix: If using Newtonsoft.Json, converts the JObject to Dictionary<string, dynamic>().
                if (result is JObject jObject)
                    result = jObject.ToObject<Dictionary<string, dynamic>>();
                
                if (result.ContainsKey(keyPath[keyPath.Length - 1]) || keyPath[keyPath.Length - 1] == "*")
                    return OptionFound.Yes;

                return OptionFound.OnlyDictionaryFound;
            }
            catch (Exception e)
            {
                return OptionFound.No;
            }
        }
    }
}
