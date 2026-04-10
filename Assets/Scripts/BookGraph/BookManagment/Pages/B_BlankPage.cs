using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace BookGraph.Runtime
{
    public class B_BlankPage : B_Page
    {
        [SerializeField] TextMeshProUGUI pageNumber;
        [SerializeField] Image background;

        public override void WriteThePage(RuntimeNode node)
        {
            background.sprite = B_PagesBackground.Instance.GetRandom();

            if (node is RuntimeBlankPageNode)
            {
                var blankPage = (RuntimeBlankPageNode)node;
                pageNumber.text = blankPage.TargetPage.ToString();
            }
        }
    }
}