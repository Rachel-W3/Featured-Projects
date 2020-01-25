using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Vehicle : MonoBehaviour
{
    // Vectors necessary for force-based movement
    public Vector3 vehiclePosition;
    public Vector3 acceleration;
    public Vector3 direction;
    public Vector3 velocity;

    // Floats
    public float mass;
    public float maxSpeed;
    public float radius;
    public float avoidRadius;

    protected SceneManager sceneManager;

    protected void Start()
    {
        sceneManager = GameObject.Find("SceneManager").GetComponent<SceneManager>();
        vehiclePosition = transform.position;
        radius = 2f;
        // fov = 10f;
        // SetBounds();
    }

    // Update is called once per frame
    protected void Update()
    {
        float scale = 1f;

        // futurePos = transform.position + (velocity.normalized * 3.5f);
        CalcSteeringForces();

        if (gameObject.tag == "Boid")
        {
            ApplyForce(KeepInTank());
        }

        if (sceneManager != null)
        {
            foreach (GameObject obstacle in sceneManager.activeObstacles)
            {
                ApplyForce(AvoidObstacles(obstacle) * scale); // scaled up for boids
            }
        }

        if(gameObject.tag == "Path Follower")
        {
            vehiclePosition.y = Terrain.activeTerrain.SampleHeight(transform.position) + 1f;
            //acceleration.y = 0;
        }

        transform.forward = direction;
        // ------------------------------
        // REMAIN SAME - NO CHANGES
        velocity += acceleration * Time.deltaTime;
        vehiclePosition += velocity * Time.deltaTime;
        direction = velocity.normalized;
        acceleration = Vector3.zero;
        transform.position = vehiclePosition;
        // ------------------------------
    }

    public abstract void CalcSteeringForces();

    public void ApplyForce(Vector3 force)
    {
        acceleration += force / mass;
    }

    #region ----------------- Flocking behavior methods -----------------
    public Vector3 Separation()
    {
        List<GameObject> tooClose = GetNearby(avoidRadius);

        Vector3 steerTotal = new Vector3();
        if (tooClose.Count == 0)
        {
            return steerTotal;
        }

        foreach (GameObject neighbor in tooClose)
        {
            Vector3 steerVector = transform.position - neighbor.transform.position;
            float dist = steerVector.magnitude;
            steerVector = steerVector.normalized / dist;
            steerTotal += steerVector * 3f; // scaled up for visible effect
        }

        return steerTotal;
    }

    public Vector3 Alignment()
    {
        Vector3 desiredVelocity = sceneManager.GetFlockDirection() * maxSpeed;
        Vector3 steeringForce = desiredVelocity - velocity;

        return steeringForce;
    }

    public Vector3 Cohesion()
    {
        Vector3 centroid = sceneManager.GetCentroid();

        return Seek(centroid);
    }
    #endregion

    public Vector3 AvoidObstacles(GameObject obstacle)
    {
        Vector3 vectorToObstacle = obstacle.transform.position - transform.position;
        Vector3 steeringForce = new Vector3();
        float minDist_x = obstacle.GetComponent<Obstacle>().radius_x + avoidRadius;
        float minDist_y = obstacle.GetComponent<Obstacle>().radius_y + avoidRadius;
        float minDist_z = obstacle.GetComponent<Obstacle>().radius_z + avoidRadius;

        // Is obstacle behind?
        if (Vector3.Dot(transform.forward, vectorToObstacle) < 0)
        {
            return Vector3.zero;
        }
        // Far enough ahead?
        if (vectorToObstacle.sqrMagnitude > minDist_z * minDist_z)
        {
            return Vector3.zero;
        }
        // Far enough to the right/left?
        if (Mathf.Abs(Vector3.Dot(transform.right, vectorToObstacle)) >= minDist_x)
        {
            return Vector3.zero;
        }
        // Far enough below/above?
        if(Mathf.Abs(Vector3.Dot(transform.up, vectorToObstacle)) >= minDist_y)
        {
            return Vector3.zero;
        }

        // If all fails...
        // Is obstacle on the right?
        if (Vector3.Dot(transform.right, vectorToObstacle) > 0)
        {
            steeringForce += -(transform.right * maxSpeed);
        }
        else
        {
            steeringForce += transform.right * maxSpeed;
        }
        // Is obstacle below?
        if (Vector3.Dot(transform.up, vectorToObstacle) < 0)
        {
            steeringForce += transform.up * maxSpeed;
        }
        else
        {
            steeringForce += -(transform.up * maxSpeed);
        }

        return steeringForce;
    }

    public List<GameObject> GetNearby(float area)
    {
        List<GameObject> members = new List<GameObject>();
        
        members.AddRange(sceneManager.activeBoids);

        // This game object shouldn't have to worry about
        // avoiding itself
        members.Remove(gameObject);

        List<GameObject> tooClose = new List<GameObject>();

        foreach(GameObject member in members)
        {
            Vector3 vToNeighbor = member.transform.position - transform.position;
            if (area >= vToNeighbor.magnitude)
            {
                tooClose.Add(member);
            }
        }

        return tooClose;
    }

    public GameObject GetNearestObject(List<GameObject> objList)
    {
        GameObject nearestObject = null;
        float minDistSqr = Mathf.Infinity;
        Vector3 currentPos = transform.position;

        if (objList.Count > 0)
        {
            foreach (GameObject potentialNeighbor in objList)
            {
                Vector3 directionToTarget = potentialNeighbor.transform.position - currentPos;
                float distanceSqrToTarget = directionToTarget.sqrMagnitude;
                if (distanceSqrToTarget < minDistSqr)
                {
                    minDistSqr = distanceSqrToTarget;
                    nearestObject = potentialNeighbor;
                }
            }
        }

        return nearestObject;
    }

    /// <summary>
    /// Temporarily used to move boids until flocking behavior is implemented
    /// </summary>
    public Vector3 Wander()
    {
        Vector3 circlePos = transform.position + transform.forward * 2f;
        float radius = 2f;
        float angle = Random.Range(0, 180);

        Vector3 wanderTo = new Vector3();
        wanderTo.x = circlePos.x + Mathf.Cos(angle) * radius;
        wanderTo.z = circlePos.z + Mathf.Sin(angle) * radius;

        return Seek(wanderTo);
    }

    public Vector3 KeepInTank()
    {
        if (vehiclePosition.x < -SceneManager.tankBounds + 2f || vehiclePosition.x > SceneManager.tankBounds - 2f ||
            vehiclePosition.z < -SceneManager.tankBounds + 2f || vehiclePosition.z > SceneManager.tankBounds - 2f ||
            vehiclePosition.y <= Terrain.activeTerrain.SampleHeight(vehiclePosition) + 2f || vehiclePosition.y >= SceneManager.tankHeight)
        {
            return Seek(new Vector3(0, SceneManager.tankHeight / 2, 0)) * 3f;
        }
        else
        {
            return Vector3.zero;
        }
    }

    public Vector3 Seek(Vector3 targetPosition)
    {
        // Step 1: Find DV (desired velocity)
        // TargetPos - CurrentPos
        Vector3 desiredVelocity = targetPosition - vehiclePosition;

        // Step 2: Scale vel to max speed
        // desiredVelocity = Vector3.ClampMagnitude(desiredVelocity, maxSpeed);
        desiredVelocity.Normalize();
        desiredVelocity = desiredVelocity * maxSpeed;

        // Step 3:  Calculate seeking steering force
        Vector3 seekingForce = desiredVelocity - velocity;

        // Step 4: Return force
        return seekingForce;
    }

    public Vector3 Seek(GameObject target)
    {
        return Seek(target.transform.position);
    }

    #region Method graveyard
    //public void Wrap()
    //{
    //    // Constraining x-axis of vehicle
    //    if (vehiclePosition.x < leftBound)
    //    {
    //        vehiclePosition.x = rightBound;
    //    }
    //    else if (vehiclePosition.x > rightBound)
    //    {
    //        vehiclePosition.x = leftBound;
    //    }
    //    // Constraining y-axis of vehicle
    //    if (vehiclePosition.z < bottomBound)
    //    {
    //        vehiclePosition.z = topBound;
    //    }
    //    else if (vehiclePosition.z > topBound)
    //    {
    //        vehiclePosition.z = bottomBound;
    //    }
    //}

    //public Vector3 Pursue(GameObject target)
    //{
    //    return Seek(target.GetComponent<Vehicle>().futurePos);
    //}

    //public Vector3 Flee(Vector3 targetPos)
    //{
    //    // Step 1: Find DV (desired velocity)
    //    Vector3 desiredVelocity = transform.position - targetPos;

    //    // Step 2: Scale vel to max speed
    //    // desiredVelocity = Vector3.ClampMagnitude(desiredVelocity, maxSpeed);
    //    desiredVelocity.Normalize();
    //    desiredVelocity = desiredVelocity * maxSpeed;

    //    // Step 3:  Calculate fleeing steering force
    //    Vector3 fleeingForce = desiredVelocity - velocity;

    //    // Step 4: Return force
    //    return fleeingForce;
    //}

    //public Vector3 Flee(GameObject target)
    //{
    //    return Flee(target.transform.position);
    //}

    //public Vector3 Evade(GameObject target)
    //{
    //    return Flee(target.GetComponent<Vehicle>().futurePos);
    //}

    //// Helper for KeepInBounds method
    //void SetBounds()
    //{
    //    float floorLength = sceneManager.floor.transform.localScale.z;
    //    float floorWidth = sceneManager.floor.transform.localScale.x;

    //    topBound = (floorLength / 2);
    //    bottomBound = -topBound;
    //    rightBound = (floorWidth / 2);
    //    leftBound = -rightBound;
    //}
    #endregion
}
