using System.Collections.Generic;
using UnityEngine;
using System;

namespace BookGraph.Runtime
{
    [Serializable]
    public abstract class RuntimeNode
    {
        public string NodeId;
    }

    [Serializable]
    public class RuntimeStartNode : RuntimeNode
    {
        public string NextNodeId;
    }

    [Serializable]
    public class RuntimeEndNode : RuntimeNode
    {
        // empty for now
    }

    [Serializable]
    public class RuntimeBlankPageNode : RuntimeNode
    {
        public int TargetPage;
    }

    [Serializable]
    public class RuntimeDefaultPageNode : RuntimeNode
    {
        public PageEffect PageEffect;
        public string PageText;
        public int TargetPage;
        public string NextNodeId;
    }

    [Serializable]
    public class RuntimePageWithHeaderNode : RuntimeNode
    {
        public PageEffect PageEffect;
        public string PageText;
        public string HeaderText;
        public int TargetPage;
        public string NextNodeId;
    }

    [Serializable]
    public class RuntimeSpecialPageNode : RuntimeNode
    {
        public PageEffect PageEffect;
        public GameObject PagePrefab;
        public int TargetPage;
        public string NextNodeId;
    }

    [Serializable]
    public class RuntimeErasePageNode : RuntimeNode
    {
        public int TargetPage;
        public string NextNodeId;
    }

    [Serializable]
    public class RuntimeFlipPagesNode : RuntimeNode
    {
        public int TargetPage;
        public string NextNodeId;
    }

    [Serializable]
    public class RuntimeChoicePageNode : RuntimeNode
    {
        public PageEffect PageEffect;
        public int PageLayout;
        public string PageText;
        public int TargetPage;

        public List<ChoiceData> Choices = new List<ChoiceData>();
    }

    [Serializable]
    public class RuntimeConditionNode : RuntimeNode
    {
        public int TargetPage;
        public string NextNodeId;
    }


    [Serializable]
    public class RuntimeAudioNode : RuntimeNode
    {
        public AudioClip Clip;
        public float Volume;
        public string NextNodeId;
    }

    [Serializable]
    public class ChoiceData
    {
        public string ChoiceText;
        public string NextNodeId;
    }

    public enum PageEffect
    {
        Write = 0,
        Instant = 1
    }
}