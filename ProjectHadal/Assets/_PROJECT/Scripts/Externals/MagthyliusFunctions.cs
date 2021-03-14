using UnityEngine;
using System;
using UnityEngine.UI;
using TMPro;
using UnityEngine.Events;

// Version 1.4.6
namespace Magthylius
{
    namespace LerpFunctions
    {
        public enum FaderState
        {
            OPAQUE = 0,
            TRANSPARENT,
            FADING,
        }

        //! Data Struct
        [Serializable]
        public class OffsetGroup
        {
            public Vector2 min;
            public Vector2 max;

            public OffsetGroup()
            {
                min = Vector2.zero;
                max = Vector2.zero;
            }

            public OffsetGroup(Vector2 n, Vector2 x)
            {
                min = n;
                max = x;
            }

            public OffsetGroup(RectTransform rect)
            {
                min = rect.offsetMin;
                max = rect.offsetMax;
            }

            public OffsetGroup(Rect rect)
            {
                min = new Vector2(rect.x, rect.y - rect.height);
                max = new Vector2(rect.x + rect.width, rect.y);
            }

            public OffsetGroup(OffsetGroup copy)
            {
                min = copy.min;
                max = copy.max;
            }

            public void AddWidth(float width)
            {
                min.x += width;
                max.x += width;
            }

            public void AddHeight(float height)
            {
                min.y += height;
                min.y += height;
            }

            public static void Copy(RectTransform target, OffsetGroup other)
            {
                target.offsetMin = other.min;
                target.offsetMax = other.max;
            }
        }

        [Serializable]
        public class FlexibleRect
        {

            public RectTransform rectTransform;
            public Vector2 originalPosition;
            public Vector2 targetPosition;

            Vector2 endPosition;
            bool allowTransition = false;
            bool isMovingAway = true;

            public FlexibleRect(RectTransform rectTr)
            {
                rectTransform = rectTr;
                originalPosition = center;

                targetPosition = Vector2.zero;
                endPosition = Vector2.zero;

                isMovingAway = true;
            }

            public FlexibleRect(RectTransform rectTr, Vector2 targetPos)
            {
                rectTransform = rectTr;
                originalPosition = center;

                targetPosition = targetPos;
                endPosition = Vector2.zero;

                isMovingAway = true;
            }

            public void Step(float speed, float precision = 0.1f)
            {
                if (allowTransition)
                {
                    allowTransition = !Lerp(endPosition, speed, precision);
                }
            }

            //! Lerp
            public void StartLerp()
            {
                allowTransition = true;
                isMovingAway = !isMovingAway;
                DetermineEndPosition();
            }

            public void StartLerp(bool movingAway)
            {
                allowTransition = true;
                isMovingAway = movingAway;

                DetermineEndPosition();
            }

            public void StartLerp(Vector2 endPos)
            {
                allowTransition = true;
                endPosition = endPos;

                isMovingAway = true;
            }

            void DetermineEndPosition()
            {
                if (isMovingAway) endPosition = targetPosition;
                else endPosition = originalPosition;
            }

            public void EndLerp()
            {
                allowTransition = false;
            }

            public bool Lerp(Vector2 targetPosition, float speed, float precision = 0.1f)
            {
                Vector2 destination = Vector2.Lerp(center, targetPosition, speed);

                if ((targetPosition - destination).sqrMagnitude <= precision * precision)
                {
                    MoveTo(targetPosition);
                    return true;
                }

                MoveTo(destination);
                return false;
            }

            public void NormalLerp(Vector2 targetPosition, float progress)
            {
                Vector2 destination = Vector2.Lerp(center, targetPosition, progress);

                MoveTo(destination);
            }

            //! Movement
            public void MoveTo(Vector2 targetPosition)
            {
                Vector2 diff = targetPosition - center;
                rectTransform.offsetMax += diff;
                rectTransform.offsetMin += diff;
            }

            public void MoveToStart() => MoveTo(originalPosition);
            public void MoveToEnd() => MoveTo(targetPosition);

            //! Setters
            public void SetMovingAway(bool status) => isMovingAway = status;
            public void ToggleMovement() => isMovingAway = !isMovingAway;
            public void SetTargetPosition(Vector2 targetPos) => targetPosition = targetPos;
            public void SetEndPosition(Vector2 targetPos) => endPosition = targetPos;

            //! Queries
            public Vector2 GetBodyOffset(Vector2 direction)
            {
                return new Vector2(originalPosition.x + (direction.x * width), originalPosition.y + (direction.y * height));
            }

            public Vector2 GetBodyOffset(Vector2 direction, float degreeOfSelf)
            {
                return new Vector2(originalPosition.x + (direction.normalized.x * degreeOfSelf * halfWidth), originalPosition.y + (direction.normalized.y * degreeOfSelf * halfHeight));
            }

