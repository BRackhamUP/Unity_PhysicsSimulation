using UnityEngine;

/// <summary>
/// Surface properties to showcase effective grip for wheels on different terrains
/// </summary>
public class SurfaceProperties : MonoBehaviour
{
    public enum Surfaces
    {
        Cement,
        Dirt,
        Ice
    }

    // surface parameters
    public Surfaces surface;
    public float gripMultiplier;
    public float gripAtStopMultiplier;
    public float slideResistanceMultiplier;

    private void OnValidate()
    {
        ApplySurface(surface);
    }

    // adjusting different multipliers to simulate different terrains for my vehicles
    private void ApplySurface(Surfaces type)
    {
        switch (type)
        {
            case Surfaces.Cement:
                gripMultiplier = 1.0f;
                gripAtStopMultiplier = 1.0f;
                slideResistanceMultiplier = 1.0f;
                break;

            case Surfaces.Dirt:
                gripMultiplier = 0.85f;
                gripAtStopMultiplier = 0.9f;
                slideResistanceMultiplier = 1.1f;
                break;

            case Surfaces.Ice:
                gripMultiplier = 0.15f;
                gripAtStopMultiplier = 0.1f;
                slideResistanceMultiplier = 0.3f;
                break;
        }
    }
}

