using UnityEngine;

public class SurfaceProperties : MonoBehaviour
{
    [Range(0f, 1f)]
    public float frictionCoefficient = 0.5f; // 0 to 1 = amount of grip.
}
