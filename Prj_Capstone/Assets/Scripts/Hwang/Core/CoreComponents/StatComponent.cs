using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public enum TurnPeriod { Start, End };

[Serializable]
public class StatComponent
{
    public string name { get; set; }
    public Entity entity { get; set; }

    public event Action OnCurrentValueMax;
    public event Action OnCurrentValueMin;

    [SerializeField] private Slider slider;

    [field: SerializeField] public float maxValue { get; private set; }
    [field: SerializeField] public float minValue { get; private set; } = 0.0f;
    [field: SerializeField] public float initialValue { get; private set; }
    [field: SerializeField] public float recoveryValue { get; private set; }
    public float currentValue { get; private set; }

    private float originalValue;
    private const float epsilon = 0.001f;
    

    public void Init()
    {
        currentValue = initialValue;
    }

    // TODO: Add turn base logic to stats
    /// <summary>
    /// Duration turn of 0 means that it will change the value only current turn. Duration turn of 1 means that it will change the value on this turn and the next turn.
    /// </summary>
    /// <param name="amount"></param>
    /// <param name="allowMaxValue"></param>
    /// <param name="durationTurn"></param>
    /// <param name="turnPeriod"></param>
    public void IncreaseCurrentValue(float amount, bool allowMaxValue = true, int durationTurn = 0, TurnPeriod turnPeriod = TurnPeriod.Start)
    {
        _IncreaseCurrentValue(amount, allowMaxValue);

        if (durationTurn > 0)
        {
            int endTurn = Manager.Instance.gameManager.currentTurnCount + durationTurn;

            Action turnAction = null;
            turnAction = () =>
            {
                _IncreaseCurrentValue(amount, allowMaxValue);

                if (Manager.Instance.gameManager.currentTurnCount >= endTurn)
                {
                    Manager.Instance.gameManager.playerTurnStart -= turnAction;
                }
            };

            Manager.Instance.gameManager.playerTurnStart += turnAction;
        }
    }

    private void _IncreaseCurrentValue(float amount, bool allowMaxValue = true)
    {
        currentValue += amount;
        currentValue = allowMaxValue ? Mathf.Clamp(currentValue, minValue, maxValue) : Mathf.Clamp(currentValue, minValue, maxValue - epsilon);
        SetSliderValue();

        if (currentValue.Equals(maxValue))
        {
            OnCurrentValueMax?.Invoke();
        }
    }

    public void DecreaseCurrentValue(float amount, bool allowMinValue = true, int durationTurn = 0, TurnPeriod turnPeriod = TurnPeriod.Start)
    {
        currentValue -= amount;
        currentValue = allowMinValue ? Mathf.Clamp(currentValue, minValue, maxValue) : Mathf.Clamp(currentValue, minValue + epsilon, maxValue);
        SetSliderValue();

        if (currentValue.Equals(minValue))
        {
            OnCurrentValueMin?.Invoke();
        }
    }

    /// <summary>
    /// This function returns the value to original value. It will get total value change and subtract it after duration turn.
    /// </summary>
    /// <param name="durationTurn"></param>
    public void ReturnToOriginalValue(float totalValueChange, int durationTurn)
    {
        int endTurn = Manager.Instance.gameManager.currentTurnCount + durationTurn;

        Action turnAction = null;
        turnAction = () =>
        {
            if (Manager.Instance.gameManager.currentTurnCount >= endTurn)
            {
                DecreaseCurrentValue(totalValueChange, true);
                Manager.Instance.gameManager.playerTurnStart -= turnAction;
            }
        };

        Manager.Instance.gameManager.playerTurnStart += turnAction;
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
