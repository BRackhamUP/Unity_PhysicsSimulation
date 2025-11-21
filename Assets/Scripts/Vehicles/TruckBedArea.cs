using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Collider))]
public class TruckBedArea : MonoBehaviour
{
    public string rockTag = "Rock";

    // create a hashset to keep track of the rocks currently within the collider of the truck bed
    private readonly HashSet<Rigidbody> rocksInBed = new HashSet<Rigidbody>();

    /// <summary>
    /// simple foreach to scan the truck bed collider for a rock within, to call DropRock()
    /// </summary>
    public Rigidbody GetAnyRock()
    {
        foreach (var rock in rocksInBed)
        {
            return rock;
        }
        return null;
    }

    /// <summary>
    /// Checking the rigidbody of the rock in the truck bed to prevent trying to pickUp that rock again
    /// </summary>
    public bool IsInBed(Rigidbody rb)
    {
        return rocksInBed.Contains(rb);
    }

    /// <summary>
    /// check the collider triggers with the rocks collider, ensure the rock collider is not null, compare tag, add to the rocksinbed hashset
    /// </summary>
    private void OnTriggerEnter(Collider rockCollider)
    {
        Rigidbody rb = rockCollider.attachedRigidbody;

        if (rb == null)
        {
            rb = rockCollider.GetComponentInParent<Rigidbody>();
        }


        if (rb == null) return;
        if (!rb.CompareTag(rockTag)) return;

        rocksInBed.Add(rb);
    }

    /// <summary>
    /// check the collider triggers with the rocks collider, ensure the rock collider is not null, compare tag, remove from hashset
    /// </summary>
    private void OnTriggerExit(Collider rockCollider)
    {
        Rigidbody rb = rockCollider.attachedRigidbody;

        if (rb == null)
        {
            rb = rockCollider.GetComponentInParent<Rigidbody>();
        }


        if (rb == null) return;
        if (!rb.CompareTag(rockTag)) return;

        rocksInBed.Remove(rb);
    }
}
