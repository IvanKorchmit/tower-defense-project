using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ButtonAction : MonoBehaviour
{
    public void OnClick ()
    {
        GameObject.Find("Caravan").GetComponent<Caravan>().Deploy();
        Debug.Log("Test");
    }
}
