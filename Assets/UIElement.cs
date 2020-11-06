using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
public class UIElement : MonoBehaviour
{
    public enum UIType
    {
        DisplayResource
    }
    public UIType ui;
    public TextMeshProUGUI tmPro;
    public Resources resource;
    private void Start()
    {
        tmPro = GetComponent<TextMeshProUGUI>();
    }
    private void Update()
    {
        switch(ui)
        {
            case UIType.DisplayResource:
                tmPro.text = $"Wood: {resource.Wood}\tCoal: {resource.Coal}\t Copper: {resource.Copper}\nStone: {resource.Stone}\tGold: {resource.Gold}\tIron: {resource.Iron}";
                break;
        }
    }
}
