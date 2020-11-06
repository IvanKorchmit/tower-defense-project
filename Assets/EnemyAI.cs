using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Pathfinding;
using UnityEngine.Tilemaps;

public class EnemyAI : MonoBehaviour
{
    #region Components & technical fields
    #region AI Components
    private Path path;
    private Color initColor;
    private int waterTick = 0;
    public int needsTick = 0;
    public GameObject WorkingTarget;
    public int currentWayPoint = 0;
    public bool isReached; // Checks if the path ended
    public Interactable.ResourceType Carrying; // What is he/she carrying right now?
    private Tilemap tilemap;
    public PathTask task; // Task for Pathfinding to complete
    public enum PathTask
    {
        Carry, None, Fight, Drink, Eat
    }
    public Interactable.ResourceType needForConstruction; // What unit needs to bring to the contruction yard?
    public GameObject ConstructionYard; // Which yard is he or she working on?
    #endregion
    public GameObject TargetAttack;
    private Seeker seeker;
    private SpriteRenderer spriteRenderer;
    public Resources resources; // Reference to the storage
    private Vector2 WaterPosition; // Position of water source to drink
    public bool isDrinking;
    public int speed;
    private int currentStep;
    private Vector2 targetOldPosition;
    #endregion
    public int Health = 100;
    public int Hunger = 100;
    public int Thirst = 100;
    public Unit.Status status;
    private void OnFinishedPath()
    {
        isReached = true;
    }
    public void DrinkWater()
    {
        GraphMask gmask = GraphMask.FromGraphName("1x1");
        spriteRenderer.color = initColor;
        int WaterDist = (int)Vector2.Distance(transform.position, WaterPosition);
        if (WaterDist >= 0 && WaterDist <= 1)
        {
            if (task != PathTask.Carry)
            {
                if (task == PathTask.Drink)
                {
                    if (waterTick == 3)
                    {

                        spriteRenderer.color = Color.Lerp(initColor, Color.blue, 0.5f);
                        Thirst += 15;
                        waterTick = 0;
                        Thirst = Mathf.Clamp(Thirst, 0, 100);
                        isDrinking = true;
                        status = Unit.Status.Drinking;
                        if (Thirst >= 100)
                        {
                            isDrinking = false;
                            Vector2 target = new Vector2(1.5f, 1.5f);
                            MoveTo(target, true, PathTask.None);
                            status = Unit.Status.Idling;
                            return;
                        }
                    }
                    else waterTick++;
                }
            }
        }
    }
    private void onPathComplete(Path p)
    {
        if (!p.error)
        {
            currentWayPoint = 0; // If path is available and has no errors, then argument p will be applied for path variable
            path = p;
            isReached = false;
        }
        else
        {
            currentWayPoint = 0;
            isReached = false;
            Debug.LogError(p.errorLog);
        }

    }
    public bool MoveTo(Vector2 target, bool isObstacle, PathTask objective)
    {
        seeker = GetComponent<Seeker>();
        task = objective;
        GraphMask gmask = GraphMask.FromGraphName("1x1");
        NNConstraint nn = new NNConstraint
        {
            graphMask = gmask,
            constrainWalkability = true,
            walkable = true,
            constrainArea = false,
            constrainTags = false,
            constrainDistance = false
        };
        path = null;
        if (!isObstacle)
        {
            var gn2 = AstarPath.active.GetNearest(target, nn).node;
            if (!gn2.Walkable)
            {
                MoveTo(target, true, objective);
            }
            else
            {
                seeker.StartPath(transform.position, target, onPathComplete, gmask);
                return true;
            }
        }
        else
        {
            var gn1 = AstarPath.active.GetNearest((Vector2)transform.position, nn).node;
            for (int x = -1; x <= 1; x++)
            {
                for (int y = -1; y <= 1; y++)
                {
                    if (x == 0 && y == 0) continue;
                    Vector2 offset = new Vector2(x, y);
                    var gn2 = AstarPath.active.GetNearest(target + offset, nn).node;
                    if (gn2.Walkable)
                    {
                        if (PathUtilities.IsPathPossible(gn1, gn2))
                        {
                            seeker.StartPath(transform.position, target + offset, onPathComplete, gmask);
                            return true;
                        }
                    }
                }
            }
        }
        return false;
    }
    public void FindWater()
    {
        var nn = new NNConstraint
        {
            graphMask = GraphMask.FromGraphName("1x1")
        };
        Vector3Int intPosition = Vector3Int.FloorToInt(transform.position);
        if (task != PathTask.Drink && status != Unit.Status.Working && task != PathTask.Carry)
        {
            for (int x = intPosition.x; x < intPosition.x + 50; x++)
            {
                for (int y = intPosition.y; y < intPosition.y + 50; y++)
                {
                    Vector3Int iteration = new Vector3Int(x, y, 0);
                    if (tilemap.GetTile(iteration) != null && tilemap.GetTile(iteration).name == "water")
                    {
                        for (int xCheck = -1; xCheck < 2; xCheck++)
                        {
                            for (int yCheck = -1; yCheck < 2; yCheck++)
                            {
                                Vector2 offset = new Vector2(xCheck, yCheck);
                                Vector2 Destination = new Vector2(iteration.x + 0.5f, iteration.y + 0.5f);
                                if (AstarPath.active.GetNearest(Destination + offset, nn).node == null)
                                {
                                    WaterPosition = Destination;
                                    MoveTo(Destination + offset, false, PathTask.Drink);
                                    return;
                                }
                            }
                        }
                    }
                }
            }
            for (int x = intPosition.x; x > intPosition.x - 50; x--)
            {
                for (int y = intPosition.y; y > intPosition.y - 50; y--)
                {
                    Vector3Int iteration = new Vector3Int(x, y, 0);
                    if (tilemap.GetTile(iteration) != null && tilemap.GetTile(iteration).name == "water")
                    {
                        for (int xCheck = -1; xCheck < 2; xCheck++)
                        {
                            for (int yCheck = -1; yCheck < 2; yCheck++)
                            {
                                Vector2 offset = new Vector2(xCheck, yCheck);
                                Vector2 Destination = new Vector2(iteration.x + 0.5f, iteration.y + 0.5f);
                                if (AstarPath.active.GetNearest(Destination + offset).node.Walkable)
                                {
                                    WaterPosition = Destination;
                                    MoveTo(Destination + offset, false, PathTask.Drink);
                                    return;
                                }
                            }
                        }
                    }
                }
            }
        }
    }
    public void NextStep()
    {
        if (Thirst <= 0)
        {
            Debug.Log("Finding water");
            FindWater();
        }
        if (needsTick == 24)
        {
            float WaterDist = Vector2.Distance(transform.position, WaterPosition);
            if (WaterPosition == Vector2.zero || WaterDist > 1)
            {
                if (!isDrinking)
                    Thirst -= 6;
            }
            needsTick = 0;
        }
        else needsTick++;
        DrinkWater();
        if (currentStep >= speed)
        {

            var unit = FindNearest("Player Unit",transform.position);
            TargetAttack = unit;
            if (TargetAttack != null)
            {
                if ((Vector2)TargetAttack.transform.position != targetOldPosition)
                {
                    MoveTo(unit.transform.position, true, PathTask.Fight);
                    targetOldPosition = TargetAttack.transform.position;
                }
                int distanceAtt = (int)Vector2.Distance(transform.position, TargetAttack.transform.position);
                if (distanceAtt >= 0 && distanceAtt <= 1)
                {
                    Vector2 offset = new Vector2(Random.Range(-5, 5), Random.Range(-5, 5));
                    if (TargetAttack.GetComponent<Unit>().job != Unit.Job.Soldier)
                    {
                        TargetAttack.GetComponent<Unit>().MoveTo((Vector2)TargetAttack.transform.position + offset, true, Unit.PathTask.Panic);
                        TargetAttack.GetComponent<Unit>().CancelInvoke("NextStep");
                        TargetAttack.GetComponent<Unit>().InvokeRepeating("NextStep", 0, 0.05f);
                        TargetAttack.GetComponent<Unit>().NextStep();
                    }
                    TargetAttack.GetComponent<Unit>().Health -= 1;
                    if (TargetAttack.GetComponent<Unit>().Health <= 0)
                    {
                        Destroy(TargetAttack);
                    }
                    Debug.Log("Attacking");
                }
            }
            currentStep = 0;
            if (Carrying != Interactable.ResourceType.None)
            {
                speed = 3;
            }
            else
            {
                speed = 0;
            }
            #region Pathfinding and Movement
            if (isReached || path == null)
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
                OnFinishedPath();
            }
            #endregion
        }
        else currentStep++;
    }
    private void Start()
    {
        needForConstruction = Interactable.ResourceType.None;
        Carrying = Interactable.ResourceType.None;
        GraphMask gmask = GraphMask.FromGraphName("1x1");
        #region Assigning Components
        tilemap = GameObject.Find("Grid").transform.Find("Ground").GetComponent<Tilemap>();
        resources = GameObject.Find("MainResources").GetComponent<Resources>();
        seeker = GetComponent<Seeker>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        #endregion
        initColor = spriteRenderer.color;
        InvokeRepeating("NextStep", 0, 0.1f);
    }
    private GameObject FindNearest(string tag, Vector2 Position)
    {
        GameObject[] targets = GameObject.FindGameObjectsWithTag(tag);
        GameObject result;
        if (targets.Length == 0) return null;
        GameObject currentObject = targets[0];
        foreach (var item in targets)
        {
            float distance = Vector2.Distance(item.transform.position, Position);
            float oldDistance = Vector2.Distance(currentObject.transform.position, Position);
            if(distance < oldDistance)
            {
                currentObject = item;
            }
        }
        result = currentObject;
        return result;
    }
}
