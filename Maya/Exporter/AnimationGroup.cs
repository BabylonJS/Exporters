using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Maya2Babylon
{
    public class AnimationGroup
    {
        public static List<AnimationGroup> animationGroups = new List<AnimationGroup>();

        public bool IsDirty { get; private set; } = true;

        private Guid serializedId = Guid.NewGuid();
        public Guid SerializedId
        {
            get { return serializedId; }
            set
            {
                if (value.Equals(SerializedId))
                    return;
                IsDirty = true;
                serializedId = value;
            }
        }

        private string name = "Animation";
        public string Name
        {
            get { return name; }
            set
            {
                if (value.Equals(name))
                    return;
                IsDirty = true;
                name = value;
            }
        }

        private int ticksStart = Loader.GetMinTime();
        public int FrameStart
        {
            get { return ticksStart; }
            set
            {
                if (value.Equals(FrameStart)) // property getter
                    return;
                IsDirty = true;
                ticksStart = value;
            }
        }

        private int ticksEnd = Loader.GetMaxTime();
        public int FrameEnd
        {
            get { return ticksEnd; }
            set
            {
                if (value.Equals(FrameEnd)) // property getter
                    return;
                IsDirty = true;
                ticksEnd = value;
            }
        }

        // use current timeline frame range by default
        private List<uint> nodeHandles = new List<uint>();
        public IList<uint> NodeHandles
        {
            get { return nodeHandles.AsReadOnly(); }
            set
            {
                // if the lists are equal, return early so isdirty is not touched
                if (nodeHandles.Count == value.Count)
                {
                    bool equal = true;
                    int i = 0;
                    foreach (uint newNodeHandle in value)
                    {
                        if (!newNodeHandle.Equals(nodeHandles[i]))
                        {
                            equal = false;
                            break;
                        }
                        ++i;
                    }
                    if (equal)
                        return;
                }

                IsDirty = true;
                nodeHandles.Clear();
                nodeHandles.AddRange(value);
            }
        }

        const char s_PropertySeparator = ';';
        const string s_PropertyFormat = "{0};{1};{2};{3}";

        public AnimationGroup() { }
        public AnimationGroup(AnimationGroup other)
        {
            DeepCopyFrom(other);
        }
        public void DeepCopyFrom(AnimationGroup other)
        {
            serializedId = other.serializedId;
            name = other.name;
            ticksStart = other.ticksStart;
            ticksEnd = other.ticksEnd;
            nodeHandles.Clear();
            nodeHandles.AddRange(other.nodeHandles);
            IsDirty = true;
        }

        public override string ToString()
        {
            return $"{Name} ({FrameStart}, {FrameEnd})";
        }

        #region Serialization

        public string GetPropertyName() { return serializedId.ToString(); }

        public void LoadFromData(string propertyName)
        {
            if (!Guid.TryParse(propertyName, out serializedId))
                throw new Exception("Invalid ID, can't deserialize.");

            string propertiesString = string.Empty;
            if (!Loader.GetUserPropString(propertyName, ref propertiesString))
                return;

            string[] properties = propertiesString.Split(s_PropertySeparator);

            if (properties.Length < 4)
                throw new Exception("Invalid number of properties, can't deserialize.");

            // set dirty explicitly just before we start loading, set to false when loading is done
            // if any exception is thrown, it will have a correct value
            IsDirty = true;

            name = properties[0];
            if (!int.TryParse(properties[1], out ticksStart))
                throw new Exception("Failed to parse FrameStart property.");
            if (!int.TryParse(properties[2], out ticksEnd))
                throw new Exception("Failed to parse FrameEnd property.");

            IsDirty = false;
        }

        public void SaveToData()
        {
            //' ' and '=' are not allowed by max, ';' is our data separator
            if (name.Contains(' ') || name.Contains('=') || name.Contains(s_PropertySeparator))
                throw new FormatException("Invalid character(s) in animation Name: " + name + ". Spaces, equal signs and the separator '" + s_PropertySeparator + "' are not allowed.");

            string nodes = string.Join(s_PropertySeparator.ToString(), nodeHandles);
            StringBuilder stringBuilder = new StringBuilder();
            stringBuilder.AppendFormat(s_PropertyFormat, name, ticksStart, ticksEnd, nodes);

            Loader.SetStringProperty(GetPropertyName(), stringBuilder.ToString());

            IsDirty = false;
        }

        public void DeleteFromData()
        {
            Loader.DeleteProperty(GetPropertyName());
            IsDirty = true;
        }

        #endregion
    }

    public class AnimationGroupList : List<AnimationGroup>
    {
        const string s_AnimationListPropertyName = "babylonjs_AnimationList";

        public void LoadFromData()
        {
            string[] animationPropertyNames = Loader.GetStringArrayProperty(s_AnimationListPropertyName);

            if (Capacity < animationPropertyNames.Length)
                Capacity = animationPropertyNames.Length;

            foreach (string propertyNameStr in animationPropertyNames)
            {
                AnimationGroup info = new AnimationGroup();
                info.LoadFromData(propertyNameStr);
                Add(info);
            }
        }

        public void SaveToData()
        {
            List<string> animationPropertyNameList = new List<string>();
            for (int i = 0; i < Count; ++i)
            {
                if (this[i].IsDirty)
                    this[i].SaveToData();
                animationPropertyNameList.Add(this[i].GetPropertyName());
            }

            Loader.SetStringArrayProperty(s_AnimationListPropertyName, animationPropertyNameList);
        }
    }
}
