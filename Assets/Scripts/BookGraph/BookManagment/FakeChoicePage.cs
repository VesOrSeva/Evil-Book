using System.Collections;
using System.Collections.Generic;
using System.Text;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;
using static System.Net.Mime.MediaTypeNames;

namespace BookGraph.Runtime
{
    public class FakeChoicePage : MonoBehaviour
    {
        [SerializeField] GameObject optionsContainer;
        [SerializeField] GameObject optionButtonPrefab;
        [SerializeField] GameObject pageText;
        [SerializeField] TextMeshProUGUI pageNumber;

        private readonly Dictionary<TextMeshProUGUI, Coroutine> typingCoroutines = new();
        private readonly List<GameObject> spawnedButtons = new();

        private B_OptionsPage optionsPage;

        public void Initialize(RuntimeNode node, GameObject originalPage)
        {
            if (node is not RuntimeChoicePageNode choicePage) return;
            ClearOptions();

            optionsPage = originalPage.GetComponent<B_OptionsPage>();
            if (optionsPage == null)
            {
                Debug.LogWarning("No Options Page!");
                return;
            }

            if (optionsPage.WasWritten) pageText.GetComponent<TextMeshProUGUI>().text = choicePage.PageText;
            else TypeText(pageText.GetComponent<TextMeshProUGUI>(), choicePage.PageText);
            pageNumber.text = choicePage.TargetPage.ToString();

            for (int i = 0; i < choicePage.Choices.Count; i++)
            {
                var choice = choicePage.Choices[i];

                var buttonGO = Instantiate(optionButtonPrefab, optionsContainer.transform);
                spawnedButtons.Add(buttonGO);

                var button = buttonGO.GetComponent<Button>();
                var text = buttonGO.GetComponentInChildren<TextMeshProUGUI>();
                if (text != null)
                {
                    if (optionsPage.WasWritten) text.text = choice.ChoiceText;
                    else TypeText(text, choice.ChoiceText, 0.05f);
                }

                string nextNodeId = choice.NextNodeId;
                int index = i;

                button.onClick.RemoveAllListeners();
                button.onClick.AddListener(() =>
                {
                    OnChoiceSelected(nextNodeId, index);
                });
            }

            if (choicePage.PageLayout == 0)
            {
                optionsContainer.transform.SetAsFirstSibling();
            }

            optionsPage.Written();
        }

        private void OnChoiceSelected(string nodeId, int buttonIndex)
        {
            optionsPage.UsedPage(buttonIndex);
            BookManager.Instance.GoToNode(nodeId);
            Destroy(gameObject);
        }

        private void ClearOptions()
        {
            foreach (var btn in spawnedButtons)
            {
                if (btn != null) Destroy(btn);
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
            text = text.Replace("\\n", "\n");

            StringBuilder builder = new();
            bool insideTag = false;

            float timer = 0f;
            int index = 0;

            while (index < text.Length)
            {
                timer += Time.deltaTime;

                while (timer >= characterDelay && index < text.Length)
                {
                    char c = text[index++];

                    if (c == '<') insideTag = true;

                    builder.Append(c);

                    if (c == '>') insideTag = false;

                    if (!insideTag && !char.IsWhiteSpace(c))
                        timer -= characterDelay;
                }

                textObject.SetText(builder.ToString());
                yield return null;
            }

            typingCoroutines.Remove(textObject);
        }
    }
}