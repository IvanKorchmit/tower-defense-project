using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Building : MonoBehaviour
{
    public BuildingClass building;
    public int health;
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
[System.Serializable]
public class BuildingClass
{
    public string name;
    public enum BuildingType
    {
        School, Storage, Barrack, House, Workshop
    }
}
