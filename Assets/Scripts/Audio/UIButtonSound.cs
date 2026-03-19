using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class UIButtonSound : MonoBehaviour, IPointerEnterHandler
{
    private void Awake()
    {
        GetComponent<Button>().onClick.AddListener(PlaySound);
    }
    public void PlaySound()
    {

    }
    public void OnPointerEnter(PointerEventData eventData)
    {
        HoverSound();
    }
    public void HoverSound()
    {

    }
}
