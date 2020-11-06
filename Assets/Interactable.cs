using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using Pathfinding;
using UnityEngine.Tilemaps;

public class Interactable : MonoBehaviour, IPointerDownHandler
{
    private void Start()
    {
        initColor = GetComponent<SpriteRenderer>().color;
        switch (type)
        {
            case ResourceType.Wood:
                Durability = Random.Range(10, 15);
                break;
            case ResourceType.Gold:
            case ResourceType.Iron:
            case ResourceType.Stone:
            case ResourceType.Copper:
                Durability = Random.Range(25, 60);
                break;

        }
    }
    public int Durability;
    public Color initColor;
    public GameObject User;
    public enum ResourceType
    {
        Wood, Gold, Copper, Iron, Stone, None
    }
    public ResourceType type;
    public void Interact()
    {
        if (User == null)
        {
            switch (type)
            {
                case ResourceType.Wood:
                    CallLabor(Unit.Job.Lumber);
                    break;
                case ResourceType.Gold:
                case ResourceType.Iron:
                case ResourceType.Stone:
                case ResourceType.Copper:
                    CallLabor(Unit.Job.Miner);
                    break;
            }
        }
    }
    private void CallLabor(Unit.Job Specialist)
    {
        if (User == null)
        {
            var units = GameObject.FindGameObjectsWithTag("Player Unit");
            foreach (var item in units)
            {
                Unit unit = item.GetComponent<Unit>();
                if (unit.job == Specialist)
                {
                    if (unit.status != Unit.Status.Working && !unit.isDrinking)
                    {
                        if (unit.task != Unit.PathTask.Carry && unit.task != Unit.PathTask.Drink && !unit.isDrinking)
                        {
                            var gmask = GraphMask.FromGraphName("1x1");
                            NNConstraint nn = new NNConstraint
                            {
                                graphMask = gmask,
                                constrainWalkability = true,
                                walkable = true
                            };  
                            Vector2 offset;
                            for (int x = -1; x < 2; x++)
                            {   
                                for (int y = -1; y < 2; y++)
                                {
                                    if (x == 0 && y == 0) continue;
                                    offset = new Vector2(x, y);
                                    var graphnode = AstarPath.active.GetNearest((Vector2)transform.position + offset, nn).node;
                                    if (graphnode != null && graphnode.Walkable)
                                    {
                                        goto FoundSpot;
                                    }
                                }
                            }
                            return;
                        FoundSpot:
                            if (User == null)
                            {
                                Vector2 dest = (Vector2)transform.position + offset;
                                if (unit.MoveTo(dest, false, Unit.PathTask.None))
                                {
                                    unit.status = Unit.Status.Working;
                                    unit.WorkingTarget = gameObject;
                                    unit.isReached = false;
                                    User = item;
                                }
                                return;
                            }
                        }
                    }
                }
            }
        }
    }
    public void OnPointerDown(PointerEventData eventData)
    {
        Interact();
    }
    public void CheckDurability()
    {
        if (Durability < 0) 
        {
            Unit User_Unit = User.GetComponent<Unit>();
            User_Unit.Carrying = type;
            User_Unit.status = Unit.Status.Idling;
            User_Unit.isReached = false;
            gameObject.GetComponent<BoxCollider2D>().enabled = false;
            User.GetComponent<Unit>().CarryToStorage();
            AstarPath.active.UpdateGraphs(gameObject.GetComponent<BoxCollider2D>().bounds);
            Destroy(gameObject);
        }
    }
}