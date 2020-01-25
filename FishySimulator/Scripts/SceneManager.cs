using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SceneManager : MonoBehaviour
{
    public GameObject centroidPrefab;
    public GameObject boidPrefab;
    public GameObject pfPrefab;
    public int boidCount;
    public int camCount;
    public List<GameObject> activeBoids;
    public List<GameObject> activeObstacles;
    public GameObject pathFollower;
    public static float tankBounds = 12;
    public static float tankHeight = 8;

    // Camera stuff
    public Camera[] cameras;
    private int currentCameraIndex;

    // Debugging
    public GameObject centroid;
    public Material mat1;
    public bool debugLinesOn;

    // Start is called before the first frame update
    void Start()
    {
        // Environment settings
        RenderSettings.fogColor = Camera.main.backgroundColor;
        RenderSettings.fogDensity = 0.03f;
        RenderSettings.fog = true;

        activeBoids = new List<GameObject>();
        activeObstacles = new List<GameObject>();

        #region Camera Control
        // Turn all cameras off, except the first default one
        for (int i = 1; i < cameras.Length; i++)
        {
            cameras[i].gameObject.SetActive(false);
        }

        // If any cameras were added to the controller, enable the first one
        if (cameras.Length > 0)
        {
            cameras[0].gameObject.SetActive(true);
        }

        currentCameraIndex = 0;
        #endregion

        for (int i = 0; i < boidCount; i++)
        {
            Vector3 spawnPos = new Vector3(Random.Range(-tankBounds, tankBounds), Random.Range(1.5f, tankHeight), Random.Range(-tankBounds, tankBounds));
            activeBoids.Add(Instantiate(boidPrefab, spawnPos, Quaternion.Euler(0, 90, 0)));
        }

        pathFollower = Instantiate(pfPrefab, Vector3.zero, Quaternion.identity);

        foreach (GameObject obstacle in GameObject.FindGameObjectsWithTag("Obstacle"))
        {
            activeObstacles.Add(obstacle);
        }

        centroid = Instantiate(centroidPrefab, GetCentroid(), Quaternion.identity);
        centroid.GetComponent<Renderer>().enabled = false;

        for (int i = 1; i < cameras.Length; i++)
        {
            if(i == cameras.Length - 1)
            {
                cameras[i].GetComponent<SmoothFollow>().target = pathFollower.transform;
            }
            else
            {
                cameras[i].GetComponent<SmoothFollow>().target = centroid.transform;
            }
        }

        debugLinesOn = false;
    }

    // Update is called once per frame
    void Update()
    {
        // If user presses D on keyboard, debug lines are toggled
        if (Input.GetKeyDown(KeyCode.D))
        {
            debugLinesOn = !debugLinesOn;
        }
        centroid.transform.position = GetCentroid();

        Vector3 centroidDirection = GetFlockDirection();

        if (currentCameraIndex == 2)
        {
            centroid.transform.forward = -centroidDirection;
        }
        else
        {
            centroid.transform.forward = centroidDirection;
        }

        #region Camera controls
        // Press the 'C' key to cycle through cameras in the array
        if (Input.GetKeyDown(KeyCode.C))
        {
            // Cycle to the next camera
            currentCameraIndex++;

            // If cameraIndex is in bounds, set this camera active and last one inactive
            if (currentCameraIndex < cameras.Length)
            {
                cameras[currentCameraIndex - 1].gameObject.SetActive(false);
                cameras[currentCameraIndex].gameObject.SetActive(true);
            }
            // If last camera, cycle back to first camera
            else
            {
                cameras[currentCameraIndex - 1].gameObject.SetActive(false);
                currentCameraIndex = 0;
                cameras[currentCameraIndex].gameObject.SetActive(true);
            }
        }
        #endregion
    }

    /// <summary>
    /// Getting flock's general direction for Alignment
    /// </summary>
    /// <returns></returns>
    public Vector3 GetFlockDirection()
    {
        Vector3 flockDir = new Vector3();

        // Get sum of all other boids' forward vectors
        foreach (GameObject boid in activeBoids)
        {
            flockDir += boid.transform.forward;
        }

        return flockDir.normalized;
    }

    /// <summary>
    /// Getting centroid for Cohesion
    /// </summary>
    /// <returns></returns>
    public Vector3 GetCentroid()
    {
        Vector3 sum = new Vector3();

        // Get sum of all other boids' positions
        foreach (GameObject boid in activeBoids)
        {
            sum += boid.transform.position;
        }

        return (sum / activeBoids.Count);
    }

    private void OnRenderObject()
    {
        if(debugLinesOn)
        {
            centroid.GetComponent<Renderer>().enabled = true;
            mat1.SetPass(0);
            GL.Begin(GL.LINES);
            GL.Vertex(centroid.transform.position);
            GL.Vertex(centroid.transform.position + centroid.transform.forward * 2f);
            GL.End();
        }
        else
        {
            centroid.GetComponent<Renderer>().enabled = false;
        }
    }

    private void OnGUI()
    {
        GUI.color = Color.white;
        GUI.TextArea(new Rect(150, 30, 115, 50), "Press 'D' to toggle debug lines");

        GUI.color = Color.white;
        GUI.TextArea(new Rect(150, 100, 175, 75), "Press 'C' to change camera views\n\nCurrent: " + cameras[currentCameraIndex].name);
    }
}
