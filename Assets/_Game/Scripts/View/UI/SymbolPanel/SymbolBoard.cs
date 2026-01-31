using System;
using System.Collections.Generic;
using _Game.Scripts.Controllers.Machines;
using _Game.Scripts.Core.Data;
using UnityEngine;

public class SymbolBoard : MonoBehaviour
{
    private float totalWeight = 0f;

    private void OnEnable()
    {
        for (int i = 0; i < this.transform.childCount; i++)
        {
            GameObject child = this.transform.GetChild(i).gameObject;
            totalWeight += child.GetComponent<SymbolSlot>().symbolData.currentWeight;
        }
        
        SetUpUI();
    }
    private void SetUpUI()
    {
        for (int i = 0; i < this.transform.childCount; i++)
        {
            GameObject child = this.transform.GetChild(i).gameObject;
            child.GetComponent<SymbolSlot>().TakeInformation(totalWeight);
        }
    }
}
