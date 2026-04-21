using TMPro;
using UnityEngine;

public class FontObject : MonoBehaviour
{
    private TextMeshProUGUI text;

    private void Awake()
    {
        text = GetComponent<TextMeshProUGUI>();
        FontManager.Instance.RegisterText(this);
    }

    private void OnDestroy()
    {
        FontManager.Instance.UnregisterText(this);
    }

    public void ChangeFont(TMP_FontAsset font, float fontScaleDiffrence)
    {
        text.font = font;
        text.fontSize += fontScaleDiffrence;
    }
}