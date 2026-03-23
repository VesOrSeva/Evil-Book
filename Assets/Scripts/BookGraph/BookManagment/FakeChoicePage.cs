using System.Collections;
using System.Collections.Generic;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace BookGraph.Runtime
{
    public class FakeChoicePage : MonoBehaviour
    {
        [SerializeField] GameObject optionsContainer;
        [SerializeField] GameObject optionButtonPrefab;
        [SerializeField] GameObject pageText;

        private readonly Dictionary<TextMeshProUGUI, Coroutine> typingCoroutines = new();
        private readonly List<GameObject> spawnedButtons = new();

        private B_OptionsPage optionsPage;

        public void Initialize(RuntimeNode node, B_OptionsPage page)
        {
            if (node is not RuntimeChoicePageNode choicePage) return;
            ClearOptions();

            optionsPage = page;

            pageText.GetComponent<TextMeshProUGUI>().text = choicePage.PageText;

            foreach (var choice in choicePage.Choices)
            {
                var buttonGO = Instantiate(optionButtonPrefab, optionsContainer.transform);
                spawnedButtons.Add(buttonGO);

                var button = buttonGO.GetComponent<Button>();
                var text = buttonGO.GetComponentInChildren<TMP_Text>();

                if (text != null)
                    text.text = choice.ChoiceText;

                string nextNodeId = choice.NextNodeId;

                button.onClick.RemoveAllListeners();
                button.onClick.AddListener(() =>
                {
                    OnChoiceSelected(nextNodeId);
                });
            }
        }

        private void OnChoiceSelected(string nodeId)
        {
            optionsPage.UsedPage();
            BookManager.Instance.GoToNode(nodeId);
            Destroy(gameObject);
        }

        private void ClearOptions()
        {
            foreach (var btn in spawnedButtons)
            {
                if (btn != null)
                    Destroy(btn);
            }
            spawnedButtons.Clear();
        }

        public void TypeText(TextMeshProUGUI textObject, string textToType, float charDelay = 0.03f)
        {
            if (typingCoroutines.TryGetValue(textObject, out var existingCoroutine))
                StopCoroutine(existingCoroutine);

            Coroutine newCoroutine = StartCoroutine(TypeTextCoroutine(textObject, textToType, charDelay));
            typingCoroutines[textObject] = newCoroutine;
        }


        private IEnumerator TypeTextCoroutine(TextMeshProUGUI textObject, string text, float characterDelay)
        {
            textObject.SetText("");

            StringBuilder builder = new();
            bool insideTag = false;

            foreach (char c in text)
            {
                // Handle rich text tags (e.g., <b>, </i>)
                if (c == '<') insideTag = true;

                builder.Append(c);

                if (c == '>') insideTag = false;

                textObject.SetText(builder.ToString());

                if (!insideTag && !char.IsWhiteSpace(c))
                    yield return new WaitForSeconds(characterDelay);
            }

            typingCoroutines.Remove(textObject);
        }
    }
}