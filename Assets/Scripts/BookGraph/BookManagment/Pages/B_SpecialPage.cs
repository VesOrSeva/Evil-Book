using TMPro;
using UnityEngine;

namespace BookGraph.Runtime
{
    public class B_SpecialPage : B_Page
    {
        [SerializeField] SpecialPageTextFields[] texts;
        [SerializeField] TextMeshProUGUI pageNumber;
        [SerializeField] GameObject specialPageParent;
        [SerializeField] GameObject onPagePrefab;
        [SerializeField] bool hasOnPageContent = false;
        [SerializeField] bool hasInPageContent = false;
        private bool used = false;

        public void UsedPage()
        {
            if (!hasOnPageContent) return;
            used = true;
            if (specialPageParent) specialPageParent.SetActive(false);
        }

        public override void OnPageVisible(RuntimeNode node)
        {
            if (!hasOnPageContent) return;
            if (specialPageParent) specialPageParent.SetActive(true);
        }

        public override void OnPageOpened(RuntimeNode node, RenderingPageType type)
        {
            if (!hasOnPageContent && !hasInPageContent) return;

            if (used) return;
            if (type != RenderingPageType.LeftFront && type != RenderingPageType.RightFront) return;

            if (hasInPageContent)
            {
                onPagePrefab.SetActive(true);
                used = true;
                return;
            }

            B_OnPageContentManager.Instance.ClearPages();
            B_OnPageContentManager.Instance.SpawnPage(onPagePrefab, node, type, gameObject);
            if (specialPageParent) specialPageParent.SetActive(false);
        }

        public override void WriteThePage(RuntimeNode node)
        {
            if (node is RuntimeSpecialPageNode)
            {
                var specialPage = (RuntimeSpecialPageNode)node;
                if (pageNumber) pageNumber.text = specialPage.TargetPage.ToString();

                if (specialPage.PageEffect == PageEffect.Write)
                {
                    foreach (var text in texts)
                    {
                        TypeText(text.textField, text.textToType);
                    }
                }
                else
                {
                    foreach (var text in texts)
                    {
                        text.textField.text = text.textToType;
                    }
                }
            }
        }
    }

    [System.Serializable]
    public class SpecialPageTextFields
    {
        public TextMeshProUGUI textField;
        public string textToType;
    }
}