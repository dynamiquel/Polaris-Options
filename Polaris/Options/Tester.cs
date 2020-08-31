using System;
using System.Collections.Generic;
using UnityEngine;

namespace Polaris.Options
{
    class Tester
    {
        /*
         * Known issues:
         * Reading: Lists don't convert from List<object> (or Newtonsoft.Json.Linq.JArray) to List<T>.
         * Reading: Arrays don't convert from List<object> (or Newtonsoft.Json.Linq.JArray) to T[].
         * Setting: When manually adding a dictionary, if the value type isn't dynamic, then types not equal to T
         * can not be added to the dictionary until a Save and Refresh. This is because when reading Options, all
         * dictionaries are processed as Dictionary<string, dynamic>.
         * Reading: Yaml can't determine the difference between floating points and decimals.
         * All floating points from Yaml will be doubles.
         */
        [RuntimeInitializeOnLoadMethod]
        // Start is called before the first frame update
        static void Start()
        {
            var stringTest = "2.4";
            var intTest = 567457544;
            var boolTest = true;
            var floatTest = 3.14159265f;
            var double_test = 3.77777777777777777;
            var longTest = 922337203685477580;
            var arrayTest = new string[]
            {
                "one", "two", "three", "four"
            };
            var listTest = new List<int>
            {
                1, 2, 3, 4
            };
            var dictionaryTest = new Dictionary<string, float>
            {
                ["one"] = 1.1f,
                ["two"] = 2.2f,
                ["three"] = 3.3f,
                ["four"] = 4.4f
            };

            Debug.Assert(OptionsManager.GetValue("string_test", stringTest) == stringTest,
                "'string_test' key doesn't equal 2.4");

            Debug.Assert(OptionsManager.ContainsKey("string_test"), "'string_test' key couldn't be found.");
            Debug.Assert(!OptionsManager.ContainsKey("invalid key"), "'invalid key' key was found.");

            Debug.Assert(OptionsManager.SetValue("int_test", intTest) != SetValueOutput.Failed,
                "'int_test' key failed to set.");

            Debug.Assert(OptionsManager.Save(), "Options couldn't be saved.");

            OptionsManager.Refresh();

            Debug.Assert((int) OptionsManager.GetValue("int_test") == intTest, "'int_test' key couldn't be retrieved.");

            Debug.Assert(OptionsManager.Delete("string_test"), "'string_test' key couldn't be deleted.");

            Debug.Assert(OptionsManager.SetValue("bool_test", boolTest) != SetValueOutput.Failed,
                "'bool_test' key couldn't be set.");

            Debug.Assert(OptionsManager.DeleteAll(), "Couldn't delete all options.");

            OptionsManager.Refresh();

            Debug.Assert(!OptionsManager.ContainsKey("bool_test"), "'bool_test' key was found in options.");
            Debug.Assert(OptionsManager.ContainsKey("int_test"), "'int_test' key couldn't be found in options.");

            Debug.Assert(Math.Abs(OptionsManager.GetValue("float_test", floatTest) - floatTest) < 0.01,
                "'float_test' key couldn't be set or get.");

            //Debug.Assert(OptionsManager.GetValue("array_test", arrayTest).Length == arrayTest.Length, "'array_test' key does not have the same length.");
            //Debug.Assert(OptionsManager.GetValue("list_test", listTest)[1] == listTest[1], "'list_test' key does not have the same values.");

            Debug.Assert(OptionsManager.SetValue("group:one", 1) != SetValueOutput.Failed,
                "'group:one' key set failed.");
            Debug.Assert(OptionsManager.Save(), "Could not save options.");
            OptionsManager.Refresh();
            Debug.Assert((int) OptionsManager.GetValue("group:one") == 1, "'group:one' key doesn't equal 1.");

            Debug.Assert(OptionsManager.SetValue("dictionary_test", dictionaryTest) != SetValueOutput.Failed,
                "'dictionary_test' key couldn't be set.");
            Debug.Assert((float) OptionsManager.GetValue("dictionary_test:three") == 3.3f,
                "'dictionary_test:three' key couldn't be retrieved.");

            Debug.Assert(OptionsManager.SetValue("dictionary_test:five", 5.5f) != SetValueOutput.Failed,
                "'dictionary_test:five' key couldn't be set.");
            OptionsManager.Save();
            OptionsManager.Refresh();
            Debug.Assert(OptionsManager.SetValue("dictionary_test:six", "six") != SetValueOutput.Failed,
                "'dictionary_test:six' key couldn't be set.");
            OptionsManager.Save();
            OptionsManager.Refresh();
            Debug.Assert((string) OptionsManager.GetValue("dictionary_test:six") == "six",
                "'dictionary_test:six' key couldn't be get.");

            Debug.Assert(OptionsManager.SetValue("group:display:x", 1920) != SetValueOutput.Failed,
                "'group:display:x' key could not be set.");
            Debug.Assert(OptionsManager.SetValue("group:display:y", 1080) != SetValueOutput.Failed,
                "'group:display:y' key could not be set.");
            Debug.Assert(OptionsManager.SetValue("group:display:hdr", false) != SetValueOutput.Failed,
                "'group:display:hdr' key could not be set.");
            OptionsManager.Save();
            OptionsManager.Refresh();

            Debug.Assert(OptionsManager.GetValue<int>("group:display:x") == 1920,
                "'group:display:x' key is not equal.");
            Debug.Assert((int) OptionsManager.GetValue("group:display:y") == 1080,
                "'group:display:y' key is not equal.");
            Debug.Assert(OptionsManager.GetValue<bool>("group:display:hdr") == false,
                "'group:display:hdr' key is not equal.");

            Debug.Assert(OptionsManager.SetValue("double_test", double_test) != SetValueOutput.Failed,
                "'double_test' key failed to set.");
            OptionsManager.Save();
            OptionsManager.Refresh();
            Debug.Assert(OptionsManager.GetValue<double>("double_test") == double_test,
                "'double_test' key is not equal.");
            Debug.Assert(OptionsManager.GetValue<bool>("group:display:hdr") == false,
                "'group:display:hdr' key is not equal.");

            Debug.Assert(OptionsManager.SetValue("long_test", longTest) != SetValueOutput.Failed,
                "'long_test' key could not be set.");
            OptionsManager.Save();
            OptionsManager.Refresh();
            Debug.Assert(OptionsManager.GetValue<long>("long_test") == longTest, "'long_test' key could not be get.");
        }
    }
}