            public Vector2 ofMin => rectTransform.offsetMin;
            public Vector2 ofMax => rectTransform.offsetMax;
            public Vector2 center => (ofMin + ofMax) * 0.5f;
            public Vector2 centerPivoted => (ofMin + ofMax) * 0.5f * rectTransform.pivot;
            public float width => rectTransform.rect.width;
            public float height => rectTransform.rect.height;
            public float halfWidth => width * 0.5f;
            public float halfHeight => height * 0.5f;
            public virtual bool IsTransitioning => allowTransition;
            public bool IsMovingAway => isMovingAway;
        }

        [Serializable]
        public class FlexibleRectCorners : FlexibleRect
        {
            enum FRCornerMode
            {
                CENTER = 0,
                MIDDLE
            }

            public Vector2 originalOffSetMax;
            public Vector2 originalOffsetMin;

            Vector2 centeredOffsetMax;
            Vector2 centeredOffsetMin;

            Vector2 middledOffsetMax;
            Vector2 middledOffsetMin;

            float lerpPrecision = 0.01f;
            bool cornerTransition = false;
            bool goingOpen = false;
            FRCornerMode mode = FRCornerMode.CENTER;

            bool debugEnabled = false;

            public FlexibleRectCorners(RectTransform rectTr) : base(rectTr)
            {
                originalOffsetMin = rectTr.offsetMin;
                originalOffSetMax = rectTr.offsetMax;

                //! not a stretching RectTransform
                if (rectTr.anchorMax == rectTr.anchorMin)
                {
                    centeredOffsetMax = new Vector2(0f, originalOffSetMax.y);
                    centeredOffsetMin = new Vector2(0f, originalOffsetMin.y);

                    middledOffsetMax = new Vector2(originalOffSetMax.x, 0f);
                    middledOffsetMin = new Vector2(originalOffsetMin.x, 0f);
                }
                else Debug.LogError("Strecthed RectTr!");

                mode = FRCornerMode.CENTER;
                lerpPrecision = 0.01f;
            }

            public void CornerStep(float speed)
            {
                if (cornerTransition)
                {
                    if (goingOpen) cornerTransition = !CornerLerp(originalOffsetMin, originalOffSetMax, speed);
                    else
                    {
                        if (mode == FRCornerMode.CENTER) cornerTransition = !CornerLerp(centeredOffsetMin, centeredOffsetMax, speed);
                        else cornerTransition = !CornerLerp(middledOffsetMin, middledOffsetMax, speed);
                    }

                    if (debugEnabled) Debug.Log("Corner transitioning");
                    if (!cornerTransition) goingOpen = !goingOpen;
                }
            }

            public void CornerJump(Vector2 minTarget, Vector2 maxTarget)
            {
                rectTransform.offsetMin = minTarget;
                rectTransform.offsetMax = maxTarget;
            }

            bool CornerLerp(Vector2 minTarget, Vector2 maxTarget, float speed)
            {
                rectTransform.offsetMin = Vector2.Lerp(rectTransform.offsetMin, minTarget, speed);
                rectTransform.offsetMax = Vector2.Lerp(rectTransform.offsetMax, maxTarget, speed);

                if (Vector2.SqrMagnitude(rectTransform.offsetMin - minTarget) <= lerpPrecision * lerpPrecision)
                {
                    rectTransform.offsetMin = minTarget;
                    rectTransform.offsetMax = maxTarget;
                    return true;
                }
                return false;
            }

            public void StartMiddleLerp()
            {
                if (debugEnabled) Debug.Log("Middle lerp triggered");

                cornerTransition = true;
                mode = FRCornerMode.MIDDLE;
            }

            public void StartCenterLerp()
            {
                if (debugEnabled) Debug.Log("Center lerp triggered");

                cornerTransition = true;
                mode = FRCornerMode.CENTER;
            }

            public void Open()
            {
                goingOpen = false;
                CornerJump(originalOffsetMin, originalOffSetMax);
            }

            public void Close()
            {
                goingOpen = true;
                if (mode == FRCornerMode.CENTER) CornerJump(centeredOffsetMin, centeredOffsetMax);
                else CornerJump(middledOffsetMin, middledOffsetMax);
            }

            public void DebugEnable() => debugEnabled = true;
            public void DebugDisable() => debugEnabled = false;
            public override bool IsTransitioning => cornerTransition;
        }

        public class ParallaxRect : FlexibleRect
        {
            public ParallaxRect(RectTransform rectTr) : base(rectTr)
            {
                rectTransform = rectTr;
            }

