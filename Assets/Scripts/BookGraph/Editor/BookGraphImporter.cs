using UnityEditor.AssetImporters;
using System.Collections.Generic;
using Unity.GraphToolkit.Editor;
using BookGraph.Runtime;
using UnityEngine;
using System.Linq;
using System;

namespace BookGraph.Editor
{
    [ScriptedImporter(1, BookGraph.AssetExtension)]
    public class BookGraphImporter : ScriptedImporter
    {
        public override void OnImportAsset(AssetImportContext ctx)
        {
            BookGraph editorGraph = GraphDatabase.LoadGraphForImporter<BookGraph>(ctx.assetPath);
            RuntimeGraph runtimeGraph = ScriptableObject.CreateInstance<RuntimeGraph>();

            var nodeIDMap = new Dictionary<INode, string>();

            foreach (var node in editorGraph.GetNodes())
            {
                nodeIDMap[node] = Guid.NewGuid().ToString();
            }

            var startNode = editorGraph.GetNodes().OfType<StartNode>().FirstOrDefault();
            if (startNode != null)
            {
                runtimeGraph.Pages = GetPortValue<int>(startNode.GetInputPortByName("Pages Count (Must be even)"));
                var entryPort = startNode.GetOutputPorts().FirstOrDefault()?.firstConnectedPort;
                if (entryPort != null) runtimeGraph.EntryNodeId = nodeIDMap[entryPort.GetNode()];
            }

            foreach (var iNode in editorGraph.GetNodes())
            {
                if (iNode is StartNode) continue;

                // to ignore internal nodes, constants, variable nodes, etc
                if (!IsValidNode(iNode)) continue;

                RuntimeNode runtimeNode = CreateRuntimeNode(iNode);
                runtimeNode.NodeId = nodeIDMap[iNode];

                ProcessRuntimeNode(iNode, runtimeNode, nodeIDMap);

                runtimeGraph.AllNodes.Add(runtimeNode);
            }

            ctx.AddObjectToAsset("RuntimeData", runtimeGraph);
            ctx.SetMainObject(runtimeGraph);
        }

        private void ProcessRuntimeNode(INode sourceNode, RuntimeNode runtimeNode, Dictionary<INode, string> idMap)
        {
            switch (sourceNode)
            {
                case DefaultPage defaultPage:
                    ProcessDefaultPageNode(defaultPage, (RuntimeDefaultPageNode)runtimeNode, idMap);
                    break;

                case PageWithHeader pageWithHeader:
                    ProcessPageWithHeaderNode(pageWithHeader, (RuntimePageWithHeaderNode)runtimeNode, idMap);
                    break;

                case SpecialPage specialPage:
                    ProcessSpecialPageNode(specialPage, (RuntimeSpecialPageNode)runtimeNode, idMap);
                    break;

                case ErasePage erasePage:
                    ProcessErasePageNode(erasePage, (RuntimeErasePageNode)runtimeNode, idMap);
                    break;

                case FlipToPages flipPages:
                    ProcessFlipPagesNode(flipPages, (RuntimeFlipPagesNode)runtimeNode, idMap);
                    break;

                case ChoicePage choicePage:
                    ProcessChoicePageNode(choicePage, (RuntimeChoicePageNode)runtimeNode, idMap);
                    break;

                case EndNode endNode:
                    ProcessEndNode(endNode, (RuntimeEndNode)runtimeNode, idMap);
                    break;

                default: throw new NotImplementedException($"Unsuported node type: {sourceNode.GetType()}");
            }
        }

        #region Processors

        private void ProcessDefaultPageNode(DefaultPage node, RuntimeDefaultPageNode runtimeNode, Dictionary<INode, string> nodeIDMap)
        {
            runtimeNode.PageAction = GetOptionValue<PageAction>(node, "Action");
            runtimeNode.PageEffect = GetOptionValue<PageEffect>(node, "Effect");
            runtimeNode.TargetPage = GetOptionValue<int>(node, "Target Page");

            runtimeNode.PageText = GetPortValue<string>(node.GetInputPortByName("Page Text"));

            var nextNodePort = node.GetOutputPortByName("Out").firstConnectedPort;
            if (nextNodePort != null) runtimeNode.NextNodeId = nodeIDMap[nextNodePort.GetNode()];
        }

        private void ProcessPageWithHeaderNode(PageWithHeader node, RuntimePageWithHeaderNode runtimeNode, Dictionary<INode, string> nodeIDMap)
        {
            runtimeNode.PageAction = GetOptionValue<PageAction>(node, "Action");
            runtimeNode.PageEffect = GetOptionValue<PageEffect>(node, "Effect");
            runtimeNode.TargetPage = GetOptionValue<int>(node, "Target Page");

            runtimeNode.PageText = GetPortValue<string>(node.GetInputPortByName("Page Text"));
            runtimeNode.HeaderText = GetPortValue<string>(node.GetInputPortByName("Header Text"));

            var nextNodePort = node.GetOutputPortByName("Out").firstConnectedPort;
            if (nextNodePort != null) runtimeNode.NextNodeId = nodeIDMap[nextNodePort.GetNode()];
        }

