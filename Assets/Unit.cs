using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Pathfinding;
using UnityEngine.Tilemaps;
public class Unit : MonoBehaviour
{
    #region Components & technical fields
    #region AI Components
    private Path path;
    private Color initColor;
    private int tick = 0;
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
        Carry, None, Fight, Drink, Eat, Panic
    }
    public Interactable.ResourceType needForConstruction; // What unit needs to bring to the contruction yard?
    public GameObject ConstructionYard; // Which yard is he or she working on?
    #endregion
    private Seeker seeker;
    private SpriteRenderer spriteRenderer;
    public Resources resources; // Reference to the storage
    private Vector2 WaterPosition; // Position of water source to drink
    public bool isDrinking;
    #endregion
    public bool Debugging;
    public int speed;
    private int currentStep;
    public int Health = 100;
    public int Hunger = 100;
    public int Thirst = 100;
    public int Tobacco;
    public int Alcohol;
    public bool isChild;
    public int Age;
    public enum Gender
    {
        Male, Female
    }
    #region AI
    public enum AttackType
    {
        Ranged, Melee, Fists
    }
    public enum Status
    {
        Fighting, Working, Idling, Eating, Drinking
    }
    public enum Job
    {
        Farmer, Miner, Constructor, Soldier, Lumber
    }
    public AttackType attackType;
    public Status status;
    private void OnFinishedPath()
    {
        switch (task)
        {
            case PathTask.Carry:
                if (Carrying != Interactable.ResourceType.None)
                {
                    resources.AddInto(Carrying);
                    var constructors = GameObject.FindGameObjectsWithTag("Player Unit");
                    foreach (var item in constructors)
                    {
                        if (item.GetComponent<Unit>().job == Job.Constructor)
                        {
                            item.GetComponent<Unit>().NotifyConstructors();
                            break;
                        }
                    }
                    Vector2 target = new Vector2(1.5f, 1.5f);
                    Carrying = Interactable.ResourceType.None;
                    MoveTo(target, true, PathTask.None);
                }
                break;
            case PathTask.Fight:
                isReached = true;
                break;
            case PathTask.None:
                if (job == Job.Constructor)
                {

                }
                isReached = true;
                break;
            case PathTask.Panic:
                isReached = true;
                Vector2 targetHome = new Vector2(1.5f, 1.5f);
                CancelInvoke("NextStep");
                InvokeRepeating("NextStep", 0, 0.1f);
                MoveTo(targetHome, true, PathTask.None);
                break;
        }
        switch (status)
        {
            case Status.Fighting:
                break;
            case Status.Working:
                if (job == Job.Constructor && needForConstruction == Interactable.ResourceType.None)
                { // If he's constructor and doesn't know what to bring
                    var storage = GameObject.FindGameObjectWithTag("Storage");
                    var yard = GameObject.FindGameObjectWithTag("Construction Yard");
                    if (yard.GetComponent<Construction>().neededResources.Count > 0)
                    {
                        MoveTo(storage.transform.position, false, PathTask.None);
                        yard.GetComponent<Construction>().neededResources[0].incoming++;
                        needForConstruction = yard.GetComponent<Construction>().neededResources[0].resource;
                        ConstructionYard = yard;
                        status = Status.Working;
                    }
                }
                else if (ConstructionYard != null && Carrying == Interactable.ResourceType.None && job == Job.Constructor && needForConstruction != Interactable.ResourceType.None &&
                    Vector2.Distance(transform.position, ConstructionYard.transform.position) > 1)
                { // Or else he is the constructor, but knows what to bring.
                    if (resources.Take(needForConstruction))
                    { // If we have needed resources to bring then...
                        Carrying = needForConstruction;
                        MoveTo(ConstructionYard.transform.position, false, PathTask.None);
                    }
                    else
                    { // Or if it's not then we go back to home
                        Vector2 target = new Vector2(1.5f, 1.5f);
                        MoveTo(target, true, PathTask.None);
                        ConstructionYard.GetComponent<Construction>().neededResources[0].incoming--;
                        // needForConstruction = Interactable.ResourceType.None;
                        status = Status.Idling;
                    }
                }
                else if (ConstructionYard != null && Carrying != Interactable.ResourceType.None && job == Job.Constructor && Vector2.Distance(transform.position, ConstructionYard.transform.position) <= 1)
                {
                    ConstructionYard.GetComponent<Construction>().addResource(Carrying);
                    ConstructionYard.GetComponent<Construction>().CheckResources();
                    Carrying = Interactable.ResourceType.None;
                    path = null;
                    NotifyConstructors();
                }
                else if (Carrying == Interactable.ResourceType.None && job == Job.Constructor)
                {
                    if (!resources.Check(needForConstruction))
                    {
                        Vector2 target = new Vector2(1.5f, 1.5f);
                        MoveTo(target, true, PathTask.None);
                        status = Status.Idling;
                    }
                    else
                    {
                        var storage = GameObject.FindGameObjectWithTag("Storage");
                        var yard = GameObject.FindGameObjectWithTag("Construction Yard");
                        if (yard != null && yard.GetComponent<Construction>().neededResources.Count > 0)
                        {
                            MoveTo(storage.transform.position, false, PathTask.None);
                            yard.GetComponent<Construction>().neededResources[0].incoming++;
                            needForConstruction = yard.GetComponent<Construction>().neededResources[0].resource;
                            ConstructionYard = yard;
                            status = Status.Working;
                        }
                        else
                        {
                            Vector2 target = new Vector2(1.5f, 1.5f);
                            MoveTo(target, true, PathTask.None);
                            status = Status.Idling;
                        }
                    }
                }
                break;
            case Status.Idling:
                path = null;
                break;
            case Status.Eating:
                break;
            case Status.Drinking:
                break;
        }
    }
    public void DoLabor()
    {
        if (job != Job.Constructor && job != Job.Soldier)
        {
            if (WorkingTarget != null)
            {
                WorkingTarget.GetComponent<SpriteRenderer>().color = WorkingTarget.GetComponent<Interactable>().initColor;
            }
            if (tick == 2)
            {
                int LaborTargetDist;
                if (WorkingTarget != null)
                {
                    LaborTargetDist = (int)Vector2.Distance(transform.position, WorkingTarget.transform.position);
                }
                else
                {
                    LaborTargetDist = -1;
                }
                if (LaborTargetDist >= 0 && LaborTargetDist <= 1)
                {
                    WorkingTarget.GetComponent<SpriteRenderer>().color = Color.black;
                    WorkingTarget.GetComponent<Interactable>().Durability -= 2;
                    WorkingTarget.GetComponent<Interactable>().CheckDurability();

                }
                tick = 0;
            }
            else
            {
                tick++;
            }
        }
        else if (job == Job.Soldier)
        {
            Debug.Log("Trying to do job as the soldier");
            var enemy = FindNearest("Enemy", transform.position);
            if (enemy != null)
            {
                var distance = (int)Vector2.Distance(transform.position, enemy.transform.position);
                if (distance < 10 && path == null)
                    MoveTo(enemy.transform.position, false, PathTask.Fight);
                int dist = (int)Vector2.Distance(transform.position, enemy.transform.position);
                if (dist >= 0 && dist <= 1)
                {
                    enemy.GetComponent<EnemyAI>().Health -= 1;
                    if (enemy.GetComponent<EnemyAI>().Health <= 0)
                    {
                        Destroy(enemy);
                    }
                    Debug.Log("Attacking");
                }
            }
            else if (enemy == null && path != null)
            {
                path = null;
                Vector2 target = new Vector2(1.5f, 1.5f);
                MoveTo(target, true, PathTask.None);
            }
        }
    }
    public void NotifyConstructors()
    {
        if(job == Job.Constructor)
        {
            var yard = GameObject.FindGameObjectWithTag("Construction Yard");
            if (yard != null && path == null)
            {
                MoveTo(yard.transform.position, false, PathTask.None);
                status = Status.Working;
            }
        }
    }
    public void DrinkWater()
    {
        GraphMask gmask = GraphMask.FromGraphName("1x1");
        spriteRenderer.color = initColor;
        int WaterDist = (int)Vector2.Distance(transform.position, WaterPosition);
        if (WaterDist >= 0 && WaterDist <= 1)
        {
            if (status != Status.Working && task != PathTask.Carry)
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
                        status = Status.Drinking;
                        if (Thirst >= 100)
                        {
                            isDrinking = false;
                            Vector2 target = new Vector2(1.5f, 1.5f);
                            MoveTo(target, true, PathTask.None);
                            status = Status.Idling;
                            return;
                        }
                    }
                    else waterTick++;
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
            currentStep = 0;
            if (Carrying != Interactable.ResourceType.None)
            {
                speed = 3;
            }
            else
            {
                speed = 0;
            }
            DoLabor();
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
                OnFinishedPath();
            }
            #endregion
        }
        else currentStep++;
    }
    public void CarryToStorage()
    {
        var storage = GameObject.FindGameObjectWithTag("Storage");
        if(storage != null)
            MoveTo(storage.transform.position, false, PathTask.Carry);
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
    #endregion
    public Gender gender;
    public Job job;
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
        if (Debugging)
        {
            switch (gender)
            {
                case Gender.Female:
                    spriteRenderer.color = Color.Lerp(Color.red, Color.white, 0.5f);
                    break;
                case Gender.Male:
                    spriteRenderer.color = Color.Lerp(Color.blue, Color.white, 0.5f);
                    break;
            }
            if (isChild)
            {
                spriteRenderer.color = Color.red;
            }
        }
        initColor = spriteRenderer.color;
        Vector2 target = new Vector2(1.5f, 1.5f);
        MoveTo(target, true, PathTask.None);
        InvokeRepeating("NextStep", 0, 0.1f);
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
                isObstacle = true;
                goto ActuallyObstacle;
            }
            else
            {
                seeker.StartPath(transform.position, target, onPathComplete, gmask);
                return true;
            }
        }
        ActuallyObstacle:
        if(isObstacle)
        {
            var gn1 = AstarPath.active.GetNearest((Vector2)transform.position,nn).node;
            for (int x = -1; x <= 1; x++)
            {
                for (int y = -1; y <= 1; y++)
                {
                    if (x == 0 && y == 0) continue;
                    Vector2 offset = new Vector2(x, y);
                    var gn2 = AstarPath.active.GetNearest(target + offset,nn).node;
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
        if (task != PathTask.Drink && status != Status.Working && task != PathTask.Carry)
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
                                    MoveTo(Destination+offset, false, PathTask.Drink);
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
                                    MoveTo(Destination+offset, false, PathTask.Drink);
                                    return;
                                }
                            }
                        }
                    }
                }
            }
        }
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
            if (distance < oldDistance)
            {
                currentObject = item;
            }
        }
        result = currentObject;
        return result;
    }
}