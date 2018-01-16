using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Max2Babylon
{
    public class AnimationGroup
    {
        public Guid SerializedId = Guid.NewGuid();
        public string Name = "Animation";
        public int FrameStart = 0;
        public int FrameEnd = 100;
        public List<uint> NodeHandles = new List<uint>();

        public AnimationGroup() { }
        public AnimationGroup(AnimationGroup other)
        {
            DeepCopyFrom(other);
        }
        public void DeepCopyFrom(AnimationGroup other)
        {
            SerializedId = other.SerializedId;
            Name = other.Name;
            FrameStart = other.FrameStart;
            FrameEnd = other.FrameEnd;
            NodeHandles.Clear();
            NodeHandles.AddRange(other.NodeHandles);
        }

        const string s_DisplayNameFormat = "{0} ({1:d}, {2:d})";
        public override string ToString()
        {
            return string.Format(s_DisplayNameFormat, Name, FrameStart, FrameEnd);
        }

        #region Serialization

        const char s_PropertySeparator = ';';
        const string s_PropertyFormat = "{0};{1};{2};{3}";
        public string GetPropertyName() { return SerializedId.ToString(); }

        public void LoadFromData(string propertyName)
        {
            if (!Guid.TryParse(propertyName, out SerializedId))
                throw new Exception("Invalid ID, can't deserialize.");

            string propertiesString = string.Empty;
            if (!Loader.Core.RootNode.GetUserPropString(propertyName, ref propertiesString))
                return;

            string[] properties = propertiesString.Split(s_PropertySeparator);

            if (properties.Length < 4)
                throw new Exception("Invalid number of properties, can't deserialize.");

            Name = properties[0];
            if (!int.TryParse(properties[1], out FrameStart))
                throw new Exception("Failed to parse FrameStart property.");
            if (!int.TryParse(properties[2], out FrameEnd))
                throw new Exception("Failed to parse FrameEnd property.");

            if (string.IsNullOrEmpty(properties[3]))
                return;

            int numNodeIDs = properties.Length - 3;
            if (NodeHandles.Capacity < numNodeIDs) NodeHandles.Capacity = numNodeIDs;
            int numFailed = 0;
            for (int i = 0; i < numNodeIDs; ++i)
            {
                if (!uint.TryParse(properties[3 + i], out uint id))
                {
                    ++numFailed;
                    continue;
                }
                NodeHandles.Add(id);
            }

            if (numFailed > 0)
                throw new Exception(string.Format("Failed to parse {0} node ids.", numFailed));
        }

        public void SaveToData()
        {
            // ' ' and '=' are not allowed by max, ';' is our data separator
            if (Name.Contains(' ') || Name.Contains('=') || Name.Contains(s_PropertySeparator))
                throw new FormatException("Invalid character(s) in animation Name: " + Name + ". Spaces, equal signs and the separator '" + s_PropertySeparator + "' are not allowed.");

            string nodes = string.Join(s_PropertySeparator.ToString(), NodeHandles);
            StringBuilder stringBuilder = new StringBuilder();
            stringBuilder.AppendFormat(s_PropertyFormat, Name, FrameStart, FrameEnd, nodes);

            Loader.Core.RootNode.SetStringProperty(GetPropertyName(), stringBuilder.ToString());
        }

        public void DeleteFromData()
        {
            Loader.Core.RootNode.DeleteProperty(GetPropertyName());
        }

        #endregion
    }
}
