using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace BookGraph.Runtime
{
    public class B_OptionsPage : B_Page
    {
        [SerializeField] GameObject fakePagePrefab;
        [SerializeField] GameObject optionPageParent;
        [SerializeField] GameObject optionsContainer;
        [SerializeField] GameObject optionButtonPrefab;
        [SerializeField] GameObject pageText;
        [SerializeField] TextMeshProUGUI pageNumber;
        [SerializeField] Image background;

        private bool used = false;

        private readonly List<GameObject> spawnedButtons = new();

        public void UsedPage(int buttonIndex)
        {
            used = true;
            optionPageParent.SetActive(true);
            foreach (var button in spawnedButtons) button.GetComponent<Button>().interactable = false;
            var usedButton = spawnedButtons[buttonIndex].GetComponent<Button>();
            usedButton.interactable = true;
        }

        public override void WriteThePage(RuntimeNode node)
        {
            SpawnPage(node);
            background.sprite = B_PagesBackground.Instance.GetRandom();
        }

        public override void OnPageVisible(RuntimeNode node)
        {
            optionPageParent.SetActive(true);
        }

        public override void OnPageOpened(RuntimeNode node, RenderingPageType type)
        {
            if (used) return;
            if (type != RenderingPageType.LeftFront && type != RenderingPageType.RightFront) return;

            B_OnPageContentManager.Instance.ClearPages();
            B_OnPageContentManager.Instance.SpawnPage(fakePagePrefab, node, type, gameObject);
            optionPageParent.SetActive(false);
        }

        private void SpawnPage(RuntimeNode node)
        {
            if (node is not RuntimeChoicePageNode choicePage) return;
            ClearOptions();

            pageText.GetComponent<TextMeshProUGUI>().text = choicePage.PageText;
            pageNumber.text = choicePage.TargetPage.ToString();

            foreach (var choice in choicePage.Choices)
            {
                var buttonGO = Instantiate(optionButtonPrefab, optionsContainer.transform);
                spawnedButtons.Add(buttonGO);
                var text = buttonGO.GetComponentInChildren<TMP_Text>();
                if (text != null) text.text = choice.ChoiceText;
            }

            if (choicePage.PageLayout == 0)
            {
                optionsContainer.transform.SetAsFirstSibling();
            }
        }

        private void ClearOptions()
        {
            foreach (var btn in spawnedButtons)
            {
                if (btn != null) Destroy(btn);
            }

            spawnedButtons.Clear();
        }
    }
}