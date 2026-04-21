using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class FontManager : Singleton<FontManager>
{
    [SerializeField] TMP_FontAsset readableFont;
    [SerializeField] TMP_FontAsset fancyFont;
    private List<FontObject> texts = new List<FontObject>();
    private bool readableFontActive = false;

    public void RegisterText(FontObject text)
    {
        texts.Add(text);
        if (readableFontActive) text.ChangeFont(readableFont, 2f);
        else text.ChangeFont(fancyFont, 0f);

    }
    public void UnregisterText(FontObject text)
    {
        texts.Remove(text);
    }

    public void ChangeFont()
    {
        if (readableFontActive)
        {
            foreach (var text in texts) text.ChangeFont(fancyFont, -2f);
            readableFontActive = false;
        }
        else
        {
            foreach (var text in texts) text.ChangeFont(readableFont, 2f);
            readableFontActive = true;
        }
    }
}
