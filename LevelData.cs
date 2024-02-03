using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;


namespace MiniGolf
{
    /// <summary>
    /// 
    /// </summary>
    internal class LevelData
    {
        private readonly Dictionary<string, string> _values = new();

        private readonly Dictionary<ObjectType, List<ObjectData>> _objectDatas = new();
        public Dictionary<ObjectType, List<ObjectData>> ObjectDatas => _objectDatas;

        private string _path = null;

        /// <summary>
        /// Creates a new LevelData by loading the info as well as the level data.
        /// </summary>
        /// <param name="text"></param>
        public LevelData(string path)
        {
            Load(path);
        }

        public void Load(string path)
        {
            _path = path;

            if(string.IsNullOrEmpty(_path))
            {
                return;
            }

            if(!File.Exists(_path))
            {
                // file does not exist: it is empty
                return;
            }

            string[] lines = File.ReadAllLines(_path);

            // add those to a dictionary for quick lookup
            ObjectType type = ObjectType.Generic;

            foreach (string line in lines)
            {
                // ignore empty lines or lines starting with a comment (#)
                if (string.IsNullOrWhiteSpace(line) || line.StartsWith('#')) continue;

                // if starts with a tab, it is some data, not a header
                if (line.StartsWith('\t'))
                {
                    // parse data

                    // create a list if no list yet for this type
                    if (!_objectDatas.TryGetValue(type, out List<ObjectData> datas))
                    {
                        datas = new List<ObjectData>();
                        _objectDatas.Add(type, datas);
                    }

                    // add to that list, but remove the '\t' first
                    datas.Add(ObjectData.FromString(line.TrimStart()));

                    continue;
                }

                // no starting tab means header data
                string[] parts = line.Split(": ");

                if (!Enum.TryParse(parts[0], out type))
                {
                    // not an ObjectType, so add to values
                    _values.Add(parts[0], parts[1]);

                    type = ObjectType.Generic;
                }
            }
        }

        public void Save()
        {
            if(string.IsNullOrEmpty(_path))
            {
                return;
            }

            List<string> lines = new();

            // add values
            foreach(var pair in _values)
            {
                lines.Add($"{pair.Key}: {pair.Value}");
            }

            // add the empty line for spacing
            lines.Add(string.Empty);

            // write the data from the scene
            foreach(var pair in _objectDatas)
            {
                // ignore if no items
                if (!pair.Value.Any()) continue;

                // add object type header
                lines.Add(pair.Key.ToString());

                foreach(ObjectData data in pair.Value)
                {
                    // add each data for that type
                    lines.Add($"\t{data}");
                }
            }

            // all done, save the file
            File.WriteAllLines(_path, lines.ToArray());
        }

        public void AddValue(string name, string value)
        {
            _values[name] = value;
        }

        public void AddValue<T>(string name, IEnumerable<T> values)
        {
            _values[name] = string.Join(' ', values.ToArray());
        }

        public string TakeValue(string name)
        {
            // remove the value and return it if found
            if(_values.TryGetValue(name, out string value))
            {
                _values.Remove(name);
                return value;
            }

            // no value found
            return null;
        }

        public BallType[] TakeBalls()
        {
            List<BallType> result = new();

            if (_values.ContainsKey("Balls"))
            {
                // get the ball names, put them in a list so they can easily be grabbed
                foreach (string name in TakeValue("Balls").Split(' '))
                {
                    if(string.IsNullOrEmpty(name)) continue;

                    result.Add(Enum.Parse<BallType>(name));
                }
            }

            return result.ToArray();
        }

        public Color TakeColor(Color defaultColor = new Color())
        {
            if(_values.ContainsKey("Color"))
            {
                return Parse.ParseColor(TakeValue("Color"));
            }

            return defaultColor;
        }

        public void Clear()
        {
            _values.Clear();
            _objectDatas.Clear();
        }
    }
}
