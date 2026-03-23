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


        private readonly Dictionary<TextMeshProUGUI, Coroutine> typingCoroutines = new();

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