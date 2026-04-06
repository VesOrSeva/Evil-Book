using TMPro;
using UnityEngine;

namespace BookGraph.Runtime
{
    public class B_BlankPage : B_Page
    {
        [SerializeField] TextMeshProUGUI pageNumber;
        public override void WriteThePage(RuntimeNode node)
        {
            if (node is RuntimeBlankPageNode)
            {
                var blankPage = (RuntimeBlankPageNode)node;
                pageNumber.text = blankPage.TargetPage.ToString();
            }
        }
    }
}