            public void ParallaxStep(Vector2 mousePos, Canvas canvas, float speed, float precision = 0.1f)
            {
                Vector3 endPoint;
                if (canvas.renderMode == RenderMode.ScreenSpaceCamera)
                {
                    endPoint = new Vector3(mousePos.x, mousePos.y, canvas.planeDistance);
                    mousePos = Camera.main.ScreenToWorldPoint(endPoint);

                }

                Vector2 nextPos = mousePos - originalPosition;
                //Debug.Log(endPosition);
                StartLerp(nextPos);
                Step(speed, precision);
            }
        }

        public class ImageFiller
        {
            public Image image;
            public UnityEvent fillCompleteEvent;

            public delegate void FillCompleteEvent();
            public event FillCompleteEvent OnFillComplete;

            float chargeRate;
            float charge;
            float maxCharge;

            float progress;
            bool allowCharge;
            bool stopChargeWhenFilled;

            bool isFilled;

            public ImageFiller(Image _image, float _chargeRate = 0.1f, float _maxCharge = 1f, bool _stopChargeWhenFilled = true)
            {
                image = _image;

                chargeRate = _chargeRate;
                charge = 0f;
                maxCharge = _maxCharge;
                stopChargeWhenFilled = _stopChargeWhenFilled;

                progress = 0f;
                isFilled = false;

                fillCompleteEvent = new UnityEvent();
                if (maxCharge <= 0) maxCharge = 1f;
            }

            public void Step(float speed)
            {
                if (isFilled && stopChargeWhenFilled) return;

                if (allowCharge) charge += chargeRate * speed;
                else if (charge > 0f) charge -= chargeRate * speed;

                progress = charge / maxCharge;

                if (progress >= 1f)
                {
                    isFilled = true;
                    fillCompleteEvent.Invoke();
                    OnFillComplete?.Invoke();

                    progress = 1f;
                    charge = maxCharge;
                }
                UpdateImage();
            }

            public void UpdateImage()
            {
                image.fillAmount = progress;
            }

            public void StartCharge()
            {
                allowCharge = true;
            }

            public void StopCharge()
            {
                allowCharge = false;
            }

            public void ResetCharge()
            {
                charge = 0f;
                progress = charge / maxCharge;
                isFilled = false;

                UpdateImage();
            }

            public bool IsFilled => isFilled;
        }

        #region Faders
        //! Canvas Group
        [Serializable]
        public class CanvasGroupFader
        {
            public enum CanvasState
            {
                FADE_OUT,
                FADE_IN
            }

            public CanvasGroup canvas;
            public bool affectsTouch;
            public float precision;
            public UnityEvent fadeEndedEvent;

            CanvasState state;
            bool allowFade;

            public CanvasGroupFader(CanvasGroup canvasGroup, bool startFadeInState, bool canAffectTouch, float alphaPrecision = 0.001f)
            {
                canvas = canvasGroup;
                affectsTouch = canAffectTouch;
                if (startFadeInState) SetStateFadeIn();
                else SetStateFadeOut();

                allowFade = false;
                precision = alphaPrecision;
                fadeEndedEvent = new UnityEvent();
            }

            public void Step(float speed)
            {
                if (allowFade)
                {
                    if (state == CanvasState.FADE_IN)
                    {
                        canvas.alpha = Mathf.Lerp(canvas.alpha, 1f, speed);

                        if (1f - canvas.alpha <= precision)
                        {
                            allowFade = false;
                            canvas.alpha = 1f;

                            if (affectsTouch)
                            {
                                canvas.blocksRaycasts = true;
                                canvas.interactable = true;
                            }

                            fadeEndedEvent.Invoke();
                        }
                    }
                    else
                    {
                        canvas.alpha = Mathf.Lerp(canvas.alpha, 0f, speed);

                        if (canvas.alpha <= precision)
                        {
                            allowFade = false;
                            canvas.alpha = 0f;

                            if (affectsTouch)
                            {
                                canvas.blocksRaycasts = false;
                                canvas.interactable = false;
                            }

                            fadeEndedEvent.Invoke();
                        }
                    }
                }
            }

            public void SetAlpha(float alpha)
            {
                canvas.alpha = alpha;
            }
            public void SetInteraction(bool interaction)
            {
                canvas.blocksRaycasts = interaction;
                canvas.interactable = interaction;
            }
            public void StartFadeIn()
            {
                SetStateFadeIn();
                TriggerFade();
            }
            public void StartFadeOut()
            {
                SetStateFadeOut();
                TriggerFade();
            }

            public void SetTransparent()
            {
                SetAlpha(0f);
                if (affectsTouch) SetInteraction(false);
            }
            public void SetOpaque()
            {
                SetAlpha(1f);
                if (affectsTouch) SetInteraction(true);
            }

