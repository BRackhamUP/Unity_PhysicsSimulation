using UnityEngine;

public class SurfaceProperties : MonoBehaviour
{
    public enum Surfaces
    {
        Cement,
        Dirt,
        Ice
    }

    public Surfaces surface;

    public float gripMultiplier;
    public float gripAtStopMultiplier;
    public float slideResistanceMultiplier;

    private void OnValidate()
    {
        ApplySurface(surface);
    }

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

