using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Pathfinding;
public class Caravan : MonoBehaviour
{
    public Units[] units;
    public GameObject TownHall;
    public void Deploy()
    {
        for (int i = 0; i < units.Length; i++)
        {
            Vector2 offset = new Vector2(Random.Range(-1, 2), Random.Range(-1, 2));
            GameObject unit = Instantiate(units[i].Unit, (Vector2)transform.position+offset, Quaternion.identity);
            Unit unit_ = unit.GetComponent<Unit>();
            unit_.isChild = units[i].isChild;
            unit_.Age = units[i].Age;
            unit_.gender = units[i].gender;
            unit_.job = units[i].job;
            unit_.Debugging = true;
            Vector2 target = new Vector2(1.5f, 1.5f);
            unit_.MoveTo(target, true, Unit.PathTask.None);
        }
        Instantiate(TownHall, transform.position, Quaternion.identity);
        Destroy(gameObject);
    }
}
[System.Serializable]
public class Units
{
    public GameObject Unit;
    public bool isChild;
    public int Age;
    public Unit.Gender gender;
    public Unit.Job job;
}