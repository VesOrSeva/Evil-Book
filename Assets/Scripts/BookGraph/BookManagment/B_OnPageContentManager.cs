using TMPro;
using UnityEngine;

namespace BookGraph.Runtime
{
    public class B_OnPageContentManager : Singleton<B_OnPageContentManager>
    {
        [SerializeField] Transform LeftPageParent;
        [SerializeField] Transform RightPageParent;

        private GameObject currentLeftPage;
        private GameObject currentRightPage;

        public void SpawnPage(GameObject prefab, RuntimeNode node, RenderingPageType pageType, GameObject originalPage)
        {
            Transform parent = pageType == RenderingPageType.LeftFront ? LeftPageParent : RightPageParent;

            if (parent == null)
            {
                Debug.LogError("Missing page parent!");
                return;
            }

            var instance = Instantiate(prefab, parent);

            if (pageType == RenderingPageType.LeftFront) currentLeftPage = instance;
            else currentRightPage = instance;

            instance.GetComponent<FakeChoicePage>()?.Initialize(node, originalPage);
            instance.GetComponent<OnPageContent>()?.Initialize(node, originalPage);
        }

        public void ClearPages()
        {
            if (currentLeftPage != null) Destroy(currentLeftPage);
            if (currentRightPage != null) Destroy(currentRightPage);
        }

        public void UpdatePageDepth(Vector3 leftOffset, Vector3 rightOffset)
        {
            var localLeft = LeftPageParent.localPosition;
            localLeft.z = leftOffset.z * 1700f;
            LeftPageParent.localPosition = localLeft;

            var localRight = RightPageParent.localPosition;
            localRight.z = rightOffset.z * 1700f;
            RightPageParent.localPosition = localRight;
        }
    }
}