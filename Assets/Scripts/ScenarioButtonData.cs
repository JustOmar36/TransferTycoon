using UnityEngine;

public class ScenarioButtonData : MonoBehaviour
{
    private int itemID = 0;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public int GetItemID()
    {
        return itemID;
    }

    public void SetItemID(int ID) { 
        itemID = ID;
    }
}
