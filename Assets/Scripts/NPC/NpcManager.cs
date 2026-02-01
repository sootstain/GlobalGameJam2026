using System.Collections.Generic;
using UnityEngine;

public class NpcManager : MonoBehaviour
{

    private static NpcManager _instance;
    public static NpcManager instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = FindAnyObjectByType<NpcManager>();
            }
            return _instance;
        }
    }

    public List<NPC> npcAssets;
    

    
}