using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using Yarn.Unity;
using Yarn.Unity.Attributes;

public class fuck_hack : MonoBehaviour
{
    public List<DoorOpen> doors;
    private bool startedDdialoggOnce;

    public DialogueRunner dr;
    public YarnProject yarnProject;
    [YarnNode("yarnProject")]
    public string startNode;
    
    private void OnEnable()
    {
        foreach (var d in doors)
        {
            d.OnDoorOpen += OnAnyDoor;
        }
    }

    private void OnAnyDoor()
    {
        if (!startedDdialoggOnce)
        {
            startedDdialoggOnce = true;
            dr.StartDialogue(startNode);
        }

    }
}
