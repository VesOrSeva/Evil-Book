using TMPro;
using UnityEngine;

namespace BookGraph.Runtime
{
    public class B_PageWithHeader : B_Page
    {
        [SerializeField] TextMeshProUGUI headerText;
        [SerializeField] TextMeshProUGUI text;
        [SerializeField] TextMeshProUGUI pageNumber;

        public override void WriteThePage(RuntimeNode node)
        {
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