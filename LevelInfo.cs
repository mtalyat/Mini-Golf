using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;

namespace MiniGolf
{
    internal class LevelInfo
    {
        private readonly Dictionary<ObjectType, ObjectTypeData> _objectTypeDatas = new();
        public Dictionary<ObjectType, ObjectTypeData> ObjectTypeDatas => _objectTypeDatas;

        public LevelInfo(string path)
        {
            if (!File.Exists(path)) return;

            string[] lines = File.ReadAllLines(path);

            // get ObjectType names
            HashSet<string> objectTypeNames = new(Enum.GetNames<ObjectType>());

            // add those to a dictionary for quick lookup
            ObjectTypeData typeData;

            foreach (string line in lines)
            {
                // ignore empty lines or lines starting with a comment (#)
                if (string.IsNullOrWhiteSpace(line) || line.StartsWith('#')) continue;

                // no starting tab means header data
                string[] parts = line.Split(": ");

                // parse type data
                if (!objectTypeNames.Contains(parts[0]))
                {
                    // ignore if not an ObjectType
                    continue;
                }

                typeData = ObjectTypeData.FromString(line);

                // add to type datas
                _objectTypeDatas.Add(typeData.Type, typeData);
            }
        }
    }
}
