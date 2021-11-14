using UnityEngine;
using System;

[Serializable]
public class IdentifierRegistry : IIdentifiable, ICSVDataHandler
{
    [SerializeField] private string identifier = null;
    [SerializeField] private string category = null;
    [SerializeField] private string hexaId = null;

    public IdentifierRegistry(string identifier, string hash, string category = "")
    {
        this.identifier = identifier;
        this.category = category;
        this.hexaId = hash;
    }

    public string Identifier
    {
        get => identifier;
        set => identifier = value;
    }
    public string Category
    {
        get => category;
        set => category = value;
    }
    
    public string GUID => hexaId;
    
    public string CSVData => CSVGenerator.GetCSVLineForElements(identifier, category, GUID);
    
    public void UpdateRegistry(string identifier, string category)
    {
        this.identifier = identifier;
        this.category = category;
    }
}
