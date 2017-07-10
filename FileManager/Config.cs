using System;
using System.IO;
using System.Collections.Generic;

namespace FileManager
{
    public class Config
    {

        public class KeyValue
        {
            public KeyValue(string key, string value) { Key = key; Value = value; }
            public string Key { get; private set; }
            public string Value { get; private set; }
        }

        private string _path;

        private readonly Dictionary<string, string> _configs = new Dictionary<string, string>();
        private KeyValue[] _configKeysValues;
        private bool _refreshKeysValues = true;

        public Config(string path)
        {
            _path = path;
            if (!File.Exists(path))
            {
                File.WriteAllText(path, "");
            }
            foreach (string line in File.ReadAllLines(path))
            {
                string[] keyValue = line.Split('=');
                _configs[keyValue[0]] = keyValue[1];
            }
        }

        public string GetValue(string key)
        {
            if (_configs.ContainsKey(key))
            {
                return _configs[key];
            }
            return "";
        }

        public KeyValue[] GetConfigs()
        {
            if (_refreshKeysValues)
            {
                _refreshKeysValues = false;
                _configKeysValues = new KeyValue[_configs.Count];
                int index = 0;
                foreach (KeyValuePair<string, string> pair in _configs)
                {
                    _configKeysValues[index++] = new KeyValue(pair.Key, pair.Value);
                }
            }
            return _configKeysValues;
        }

        public void SetValue(string key, string value)
        {
            if (key.Contains("=") || value.Contains("=")) throw new Exception("key or value can not contain '=' char");

            if (_configs[key] != value){
                _refreshKeysValues = true;
				_configs[key] = value;   
            }
        }

        public void RemoveKey(string key)
        {
            if (_configs.ContainsKey(key))
            {
                _configs.Remove(key);
                _refreshKeysValues = true;
            }
        }

        public void Apply()
        {
            File.WriteAllText(_path, AsString());
        }

        private string AsString()
        {
            string output = "";
            foreach (KeyValuePair<string, string> pair in _configs)
            {
                output += pair.Key + "=" + pair.Value + "\n";
            }
            return output.Trim('\n');
        }
    }
}