            public void TriggerFade() => allowFade = true;
            public void SetStateFadeIn() => state = CanvasState.FADE_IN;
            public void SetStateFadeOut() => state = CanvasState.FADE_OUT;
            public bool IsFading => allowFade;
            public CanvasState State => state;
        }

        //! Graphic (Images and TMP)
        [Serializable]
        public class UIFader
        {
            public Graphic uiObject;
            FaderState state;
            FaderState targetState;

            Color original;
            Color opaque;
            Color transparent;

            bool allowTransition;
            UnityEvent fadeEndedEvent;

            public UIFader(Graphic graphicObject)
            {
                uiObject = graphicObject;

                original = graphicObject.color;

                opaque = original;
                opaque.a = 1f;

                transparent = original;
                transparent.a = 0f;

                if (Alpha >= 1f) state = FaderState.OPAQUE;
                else if (Alpha <= 0f) state = FaderState.TRANSPARENT;
                else state = FaderState.FADING;

                targetState = state;
                fadeEndedEvent = new UnityEvent();
            }

            public void Step(float speed)
            {
                if (allowTransition)
                {
                    if (targetState == FaderState.TRANSPARENT)
                    {
                        uiObject.color = Color.Lerp(uiObject.color, transparent, speed);

                        if (uiObject.color.a <= 0.001f)
                        {
                            uiObject.color = transparent;
                            state = FaderState.TRANSPARENT;
                            allowTransition = false;
                            fadeEndedEvent.Invoke();
                        }
                    }
                    else if (targetState == FaderState.OPAQUE)
                    {
                        uiObject.color = Color.Lerp(uiObject.color, opaque, speed);

                        if (1f - uiObject.color.a <= 0.001f)
                        {
                            uiObject.color = opaque;
                            state = FaderState.OPAQUE;
                            allowTransition = false;
                            fadeEndedEvent.Invoke();
                        }
                    }
                    else allowTransition = false;
                }
            }

            public void FadeToTransparent()
            {
                allowTransition = true;
                targetState = FaderState.TRANSPARENT;
            }

            public void FadeToOpaque()
            {
                allowTransition = true;
                targetState = FaderState.OPAQUE;
            }

            public void ForceTransparent()
            {
                uiObject.color = transparent;
                state = FaderState.TRANSPARENT;
            }

            public void ForceOpaque()
            {
                uiObject.color = opaque;
                state = FaderState.OPAQUE;
            }
            public float Alpha => uiObject.color.a;
            public Color OriginalColor => original;
            public Color OpaqueColor => opaque;
            public Color TransparentColor => transparent;
            public UnityEvent FadeEndedEvent => fadeEndedEvent;
            public FaderState CurrentState()
            {
                if (Alpha >= 1f) return FaderState.OPAQUE;
                else if (Alpha <= 0f) return FaderState.TRANSPARENT;

                return FaderState.FADING;
            }
        }

        public class LineRendererFader
        {
            public LineRenderer renderer;
            FaderState state;
            FaderState targetState;

            //! Colors
            Color startOriginal;
            Color endOriginal;

            Color startOpaque;
            Color endOpaque;
            Color startTransparent;
            Color endTransparent;

            //! Width
            float startWidthOriginal;
            float endWidthOriginal;

            float startWidthHidden;
            float endWidthHidden;

            bool colorMode;
            bool allowTransition;
            UnityEvent fadeEndedEvent;

            public LineRendererFader(LineRenderer lineRenderer, bool isColorMode)
            {
                renderer = lineRenderer;
                startOriginal = renderer.startColor;
                endOriginal = renderer.endColor;

                startOpaque = startOriginal;
                endOpaque = endOriginal;
                startOpaque.a = 1f;
                endOpaque.a = 1f;

                startTransparent = startOpaque;
                endTransparent = endOpaque;
                startTransparent.a = 0f;
                endTransparent.a = 0f;

                startWidthOriginal = renderer.startWidth;
                endWidthOriginal = renderer.endWidth;
                startWidthHidden = 0f;
                endWidthHidden = 0f;

                colorMode = isColorMode;

                if (AlphaStart >= 1f) state = FaderState.OPAQUE;
                else if (AlphaStart <= 0f) state = FaderState.TRANSPARENT;
                else state = FaderState.FADING;

                targetState = state;
                fadeEndedEvent = new UnityEvent();
            }

