using DG.Tweening;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using UnityEngine.Events;


public class SequancerEffect : MonoBehaviour
{
    public List<SequencerEffectStruct> actions;
    public bool isLooping = false;
    public bool playOnAwake = false;

    private Coroutine sequenceCoroutine;

    private void Start()
    {
        if (playOnAwake)
            Play();
    }

    public void Play()
    {
        if (actions == null || actions.Count == 0)
            return;

        if (sequenceCoroutine != null)
            StopCoroutine(sequenceCoroutine);

        sequenceCoroutine = StartCoroutine(RunSequence());
    }

    private IEnumerator RunSequence()
    {
        do
        {
            foreach (var action in actions)
            {
                if (action.onStart != null)
                {
                    foreach (var startAction in action.onStart)
                        startAction?.Invoke();
                }

                foreach (var tween in action.tweens)
                {
                    if (tween.targetComponent == null || string.IsNullOrEmpty(tween.propertyName))
                        continue;

                    StartCoroutine(TweenProperty(tween));
                }

                float elapsed = 0f;
                while (elapsed < action.Duration)
                {
                    elapsed += Time.deltaTime;
                    float t = Mathf.Clamp01(elapsed / action.Duration);

                    float curveValue = action.AnimationCurve != null ? action.AnimationCurve.Evaluate(t) : t;

                    if (action.onUpdate != null)
                    {
                        foreach (var updateAction in action.onUpdate)
                            updateAction?.Invoke();
                    }

                    yield return null;
                }

                if (action.onEnd != null)
                {
                    foreach (var endAction in action.onEnd)
                        endAction?.Invoke();
                }
            }

        } while (isLooping);
    }

    private IEnumerator TweenProperty(SequencerTween tween)
    {
        float elapsed = 0f;
        var curve = tween.curve != null ? tween.curve : AnimationCurve.Linear(0, 0, 1, 1);
        var target = tween.targetComponent;
        var type = target.GetType();

        PropertyInfo prop = type.GetProperty(tween.propertyName);
        FieldInfo field = null;

        if (prop == null)
        {
            // Try Field
            field = type.GetField(tween.propertyName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            if (field == null)
            {
                Debug.LogWarning($"Property or Field '{tween.propertyName}' not found on {target.name}");
                yield break;
            }
        }

        while (elapsed < tween.duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / tween.duration);
            float value = Mathf.Lerp(tween.from, tween.to, curve.Evaluate(t));

            object boxedValue = tween.type == TweenTargetType.Int ? (object)Mathf.RoundToInt(value) : value;

            if (prop != null)
                prop.SetValue(target, boxedValue);
            else if (field != null)
                field.SetValue(target, boxedValue);

            yield return null;
        }

        object finalValue = tween.type == TweenTargetType.Int ? (object)Mathf.RoundToInt(tween.to) : tween.to;

        if (prop != null)
            prop.SetValue(target, finalValue);
        else if (field != null)
            field.SetValue(target, finalValue);
    }
}

[Serializable]
public class SequencerEffectStruct
{
    public AnimationCurve AnimationCurve = AnimationCurve.Linear(0, 0, 1, 1);
    public float Duration = 1.0f;
    public List<UnityEvent> onStart;
    public List<UnityEvent> onUpdate;
    public List<UnityEvent> onEnd;
    public List<SequencerTween> tweens;
}

public enum TweenTargetType { Float, Int }

[Serializable]
public class SequencerTween
{
    public TweenTargetType type = TweenTargetType.Float;
    public Component targetComponent;
    public string propertyName;
    public float from;
    public float to;
    public float duration = 1f;
    public AnimationCurve curve = AnimationCurve.Linear(0, 0, 1, 1);
}