using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

public class IdentifiersDatabaseWrapper : ScriptableObject, ICSVDataHandler, ISerializationCallbackReceiver
{
    [SerializeField] private int maxHashDigits = 2;
    [SerializeField] private List<IdentifiersDatabase> databases = new List<IdentifiersDatabase>();

    public List<IdentifiersDatabase> Databases => databases;

    public int RegistriesCount
    {
        get
        {
            int toReturn = 0;
            foreach(IdentifiersDatabase database in databases)
            {
                toReturn += database.Registries.Count;
            }
            return toReturn;
        }
    }

    public string[] DatabaseNames
    {
        get
        {
            string[] toReturn = new string[databases.Count];

            for (int i = 0; i < databases.Count; i++)
            {
                toReturn[i] = databases[i].Name;
            }
            return toReturn;
        }
    }
    
    public string CSVData => CSVGenerator.GetCSVLineForElements("Identifier", "Label", "GUID", "Database", "Database GUID") + CSVGenerator.MergeCSVHandlers(databases);

    public void GetRegistriesPathsAndGUID(out string[] paths, out string[] guids)
    {
        List<string> pathsList = new List<string>();
        List<string> guidList = new List<string>();

        for (int i = 0; i < databases.Count; i++)
        {
            for (int j = 0; j < databases[i].Registries.Count; j++)
            {
                databases[i].GetPathsArrays(out string[] ca, out string[] ba);
                pathsList.AddRange(ca);
                guidList.AddRange(ba);
            }
        }

        paths = new string[pathsList.Count];
        guids = new string[guidList.Count];
        pathsList.CopyTo(paths);
        guidList.CopyTo(guids);
    }

    private void CleanEmptyElements()
    {
        for (int i = databases.Count - 1; i > 0; i--)
        {
            if(databases[i] == null)
            {
                databases.RemoveAt(i);
            }
        }
    }
    
    public void OnBeforeSerialize()
    {
        CleanEmptyElements();
    }

    public void OnAfterDeserialize() { }
    

    #region Editor Methods
    
#if UNITY_EDITOR
    public IdentifiersDatabase CreateDatabase(string name, string description, string path)
    {
        IdentifiersDatabase newDatabase = CreateInstance<IdentifiersDatabase>();
        newDatabase.Setup(name, description, IdentifiersDatabaseUtils.GenerateHash(maxHashDigits, databases));
        string finalAssetPath = System.IO.Path.Combine(path, name + ".asset");
        AssetDatabase.CreateAsset(newDatabase, finalAssetPath);
        databases.Add(newDatabase);

        EditorUtility.SetDirty(newDatabase);
        SaveAsset();
        return newDatabase;
    }

    private void SaveAsset()
    {
        EditorUtility.SetDirty(this);
        AssetDatabase.SaveAssets();
    }

    public void RemoveDatabase(IdentifiersDatabase toRemove, bool deleteFile = true)
    {
        databases.Remove(toRemove);
        if(deleteFile)
        {
            AssetDatabase.DeleteAsset(AssetDatabase.GetAssetPath(toRemove));
        }
        SaveAsset();
    }

#endif
    
    #endregion
}