            public void Step(float speed)
            {
                if (allowTransition)
                {
                    if (targetState == FaderState.TRANSPARENT)
                    {
                        if (colorMode)
                        {
                            renderer.startColor = Color.Lerp(renderer.startColor, startTransparent, speed);
                            renderer.endColor = Color.Lerp(renderer.endColor, endTransparent, speed);

                            if (renderer.startColor.a <= 0.001f && renderer.endColor.a <= 0.001f)
                            {
                                renderer.startColor = startTransparent;
                                renderer.endColor = endTransparent;
                                state = FaderState.TRANSPARENT;
                                allowTransition = false;
                                fadeEndedEvent.Invoke();
                            }
                        }
                        else
                        {
                            renderer.startWidth = Mathf.Lerp(renderer.startWidth, startWidthHidden, speed);
                            renderer.endWidth = Mathf.Lerp(renderer.endWidth, endWidthHidden, speed);

                            if (renderer.startWidth <= 0.001f && renderer.endWidth <= 0.001f)
                            {
                                renderer.startWidth = startWidthHidden;
                                renderer.endWidth = endWidthHidden;
                                state = FaderState.TRANSPARENT;
                                allowTransition = false;
                                fadeEndedEvent.Invoke();
                            }
                        }
                    }
                    else if (targetState == FaderState.OPAQUE)
                    {
                        if (colorMode)
                        {
                            renderer.startColor = Color.Lerp(renderer.startColor, startOpaque, speed);
                            renderer.endColor = Color.Lerp(renderer.endColor, endOpaque, speed);

                            if (1f - renderer.startColor.a <= 0.001f && 1f - renderer.endColor.a <= 0.001f)
                            {
                                renderer.startColor = startOpaque;
                                renderer.endColor = endOpaque;
                                state = FaderState.OPAQUE;
                                allowTransition = false;
                                fadeEndedEvent.Invoke();
                            }
                        }
                        else
                        {
                            renderer.startWidth = Mathf.Lerp(renderer.startWidth, startWidthOriginal, speed);
                            renderer.endWidth = Mathf.Lerp(renderer.endWidth, endWidthOriginal, speed);

                            if (startWidthOriginal - renderer.startWidth <= 0.001f && endWidthOriginal - renderer.endWidth <= 0.001f)
                            {
                                renderer.startWidth = startWidthOriginal;
                                renderer.endWidth = endWidthOriginal;
                                state = FaderState.TRANSPARENT;
                                allowTransition = false;
                                fadeEndedEvent.Invoke();
                            }
                        }
                    }
                    else allowTransition = false;
                }
            }

            public void FadeToTransparent()
            {
                allowTransition = true;
                targetState = FaderState.TRANSPARENT;
            }

            public void FadeToOpaque()
            {
                allowTransition = true;
                targetState = FaderState.OPAQUE;
            }

            public void ForceTransparent()
            {
                if (colorMode)
                {
                    renderer.startColor = startTransparent;
                    renderer.endColor = endTransparent;
                }
                else
                {
                    renderer.startWidth = startWidthHidden;
                    renderer.endWidth = endWidthHidden;
                }

                state = FaderState.TRANSPARENT;
            }

            public void ForceOpaque()
            {
                if (colorMode)
                {
                    renderer.startColor = startOpaque;
                    renderer.endColor = endOpaque;
                }
                else
                {
                    renderer.startWidth = startWidthOriginal;
                    renderer.endWidth = endWidthOriginal;
                }

                state = FaderState.OPAQUE;
            }

            public float AlphaStart => renderer.startColor.a;
            public float AlphaEnd => renderer.endColor.a;
            public UnityEvent FadeEndedEvent => fadeEndedEvent;
        }
        #endregion

        #region Lerp Functions
        public class Lerp : MonoBehaviour
        {
            // rects
            public static bool Rect(OffsetGroup targetOffset, RectTransform movingObject, float lerpSpeed = 1f)
            {
                Vector2 botLeft = Vector2.Lerp(movingObject.offsetMin, targetOffset.min, lerpSpeed * Time.deltaTime);
                Vector2 topRight = Vector2.Lerp(movingObject.offsetMax, targetOffset.max, lerpSpeed * Time.deltaTime);

                if (NegligibleDistance(movingObject.offsetMin, targetOffset.min))
                {
                    movingObject.offsetMin = targetOffset.min;
                    movingObject.offsetMax = targetOffset.max;
                    return true;
                }

                movingObject.offsetMin = botLeft;
                movingObject.offsetMax = topRight;
                return false;
            }
            public static bool Rect(RectTransform targetRect, RectTransform movingObject, float lerpSpeed = 1f)
            {
                Vector2 botLeft = Vector2.Lerp(movingObject.offsetMin, targetRect.offsetMin, lerpSpeed * Time.deltaTime);
                Vector2 topRight = Vector2.Lerp(movingObject.offsetMax, targetRect.offsetMax, lerpSpeed * Time.deltaTime);

                if (NegligibleDistance(movingObject.offsetMin, targetRect.offsetMin))
                {
                    movingObject.offsetMin = targetRect.offsetMin;
                    movingObject.offsetMax = targetRect.offsetMax;
                    return true;
                }

                movingObject.offsetMin = botLeft;
                movingObject.offsetMax = topRight;
                return false;
            }

