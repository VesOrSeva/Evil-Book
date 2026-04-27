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
        private bool wasWritten = false;
        public bool WasWritten => wasWritten;

        private readonly List<GameObject> spawnedButtons = new();

        public void UsedPage(int buttonIndex)
        {
            used = true;
            optionPageParent.SetActive(true);
            foreach (var button in spawnedButtons) button.GetComponent<Button>().interactable = false;
            var usedButton = spawnedButtons[buttonIndex].GetComponent<Button>();

            ColorBlock colors = usedButton.colors;
            colors.disabledColor = new Color(0.6f, 0f, 0f);
            usedButton.colors = colors;
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

            TypeText(pageText.GetComponent<TextMeshProUGUI>(), choicePage.PageText, 0.03f);
            pageNumber.text = choicePage.TargetPage.ToString();

            foreach (var choice in choicePage.Choices)
            {
                var buttonGO = Instantiate(optionButtonPrefab, optionsContainer.transform);
                spawnedButtons.Add(buttonGO);
                var text = buttonGO.GetComponentInChildren<TextMeshProUGUI>();
                if (text != null) TypeText(text, choice.ChoiceText, 0.05f);
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

        public void Written()
        {
            wasWritten = true;
        }
    }
}