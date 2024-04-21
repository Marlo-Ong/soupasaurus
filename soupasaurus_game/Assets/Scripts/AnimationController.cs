using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public enum IdleAnimationType
{
    UpDown,
    FlipThrough
}

public class AnimationController : MonoBehaviour
{
    public IdleAnimationType AnimType;
    public AnimationCurve AnimCurve;
    public List<Sprite> FlipThroughSprites;
    public int FramesPerSecond = 1;
    public bool IsLooped;
    public bool PlaysOnStart;
    private Image _imageToAnimate;
    private SpriteRenderer _rendererToAnimate;
    private Vector3 _updownOffset;

    void OnEnable()
    {
        _updownOffset = new Vector3(0,10,0);

        if (TryGetComponent(out Image i))
        {
            _imageToAnimate = i;
        };
        if (TryGetComponent(out SpriteRenderer r))
        {
            _rendererToAnimate = r;
        };

        if (PlaysOnStart)
        {
            Play();
        }
    }

    public void Play()
    {
        StartCoroutine(PlayAnimation());
    }

    IEnumerator PlayAnimation()
    {
        switch (AnimType)
        {
            case IdleAnimationType.UpDown:
                transform.localPosition += _updownOffset;
                Debug.Log($"Up (T, LocalT): {transform.position}, {transform.localPosition}");
                yield return new WaitForSeconds(1/FramesPerSecond);
                transform.localPosition -= _updownOffset;
                Debug.Log($"Down (T, LocalT): {transform.position}, {transform.localPosition}");
                break;

            case IdleAnimationType.FlipThrough:
                foreach (Sprite s in FlipThroughSprites)
                {
                    if (_imageToAnimate != null)
                    {
                        _imageToAnimate.sprite = s;
                    }

                    if (_rendererToAnimate != null)
                    {
                        _rendererToAnimate.sprite = s;
                    }

                    yield return new WaitForSeconds(1/FramesPerSecond);
                }
                break;
        }

        CheckLoop();
    }

    private void CheckLoop()
    {
        if (IsLooped)
        {
            Play();
        }
    }
}
