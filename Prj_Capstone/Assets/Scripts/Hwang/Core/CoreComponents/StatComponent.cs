using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[Serializable]
public class StatComponent
{
    public event Action OnCurrentValueMax;
    public event Action OnCurrentValueMin;

    [SerializeField] private Slider slider;

    [field: SerializeField] public float maxValue { get; private set; }
    [field: SerializeField] public float minValue { get; private set; } = 0.0f;
    [field: SerializeField] public float initialValue { get; private set; }
    [field: SerializeField] public float recoveryValue { get; private set; }
    public float currentValue { get; private set; }

    private const float epsilon = 0.001f;

    public void Init()
    {
        currentValue = initialValue;
    }

    // TODO: Add turn base logic to stats
    public void IncreaseCurrentValue(float amount, bool allowMaxValue = true, int durationTurn = 1, bool returnToInitialValue = false)
    {
        currentValue += amount;
        currentValue = allowMaxValue ? Mathf.Clamp(currentValue, minValue, maxValue) : Mathf.Clamp(currentValue, minValue, maxValue - epsilon);
        SetSliderValue();

        if (currentValue.Equals(maxValue))
        {
            OnCurrentValueMax?.Invoke();
        }
    }

    public void DecreaseCurrentValue(float amount, bool allowMinValue = true, int durationTurn = 1, bool returnToInitialValue = false)
    {
        currentValue -= amount;
        currentValue = allowMinValue ? Mathf.Clamp(currentValue, minValue, maxValue) : Mathf.Clamp(currentValue, minValue + epsilon, maxValue);
        SetSliderValue();

        if (currentValue.Equals(minValue))
        {
            OnCurrentValueMin?.Invoke();
        }
    }

    public void IncreaseMaxValue(float amount)
    {
        maxValue += amount;
        IncreaseCurrentValue(amount);
        SetSliderValue();
    }

    public void DecreaseMaxValue(float amount)
    {
        maxValue -= amount;
        Mathf.Clamp(currentValue, minValue, maxValue);
        SetSliderValue();
    }

    private void SetSliderValue()
    {
        if (slider != null)
        {
            slider.value = currentValue / maxValue;
        }
    }
}
