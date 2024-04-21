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
    public float FramesPerSecond = 1;
    public bool IsLooped;
    public bool PlaysOnStart;
    private Image _imageToAnimate;
    private SpriteRenderer _rendererToAnimate;
    private Vector3 _updownOffset;
    private Vector3 _startingPosition;
    private Coroutine _animationCoroutine;

    void OnEnable()
    {
        _updownOffset = new Vector3(0,10,0);
        _startingPosition = transform.position;

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
        _animationCoroutine = StartCoroutine(PlayAnimation());
    }

    public void Stop()
    {
        if (_animationCoroutine != null) StopCoroutine(_animationCoroutine);
        transform.position = _startingPosition;
    }

    IEnumerator PlayAnimation()
    {
        switch (AnimType)
        {
            case IdleAnimationType.UpDown:
                //transform.Translate(_updownOffset);
                //Debug.Log($"Up (T, LocalT): {transform.position}, {transform.localPosition}");
                yield return new WaitForSeconds(1/FramesPerSecond);
                //transform.Translate(-_updownOffset);
                //Debug.Log($"Down (T, LocalT): {transform.position}, {transform.localPosition}");
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
