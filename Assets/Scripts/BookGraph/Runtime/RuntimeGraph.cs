using UnityEngine;
using System.Collections.Generic;

namespace BookGraph.Runtime
{
    public class RuntimeGraph : ScriptableObject
    {
        public string EntryNodeId;
        public int Pages;
        [SerializeReference] public List<RuntimeNode> AllNodes = new();
    }
}