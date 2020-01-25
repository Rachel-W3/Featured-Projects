using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Boid : Vehicle
{
    //public GameObject nearestEnemy;

    public Material mat1;
    public Material mat2;

    // Start is called before the first frame update
    void Start()
    {
        base.Start();
        avoidRadius = 1.5f;
    }

    void Update()
    {
        base.Update();
    }

    public override void CalcSteeringForces()
    {
        Vector3 totalForce = new Vector3();

        //foreach (GameObject obstacle in sceneManager.activeObstacles)
        //{
        //    totalForce += AvoidObstacles(obstacle) * 100f; // Scaled up for stronger effect
        //}

        totalForce += Separation();
        totalForce += Alignment();
        totalForce += Cohesion();

        // Scales totalForce to maxSpeed
        totalForce.Normalize();
        totalForce = totalForce * maxSpeed;

        ApplyForce(totalForce);
    }

    Vector3 CalcFriction(float coeff)
    {
        Vector3 friction = velocity * -1;
        friction.Normalize();
        friction = friction * coeff;
        return friction;
    }

    #region Code graveyard
    //// Evading nearby zombies
    //List<GameObject> tooClose = GetNearby("Zombie", avoidRadius);
    //List<GameObject> inFOV = GetNearby("Zombie", fov);
    //if (tooClose.Count == 0)
    //{
    //    if (inFOV.Count == 0)
    //    {
    //        totalForce += Wander();
    //    }
    //    else
    //    {
    //        foreach (GameObject zombie in inFOV)
    //        {
    //            totalForce += Evade(zombie);
    //        }
    //    }
    //}
    //else
    //{
    //    foreach (GameObject zombie in tooClose)
    //    {
    //        float dToZombie = Vector3.Distance(zombie.transform.position, transform.position);
    //        totalForce += Flee(zombie) * (5 / dToZombie);
    //    }
    //}

    //void OnRenderObject()
    //{
    //    if (sceneManager.debugLinesOn)
    //    {
    //        // Set the material to be used for the forward line
    //        mat1.SetPass(0);
    //        // Draws one line
    //        GL.Begin(GL.LINES); // Begin to draw lines
    //        GL.Vertex(transform.position); // First endpoint of this line
    //        GL.Vertex(transform.position + (transform.forward * 3f)); // Second endpoint of this line
    //        GL.End(); // Finish drawing the line

    //        // Set another material to draw the right line in a different color
    //        mat2.SetPass(0);
    //        GL.Begin(GL.LINES);
    //        GL.Vertex(transform.position);
    //        GL.Vertex(transform.position + (transform.right * 3f));
    //        GL.End();
    //    }
    //}
    #endregion
}
