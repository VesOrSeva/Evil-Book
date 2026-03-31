using BookGraph.Runtime;
using System.Collections.Generic;
using UnityEngine;

public class PagesRendering : Singleton<PagesRendering>
{
    [Header("Parents")]
    [SerializeField] Transform leftFrontPageParent;
    [SerializeField] Transform leftBackPageParent;
    [SerializeField] Transform rightFrontPageParent;
    [SerializeField] Transform rightBackPageParent;
    [SerializeField] Transform hiddenPagesParent;

    GameObject currentLeftFront;
    GameObject currentLeftBack;
    GameObject currentRightFront;
    GameObject currentRightBack;

    public void SpawnPage(PageEntry entry, RenderingPageType type, bool flipped = true)
    {
        Transform parent = GetParent(type);
        if (parent == null) return;

        GameObject current = GetReference(type);

        if (entry == null || entry.Prefab == null)
        {
            if (current != null) MoveToHidden(current);
            SetReference(type, null);
            return;
        }

        GameObject instance = entry.GetOrCreateInstance(parent);
        if (current != null && current != instance)
        {
            if (!IsInstanceUsedElsewhere(current, type))
            {
                MoveToHidden(current);
            }
        }

        B_Page page = instance.GetComponent<B_Page>();
        if (flipped) page?.OnPageOpened(entry.Node, type);
        else page?.OnPageVisible(entry.Node);

        instance.transform.SetParent(parent, false);
        instance.transform.localPosition = Vector3.zero;
        instance.transform.localRotation = Quaternion.identity;

        SetReference(type, instance);
    }

    bool IsInstanceUsedElsewhere(GameObject obj, RenderingPageType currentType)
    {
        return
            (currentType != RenderingPageType.LeftFront && currentLeftFront == obj) ||
            (currentType != RenderingPageType.LeftBack && currentLeftBack == obj) ||
            (currentType != RenderingPageType.RightFront && currentRightFront == obj) ||
            (currentType != RenderingPageType.RightBack && currentRightBack == obj);
    }

    void MoveToHidden(GameObject obj)
    {
        if (obj == null) return;

        obj.transform.SetParent(hiddenPagesParent, false);
        obj.transform.localPosition = Vector3.zero;
        obj.transform.localRotation = Quaternion.identity;
    }

    public void Clear(RenderingPageType type)
    {
        GameObject obj = GetReference(type);
        if (obj != null) MoveToHidden(obj);

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