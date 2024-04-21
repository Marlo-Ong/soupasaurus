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

    [Header("Flip-Through")]
    public List<Sprite> FlipThroughSprites;
    public float FramesPerSecond = 1;

    [Header("Up-Down")]
    public float Y_Offset;
    
    public bool IsLooped;
    public bool PlaysOnStart;
    private Image _imageToAnimate;
    private SpriteRenderer _rendererToAnimate;
    private Vector3 _updownOffset;
    private Vector3 _startingPosition;
    private Coroutine _animationCoroutine;

    void OnEnable()
    {
        _updownOffset = new Vector3(0,Y_Offset,0);
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
                transform.localPosition = transform.localPosition + _updownOffset;
                yield return new WaitForSeconds(1/FramesPerSecond);
                transform.localPosition = transform.localPosition - _updownOffset;
                yield return new WaitForSeconds(1/FramesPerSecond);
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
