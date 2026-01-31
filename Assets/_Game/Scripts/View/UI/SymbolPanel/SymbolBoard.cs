using System;
using System.Collections.Generic;
using _Game.Scripts.Core.Data;
using UnityEngine;

public class SymbolBoard : MonoBehaviour
{
    private void OnEnable()
    {
        SetUpUI();
    }

    public void SetUpUI()
    {
        for (int i = 0; i < this.transform.childCount; i++)
        {
            GameObject child = this.transform.GetChild(i).gameObject;
            
            child.GetComponent<SymbolSlot>().TakeInformation();
        }
    }
}
