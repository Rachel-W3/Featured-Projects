using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PathFollower : Vehicle
{
    public GameObject targetWP;
    public Path path;

    // Start is called before the first frame update
    void Start()
    {
        path = GameObject.Find("Path").GetComponent<Path>();
        targetWP = path.waypoints[0];
        vehiclePosition = targetWP.transform.position;
    }

    // Update is called once per frame
    void Update()
    {

        base.Update();

        vehiclePosition = transform.position;
        vehiclePosition.y = Terrain.activeTerrain.SampleHeight(vehiclePosition);

        transform.position = vehiclePosition;
    }

    public override void CalcSteeringForces()
    {
        Vector3 totalForce = new Vector3();

        totalForce += ToNextWP() * 0.5f; //scaled down

        // Scales totalForce to maxSpeed
        totalForce.Normalize();
        totalForce = totalForce * maxSpeed;

        ApplyForce(totalForce);
    }

    public Vector3 ToNextWP()
    {
        Vector3 vToWP = targetWP.transform.position - transform.position;
        float minDist = targetWP.GetComponent<Waypoint>().radius + avoidRadius;
        
        if(vToWP.magnitude < minDist)
        {
            targetWP = path.NextWP();
        }

        return Seek(targetWP);
    }
}
