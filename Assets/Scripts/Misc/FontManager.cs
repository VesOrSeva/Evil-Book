using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class FontManager : Singleton<FontManager>
{
    [SerializeField] TMP_FontAsset readableFont;
    [SerializeField] TMP_FontAsset fancyFont;
    private List<FontObject> texts = new List<FontObject>();
    private bool readableFontActive = true;

    public void RegisterText(FontObject text)
    {
        texts.Add(text);
        if (readableFontActive) text.ChangeFont(readableFont);
        else text.ChangeFont(fancyFont);

    }
    public void UnregisterText(FontObject text)
    {
        texts.Remove(text);
    }

    public void ChangeFont()
    {
        if (readableFontActive)
        {
            foreach (var text in texts) text.ChangeFont(fancyFont);
            readableFontActive = false;
        }
        else
        {
            foreach (var text in texts) text.ChangeFont(readableFont);
            readableFontActive = true;
        }
    }
}
