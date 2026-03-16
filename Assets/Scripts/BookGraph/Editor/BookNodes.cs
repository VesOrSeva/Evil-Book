using System;
using Unity.GraphToolkit.Editor;
using BookGraph.Runtime;
using UnityEngine;

namespace BookGraph.Editor
{
    [Serializable]
    public class StartNode : Node
    {
        protected override void OnDefinePorts(IPortDefinitionContext context)
        {
            context.AddOutputPort("Out").WithConnectorUI(PortConnectorUI.Arrowhead).Build();
            context.AddInputPort<int>("Pages Count (Must be even)").Build();
            context.AddInputPort<int>("Starting Page").Build();
        }
    }

    [Serializable]
    public class DefaultPage : Node
    {
        protected override void OnDefinePorts(IPortDefinitionContext context)
        {
            context.AddInputPort("In").WithConnectorUI(PortConnectorUI.Arrowhead).Build();
            context.AddInputPort<string>("Page Text").Build();

            context.AddOutputPort("Out").WithConnectorUI(PortConnectorUI.Arrowhead).Build();
        }

        protected override void OnDefineOptions(IOptionDefinitionContext context)
        {
            context.AddOption<PageAction>("Action").WithDefaultValue(PageAction.Add).Delayed();
            context.AddOption<PageEffect>("Effect").WithDefaultValue(PageEffect.Write).Delayed();
            context.AddOption<int>("Target Page").Delayed();
        }
    }

    [Serializable]
    public class PageWithHeader : Node
    {
        protected override void OnDefinePorts(IPortDefinitionContext context)
        {
            context.AddInputPort("In").WithConnectorUI(PortConnectorUI.Arrowhead).Build();
            context.AddInputPort<string>("Header Text").Build();
            context.AddInputPort<string>("Page Text").Build();

            context.AddOutputPort("Out").WithConnectorUI(PortConnectorUI.Arrowhead).Build();
        }

        protected override void OnDefineOptions(IOptionDefinitionContext context)
        {
            context.AddOption<PageAction>("Action").WithDefaultValue(PageAction.Add).Delayed();
            context.AddOption<PageEffect>("Effect").WithDefaultValue(PageEffect.Write).Delayed();
            context.AddOption<int>("Target Page").Delayed();
        }
    }

    [Serializable]
    public class SpecialPage : Node
    {
        protected override void OnDefinePorts(IPortDefinitionContext context)
        {
            context.AddInputPort("In").WithConnectorUI(PortConnectorUI.Arrowhead).Build();
            context.AddInputPort<GameObject>("Page Prefab").Build();

            context.AddOutputPort("Out").WithConnectorUI(PortConnectorUI.Arrowhead).Build();
        }

        protected override void OnDefineOptions(IOptionDefinitionContext context)
        {
            context.AddOption<PageAction>("Action").WithDefaultValue(PageAction.Add).Delayed();
            context.AddOption<PageEffect>("Effect").WithDefaultValue(PageEffect.Write).Delayed();
            context.AddOption<int>("Target Page").Delayed();
        }
    }

    [Serializable]
    public class ErasePage : Node
    {
        protected override void OnDefinePorts(IPortDefinitionContext context)
        {
            context.AddInputPort("In").WithConnectorUI(PortConnectorUI.Arrowhead).Build();
            context.AddOutputPort("Out").WithConnectorUI(PortConnectorUI.Arrowhead).Build();
        }

        protected override void OnDefineOptions(IOptionDefinitionContext context)
        {
            context.AddOption<int>("Target Page").Delayed();
        }
    }

    [Serializable]
    public class FlipToPages : Node
    {
        protected override void OnDefinePorts(IPortDefinitionContext context)
        {
            context.AddInputPort("In").WithConnectorUI(PortConnectorUI.Arrowhead).Build();
            context.AddOutputPort("Out").WithConnectorUI(PortConnectorUI.Arrowhead).Build();
        }

        protected override void OnDefineOptions(IOptionDefinitionContext context)
        {
            context.AddOption<int>("Target Page").Delayed();
        }
    }

    [Serializable]
    public class ChoicePage : Node
    {
        public enum ChoicesPosition
        {
            Top = 0,
            Bottom = 1
        }

        const string optionID = "portCount";
        protected override void OnDefinePorts(IPortDefinitionContext context)
        {
            context.AddInputPort("In").WithConnectorUI(PortConnectorUI.Arrowhead).Build();

            context.AddInputPort<string>("Page Text").Build();

            var option = GetNodeOptionByName(optionID);
            option.TryGetValue(out int portCout);
            for (int i = 0; i < portCout; i++)
            {
                context.AddInputPort<string>($"Choice Text {i}").Build();
                context.AddOutputPort($"Choice {i}").WithConnectorUI(PortConnectorUI.Arrowhead).Build();
            }
        }

        protected override void OnDefineOptions(IOptionDefinitionContext context)
        {
            context.AddOption<int>(optionID).WithDefaultValue(2).Delayed();
            context.AddOption<PageAction>("Action").WithDefaultValue(PageAction.Add).Delayed();
            context.AddOption<PageEffect>("Effect").WithDefaultValue(PageEffect.Write).Delayed();
            context.AddOption<ChoicesPosition>("Choices Position").WithDefaultValue(ChoicesPosition.Bottom).Delayed();
            context.AddOption<int>("Target Page").Delayed();
        }
    }

    [Serializable]
    public class EndNode : Node
    {
        protected override void OnDefinePorts(IPortDefinitionContext context)
        {
            context.AddInputPort("In").WithConnectorUI(PortConnectorUI.Arrowhead).Build();
        }
    }

}