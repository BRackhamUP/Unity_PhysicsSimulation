using UnityEngine;

public class TrackCar : Vehicle
{
    public float downforce;

    public override void UpdatePhysics(float deltaTime)
    {
        throw new System.NotImplementedException();

        //aerodynamic drag, speed limits etc...
    }
}
