using System.Collections;
using System.Collections.Generic;
using System.Text;
using TMPro;
using UnityEngine;

namespace BookGraph.Runtime
{
    public class B_Page : MonoBehaviour
    {
        public virtual void WriteThePage(RuntimeNode node) { }

        public virtual void OnPageOpened(RuntimeNode node, RenderingPageType type) { }

        public virtual void OnPageVisible(RuntimeNode node) { }


        private readonly Dictionary<TextMeshProUGUI, Coroutine> typingCoroutines = new();

        public void TypeText(TextMeshProUGUI textObject, string textToType, float charDelay = 0.02f)
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