using TMPro;
using UnityEngine;

namespace BookGraph.Runtime
{
    public class EyePage : OnPageContent
    {
        [SerializeField] Transform eye;
        [SerializeField] TextMeshProUGUI pageNumber;

        private Camera cam;

        public override void Initialize(RuntimeNode node, GameObject originalPage)
        {
            if (node is not RuntimeSpecialPageNode specialPage) return;
            if (pageNumber != null) pageNumber.text = specialPage.TargetPage.ToString();

            cam = Camera.main;

        }
    }
}