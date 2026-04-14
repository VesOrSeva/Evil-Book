using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Book))]
public class AutoFlip : MonoBehaviour
{
    public FlipMode Mode;
    public float PageFlipTime = 0.25f;
    public float TimeBetweenPages = 0.05f;

    [SerializeField] Book ControledBook;

    private Coroutine currentFlipRoutine;
    private Coroutine flipSequenceRoutine;

    void Awake()
    {
        if (!ControledBook) ControledBook = GetComponent<Book>();
    }

    #region Public Methods

    public void FlipRightPage()
    {
        if (!CanFlipRight()) return;
        StartControlledFlip(true);
    }

    public void FlipLeftPage()
    {
        if (!CanFlipLeft()) return;
        StartControlledFlip(false);
    }

    public void StartFlipping()
    {
        if (flipSequenceRoutine != null) StopCoroutine(flipSequenceRoutine);
        flipSequenceRoutine = StartCoroutine(FlipToEnd());
    }

    public void StopFlipping()
    {
        if (flipSequenceRoutine != null) StopCoroutine(flipSequenceRoutine);
        if (currentFlipRoutine != null) StopCoroutine(currentFlipRoutine);

        ControledBook.IsAutoFlipping = false;
    }

    #endregion

    #region Core Logic

    void StartControlledFlip(bool rightToLeft)
    {
        if (currentFlipRoutine != null)  StopCoroutine(currentFlipRoutine);
        currentFlipRoutine = StartCoroutine(FlipRoutine(rightToLeft));
    }

    IEnumerator FlipRoutine(bool rightToLeft)
    {
        if (ControledBook.IsAutoFlipping) yield break;

        ControledBook.IsAutoFlipping = true;

        float duration = Mathf.Max(0.01f, PageFlipTime);
        float elapsed = 0f;

        float xc = (ControledBook.EndBottomRight.x + ControledBook.EndBottomLeft.x) / 2f;
        float xl = ((ControledBook.EndBottomRight.x - ControledBook.EndBottomLeft.x) / 2f) * 0.9f;
        float h = Mathf.Abs(ControledBook.EndBottomRight.y) * 0.9f;

        float startX = rightToLeft ? xc + xl : xc - xl;
        float endX = rightToLeft ? xc - xl : xc + xl;

        Vector3 startPos = GetPoint(startX, xc, xl, h);

        if (rightToLeft) ControledBook.DragRightPageToPoint(startPos);
        else ControledBook.DragLeftPageToPoint(startPos);

        // Animate
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);

            float eased = Mathf.SmoothStep(0f, 1f, t);

            float x = Mathf.Lerp(startX, endX, eased);
            Vector3 pos = GetPoint(x, xc, xl, h);

            if (rightToLeft) ControledBook.UpdateBookRTLToPoint(pos);
            else ControledBook.UpdateBookLTRToPoint(pos);

            yield return null;
        }


        ForceCompleteFlip(rightToLeft);
        ControledBook.IsAutoFlipping = false;

        currentFlipRoutine = null;
    }

    void ForceCompleteFlip(bool rightToLeft)
    {
        ControledBook.StopAllCoroutines();

        if (rightToLeft)
        {
            ControledBook.UpdateBookRTLToPoint(ControledBook.EndBottomLeft);
            ControledBook.currentPage += 2;
        }
        else
        {
            ControledBook.UpdateBookLTRToPoint(ControledBook.EndBottomRight);
            ControledBook.currentPage -= 2;
        }

        // Clamp
        int min = -ControledBook.TotalPageCount;
        int max = ControledBook.TotalPageCount;
        ControledBook.currentPage = Mathf.Clamp(ControledBook.currentPage, min, max);


        ResetBookState();
        typeof(Book)
            .GetMethod("UpdateRenderedPages", System.Reflection.BindingFlags.NonPublic 
            | System.Reflection.BindingFlags.Instance)
            ?.Invoke(ControledBook, null);

        ControledBook.OnFlip?.Invoke();
    }

    IEnumerator FlipToEnd()
    {
        while (true)
        {
            if (Mode == FlipMode.RightToLeft)
            {
                if (!CanFlipRight()) yield break;
                yield return FlipRoutine(true);
            }
            else
            {
                if (!CanFlipLeft()) yield break;
                yield return FlipRoutine(false);
            }

            yield return new WaitForSeconds(TimeBetweenPages);
        }
    }

    #endregion

    #region Helpers

    Vector3 GetPoint(float x, float xc, float xl, float h)
    {
        float y = (-h / (xl * xl)) * (x - xc) * (x - xc);
        return new Vector3(x, y, 0f);
    }

    bool CanFlipRight()
    {
        return !ControledBook.IsAutoFlipping && ControledBook.currentPage < ControledBook.TotalPageCount - 1;
    }

    bool CanFlipLeft()
    {
        return !ControledBook.IsAutoFlipping && ControledBook.currentPage > 0;
    }

    void ResetBookState()
    {
        ControledBook.Left.gameObject.SetActive(false);
        ControledBook.Right.gameObject.SetActive(false);

        ControledBook.Shadow.gameObject.SetActive(false);
        ControledBook.ShadowLTR.gameObject.SetActive(false);

        ControledBook.Left.transform.SetParent(ControledBook.transform, true);
        ControledBook.Right.transform.SetParent(ControledBook.transform, true);
        ControledBook.LeftNext.transform.SetParent(ControledBook.transform, true);
        ControledBook.RightNext.transform.SetParent(ControledBook.transform, true);
    }

    #endregion
}