        private void ProcessSpecialPageNode(SpecialPage node, RuntimeSpecialPageNode runtimeNode, Dictionary<INode, string> nodeIDMap)
        {
            runtimeNode.PageAction = GetOptionValue<PageAction>(node, "Action");
            runtimeNode.PageEffect = GetOptionValue<PageEffect>(node, "Effect");
            runtimeNode.TargetPage = GetOptionValue<int>(node, "Target Page");

            runtimeNode.PagePrefab = GetPortValue<GameObject>(node.GetInputPortByName("Page Prefab"));

            var nextNodePort = node.GetOutputPortByName("Out").firstConnectedPort;
            if (nextNodePort != null) runtimeNode.NextNodeId = nodeIDMap[nextNodePort.GetNode()];
        }

        private void ProcessErasePageNode(ErasePage node, RuntimeErasePageNode runtimeNode, Dictionary<INode, string> nodeIDMap)
        {
            runtimeNode.TargetPage = GetOptionValue<int>(node, "Target Page");

            var nextNodePort = node.GetOutputPortByName("Out").firstConnectedPort;
            if (nextNodePort != null) runtimeNode.NextNodeId = nodeIDMap[nextNodePort.GetNode()];
        }

        private void ProcessFlipPagesNode(FlipToPages node, RuntimeFlipPagesNode runtimeNode, Dictionary<INode, string> nodeIDMap)
        {
            runtimeNode.TargetPage = GetOptionValue<int>(node, "Target Page");

            var nextNodePort = node.GetOutputPortByName("Out").firstConnectedPort;
            if (nextNodePort != null) runtimeNode.NextNodeId = nodeIDMap[nextNodePort.GetNode()];
        }

        private void ProcessChoicePageNode(ChoicePage node, RuntimeChoicePageNode runtimeNode, Dictionary<INode, string> nodeIDMap)
        {
            runtimeNode.PageAction = GetOptionValue<PageAction>(node, "Action");
            runtimeNode.PageEffect = GetOptionValue<PageEffect>(node, "Effect");
            runtimeNode.TargetPage = GetOptionValue<int>(node, "Target Page");
            runtimeNode.PageLayout = node.GetNodeOptionByName("Choices Position")?.TryGetValue(out ChoicePage.ChoicesPosition pos) == true ? (int)pos : 0;

            runtimeNode.PageText = GetPortValue<string>(node.GetInputPortByName("Page Text"));

            var choiceOutputPorts = node.GetOutputPorts().Where(p => p.name.StartsWith("Choice "));
            foreach (var choicePort in choiceOutputPorts)
            {
                var index = choicePort.name.Substring("Choice ".Length);
                var textPort = node.GetInputPortByName($"Choice Text {index}");

                var choiceData = new ChoiceData
                {
                    ChoiceText = GetPortValue<string>(textPort),
                    NextNodeId = choicePort.firstConnectedPort != null ? nodeIDMap[choicePort.firstConnectedPort.GetNode()] : null
                };

                runtimeNode.Choices.Add(choiceData);
            }
        }

        private void ProcessEndNode(EndNode node, RuntimeEndNode runtimeNode, Dictionary<INode, string> nodeIDMap)
        {
            // no logic for now
        }

        #endregion

        #region Helpers

        private RuntimeNode CreateRuntimeNode(INode node)
        {
            return node switch
            {
                DefaultPage => new RuntimeDefaultPageNode(),
                PageWithHeader => new RuntimePageWithHeaderNode(),
                SpecialPage => new RuntimeSpecialPageNode(),
                ErasePage => new RuntimeErasePageNode(),
                FlipToPages => new RuntimeFlipPagesNode(),
                ChoicePage => new RuntimeChoicePageNode(),
                EndNode => new RuntimeEndNode(),
                _ => throw new NotImplementedException($"Unsupported node type: {node.GetType()}")
            };
        }

        private bool IsValidNode(INode node) => 

               node is DefaultPage 
            || node is PageWithHeader 
            || node is SpecialPage 
            || node is ErasePage 
            || node is FlipToPages 
            || node is ChoicePage 
            || node is EndNode;


        private T GetPortValue<T>(IPort port)
        {
            if (port == null) return default;

            if (port.isConnected)
            {
                if (port.firstConnectedPort?.GetNode() is IVariableNode variableNode)
                {
                    variableNode.variable.TryGetDefaultValue(out T value);
                    return value;
                }
            }

            port.TryGetValue(out T fallBackValue);
            return fallBackValue;
        }

        private T GetOptionValue<T>(Node node, string optionName)
        {
            var option = node.GetNodeOptionByName(optionName);
            if (option == null) return default;

            if (option.TryGetValue(out T value)) return value;

            Debug.LogWarning($"Option '{optionName}' on node {node.GetType().Name} is not of type {typeof(T)}");
            return default;
        }

        #endregion
    }
}