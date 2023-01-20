using System.Collections;
using EasingFunctions;
using UnityEngine;
using Image = UnityEngine.UI.Image;

/// <summary>
/// A camera that follows a specific target that can be changed
/// </summary>
public class SmoothCamera : MonoBehaviour
{
    public Transform Target;
    public bool FollowingOwner = true; // false if following other player
    [SerializeField] private Vector3 offset = new Vector3(0, 2f, -8f);
    [SerializeField] private float moveSpeed = 0.05f;

    private Vector3 velocity = Vector3.zero;
    private float transitionMultiplier = 1f;
    private float timeSpentFollowingOtherPlayer;
    private Image blackImage;
    private bool isFading;

    void Start()
    {

        GameObject fadeOutObject = Instantiate((Resources.Load("FadeOut") as GameObject)
            , Vector3.zero, Quaternion.identity, transform);

        if (fadeOutObject)
            blackImage = fadeOutObject.GetComponentInChildren<Image>();
    }
    void Update()
    {
        if (Target == null) return;

        Vector3 targetPos = Target.position + offset;

        if (FollowingOwner)
        {
            transitionMultiplier = 1f;
        }
        else
        {
            timeSpentFollowingOtherPlayer += Time.deltaTime;
            float progress = Mathf.Clamp(timeSpentFollowingOtherPlayer / 3, 0, 1);
            transitionMultiplier = Mathf.SmoothStep(6f, 3f, progress);
        }

        transform.position =
            Vector3.SmoothDamp(transform.position, targetPos, ref velocity, moveSpeed * transitionMultiplier);
    }

    // fades the screen to black
    public void FadeOut(float duration)
    {
        if (isFading) return;
        StartCoroutine(FadeRoutine(duration, true));
    }
    // fades the screen from black to clear
    public void FadeIn(float duration)
    {
        if (isFading) return;
        StartCoroutine(FadeRoutine(duration, false));
    }
    private IEnumerator FadeRoutine(float duration, bool isFadingOut)
    {
        if (blackImage != null && !isFading)
        {
            isFading = true;
            blackImage.enabled = true;
            Color startColor = isFadingOut ?  new Color(0, 0, 0, 0) :new Color(0, 0, 0, 1);
            Color endColor = isFadingOut ? new Color(0, 0, 0, 1) : new Color(0, 0, 0, 0);
            float elapsedTime = 0;
            while (elapsedTime < duration)
            {
                elapsedTime += Time.deltaTime;
                float progress = isFadingOut ? Ease.InOutSine(elapsedTime / duration) : Ease.In(elapsedTime / duration, 3);
                Color newColor = Color.Lerp(startColor, endColor, progress);
                blackImage.color = newColor;
                yield return new WaitForEndOfFrame();
            }
            blackImage.color = endColor;
        }
        
        isFading = false;
    }
}