            // float
            public static bool Float(float a, float b, float lerpSpeed = 1f)
            {
                if (b < a)
                    a = Mathf.Lerp(b, a, lerpSpeed * Time.deltaTime);
                else
                    a = Mathf.Lerp(a, b, lerpSpeed * Time.deltaTime);

                if (NegligibleDistance(a, b))
                {
                    a = b;
                    return true;
                }

                return false;
            }

            // anchored position
            public static bool APosition(RectTransform obj, Vector2 targetPos, float lerpSpeed = 1f)
            {
                obj.anchoredPosition = Vector2.Lerp(obj.anchoredPosition, targetPos, lerpSpeed * Time.deltaTime);
                obj.anchoredPosition = new Vector2((float)Math.Round(obj.anchoredPosition.x, 1), (float)Math.Round(obj.anchoredPosition.y, 1));

                if (NegligibleDistance(obj.anchoredPosition, targetPos))
                {
                    obj.anchoredPosition = targetPos;
                    return true;
                }

                return false;
            }

            // position
            public static bool Position(RectTransform obj, Vector2 targetPos, float lerpSpeed = 1f)
            {
                obj.position = Vector2.Lerp(obj.position, targetPos, lerpSpeed * Time.deltaTime);
                obj.position = new Vector2((float)Math.Round(obj.position.x, 1), (float)Math.Round(obj.position.y, 1));

                if (NegligibleDistance(obj.position, targetPos))
                {
                    obj.position = targetPos;
                    return true;
                }

                return false;
            }

            // offset position
            public static bool OFPosition(RectTransform target, OffsetGroup destination, float lerpSpeed = 1f)
            {
                target.offsetMin = Vector2.Lerp(target.offsetMin, destination.min, lerpSpeed * Time.deltaTime);
                target.offsetMax = Vector2.Lerp(target.offsetMax, destination.max, lerpSpeed * Time.deltaTime);

                if (NegligibleDistance(target.offsetMin, destination.min))
                {
                    target.offsetMin = destination.min;
                    target.offsetMax = destination.max;
                    return true;
                }

                return false;
            }

            // vector
            /*public static bool Vector(Vector2 target, Vector2 destination, float lerpSpeed = 1f)
            {
                target = Vector2.Lerp(target, destination, lerpSpeed * Time.deltaTime);

                if (NegligibleDistance(target, destination))
                {
                    target = destination;
                    return true;
                }

                return false;
            }*/

            public static Vector3 Vector(Vector3 target, Vector3 destination, float lerpSpeed = 1f)
            {
                target = Vector3.Lerp(target, destination, lerpSpeed * Time.deltaTime);

                if (NegligibleDistance(target, destination))
                {
                    target = destination;
                }

                return target;
            }

            // size delta
            public static bool SizeDelta(RectTransform target, Vector2 targetSizeDelta, float lerpSpeed = 1f)
            {
                target.sizeDelta = Vector2.Lerp(target.sizeDelta, targetSizeDelta, lerpSpeed * Time.deltaTime);

                if (NegligibleDistance(target.sizeDelta, targetSizeDelta))
                {
                    target.sizeDelta = targetSizeDelta;
                    return true;
                }

                return false;
            }

            // offscreens
            public static bool OffScreenBelow(RectTransform target, Vector2 offsetMin, float lerpSpeed = 1f)
            {
                Vector2 minDest = new Vector2(target.offsetMin.x, -Screen.height);
                Vector2 maxDest = new Vector2(target.offsetMax.x, 0);

                target.offsetMin = Vector2.Lerp(target.offsetMin, minDest, lerpSpeed * Time.deltaTime);
                target.offsetMax = Vector2.Lerp(target.offsetMax, maxDest, lerpSpeed * Time.deltaTime);

                if (NegligibleDistance(target.offsetMin, minDest))
                {
                    target.offsetMin = minDest;
                    target.offsetMax = maxDest;
                    return true;
                }

                return false;
            }
            public static bool OffScreenBelow(RectTransform target, OffsetGroup offsetGrp, float lerpSpeed = 1f)
            {
                Vector2 minDest = new Vector2(target.offsetMin.x, Screen.height);
                Vector2 maxDest = new Vector2(target.offsetMax.x, 0);

                target.offsetMin = Vector2.Lerp(target.offsetMin, minDest, lerpSpeed * Time.deltaTime);
                target.offsetMax = Vector2.Lerp(target.offsetMax, maxDest, lerpSpeed * Time.deltaTime);

                if (NegligibleDistance(target.offsetMin, minDest))
                {
                    target.offsetMin = minDest;
                    target.offsetMax = maxDest;
                    return true;
                }

                return false;
            }

