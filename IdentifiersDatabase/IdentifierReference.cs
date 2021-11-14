using UnityEngine;

[System.Serializable]
public class IdentifierReference
{
    [SerializeField] private string assignedId;

    public string AssignedID => assignedId;

    public void OverrideId(string newId)
    {
        assignedId = newId;
    }
}
