using BookGraph.Runtime;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public enum FlipMode
{
    RightToLeft,
    LeftToRight
}

public class Book : MonoBehaviour 
{
    [Header("Pages")]
    [SerializeField] List<PageEntry> pages = new();
    [SerializeField] List<PageEntry> frontPages = new(2);
    [SerializeField] List<PageEntry> backPages = new(2);
    [SerializeField] List<PageCondition> conditions = new();
    [SerializeField] float flipDuration = 0.2f;

    [Header("Page Stacks")]
    [SerializeField] Transform leftStack;
    [SerializeField] Transform rightStack;
    [SerializeField] float thicknessPerPage = 5f;
    [SerializeField] float baseThickness = 5f;
    [SerializeField] float uiLiftPerPage = 0.005f;

    PageEntry GetPage(int index)
    {
        // Main book pages
        if (index >= 0 && index < pages.Count) return pages[index];

        // Front pages
        if (index < 0)
        {
            int frontIndex = frontPages.Count + index;
            if (frontIndex >= 0 && frontIndex < frontPages.Count) return frontPages[frontIndex];
        }

        // Back pages
        if (index >= pages.Count)
        {
            int backIndex = index - pages.Count;
            if (backIndex >= 0 && backIndex < backPages.Count) return backPages[backIndex];
        }

        return null;
    }

    int GetLeftPageIndex()
    {
        return currentPage - (currentPage % 2);
    }

    #region Page Fliping <-------

    [Header("UI")]
    public Canvas canvas;
    [SerializeField] RectTransform BookPanel;
    public Sprite background;
    int interactionLocks = 0;
    public bool IsInteractable => interactionLocks == 0;
    public bool IsAutoFlipping = false;
    public bool enableShadowEffect = true;
    //represent the index of the sprite shown in the right page
    public int currentPage = 0;
    int MinPageIndex => -frontPages.Count;
    int MaxPageIndex => pages.Count + backPages.Count - 1;
    public int TotalPageCount => pages.Count;
    public Vector3 EndBottomLeft
    {
        get { return ebl; }
    }
    public Vector3 EndBottomRight
    {
        get { return ebr; }
    }
    public float Height
    {
        get
        {
            return BookPanel.rect.height ; 
        }
    }

    public Image ClippingPlane;
    public Image NextPageClip;
    public Image Shadow;
    public Image ShadowLTR;
    public RawImage Left;
    public RawImage LeftNext;
    public RawImage Right;
    public RawImage RightNext;
    public Transform RightHotSpot;
    public Transform LeftHotSpot;
    public float shadowsMaxAlpha = 0.8f;

    public UnityEvent OnFlip;
    float radius1, radius2;
    //Spine Bottom
    Vector3 sb;
    //Spine Top
    Vector3 st;
    //corner of the page
    Vector3 c;
    //Edge Bottom Right
    Vector3 ebr;
    //Edge Bottom Left
    Vector3 ebl;
    //follow point 
    Vector3 f;
    bool pageDragging = false;
    //current flip mode
    FlipMode mode;

    private Vector3 leftStackStartingScale;
    private Vector3 rightStackStartingScale;

    private Vector3 leftBasePos;
    private Vector3 leftNextBasePos;
    private Vector3 rightBasePos;
    private Vector3 rightNextBasePos;

    Vector3 leftDepthOffset;
    Vector3 rightDepthOffset;

    Vector3 nextPageOffsetLTR;
    Vector3 nextPageOffsetRTL;

    Vector3 leftTargetScale;
    Vector3 rightTargetScale;

    Vector3 leftTargetOffset;
    Vector3 rightTargetOffset;

    private Coroutine thicknessCoroutine;

    public void LockInteraction()
    {
        interactionLocks++;
    }

    public void UnlockInteraction()
    {
        interactionLocks = Mathf.Max(0, interactionLocks - 1);
        if (interactionLocks == 0) pageDragging = false;
    }
    public void ForceLock()
    {
        interactionLocks = int.MaxValue;
    }

