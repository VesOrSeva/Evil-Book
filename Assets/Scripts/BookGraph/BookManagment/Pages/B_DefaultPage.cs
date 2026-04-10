using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace BookGraph.Runtime
{
    public class B_DefaultPage : B_Page
    {
        [SerializeField] TextMeshProUGUI text;
        [SerializeField] TextMeshProUGUI pageNumber;
        [SerializeField] Image background;

        public override void WriteThePage(RuntimeNode node)
        {
            background.sprite = B_PagesBackground.Instance.GetRandom();

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

                pageNumber.text = defaultPage.TargetPage.ToString();
            }
        }
    }
}