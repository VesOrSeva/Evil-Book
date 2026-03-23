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

        public void SpawnPage(GameObject prefab, RuntimeNode node, RenderingPageType pageType, B_OptionsPage optionsPage)
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

            instance.GetComponent<FakeChoicePage>()?.Initialize(node, optionsPage);
        }

        public void ClearPages()
        {
            if (currentLeftPage != null) Destroy(currentLeftPage);
            if (currentRightPage != null) Destroy(currentRightPage);
        }
    }

}