    public void ForceUnlock()
    {
        interactionLocks = 0;
        pageDragging = false;
    }

    void Start()
    {
        if (!canvas) canvas = GetComponentInParent<Canvas>();
        if (!canvas) Debug.LogError("Book should be a child to canvas");

        leftStackStartingScale = leftStack.transform.localScale;
        rightStackStartingScale = rightStack.transform.localScale;

        leftBasePos = Left.transform.position;
        leftNextBasePos = LeftNext.transform.position;

        rightBasePos = Right.transform.position;
        rightNextBasePos = RightNext.transform.position;

        Left.gameObject.SetActive(false);
        Right.gameObject.SetActive(false);
        UpdateRenderedPages();
        CalcCurlCriticalPoints();

        float pageWidth = BookPanel.rect.width / 2.0f;
        float pageHeight = BookPanel.rect.height;
        NextPageClip.rectTransform.sizeDelta = new Vector2(pageWidth, pageHeight + pageHeight * 2);


        ClippingPlane.rectTransform.sizeDelta = new Vector2(pageWidth * 2 + pageHeight, pageHeight + pageHeight * 2);

        //hypotenous (diagonal) page length
        float hyp = Mathf.Sqrt(pageWidth * pageWidth + pageHeight * pageHeight);
        float shadowPageHeight = pageWidth / 2 + hyp;

        Shadow.rectTransform.sizeDelta = new Vector2(pageWidth, shadowPageHeight);
        Shadow.rectTransform.pivot = new Vector2(1, (pageWidth / 2) / shadowPageHeight);

        ShadowLTR.rectTransform.sizeDelta = new Vector2(pageWidth, shadowPageHeight);
        ShadowLTR.rectTransform.pivot = new Vector2(0, (pageWidth / 2) / shadowPageHeight);

        UpdateThickness();
    }

    private void CalcCurlCriticalPoints()
    {
        sb = new Vector3(0, -BookPanel.rect.height / 2);
        ebr = new Vector3(BookPanel.rect.width / 2, -BookPanel.rect.height / 2);
        ebl = new Vector3(-BookPanel.rect.width / 2, -BookPanel.rect.height / 2);
        st = new Vector3(0, BookPanel.rect.height / 2);
        radius1 = Vector2.Distance(sb, ebr);
        float pageWidth = BookPanel.rect.width / 2.0f;
        float pageHeight = BookPanel.rect.height;
        radius2 = Mathf.Sqrt(pageWidth * pageWidth + pageHeight * pageHeight);
    }

