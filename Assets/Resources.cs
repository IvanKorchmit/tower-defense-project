using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Resources : MonoBehaviour
{
    public int Wood;
    public int Coal;
    public int Iron;
    public int Copper;
    public int Gold;
    public int Stone;
    public void AddInto(Interactable.ResourceType resource)
    {
        switch (resource)
        {
            case Interactable.ResourceType.Wood:
                Wood++;
                break;
            case Interactable.ResourceType.Gold:
                Gold++;
                break;
            case Interactable.ResourceType.Copper:
                Copper++;
                break;
            case Interactable.ResourceType.Iron:
                Iron++;
                break;
            case Interactable.ResourceType.Stone:
                Stone++;
                break;  
        }
    }
    public bool Take(Interactable.ResourceType resource)
    {
        switch (resource)
        {
            case Interactable.ResourceType.Wood:
                if (Wood > 0)
                {
                    Wood--;
                    return true;
                }
                break;
            case Interactable.ResourceType.Gold:
                if (Gold > 0)
                {
                    Gold--;
                    return true;
                }
                break;
            case Interactable.ResourceType.Copper:
                if (Copper > 0)
                {
                    Copper--;
                    return true;
                }
                break;
            case Interactable.ResourceType.Iron:
                if (Iron > 0)
                {
                    Iron--;
                    return true;
                }
                break;
            case Interactable.ResourceType.Stone:
                if (Stone > 0)
                {
                    Stone--;
                    return true;
                }
                break;
        }
        return false;
    }
    public bool Check(Interactable.ResourceType resource)
    {
        switch (resource)
        {
            case Interactable.ResourceType.Wood:
                if (Wood > 0)
                {
                    return true;
                }
                break;
            case Interactable.ResourceType.Gold:
                if (Gold > 0)
                {
                    return true;
                }
                break;
            case Interactable.ResourceType.Copper:
                if (Copper > 0)
                {
                    return true;
                }
                break;
            case Interactable.ResourceType.Iron:
                if (Iron > 0)
                {
                    return true;
                }
                break;
            case Interactable.ResourceType.Stone:
                if (Stone > 0)
                {
                    return true;
                }
                break;
        }
        return false;
    }
}
