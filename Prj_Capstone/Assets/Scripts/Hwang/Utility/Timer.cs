using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
/// <summary>
/// Timer class for replacement of coroutines
/// Timer must be created first and then must write Tick() function in Update/FixedUpdate function
/// Timer resets its time if it is called before its assigned function gets invoked. It does not invoke many times.
/// </summary>
public class Timer
{
    public event Action timerAction;

    private float duration;
    private float startTime;
    private float timeOffset;

    private bool timerActive;
    private bool isSingleUse;
    private bool resetStartTime;
    private bool isAdjustTimeSingleUse;

    private int maxMultiUseAmount;
    private int currentMultiUseAmount;

    public Timer(float duration)
    {
        this.duration = duration;
        timerActive = false;
        resetStartTime = true;
    }

    public void Tick(bool condition = true)
    {
        startTime += Time.deltaTime * (1.0f - Time.timeScale);

        if (condition)
        {
            if (timerActive)
            {
                if (Time.time + timeOffset > startTime + duration)
                {
                    timerAction?.Invoke();

                    if (isSingleUse)
                    {
                        StopTimer();

                        if (isAdjustTimeSingleUse)
                        {
                            timeOffset = 0.0f;
                        }
                    }
                    else
                    {
                        startTime = Time.time;

                        if (maxMultiUseAmount != 0)
                        {
                            if (currentMultiUseAmount < maxMultiUseAmount)
                            {
                                currentMultiUseAmount += 1;
                            }
                            else
                            {
                                StopTimer();
                            }
                        }
                    }
                }
            }
        }
    }

    /// <summary>
    /// Used when you need to renew the startTime every time the condition is fit
    /// </summary>
    /// <param name="condition"></param>
    public void TickResetTime(bool condition)
    {
        startTime += Time.deltaTime * (1.0f - Time.timeScale);

        if (condition)
        {
            if (timerActive)
            {
                if (Time.time + timeOffset > startTime + duration)
                {
                    timerAction?.Invoke();

                    if (isSingleUse)
                    {
                        StopTimer();

                        if (isAdjustTimeSingleUse)
                        {
                            timeOffset = 0.0f;
                        }
                    }
                    else
                    {
                        startTime = Time.time;

                        if (maxMultiUseAmount != 0)
                        {
                            if (currentMultiUseAmount < maxMultiUseAmount)
                            {
                                currentMultiUseAmount += 1;
                            }
                            else
                            {
                                StopTimer();
                            }
                        }
                    }
                }
            }
        }
        else
        {
            startTime = Time.time;
        }
    }

    public void StartSingleUseTimer()
    {
        timerActive = true;
        isSingleUse = true;
        startTime = Time.time;
    }

    public void StartMultiUseTimer(int maxMultiUseAmount = 0)
    {
        timerActive = true;
        isSingleUse = false;
        startTime = Time.time;
        currentMultiUseAmount = 0;
        this.maxMultiUseAmount = maxMultiUseAmount;
    }

    public void ChangeDuration(float duration)
    {
        this.duration = duration;
    }

    public void ChangeStartTime(float startTime)
    {
        this.startTime = startTime;
    }

    public void StopTimer()
    {
        timerActive = false;
    }

    public void AdjustTimeFlow(float adjustTimeAmount, bool isAdjustTimeSingleUse = true)
    {
        timeOffset = adjustTimeAmount;
        this.isAdjustTimeSingleUse = isAdjustTimeSingleUse;
    }
}
