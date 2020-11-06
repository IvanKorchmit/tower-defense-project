using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Pathfinding;
public class Movement : MonoBehaviour
{
    private GraphNode gn1;
    private GraphNode gn2;
    private Path path;
    private Seeker seeker;
    public int currentWayPoint = 0;
    private Vector2 target;
    public bool isReached;
    private void Start()
    {
        seeker = GetComponent<Seeker>();
        InvokeRepeating("NextStep", 0, 0.1f);
    }
    private void Update()
    {
        if(Input.GetKeyDown(KeyCode.Mouse0))
        {
            Vector2 mousePos = new Vector2(Mathf.Round(Input.mousePosition.x), Mathf.Round(Input.mousePosition.y));

            target = new Vector2(Mathf.Round(Camera.main.ScreenToWorldPoint(mousePos).x)+0.5f, 
            Mathf.Round(Camera.main.ScreenToWorldPoint(mousePos).y)+0.5f);
            gn1 = AstarPath.active.GetNearest(transform.position).node;
            gn2 = AstarPath.active.GetNearest(target).node;
            isReached = false;
            currentWayPoint = 0;
            MoveTo(target, false);
        }
    }
    public void MoveTo(Vector2 target, bool isObstacle)
    {
        GraphMask gmask = GraphMask.FromGraphName("3x3");
        path = null;
        gn1 = AstarPath.active.GetNearest(transform.position).node;
        if (!isObstacle)
        {
            gn2 = AstarPath.active.GetNearest(target).node;
            seeker.StartPath(transform.position, target, onPathComplete,gmask);
        }
        else
        {
            int attempt = 0;
            while (true)
            {
                if (attempt == 10)
                {
                    Debug.Log("Failed");
                    break;
                }
                Vector2 offset = new Vector2((int)Random.Range(-2, 2), Random.Range(-2, 2));
                gn2 = AstarPath.active.GetNearest(target + offset).node;
                //if (PathUtilities.IsPathPossible(gn1, gn2))
                //{
                    seeker.StartPath(transform.position, target + offset, onPathComplete, gmask);
                    break;
                //}
                //else
                //{
                //    attempt++;
                //}
            }
        }
    }
    public void NextStep()
    {
        #region Pathfinding and Movement
        if (isReached)
        {
            return;
        }
        float distance = 0;
        if (path != null)
        {
            if (!isReached)
                distance = Vector2.Distance(transform.position, path.vectorPath[currentWayPoint]);
        }
        else
        {
            return;
        }
        if (currentWayPoint < path.vectorPath.Count - 1)
        {
            if (!isReached)
            {
                currentWayPoint++; // Increments current way point by comparing distance and WP distance so enemy can go.
                transform.position = (Vector2)path.vectorPath[currentWayPoint];
            }
        }
        if (currentWayPoint == path.vectorPath.Count - 1)
        {
            isReached = true;
        }
        #endregion
    }
    private void onPathComplete(Path p)
    {
        if (!p.error)
        {
            isReached = false;
            currentWayPoint = 0; // If path is available and has no errors, then argument p will be applied for path variable
            path = p;
        }
    }
}
