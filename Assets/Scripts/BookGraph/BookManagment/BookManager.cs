using System.Collections.Generic;
using System.Collections;
using UnityEngine;

namespace BookGraph.Runtime
{
    public class BookManager : Singleton<BookManager>
    {
        public RuntimeGraph currentGraph;
        private Dictionary<string, RuntimeNode> nodeLookup = new();
        private RuntimeNode currentNode;

        protected override void Awake()
        {
            base.Awake();
            StartBook(currentGraph);
        }
        public void StartBook(RuntimeGraph graph)
        {
            if (graph == null)
            {
                Debug.LogError("Missing Dialogue");
                return;
            }       
            nodeLookup.Clear();
            currentGraph = graph;

            foreach (var node in graph.AllNodes) nodeLookup[node.NodeId] = node;
            if (!string.IsNullOrEmpty(graph.EntryNodeId)) GoToNode(graph.EntryNodeId);
            else EndDialogue();
        }

        public void GoToNode(string nodeId)
        {
            if (!nodeLookup.TryGetValue(nodeId, out currentNode))
            {
                Debug.LogError($"Node {nodeId} not found!");
                EndDialogue();
                return;
            }

            ProceedNarrative();
        }

        private void EndDialogue()
        {
            currentGraph = null;
            currentNode = null;
        }

        private void ProceedNarrative()
        {
            switch (currentNode)
            {
                case RuntimeDefaultPageNode page:
                    HandleDialogueNode(page);
                    break;

                case RuntimeEndNode end:
                    HandleEndNode(end);
                    break;

                default:
                    Debug.LogWarning($"Unhandled node type: {currentNode.GetType()}");
                    EndDialogue();
                    break;
            }
        }

        #region Node Handlers

        private void HandleDialogueNode(RuntimeDefaultPageNode node)
        {

        }
        
        private void HandleEndNode(RuntimeEndNode node)
        {
            Debug.Log("Dialogue ended");
            EndDialogue();
        }

        #endregion
    }
}