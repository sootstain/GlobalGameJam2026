using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using Yarn.Unity;

public class fuck_hack : MonoBehaviour
{
    public static List<fuck_hack> all = new();

    private bool isFocusHere = false;
    
    [SerializeField]
    private OptionItem _optionItem;
    public OptionItem optionItem
    {
        get
        {
            if (_optionItem == null)
            {
                _optionItem = GetComponent<OptionItem>();
            }
            return _optionItem;
        }
    }
    
    private void OnEnable()
    {
        all.Add(this);
    }

    private void OnDisable()
    {
        all.Remove(this);
    }

    private void Update()
    {
        if (optionItem.IsHighlighted)
        {
            isFocusHere = true;
        }
        else
        {
            isFocusHere = false;
        }
        
        // EventSystem.current.currentSelectedGameObject
    
    }
}
