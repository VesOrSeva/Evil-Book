using TMPro;
using UnityEngine;

namespace BookGraph.Runtime
{
    public class B_SpecialPage : B_Page
    {
        [SerializeField] SpecialPageTextFields[] texts;
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