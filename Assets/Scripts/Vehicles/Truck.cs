using UnityEngine;

public class Truck : Vehicle
{
    public float cargoCapacity;

    public override void UpdatePhysics(float deltaTime)
    {
        throw new System.NotImplementedException();

        //heavier handling, slower acceleration etc...
    }
}
