using UnityEngine;

// adding skid mark effects adapted from this tutorial - https://www.youtube.com/watch?v=0LOcxZhkVwc&t=188s
public class CarEffects : MonoBehaviour
{
    public TrailRenderer[] tireMarks;
    public Wheel[] wheels;

    public float skidThreshold = 25.0f;
    private bool tireMarksFlag;

    private void Update()
    {
        CheckDrift();
    }

    // check to see if the wheels are meting the ski threshold
    private void CheckDrift()
    {
        // initial state
        bool isSkidding = false;

        // check if wheels are exceeding the skidThreshold
        foreach (Wheel wheel in wheels)
        {
            if (wheel.IsGrounded && wheel.SidewaysSlip > skidThreshold)
            {
                isSkidding = true;
                break;
            }
        }

        // start emitting skid marks if is skidding
        if (isSkidding)
            StartEmitter();
        else
            StopEmitter();
    }

    // start emitting the trailrenderer
    private void StartEmitter()
    {
        if (tireMarksFlag) 
            return;

        // emit trailrenderer from each of the wheel points
        foreach (TrailRenderer trail in tireMarks)
        {
            trail.emitting = true;
        }
        tireMarksFlag = true;
    }

    // stop emitting trailrenderer
    private void StopEmitter()
    {
        if (!tireMarksFlag) return;

        // stop emitting trailrenderer
        foreach (TrailRenderer trail in tireMarks)
        {
            trail.emitting = false;
        }
        tireMarksFlag = false;
    }
}
