using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Max2Babylon.Exporter
{

    [JsonObject]
    public class AnimationGroupData
    {
        public Guid ID { get; set; }
        public string Name { get; set; }
        public int StartTick { get; set; }
        public int EndTick { get; set; }
        public List<NodeData> NodeDataList { get; set; }

        public AnimationGroupData()
        {
            
        }
        public AnimationGroupData(Guid _id, string _name,int _startTick, int _endTick, List<NodeData> _nodeDataList)
        {
            ID = _id;
            Name = _name;
            StartTick = _startTick;
            EndTick = _endTick;
            NodeDataList = _nodeDataList;
        }

    }

    [JsonObject]
    public class NodeData
    {
        public uint Handle { get; set; }
        public string Name { get; set; }
        public string ParentName { get; set; }

        public NodeData(uint _handle, string _name, string _parentName)
        {
            Handle = _handle;
            Name = _name;
            ParentName = _parentName;
        }
    }
}