            // direct movements
            public static void ForceStay(GameObject obj, Vector3 forcedPos)
            {
                obj.transform.position = forcedPos;
            }
            public static void Warp(RectTransform target, OffsetGroup destination)
            {
                target.offsetMax = destination.max;
                target.offsetMin = destination.min;
            }
            public static void WarpOffScreenBelow(RectTransform target, Vector2 offsetMin)
            {
                target.offsetMin = new Vector2(target.offsetMin.x, -Screen.height);
                target.offsetMax = new Vector2(target.offsetMax.x, 0);
            }
            public static void WarpOffScreenBelow(RectTransform target, OffsetGroup offsetGrp)
            {
                target.offsetMin = new Vector2(target.offsetMin.x, -Screen.height);
                target.offsetMax = new Vector2(target.offsetMax.x, 0);
            }
            public static void Follow(RectTransform followTarget, RectTransform follower, bool stopChildren = false)
            {
                follower.offsetMax = followTarget.offsetMax;
                follower.offsetMin = followTarget.offsetMin;

                if (stopChildren)
                {
                    for (int i = 0; i < follower.childCount; i++)
                    {

                    }
                }
            }

            // queries
            public static bool OnPosition(Vector2 target1, Vector2 target2) => target1 == target2;
            public static bool OnAPosition(RectTransform target1, RectTransform target2) => target1.anchoredPosition == target2.anchoredPosition;
            public static bool OnAPosition(RectTransform target1, Vector2 target2) => target1.anchoredPosition == target2;
            public static bool OnOFPosition(RectTransform target1, RectTransform target2) => target1.offsetMax == target2.offsetMax && target1.offsetMin == target2.offsetMin;
            //public static bool NegligibleDistance(Vector2 a, Vector2 b, float condition = 0.1f) => Vector2.Distance(a, b) < condition;
            public static bool NegligibleDistance(Vector3 a, Vector3 b, float condition = 0.1f) => Vector3.Distance(a, b) < condition;
            public static bool NegligibleDistance(float a, float b, float condition = 0.1f) => Mathf.Abs(a - b) < condition;

            #region QUATENIONS
            public static bool Rotation(RectTransform target, Quaternion rotation, float lerpSpeed = 1f)
            {
                target.rotation = Quaternion.Lerp(target.rotation, rotation, lerpSpeed * Time.deltaTime);

                if (NegligibleDistance(target.rotation.x, rotation.x, 0.01f))
                {
                    target.rotation = rotation;
                    return true;
                }

                return false;
            }
            #endregion

            #region COLORS
            // textmeshpro
            public static bool AlphaTMP(TextMeshProUGUI target, float alpha, float lerpSpeed = 1f)
            {
                float a = Mathf.Lerp(target.color.a, alpha, lerpSpeed * Time.deltaTime);
                target.color = new Color(target.color.r, target.color.g, target.color.b, a);

                if (NegligibleDistance(target.color.a, alpha, 0.01f))
                {
                    target.color = new Color(target.color.r, target.color.g, target.color.b, alpha);
                    return true;
                }

                return false;
            }

            // image
            public static bool AlphaImage(Image img, float alpha, float lerpSpeed = 1f)
            {
                Color target = new Color(img.color.r, img.color.g, img.color.b, alpha);
                img.color = Color.Lerp(img.color, target, lerpSpeed * Time.deltaTime);

                if (NegligibleDistance(img.color.a, target.a))
                {
                    img.color = target;
                    return true;
                }

                return false;
            }

            // canvas group
            public static bool AlphaCanvasGroup(CanvasGroup group, float alpha, float lerpSpeed = 1f)
            {
                group.alpha = Mathf.Lerp(group.alpha, alpha, lerpSpeed * Time.deltaTime);

                if (NegligibleDistance(group.alpha, alpha, 0.01f))
                {
                    group.alpha = alpha;
                    return true;
                }

                return false;
            }

            // alpha jumps
            public static void AlphaJump(CanvasGroup group, float alpha) => group.alpha = alpha;
            public static void AlphaJump(Image image, float alpha) => image.color = new Color(image.color.r, image.color.g, image.color.b, alpha);
            public static void AlphaJump(TextMeshProUGUI tmp, float alpha) => tmp.color = new Color(tmp.color.r, tmp.color.g, tmp.color.b, alpha);
            #endregion

