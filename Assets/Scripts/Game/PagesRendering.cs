using BookGraph.Runtime;
using UnityEngine;

public class PagesRendering : Singleton<PagesRendering>
{
    [Header("Parents")]
    [SerializeField] Transform leftFrontPageParent;
    [SerializeField] Transform leftBackPageParent;
    [SerializeField] Transform rightFrontPageParent;
    [SerializeField] Transform rightBackPageParent;

    GameObject currentLeftFront;
    GameObject currentLeftBack;
    GameObject currentRightFront;
    GameObject currentRightBack;

    public void SpawnPage(PageEntry entry, RenderingPageType type)
    {
        Transform parent = GetParent(type);
        Clear(type);

        if (entry == null || entry.Prefab == null) return;

        GameObject instance = Instantiate(entry.Prefab, parent);
        instance.transform.localPosition = Vector3.zero;
        instance.transform.localRotation = Quaternion.identity;

        var page = instance.GetComponent<B_Page>();
        if (page != null) page.WriteThePage(entry.Node);

        SetReference(type, instance);
    }

    public void Clear(RenderingPageType type)
    {
        GameObject obj = GetReference(type);
        if (obj != null)
            Destroy(obj);

        SetReference(type, null);
    }

    public void ClearAll()
    {
        foreach (RenderingPageType type in System.Enum.GetValues(typeof(RenderingPageType)))
            Clear(type);
    }

    Transform GetParent(RenderingPageType type)
    {
        return type switch
        {
            RenderingPageType.LeftFront => leftFrontPageParent,
            RenderingPageType.LeftBack => leftBackPageParent,
            RenderingPageType.RightFront => rightFrontPageParent,
            RenderingPageType.RightBack => rightBackPageParent,
            _ => null
        };
    }

    GameObject GetReference(RenderingPageType type)
    {
        return type switch
        {
            RenderingPageType.LeftFront => currentLeftFront,
            RenderingPageType.LeftBack => currentLeftBack,
            RenderingPageType.RightFront => currentRightFront,
            RenderingPageType.RightBack => currentRightBack,
            _ => null
        };
    }

    void SetReference(RenderingPageType type, GameObject obj)
    {
        switch (type)
        {
            case RenderingPageType.LeftFront: currentLeftFront = obj; break;
            case RenderingPageType.LeftBack: currentLeftBack = obj; break;
            case RenderingPageType.RightFront: currentRightFront = obj; break;
            case RenderingPageType.RightBack: currentRightBack = obj; break;
        }
    }
}

public enum RenderingPageType
{
    LeftFront,
    LeftBack,
    RightFront,
    RightBack
}