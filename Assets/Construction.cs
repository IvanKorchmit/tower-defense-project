using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Construction : MonoBehaviour
{
    public List<NeededResources> neededResources;
    public enum Building
    {
        Storage, Barrack, House, School
    }
    public Building type;
    public void CheckResources()
    {
        if (neededResources.Count > 0)
        {
            if (neededResources[0].needed == neededResources[0].quantity)
            {
                neededResources.RemoveAt(0);
            }
        }
        if(neededResources.Count == 0)
        {
            Destroy(gameObject);
        }
    }
    public void addResource(Interactable.ResourceType resource)
    {
        if (neededResources.Count > 0)
        {
            for (int i = 0; i < neededResources.Count; i++)
            {
                if (neededResources[i].resource == resource)
                {
                    neededResources[i].quantity++;
                    neededResources[i].incoming--;
                    CheckResources();
                }
            }
        }
    }
}
[System.Serializable]
public class NeededResources
{
    public Interactable.ResourceType resource;
    public int needed;
    public int quantity;
    public int incoming;
}