            #region SCALING
            public static bool Scale(RectTransform target, Vector3 scaleSize, float lerpSpeed = 1f, float negCondition = 0.1f)
            {
                if (target.localScale.x > scaleSize.x)
                    target.localScale = Vector3.Lerp(target.localScale, scaleSize, lerpSpeed * Time.deltaTime);
                else
                    target.localScale = Vector3.Lerp(scaleSize, target.localScale, lerpSpeed * Time.deltaTime);

                if (NegligibleDistance(target.localScale, scaleSize, negCondition))
                {
                    target.localScale = scaleSize;
                    return true;
                }

                return false;
            }
            public static bool Scale(RectTransform target, float scale, float lerpSpeed = 1f, float negCondition = 0.1f)
            {
                Vector3 scaleSize = new Vector3(scale, scale, scale);
                /*if (target.localScale.x > scale)
                    target.localScale = Vector3.Lerp(target.localScale, scaleSize, lerpSpeed * Time.deltaTime);
                else
                    target.localScale = Vector3.Lerp(scaleSize, target.localScale, lerpSpeed * Time.deltaTime);*/
                target.localScale = Vector3.Lerp(target.localScale, scaleSize, lerpSpeed * Time.deltaTime);

                if (NegligibleDistance(target.localScale, scaleSize, negCondition))
                {
                    target.localScale = scaleSize;
                    return true;
                }

                return false;
            }
            public static void SizeSet(RectTransform target, float scale)
            {
                target.localScale = new Vector3(scale, scale, scale);
            }

            #endregion
        }
        #endregion
    }

    namespace GetFunctions
    {
        public class Get : MonoBehaviour
        {
            public static RectTransform RTr(Component g) => g.GetComponent<RectTransform>();
            public static RectTransform RectTr(Component g) => g.GetComponent<RectTransform>();
            public static RectTransform RectTransform(Component g) => g.GetComponent<RectTransform>();
            public static CanvasGroup CG(Component g) => g.GetComponent<CanvasGroup>();
            public static CanvasGroup CanvasGroup(Component g) => g.GetComponent<CanvasGroup>();
            public static Transform Tr(Component g) => g.GetComponent<Transform>();
            public static Transform Transform(Component g) => g.GetComponent<Transform>();
        }
    }

    namespace DataFunctions
    {
        public class Timer
        {
            UnityEvent targetTickedEvent;
            int passedCount;

            float tick;
            float tickTarget;

            bool refreshes;
            bool skipOneFrame;
            bool skipFrame;

            public Timer(float _tickTarget, bool _refreshesOnTargetReached = true, bool _skipOneFrameWhenTargetTicked = false)
            {
                tickTarget = _tickTarget;
                refreshes = _refreshesOnTargetReached;

                passedCount = 0;
                tick = 0f;

                targetTickedEvent = new UnityEvent();
                skipOneFrame = _skipOneFrameWhenTargetTicked;
            }

            public void Tick(float deltaTime)
            {
                if (skipFrame)
                {
                    skipFrame = false;
                    return;
                }

                tick += deltaTime;
                if (tick >= tickTarget)
                {
                    if (refreshes) tick = 0f;
                    else tick -= tickTarget;

                    passedCount++;
                    targetTickedEvent.Invoke();

                    if (skipOneFrame) skipFrame = true;
                }    
            }

            public void Reset() => tick = 0f;
            public float CurrentTick => tick;
            public void SetTickTarget(float target) => tickTarget = target;
            public float Progress => Mathf.Clamp01(tick / tickTarget);
            public int PassedCount => passedCount;
            public UnityEvent TargetTickedEvent => targetTickedEvent;
        }

        public class RandomInt
        {
            int value;
            int lastValue;

            int minRange;
            int maxRange;

            bool noRepeats;

            public RandomInt(int _minRange = int.MinValue, int _maxRange = int.MaxValue)
            {
                value = 0;
                lastValue = 0;
                noRepeats = false;

                minRange = _minRange;
                maxRange = _maxRange;
            }

            public int RandomValue
            {
                get 
                {
                    int temp = 0;

                    if (noRepeats)
                    {
                        do
                        {
                            temp = UnityEngine.Random.Range(minRange, maxRange);
                        } while (temp == lastValue);
                    }
                    else
                    {
                        noRepeats = true;
                        temp = UnityEngine.Random.Range(minRange, maxRange);
                    }

                    lastValue = value;
                    value = temp;

                    return value; 
                }
            }

            public int Value => value;
        }

        public class Randomizers
        {
            public static Vector2 PointInCircle(float radius, float minDistance = 0f)
            {
                Vector2 point = new Vector2();
                if (minDistance >= radius)
                {
                    Debug.LogError("Randomizer.PointInCircle: MinDistance >= radius!");
                    return point;
                }

                do
                {
                    point = UnityEngine.Random.insideUnitCircle * radius;
                } while (point.SqrMagnitude() <= minDistance * minDistance);

                return point;
            }
        }
    }

    namespace Utilities
    {
    }
}