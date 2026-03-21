using TMPro;
using UnityEngine;

namespace BookGraph.Runtime
{
    public class B_DefaultPage : B_Page
    {
        [SerializeField] TextMeshProUGUI text;
        public override void WriteThePage(RuntimeNode node)
        {
            if (node is RuntimeDefaultPageNode)
            {
                var defaultPage = (RuntimeDefaultPageNode)node;

                if (defaultPage.PageEffect == PageEffect.Write)
                {
                    TypeText(text, defaultPage.PageText);
                }
                else
                {
                    text.text = defaultPage.PageText;
                }
            }
        }
    }
}