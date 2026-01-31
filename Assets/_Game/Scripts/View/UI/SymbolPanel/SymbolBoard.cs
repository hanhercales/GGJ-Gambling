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
            
            // Lấy component ra kiểm tra trước
            var slot = child.GetComponent<SymbolSlot>();
            
            if (slot != null && slot.symbolData != null)
            {
                totalWeight += slot.symbolData.currentWeight;
            }
        }
        
        SetUpUI();
    }

    private void OnDisable()
    {
        totalWeight = 0f;
    }

    private void SetUpUI()
    {
        for (int i = 0; i < this.transform.childCount; i++)
        {
            GameObject child = this.transform.GetChild(i).gameObject;
            var slot = child.GetComponent<SymbolSlot>();

            if (slot != null)
            {
                slot.TakeInformation(totalWeight);
            }
        }
    }

    private void OnApplicationQuit()
    {
        for (int i = 0; i < this.transform.childCount; i++)
        {
            Transform t = this.transform.GetChild(i);
            if (t == null) continue;

            var slot = t.GetComponent<SymbolSlot>();
            
            if (slot != null && slot.symbolData != null)
            {
                slot.symbolData.ResetStats();
            }
        }
    }
}
