using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;


namespace Config
{
    public struct DefineStruct
    {
        public string host;
        public string port;
    }

    public class DefineConfigLoader
    {
        public static DefineStruct Load(string jsonPath)
        {
            DefineStruct file;
            try
            {
                file = LoadJson(jsonPath);
            }
            catch(System.Exception ex)
            {
                Debug.Log("EX " + ex.Message);
                file = new DefineStruct()
                {
                    host = "localhost",
                    port = "7777"
                };
            }
            return file;
        }

        internal static DefineStruct LoadJson(string certJsonPath)
        {
            string json = File.ReadAllText(certJsonPath);
            DefineStruct f = JsonUtility.FromJson<DefineStruct>(json);
            return f;
        }
    }
}