    public Vector3 transformPoint(Vector3 mouseScreenPos)
    {
        if (canvas.renderMode == RenderMode.ScreenSpaceCamera)
        {
            Vector3 mouseWorldPos = canvas.worldCamera.ScreenToWorldPoint(new Vector3(mouseScreenPos.x, mouseScreenPos.y, canvas.planeDistance));
            Vector2 localPos = BookPanel.InverseTransformPoint(mouseWorldPos);

            return localPos;
        }
        else if (canvas.renderMode == RenderMode.WorldSpace)
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            Vector3 globalEBR = transform.TransformPoint(ebr);
            Vector3 globalEBL = transform.TransformPoint(ebl);
            Vector3 globalSt = transform.TransformPoint(st);
            Plane p = new Plane(globalEBR, globalEBL, globalSt);
            float distance;
            p.Raycast(ray, out distance);
            Vector2 localPos = BookPanel.InverseTransformPoint(ray.GetPoint(distance));
            return localPos;
        }
        else
        {
            //Screen Space Overlay
            Vector2 localPos = BookPanel.InverseTransformPoint(mouseScreenPos);
            return localPos;
        }
    }
    void Update()
    {
        if (!IsInteractable) return;

        if (pageDragging)
        {
            UpdateBook();
        }
    }
    public void UpdateBook()
    {
        if (IsAutoFlipping) return;

        f = Vector3.Lerp(f, transformPoint(Input.mousePosition), Time.deltaTime * 10);
        if (mode == FlipMode.RightToLeft) UpdateBookRTLToPoint(f);
        else UpdateBookLTRToPoint(f);
    }
    public void UpdateBookLTRToPoint(Vector3 followLocation)
    {
        mode = FlipMode.LeftToRight;
        f = followLocation;
        ShadowLTR.transform.SetParent(ClippingPlane.transform, true);
        ShadowLTR.transform.localPosition = new Vector3(0, 0, 0);
        ShadowLTR.transform.localEulerAngles = new Vector3(0, 0, 0);
        Left.transform.SetParent(ClippingPlane.transform, true);

        SetLocalZ(Right.transform, leftTargetOffset.z);
        Right.transform.SetParent(BookPanel.transform, true);
        Right.transform.localEulerAngles = Vector3.zero;
        LeftNext.transform.SetParent(BookPanel.transform, true);

        c = Calc_C_Position(followLocation);
        Vector3 t1;
        float clipAngle = CalcClipAngle(c, ebl, out t1);
        // 0 < T0_T1_Angle < 180
        clipAngle = (clipAngle + 180) % 180;

        ClippingPlane.transform.localEulerAngles = new Vector3(0, 0, clipAngle - 90);
        ClippingPlane.transform.position = BookPanel.TransformPoint(t1) + leftTargetOffset;

        // shadows
        float progress = GetFlipProgressLTR();
        UpdateShadowFade(ShadowLTR, progress);

        // page position and angle
        Left.transform.position = BookPanel.TransformPoint(c) + leftTargetOffset;
        float C_T1_dy = t1.y - c.y;
        float C_T1_dx = t1.x - c.x;
        float C_T1_Angle = Mathf.Atan2(C_T1_dy, C_T1_dx) * Mathf.Rad2Deg;
        Left.transform.localEulerAngles = new Vector3(0, 0, C_T1_Angle - 90 - clipAngle);

        NextPageClip.transform.localEulerAngles = new Vector3(0, 0, clipAngle - 90);
        NextPageClip.transform.position = BookPanel.TransformPoint(t1) + nextPageOffsetLTR;
        LeftNext.transform.SetParent(NextPageClip.transform, true);
        SetLocalZ(LeftNext.transform, 0);

        Right.transform.SetParent(ClippingPlane.transform, true);
        Right.transform.SetAsFirstSibling();

        ShadowLTR.rectTransform.SetParent(Left.rectTransform, true);
    }
    public void UpdateBookRTLToPoint(Vector3 followLocation)
    {
        mode = FlipMode.RightToLeft;
        f = followLocation;
        Shadow.transform.SetParent(ClippingPlane.transform, true);
        Shadow.transform.localPosition = Vector3.zero;
        Shadow.transform.localEulerAngles = Vector3.zero;
        Right.transform.SetParent(ClippingPlane.transform, true);

        SetLocalZ(Left.transform, rightTargetOffset.z);
        Left.transform.SetParent(BookPanel.transform, true);
        Left.transform.localEulerAngles = Vector3.zero;
        RightNext.transform.SetParent(BookPanel.transform, true);
        c = Calc_C_Position(followLocation);
        Vector3 t1;
        float clipAngle = CalcClipAngle(c, ebr, out t1);
        if (clipAngle > -90) clipAngle += 180;

        ClippingPlane.rectTransform.pivot = new Vector2(1, 0.35f);
        ClippingPlane.transform.localEulerAngles = new Vector3(0, 0, clipAngle + 90);
        ClippingPlane.transform.position = BookPanel.TransformPoint(t1) + rightTargetOffset;

        // shadows
        float progress = GetFlipProgressRTL();
        UpdateShadowFade(Shadow, progress);

        // page position and angle
        Right.transform.position = BookPanel.TransformPoint(c) + rightTargetOffset;
        float C_T1_dy = t1.y - c.y;
        float C_T1_dx = t1.x - c.x;
        float C_T1_Angle = Mathf.Atan2(C_T1_dy, C_T1_dx) * Mathf.Rad2Deg;
        Right.transform.localEulerAngles = new Vector3(0, 0, C_T1_Angle - (clipAngle + 90));

        NextPageClip.transform.localEulerAngles = new Vector3(0, 0, clipAngle + 90);
        NextPageClip.transform.position = BookPanel.TransformPoint(t1) + nextPageOffsetRTL;
        RightNext.transform.SetParent(NextPageClip.transform, true);
        SetLocalZ(RightNext.transform, 0);

        Left.transform.SetParent(ClippingPlane.transform, true);
        Left.transform.SetAsFirstSibling();

        Shadow.rectTransform.SetParent(Right.rectTransform, true);
    }
    private float CalcClipAngle(Vector3 c,Vector3 bookCorner,out  Vector3 t1)
    {
        Vector3 t0 = (c + bookCorner) / 2;
        float T0_CORNER_dy = bookCorner.y - t0.y;
        float T0_CORNER_dx = bookCorner.x - t0.x;
        float T0_CORNER_Angle = Mathf.Atan2(T0_CORNER_dy, T0_CORNER_dx);
        float T0_T1_Angle = 90 - T0_CORNER_Angle;
        
        float T1_X = t0.x - T0_CORNER_dy * Mathf.Tan(T0_CORNER_Angle);
        T1_X = normalizeT1X(T1_X, bookCorner, sb);
        t1 = new Vector3(T1_X, sb.y, 0);
        
        //clipping plane angle=T0_T1_Angle
        float T0_T1_dy = t1.y - t0.y;
        float T0_T1_dx = t1.x - t0.x;
        T0_T1_Angle = Mathf.Atan2(T0_T1_dy, T0_T1_dx) * Mathf.Rad2Deg;
        return T0_T1_Angle;
    }
    private float normalizeT1X(float t1,Vector3 corner,Vector3 sb)
    {
        if (t1 > sb.x && sb.x > corner.x) return sb.x;
        if (t1 < sb.x && sb.x < corner.x) return sb.x;

        return t1;
    }
    private Vector3 Calc_C_Position(Vector3 followLocation)
    {
        Vector3 c;
        f = followLocation;
        float F_SB_dy = f.y - sb.y;
        float F_SB_dx = f.x - sb.x;
        float F_SB_Angle = Mathf.Atan2(F_SB_dy, F_SB_dx);
        Vector3 r1 = new Vector3(radius1 * Mathf.Cos(F_SB_Angle),radius1 * Mathf.Sin(F_SB_Angle), 0) + sb;

        float F_SB_distance = Vector2.Distance(f, sb);
        if (F_SB_distance < radius1) c = f;
        else c = r1;

        float F_ST_dy = c.y - st.y;
        float F_ST_dx = c.x - st.x;
        float F_ST_Angle = Mathf.Atan2(F_ST_dy, F_ST_dx);
        Vector3 r2 = new Vector3(radius2 * Mathf.Cos(F_ST_Angle), radius2 * Mathf.Sin(F_ST_Angle), 0) + st;
        float C_ST_distance = Vector2.Distance(c, st);

        if (C_ST_distance > radius2) c = r2;
        return c;
    }
    public void DragRightPageToPoint(Vector3 point)
    {
        if (GetLeftPageIndex() + 2 > MaxPageIndex) return;
        PrepareRTLPages();
        UpdateThicknessImmediate();

        pageDragging = true;
        mode = FlipMode.RightToLeft;
        f = point;

        NextPageClip.rectTransform.pivot = new Vector2(0, 0.12f);
        ClippingPlane.rectTransform.pivot = new Vector2(1, 0.35f);

        Left.gameObject.SetActive(true);
        Left.rectTransform.pivot = new Vector2(0, 0);
        Left.transform.position = RightNext.transform.position + leftTargetOffset;
        Left.transform.eulerAngles = new Vector3(0, 0, 0);
        Left.transform.SetAsFirstSibling();
        
        Right.gameObject.SetActive(true);
        Right.transform.position = RightNext.transform.position + rightTargetOffset;
        Right.transform.eulerAngles = new Vector3(0, 0, 0);

        LeftNext.transform.SetAsFirstSibling();
        if (enableShadowEffect) Shadow.gameObject.SetActive(true);
        UpdateBookRTLToPoint(f);
    }
    public void OnMouseDragRightPage()
    {
        if (!IsInteractable || IsAutoFlipping) return;
        if (GetLeftPageIndex() + 2 > MaxPageIndex) return;
        if (thicknessCoroutine != null) StopCoroutine(thicknessCoroutine);
        DragRightPageToPoint(transformPoint(Input.mousePosition));
        AudioManager.Instance.PlayRandomSound(AudioBundle.Instance.GetFlipingStartClips, 0.7f);

    }
    public void DragLeftPageToPoint(Vector3 point)
    {
        if (GetLeftPageIndex() - 2 < MinPageIndex) return;
        PrepareLTRPages();
        UpdateThicknessImmediate();

        pageDragging = true;
        mode = FlipMode.LeftToRight;
        f = point;

        NextPageClip.rectTransform.pivot = new Vector2(1, 0.12f);
        ClippingPlane.rectTransform.pivot = new Vector2(0, 0.35f);

        Right.gameObject.SetActive(true);
        Right.transform.position = LeftNext.transform.position + rightTargetOffset;
        Right.transform.eulerAngles = new Vector3(0, 0, 0);
        Right.transform.SetAsFirstSibling();

        Left.gameObject.SetActive(true);
        Left.rectTransform.pivot = new Vector2(1, 0);
        Left.transform.position = LeftNext.transform.position + leftTargetOffset;
        Left.transform.eulerAngles = new Vector3(0, 0, 0);

        RightNext.transform.SetAsFirstSibling();
        if (enableShadowEffect) ShadowLTR.gameObject.SetActive(true);
        UpdateBookLTRToPoint(f);
    }
    public void OnMouseDragLeftPage()
    {
        if (!IsInteractable || IsAutoFlipping) return;
        if (GetLeftPageIndex() - 2 < MinPageIndex) return;
        if (thicknessCoroutine != null) StopCoroutine(thicknessCoroutine);
        DragLeftPageToPoint(transformPoint(Input.mousePosition));
        AudioManager.Instance.PlayRandomSound(AudioBundle.Instance.GetFlipingStartClips, 0.7f);       
    }
    public void OnMouseRelease()
    {
        if (!IsInteractable || IsAutoFlipping) return;
        ReleasePage();
    }
    public void ReleasePage()
    {
        if (pageDragging)
        {
            pageDragging = false;
            float distanceToLeft = Vector2.Distance(c, ebl);
            float distanceToRight = Vector2.Distance(c, ebr);
            if (distanceToRight < distanceToLeft && mode == FlipMode.RightToLeft) TweenBack();
            else if (distanceToRight > distanceToLeft && mode == FlipMode.LeftToRight) TweenBack();
            else TweenForward();
        }
    }
    Coroutine currentCoroutine;

    void UpdateRenderedPages()
    {
        B_OnPageContentManager.Instance.ClearPages();
        var renderer = PagesRendering.Instance;

        int left = GetLeftPageIndex();
        int right = left + 1;

        renderer.SpawnPage(GetPage(left), RenderingPageType.LeftFront);
        renderer.SpawnPage(GetPage(right), RenderingPageType.RightFront);
        renderer.SpawnPage(GetPage(left - 2), RenderingPageType.LeftBack);
        renderer.SpawnPage(GetPage(right + 1), RenderingPageType.RightBack);
    }

    void PrepareRTLPages()
    {
        Debug.Log("To Right");
        B_OnPageContentManager.Instance.ClearPages();
        var renderer = PagesRendering.Instance;

        int left = GetLeftPageIndex();
        int nextSpread = CheckSkippedPages(left + 2, true);

        renderer.SpawnPage(GetPage(left), RenderingPageType.LeftFront, false);
        renderer.SpawnPage(GetPage(nextSpread + 1), RenderingPageType.RightFront, false);
        renderer.SpawnPage(GetPage(left + 1), RenderingPageType.RightBack, false);
        renderer.SpawnPage(GetPage(nextSpread), RenderingPageType.LeftBack, false);
    }

    void PrepareLTRPages()
    {
        Debug.Log("To Left");
        B_OnPageContentManager.Instance.ClearPages();
        var renderer = PagesRendering.Instance;

        int left = GetLeftPageIndex();
        int nextSpread = CheckSkippedPages(left - 2, false);

        renderer.SpawnPage(GetPage(left), RenderingPageType.LeftBack, false);
        renderer.SpawnPage(GetPage(nextSpread), RenderingPageType.LeftFront, false);
        renderer.SpawnPage(GetPage(left + 1), RenderingPageType.RightFront, false);
        renderer.SpawnPage(GetPage(nextSpread + 1), RenderingPageType.RightBack, false);
    }

    public void TweenForward()
    {
        if (mode == FlipMode.RightToLeft)
        {
            if (currentCoroutine != null) StopCoroutine(currentCoroutine);
            currentCoroutine = StartCoroutine(TweenTo(ebl, () => { Flip(); }));
        }
        else
        {
            if (currentCoroutine != null) StopCoroutine(currentCoroutine);
            currentCoroutine = StartCoroutine(TweenTo(ebr, () => { Flip(); }));
        }
    }

    void Flip()
    {
        int direction = (mode == FlipMode.RightToLeft) ? 2 : -2;
        int targetPage = currentPage + direction;

        targetPage = CheckSkippedPages(targetPage, direction > 0);
        currentPage = Mathf.Clamp(targetPage, MinPageIndex, MaxPageIndex);
        PagesRendering.Instance.ClearAll();

        LeftNext.transform.SetParent(BookPanel.transform, true);
        Left.transform.SetParent(BookPanel.transform, true);
        LeftNext.transform.SetParent(BookPanel.transform, true);
        Left.gameObject.SetActive(false);
        Right.gameObject.SetActive(false);
        Right.transform.SetParent(BookPanel.transform, true);
        RightNext.transform.SetParent(BookPanel.transform, true);

        Shadow.gameObject.SetActive(false);
        ShadowLTR.gameObject.SetActive(false);

        UpdateThickness();
        UpdateRenderedPages();
        if (OnFlip != null) OnFlip.Invoke();
    }

    public void TweenBack()
    {
        if (mode == FlipMode.RightToLeft)
        {
            currentCoroutine = StartCoroutine(TweenTo(ebr,
                () =>
                {
                    PagesRendering.Instance.ClearAll();
                    UpdateRenderedPages();

                    RightNext.transform.SetParent(BookPanel.transform); // I need to set it the same position as Right for it on z <--
                    Right.transform.SetParent(BookPanel.transform);

                    Left.gameObject.SetActive(false);
                    Right.gameObject.SetActive(false);
                    pageDragging = false;
                    currentCoroutine = null;

                    SetLocalZ(RightNext.transform, Right.transform.localPosition.z);
                }
                ));
        }
        else
        {
            currentCoroutine = StartCoroutine(TweenTo(ebl,
                () =>
                {
                    PagesRendering.Instance.ClearAll();
                    UpdateRenderedPages();

                    LeftNext.transform.SetParent(BookPanel.transform); // I need to set it the same position as Left for it on z <--
                    Left.transform.SetParent(BookPanel.transform);

                    Left.gameObject.SetActive(false);
                    Right.gameObject.SetActive(false);
                    pageDragging = false;
                    currentCoroutine = null;

                    SetLocalZ(LeftNext.transform, Left.transform.localPosition.z);
                }
                ));
        }
    }
    public IEnumerator TweenTo(Vector3 to, System.Action onFinish)
    {
        LockInteraction();
        AudioManager.Instance.PlayRandomSound(AudioBundle.Instance.GetFlipingEndClips, 0.7f);

        int steps = (int)(flipDuration / 0.015f);
        Vector3 displacement = (to - f) / steps;
        for (int i = 0; i < steps-1; i++)
        {
            if (mode == FlipMode.RightToLeft)
            UpdateBookRTLToPoint( f + displacement);
            else UpdateBookLTRToPoint(f + displacement);

            yield return new WaitForSeconds(0.015f);
        }

        if (onFinish != null) onFinish();
        UnlockInteraction();
    }

    #endregion

    #region Shadows

    float GetFlipProgressRTL()
    {
        float total = Vector2.Distance(ebr, ebl);
        float current = Vector2.Distance(c, ebr);
        return Mathf.Clamp01(current / total);
    }

    float GetFlipProgressLTR()
    {
        float total = Vector2.Distance(ebr, ebl);
        float current = Vector2.Distance(c, ebl);
        return Mathf.Clamp01(current / total);
    }

    void UpdateShadowFade(Image shadow, float progress)
    {
        if (!enableShadowEffect) return;
        Color col = shadow.color;
        col.a = Mathf.Lerp(shadowsMaxAlpha, 0f, progress);
        shadow.color = col;
    }

    #endregion

    #region Thickness

    void UpdateThicknessImmediate()
    {
        int totalPages = TotalPageCount + frontPages.Count + backPages.Count;
        int leftPages = Mathf.Clamp(currentPage + frontPages.Count, 0, totalPages);
        int rightPages = totalPages - leftPages;

        Vector3 depthDir = BookPanel.forward;

        nextPageOffsetLTR = depthDir * (leftPages - 2) * -uiLiftPerPage;
        nextPageOffsetRTL = depthDir * (rightPages - 2) * -uiLiftPerPage;
    }

    void UpdateThickness()
    {
        int totalPages = TotalPageCount + frontPages.Count + backPages.Count;
        int leftPages = Mathf.Clamp(currentPage + frontPages.Count, 0, totalPages);
        int rightPages = totalPages - leftPages;

        float leftHeight = baseThickness + leftPages * thicknessPerPage;
        float rightHeight = baseThickness + rightPages * thicknessPerPage;

        leftTargetScale = new Vector3(leftStackStartingScale.x, leftStackStartingScale.y, leftHeight);
        rightTargetScale = new Vector3(rightStackStartingScale.x, rightStackStartingScale.y, rightHeight);

        Vector3 depthDir = BookPanel.forward;

        leftTargetOffset = depthDir * leftPages * -uiLiftPerPage;
        rightTargetOffset = depthDir * rightPages * -uiLiftPerPage;

        if (thicknessCoroutine != null) StopCoroutine(thicknessCoroutine);
        thicknessCoroutine = StartCoroutine(AnimateThickness());
    }

    IEnumerator AnimateThickness(float duration = 0.17f)
    {
        float time = 0f;

        Vector3 leftStartScale = leftStack.localScale;
        Vector3 rightStartScale = rightStack.localScale;

        Vector3 leftStartOffset;
        Vector3 rightStartOffset;

        if (mode == FlipMode.RightToLeft)
        {
            leftStartOffset = rightDepthOffset;
            rightStartOffset = leftDepthOffset;
        }
        else
        {
            leftStartOffset = leftDepthOffset;
            rightStartOffset = rightDepthOffset;
        }


        while (time < duration)
        {
            float t = time / duration;
            t = Mathf.SmoothStep(0, 1, t);

            if (mode == FlipMode.RightToLeft)
            {
                leftStack.localScale = Vector3.Lerp(leftStartScale, leftTargetScale, t);
                leftDepthOffset = Vector3.Lerp(leftStartOffset, leftTargetOffset, t);

                Left.transform.position = leftBasePos + leftDepthOffset;
                LeftNext.transform.position = leftNextBasePos + leftDepthOffset;

                Right.transform.position = rightBasePos + rightTargetOffset;
                RightNext.transform.position = rightNextBasePos + rightTargetOffset;
                rightStack.localScale = rightTargetScale;
            }
            else
            {
                rightStack.localScale = Vector3.Lerp(rightStartScale, rightTargetScale, t);
                rightDepthOffset = Vector3.Lerp(rightStartOffset, rightTargetOffset, t);

                Right.transform.position = rightBasePos + rightDepthOffset;
                RightNext.transform.position = rightNextBasePos + rightDepthOffset;

                Left.transform.position = leftBasePos + leftTargetOffset;
                LeftNext.transform.position = leftNextBasePos + leftTargetOffset;
                leftStack.localScale = leftTargetScale;
            }

            time += Time.deltaTime;
            yield return null;
        }

        SetLocalZ(RightHotSpot, rightTargetOffset.z * 1600f);
        SetLocalZ(LeftHotSpot, leftTargetOffset.z * 1600f);

        leftStack.localScale = leftTargetScale;
        rightStack.localScale = rightTargetScale;

        leftDepthOffset = leftTargetOffset;
        rightDepthOffset = rightTargetOffset;
    }

    void SetLocalZ(Transform t, float z)
    {
        Vector3 local = t.localPosition;
        local.z = z;
        t.localPosition = local;
    }

    #endregion

    #region Pages Logic

    public List<(int left, int right)> SkippedPages = new();

    private int CheckSkippedPages(int targetPage, bool forward)
    {
        foreach (var (left, right) in SkippedPages)
        {
            if (targetPage >= left && targetPage <= right)
            {
                return forward ? targetPage + 2 : targetPage - 2;
            }
        }

        return targetPage;
    }

    public void InitializePages(GameObject blankPrefab, int pageCount)
    {
        pages.Clear();

        for (int i = 0; i < pageCount; i++)
        {
            var node = new RuntimeBlankPageNode
            {
                TargetPage = i + 1
            };

            pages.Add(new PageEntry(blankPrefab, node));
        }
    }

    public void InsertPage(int index, PageEntry entry)
    {
        index = Mathf.Clamp(index, 0, pages.Count);
        pages.Insert(index, entry);

        if (index <= currentPage)
            currentPage += 2;

        UpdateRenderedPages();
    }

    public void ReplacePage(int index, PageEntry entry)
    {
        if (index < 0 || index >= pages.Count) return;

        pages[index] = entry;
        UpdateRenderedPages();
    }

    public void RemovePage(int index)
    {
        if (index < 0 || index >= pages.Count) return;

        pages.RemoveAt(index);

        if (currentPage >= pages.Count)
        currentPage = Mathf.Max(0, pages.Count - 2);

        UpdateRenderedPages();
    }

    public void ClearPage(int index)
    {
        ReplacePage(index, null);
    }

    public void AddCondition(PageCondition condition)
    {
        conditions.Add(condition);
    }

    public PageCondition GetConditionForPage(int pageIndex)
    {
        var condition = conditions.Find(c => c.TargetPage == pageIndex);
        if (condition != null && condition.oneTime) conditions.Remove(condition);

        return condition;
    }

    #endregion

}

[System.Serializable]
public class PageEntry
{
    [SerializeField] GameObject prefab;
    [SerializeField] RuntimeNode node;

    private GameObject instance;
    private bool initialized;

    public PageEntry(GameObject prefab, RuntimeNode node = null)
    {
        this.prefab = prefab;
        this.node = node;
    }

    public GameObject Prefab => prefab;
    public RuntimeNode Node => node;

    public GameObject GetOrCreateInstance(Transform parent)
    {
        if (instance == null)
        {
            instance = GameObject.Instantiate(prefab, parent);
            instance.transform.localPosition = Vector3.zero;
            instance.transform.localRotation = Quaternion.identity;

            if (!initialized)
            {
                var page = instance.GetComponent<B_Page>();
                if (page != null) page.WriteThePage(node);

                initialized = true;
            }
        }

        return instance;
    }
}

[System.Serializable]
public class PageCondition
{
    public int TargetPage;
    public string NextNodeId;
    public bool oneTime;

    public PageCondition(int targetPage, string nextNodeId, bool oneTime)
    {
        this.TargetPage = targetPage;
        this.NextNodeId = nextNodeId;
        this.oneTime = oneTime;
    }
}