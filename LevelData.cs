using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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

        /// <summary>
        /// Creates a new LevelData by loading the info as well as the level data.
        /// </summary>
        /// <param name="text"></param>
        public LevelData(string path)
        {
            string[] lines = System.IO.File.ReadAllLines(path);

            // get ObjectType names
            HashSet<string> objectTypeNames = new(Enum.GetNames<ObjectType>());

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
                string[] parts = line.Split('\t');

                if (!Enum.TryParse(parts[0], out type))
                {
                    // not an ObjectType, so add to values
                    _values.Add(parts[0], parts[1]);

                    type = ObjectType.Generic;
                }
            }
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
    }
}
