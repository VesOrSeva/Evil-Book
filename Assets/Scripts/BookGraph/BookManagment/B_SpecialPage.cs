using TMPro;
using UnityEngine;

namespace BookGraph.Runtime
{
    public class B_SpecialPage : B_Page
    {
        [SerializeField] SpecialPageTextFields[] texts;
        [SerializeField] GameObject specialPageParent;
        [SerializeField] GameObject onPagePrefab;
        [SerializeField] bool hasOnPageContent = false;
        private bool used = false;

        public void UsedPage()
        {
            if (!hasOnPageContent) return;
            used = true;
            specialPageParent.SetActive(true);
        }

        public override void OnPageVisible(RuntimeNode node)
        {
            if (!hasOnPageContent) return;
            specialPageParent.SetActive(true);
        }

        public override void OnPageOpened(RuntimeNode node, RenderingPageType type)
        {
            if (!hasOnPageContent) return;

            if (used) return;
            if (type != RenderingPageType.LeftFront && type != RenderingPageType.RightFront) return;

            B_OnPageContentManager.Instance.ClearPages();
            B_OnPageContentManager.Instance.SpawnPage(onPagePrefab, node, type, gameObject);
            specialPageParent.SetActive(false);
        }

        public override void WriteThePage(RuntimeNode node)
        {
            if (node is RuntimeSpecialPageNode)
            {
                var specialPage = (RuntimeSpecialPageNode)node;

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