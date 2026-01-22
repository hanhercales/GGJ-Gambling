using UnityEngine;

[CreateAssetMenu(fileName = "NewSymbol", menuName = "Symbol")]
public class SymbolSO : ScriptableObject
{
    public string id;
    public string symbolName;
    public Sprite icon;
    public TierEnum tier;
    public int weight;
    public int value;
    
    public int GetWeight()
    {
        return weight;
    }

    public virtual int CalculateValue()
    {
        return value;
    }
}
