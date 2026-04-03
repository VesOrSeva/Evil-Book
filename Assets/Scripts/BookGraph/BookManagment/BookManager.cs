using System.Collections.Generic;
using System.Collections;
using UnityEngine;

namespace BookGraph.Runtime
{
    public class BookManager : Singleton<BookManager>
    {
        [SerializeField] RuntimeGraph currentGraph;
        [SerializeField] Book book;
        [SerializeField] AutoFlip autoFlip;

        [Header("Page Prefabs")]
        [SerializeField] GameObject blankPagePrefab;
        [SerializeField] GameObject defaultPagePrefab;
        [SerializeField] GameObject pageWithHeaderPrefab;
        [SerializeField] GameObject pageChoicePagePrefab;

        private Dictionary<string, RuntimeNode> nodeLookup = new();
        private RuntimeNode currentNode;
        Coroutine flipRoutine;

        protected override void Awake()
        {
            base.Awake();
            book.OnFlip.AddListener(OnBookFlipped);
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
            book.InitializePages(blankPagePrefab, graph.Pages);

            foreach (var node in graph.AllNodes) nodeLookup[node.NodeId] = node;
            if (!string.IsNullOrEmpty(graph.EntryNodeId)) GoToNode(graph.EntryNodeId);
            else EndDialogue();;
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
                    HandleDefaultPageNode(page);
                    break;

                case RuntimePageWithHeaderNode headerPage:
                    HandlePageWithHeaderNode(headerPage);
                    break;

                case RuntimeSpecialPageNode specialPage:
                    HandleSpecialPageNode(specialPage);
                    break;

                case RuntimeErasePageNode erasePage:
                    HandleErasePageNode(erasePage);
                    break;

                case RuntimeFlipPagesNode flipPages:
                    HandleFlipPagesNode(flipPages);
                    break;

                case RuntimeChoicePageNode choicePage:
                    HandleChoicePageNode(choicePage);
                    break;

                case RuntimeConditionNode condition:
                    HandleConditionNode(condition);
                    break;

                case RuntimeAudioNode audio:
                    HandleAudioNode(audio);
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

        private void HandleDefaultPageNode(RuntimeDefaultPageNode node)
        {
            var entry = new PageEntry(defaultPagePrefab, node);
            book.ReplacePage(GetPageNumber(node.TargetPage), entry);

            if (!string.IsNullOrEmpty(node.NextNodeId)) GoToNode(node.NextNodeId);
            else EndDialogue();
        }

        private void HandlePageWithHeaderNode(RuntimePageWithHeaderNode node)
        {
            var entry = new PageEntry(pageWithHeaderPrefab, node);
            book.ReplacePage(GetPageNumber(node.TargetPage), entry);

            if (!string.IsNullOrEmpty(node.NextNodeId)) GoToNode(node.NextNodeId);
            else EndDialogue();
        }

        private void HandleSpecialPageNode(RuntimeSpecialPageNode node)
        {
            var entry = new PageEntry(node.PagePrefab, node);
            book.ReplacePage(GetPageNumber(node.TargetPage), entry);

            if (!string.IsNullOrEmpty(node.NextNodeId)) GoToNode(node.NextNodeId);
            else EndDialogue();
        }

        private void HandleErasePageNode(RuntimeErasePageNode node)
        {
            book.RemovePage(GetPageNumber(node.TargetPage));

            if (!string.IsNullOrEmpty(node.NextNodeId)) GoToNode(node.NextNodeId);
            else EndDialogue();
        }

        private void HandleFlipPagesNode(RuntimeFlipPagesNode node)
        {
            StartCoroutine(HandleFlipPagesRoutine(node));
        }

        IEnumerator HandleFlipPagesRoutine(RuntimeFlipPagesNode node)
        {
            yield return FlipToPageRoutine(GetPageNumber(node.TargetPage));

            if (!string.IsNullOrEmpty(node.NextNodeId)) GoToNode(node.NextNodeId);
            else EndDialogue();
        }

        private void HandleChoicePageNode(RuntimeChoicePageNode node)
        {
            var entry = new PageEntry(pageChoicePagePrefab, node);
            book.ReplacePage(GetPageNumber(node.TargetPage), entry);
        }

        private void HandleConditionNode(RuntimeConditionNode node)
        {
            if (string.IsNullOrEmpty(node.NextNodeId)) EndDialogue();

            var condition = new PageCondition(GetPageNumber(node.TargetPage), node.NextNodeId);
            book.AddCondition(condition);
        }

        private void HandleAudioNode(RuntimeAudioNode node)
        {
            if (AudioManager.HasInstance) AudioManager.Instance.PlaySound(node.Clip, node.Volume);

            if (!string.IsNullOrEmpty(node.NextNodeId)) GoToNode(node.NextNodeId);
            else EndDialogue();
        }

        private void HandleEndNode(RuntimeEndNode node)
        {
            Debug.Log("Dialogue ended");
            EndDialogue();
        }

        #endregion

        private void OnBookFlipped()
        {
            CheckPageConditions();
        }

        private int GetPageNumber(int targetPage)
        {
            return targetPage - 1;
        }

        private void CheckPageConditions()
        {
            int leftPage = book.currentPage;
            int rightPage = leftPage + 1;

            TryTriggerCondition(leftPage);
            TryTriggerCondition(rightPage);
        }

        private void TryTriggerCondition(int pageIndex)
        {
            var condition = book.GetConditionForPage(pageIndex);
            if (condition == null) return;

            Debug.Log($"Condition triggered on page {pageIndex}, going to {condition.NextNodeId}");

            GoToNode(condition.NextNodeId);
        }

        public void FlipToPageAnimated(int targetPage)
        {
            if (flipRoutine != null) StopCoroutine(flipRoutine);
            flipRoutine = StartCoroutine(FlipToPageRoutine(targetPage));
        }

        IEnumerator FlipToPageRoutine(int targetPage)
        {
            targetPage = targetPage % 2 == 0 ? targetPage : targetPage - 1;

            while (book.currentPage != targetPage)
            {
                if (book.currentPage < targetPage)
                {
                    autoFlip.FlipRightPage();
                }
                else
                {
                    autoFlip.FlipLeftPage();
                }

                yield return new WaitUntil(() => !book.IsAutoFlipping);
                yield return new WaitForSeconds(autoFlip.TimeBetweenPages);
            }

            flipRoutine = null;
        }
    }
}