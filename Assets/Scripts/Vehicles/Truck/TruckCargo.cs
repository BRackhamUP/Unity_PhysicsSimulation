using UnityEngine;

public class TruckCargo : MonoBehaviour
{
    public Transform cargoPoint;
    public Transform dropPoint;
    public float pickupRadius = 10;
    public string rockTag = "Rock";

    public TruckBedArea truckBed;

    /// <summary>
    /// using 'Physics.OverlapSphere' - https://docs.unity3d.com/6000.0/Documentation/ScriptReference/Physics.OverlapSphere.html
    /// To send a check out for all colliders within the pickup radius that hold the tag 'Rock' and add them to an array of 'hits'
    /// check if the hits contain a rigidbody, and rockTag, also check if they are already currently in the bed of the truck (collider)
    /// if not, teleport to cargopoint position
    /// </summary>
    public void PickUpRock()
    {
        // store the hit results in an array 
        Collider[] hits = Physics.OverlapSphere(transform.position, pickupRadius);

        foreach (Collider collider in hits)
        {
            // get the rigidbody of the rock 
            Rigidbody rb = collider.attachedRigidbody ?? collider.GetComponentInParent<Rigidbody>();

            // continue if no rocks present
            if (rb == null) 
                continue;

            // ignore other objects
            if (!rb.CompareTag(rockTag)) 
                continue;

            // ignore rocks already in truck bed
            if (truckBed.IsInBed(rb)) 
                continue;

            // teleport rocks to cargopoint
            rb.transform.position = cargoPoint.position;
            rb.transform.rotation = cargoPoint.rotation;

            return; 
        }
    }

    /// <summary>
    /// Get any rock in the rock bed and teleport to the drop point 
    /// </summary>
    public void DropRock()
    {
        // get a rock from the truck bed
        Rigidbody rock = truckBed.GetAnyRock();

        // return if no rocks present
        if (rock == null)
            return;

        // teleport rock to droppoint
        rock.transform.position = dropPoint.position;
        rock.transform.rotation = dropPoint.rotation;
    }
}