using System;
using UnityEngine;

// adding skid mark effects adapted from this tutorial - https://www.youtube.com/watch?v=0LOcxZhkVwc&t=188s
public class CarEffects : MonoBehaviour
{
    public TrailRenderer[] tireMarks;
    public Wheel[] wheels;

    public float skidThreshold = 8.0f;

    private bool tireMarksFlag;

    private void Update()
    {
        CheckDrift();
    }

    private void CheckDrift()
    {
        bool isSkidding = false;

        foreach (Wheel wheel in wheels)
        {
            if (wheel.IsGrounded && wheel.SidewaysSlip > skidThreshold)
            {
                isSkidding = true;
                break;
            }
        }

        if (isSkidding)
            StartEmitter();
        else
            StopEmitter();
    }

    private void StartEmitter()
    {
        if (tireMarksFlag) return;

        foreach (TrailRenderer trail in tireMarks)
        {
            trail.emitting = true;
        }

        tireMarksFlag = true;
    }

    private void StopEmitter()
    {
        if (!tireMarksFlag) return;

        foreach (TrailRenderer trail in tireMarks)
        {
            trail.emitting = false;
        }

        tireMarksFlag = false;
    }

}
