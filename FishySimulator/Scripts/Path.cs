using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Path : MonoBehaviour
{
    public GameObject[] waypoints;
    public int curr_WP_index;

    public Material mat1;
    public SceneManager sceneManager;

    // Start is called before the first frame update
    void Start()
    {
        curr_WP_index = 0;
        sceneManager = GameObject.Find("SceneManager").GetComponent<SceneManager>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public GameObject NextWP()
    {
        curr_WP_index++;

        if (curr_WP_index >= waypoints.Length)
        {
            curr_WP_index = 0;
        }

        return waypoints[curr_WP_index];
    }

    void OnRenderObject()
    {
        if(sceneManager.debugLinesOn)
        {
            mat1.SetPass(0);

            for (int i = 0; i < waypoints.Length; i++)
            {
                Vector3 wpPos = waypoints[i].transform.position;
                wpPos.y += 0.7f;

                GL.Begin(GL.LINES);
                GL.Vertex(wpPos);

                Vector3 nextWPPos = waypoints[0].transform.position;
                if(i != waypoints.Length - 1)
                {
                    nextWPPos = waypoints[i + 1].transform.position;
                }
                nextWPPos.y += 0.7f;

                GL.Vertex(nextWPPos);
                GL.End();
            }
        }
    }
}
