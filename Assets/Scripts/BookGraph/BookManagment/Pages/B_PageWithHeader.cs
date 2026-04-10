using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace BookGraph.Runtime
{
    public class B_PageWithHeader : B_Page
    {
        [SerializeField] TextMeshProUGUI headerText;
        [SerializeField] TextMeshProUGUI text;
        [SerializeField] TextMeshProUGUI pageNumber;
        [SerializeField] Image background;

        public override void WriteThePage(RuntimeNode node)
        {
            background.sprite = B_PagesBackground.Instance.GetRandom();

            if (node is RuntimePageWithHeaderNode)
            {
                var headerPage = (RuntimePageWithHeaderNode)node;

                if (headerPage.PageEffect == PageEffect.Write)
                {
                    TypeText(headerText, headerPage.HeaderText);
                    TypeText(text, headerPage.PageText);
                }
                else
                {
                    headerText.text = headerPage.HeaderText;
                    text.text = headerPage.PageText;
                }

                pageNumber.text = headerPage.TargetPage.ToString();
            }
        }
    }
}