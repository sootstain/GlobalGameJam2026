using System.Collections.Generic;
using System.Linq;
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

/*
<<declare $RussianGuy = "Vladimir Kanibalovic">>
<<declare $SpanishGranny = "Senorita Alonso Guerrero Ortiz Maria Fernanda Muchosgracias">>
<<declare $Poquito = "Poquito Jon">>
<<declare $Kristian = "Kristian Johannson">>
<<declare $Francois = "Francois Francis">>
<<declare $Lasuke = "Lady Sasuke">>
<<declare $Pasha = "Pasha Vladislav">>
<<declare $Fester = "Sleaze Fester">>
*/

    public NPC FindNpc(string lineCharacterName)
    {
        return npcAssets.First(n=>n.name == lineCharacterName);
        
    }
}