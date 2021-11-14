using System.Collections.Generic;
using UnityEngine;

public class IdentifiersDatabase : ScriptableObject, IIdentifiable, ICSVDataHandler
{
    [SerializeField] private string dbName = null;
    [SerializeField] private string dbDesc = null;
    [SerializeField] private string dbGuid = null;
    [SerializeField] private List<IdentifierRegistry> registries = new List<IdentifierRegistry>();

    public List<IdentifierRegistry> Registries => registries;

    public string GUID => dbGuid;

    public void Setup(string name, string description, string guid)
    {
        dbName = name;
        dbDesc = description;
        dbGuid = guid;
    }

    public void DeleteRegistry(IdentifierRegistry toDelete)
    {
        registries.Remove(toDelete);
    }

    public void CreateRegistry(string identifier, string categoryId)
    {
        IdentifierRegistry newRegistry = new IdentifierRegistry(identifier, IdentifiersDatabaseUtils.GenerateHash(4, registries, GUID), categoryId);
        registries.Add(newRegistry);
    }
 
    public bool IdentifierExists(string identifier)
    {
        return registries.Find(x => x.Identifier == identifier) != null;
    }

    public void GetPathsArrays(out string[] paths, out string[] guid)
    {
        paths = new string[registries.Count];
        guid = new string[registries.Count];

        for (int i = 0; i < registries.Count; i++)
        {
            string category = string.IsNullOrEmpty(registries[i].Category) ? "<Empty>" : registries[i].Category;
            paths[i] = string.Join("/", Name, category, registries[i].Identifier);
            guid[i] = registries[i].GUID;
        }
    }

    public string Name => dbName;
    public string Description => dbDesc;

    public string[] GetCategories(string emptyValue = "")
    {
        HashSet<string> dbCategories = new HashSet<string>();

        dbCategories.Add(emptyValue);

        for (int i = 0; i < registries.Count; i++)
        {
            dbCategories.Add(registries[i].Category);
        }

        string[] toReturn = new string[dbCategories.Count];
        dbCategories.CopyTo(toReturn);
        return toReturn;
    }
    
    public string[] Identifiers
    {
        get
        {
            string[] identifiers = new string[registries.Count];

            for (int i = 0; i < registries.Count; i++)
            {
                identifiers[i] = registries[i].Identifier;
            }

            return identifiers;
        }
    }

    public string CSVData
    {
        get
        {
            string csvData = string.Empty;
            for (int i = 0; i < registries.Count; i++)
            {
                csvData += CSVGenerator.GetCSVLineForElements(registries[i], Name, GUID);
            }

            return csvData;
        }
    }
}
