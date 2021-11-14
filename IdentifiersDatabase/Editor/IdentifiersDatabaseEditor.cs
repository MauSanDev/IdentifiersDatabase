using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.IO;

public class IdentifiersDatabaseEditor : EditorWindow
{
    private static string CSV_FILE_NAME = "IdentifiersDatabase_{0}.csv";
    private const string ASSETS_FOLDER_PATH = "Assets/Resources/IdentifiersDatabase";
    private const string WRAPPER_FILE_NAME = "IdentifiersDatabaseWrapper.asset";
    private const string WRAPPER_FINAL_PATH = ASSETS_FOLDER_PATH + "/" + WRAPPER_FILE_NAME;
    private const string DATABASES_PATH = ASSETS_FOLDER_PATH + "/Databases";

    private readonly string[] categoryDefaultValues = { "<No Label>", "<Empty>", "All" };
    
    private IdentifiersDatabaseWrapper databaseWrapper = null;

    //Search Variables
    private string databaseSearch = null;
    private string toSearch = null;
    private string registriesCategorySearch = string.Empty;

    //New Database Params
    private string newDatabaseName = null;
    private string newDatabaseDesc = null;

    //New Registry Variables
    private string newRegistryIdentifier = null;
    private bool createNewCategory = false;
    private string newRegistryCategory = null;

    //Selected Elements
    private IdentifiersDatabase selectedDatabase = null;
    private Tabs currentTab = Tabs.NewDatabase;
    private SidebarSearchType sidebarSearchType = SidebarSearchType.Database;

    //Scrolls
    private Vector2 sidebarScroll = Vector2.zero;
    private Vector2 registriesScroll = Vector2.zero;

    #region Enums

    private enum Tabs
    {
        NewDatabase,
        RegistryEditor
    }

    private enum SidebarSearchType
    {
        Database,
        ID
    }

    #endregion


    [MenuItem("Noar Utils/Identifiers Database")]
    public static void ShowIdentifiersDatabaseInspector()
    {
        IdentifiersDatabaseEditor window = GetWindow<IdentifiersDatabaseEditor>("Identifiers Database");
        window.Show();
    }

    public static IdentifiersDatabaseWrapper GetIdentifiersDatabases()
    {
        IdentifiersDatabaseWrapper wrapper = AssetDatabase.LoadAssetAtPath<IdentifiersDatabaseWrapper>(WRAPPER_FINAL_PATH);
        return wrapper;
    }

    private void OnEnable()
    {
        FindWrapper();
        InitializeStyles();
    }
    private void Update()
    {
        Repaint();
    }

    private void OnGUI()
    {
        DrawToolbar();

        if(!WrapperExists())
        {
            return;
        }

        EditorGUILayout.BeginHorizontal();
        DrawSidebar();
        DrawTabs();
        EditorGUILayout.EndHorizontal();
    }

    #region Toolbar
    private void DrawToolbar()
    {
        EditorGUILayout.BeginHorizontal();

        if (GUILayout.Button("New Database", EditorStyles.toolbarButton, GUILayout.Width(120)))
        {
            selectedDatabase = null;
            ChangeTab(Tabs.NewDatabase);
        }

        if (GUILayout.Button("Ping Wrapper", EditorStyles.toolbarButton, GUILayout.Width(100)))
        {
            if (databaseWrapper == null)
            {
                EditorUtility.DisplayDialog("There's no Wrapper", "Create a Wrapper first to start working with Databases.", "Continue");
            }
            else
            {
                PingObject(databaseWrapper);
            }
        }

        if (GUILayout.Button("Create CSV", EditorStyles.toolbarButton, GUILayout.Width(100)))
        {
            GenerateAllDatabasesCSV();
        }

        EditorGUILayout.EndHorizontal();
    }
    #endregion

    #region Tabs
    private void DrawTabs()
    {
        EditorGUILayout.BeginVertical("HelpBox", GUILayout.Width(position.width * 0.7f));

        switch (currentTab)
        {
            case Tabs.RegistryEditor:
                DrawRegistryEditorTab();
                break;
            case Tabs.NewDatabase:
                DrawNewDatabaseWindow();
                break;
        }

        EditorGUILayout.EndVertical();
    }

    private void ChangeTab(Tabs newTab)
    {
        currentTab = newTab;
        ResetNewElementVariables();
    }
    #endregion

    #region Database Wrapper Methods
        private bool WrapperExists()
        {
            if (databaseWrapper == null)
            {
                EditorGUILayout.LabelField("The Database Wrapper doesn't exist. Do you want to create it?");

                if (GUILayout.Button("Create"))
                {
                    CreateDatabaseWrapper();
                }
                return false;
            }

            return true;
        }

        private void CreateDatabaseWrapper()
        {
            if(File.Exists(WRAPPER_FINAL_PATH))
            {
                EditorUtility.DisplayDialog("Wrapper Exists", "The database Wrapper already exists.", "Continue");
                return;
            }
            databaseWrapper = CreateInstance<IdentifiersDatabaseWrapper>();
            AssetDatabase.CreateAsset(databaseWrapper, WRAPPER_FINAL_PATH);
        }
        #endregion

    #region Search
        private KeyValuePair<int, int> existingSearchIndex;

        private void SearchOnDatabase()
        {
            int dbId = -1;
            int regId = -1;

            if (!string.IsNullOrEmpty(databaseSearch))
            {
                for (int i = 0; i < databaseWrapper.Databases.Count; i++)
                {
                    for (int j = 0; j < databaseWrapper.Databases[i].Registries.Count; j++)
                    {
                        IdentifierRegistry reg = databaseWrapper.Databases[i].Registries[j];
                        if(reg.Identifier.StartsWith(databaseSearch) || reg.GUID.StartsWith(databaseSearch))
                        {
                            dbId = i;
                            regId = j;
                            break;
                        }
                    }
                }
            }

            existingSearchIndex = new KeyValuePair<int, int>(dbId, regId);
        }

        private void ChangeSearchMode(SidebarSearchType searchType)
        {
            sidebarSearchType = searchType;
            databaseSearch = string.Empty;
            existingSearchIndex = new KeyValuePair<int, int>(-1, -1);
        }
    #endregion

    #region Sidebar
    private void DrawSidebar()
    {
        EditorGUILayout.BeginVertical(GUILayout.Width(position.width * 0.3f));

        GUI.backgroundColor = Color.black;
        EditorGUILayout.BeginHorizontal("HelpBox");
        EditorGUILayout.LabelField("Search:", GUILayout.Width(55));

        EditorGUI.BeginChangeCheck();
        databaseSearch = EditorGUILayout.TextField(databaseSearch);
        if(EditorGUI.EndChangeCheck())
        {
            if(sidebarSearchType == SidebarSearchType.ID)
            {
                SearchOnDatabase();
            }
            else
            {
                existingSearchIndex = new KeyValuePair<int, int>(-1, -1);
            }
        }

        //Draw the DB and ID buttons.
        GUI.backgroundColor = sidebarSearchType == SidebarSearchType.Database ? Color.yellow : Color.black;
        if (GUILayout.Button("DB", EditorStyles.miniButtonLeft, GUILayout.Width(30)))
        {
            ChangeSearchMode(SidebarSearchType.Database);
        }

        GUI.backgroundColor = sidebarSearchType == SidebarSearchType.ID ? Color.yellow : Color.black;
        if (GUILayout.Button("ID", EditorStyles.miniButtonRight, GUILayout.Width(30)))
        {
            ChangeSearchMode(SidebarSearchType.ID);
        }
        EditorGUILayout.EndHorizontal();
        GUI.backgroundColor = Color.white;


        sidebarScroll = EditorGUILayout.BeginScrollView(sidebarScroll);

        if (databaseWrapper.Databases.Count == 0)
        {
            EditorGUILayout.LabelField("There's no databases.");
        }
        else
        {
            foreach(IdentifiersDatabase database in databaseWrapper.Databases)
            {
                GUI.color = existingSearchIndex.Key == databaseWrapper.Databases.IndexOf(database)  && sidebarSearchType == SidebarSearchType.ID? Color.yellow : Color.white;
                GUI.backgroundColor = selectedDatabase == database ? Color.green : Color.black;

                if(sidebarSearchType == SidebarSearchType.Database && !string.IsNullOrEmpty(databaseSearch) && !database.Name.ToLower().Contains(databaseSearch.ToLower()))
                {
                    continue;
                }

                EditorGUILayout.BeginHorizontal("HelpBox");
                if (GUILayout.Button(database.name, EditorStyles.label))
                {
                    selectedDatabase = database;
                    ChangeTab(Tabs.RegistryEditor);
                }

                EditorGUILayout.LabelField($"#{database.GUID}", new GUIStyle(ItalicStyle) { normal = { textColor = Color.gray } }, GUILayout.Width(40));
                EditorGUILayout.LabelField($"({database.Registries.Count})", new GUIStyle(ItalicStyle) { normal = { textColor = Color.gray } }, GUILayout.Width(40));
                EditorGUILayout.EndHorizontal();
            }
            GUI.backgroundColor = Color.white;
            GUI.color = Color.white;
        }

        EditorGUILayout.EndScrollView();
        EditorGUILayout.EndVertical();
    }

    #endregion

    #region New Database Tab
    private void DrawNewDatabaseWindow()
    {
        EditorGUILayout.BeginVertical();
        EditorGUILayout.LabelField("Create New Database", new GUIStyle(BoldStyle) { fontSize = 16 });
        newDatabaseName = EditorGUILayout.TextField("Database Name", newDatabaseName);

        EditorGUILayout.LabelField("Description:");
        newDatabaseDesc = EditorGUILayout.TextArea(newDatabaseDesc);

        if (GUILayout.Button("Create Database"))
        {
            CreateNewDatabase();
        }
        EditorGUILayout.EndVertical();
    }
    #endregion

    #region Registries Drawers
    private void DrawRegistryEditorTab()
    {
        //Header
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("Database:", BoldStyle, GUILayout.Width(70));
        EditorGUILayout.LabelField("#" + selectedDatabase.GUID, new GUIStyle(ItalicStyle) { normal = { textColor = Color.gray } });

        GUI.backgroundColor = Color.gray;
        if (GUILayout.Button("Create CSV", EditorStyles.miniButtonLeft, GUILayout.Width(110)))
        {
            GenerateDatabaseCSV(selectedDatabase);
        }
        if (GUILayout.Button("Ping", EditorStyles.miniButtonMid, GUILayout.Width(70)))
        {
            PingObject(selectedDatabase);
        }
        GUI.backgroundColor = Color.red;
        if (GUILayout.Button("Delete", EditorStyles.miniButtonRight, GUILayout.Width(70)))
        {
            DeleteDatabase();
            return;
        }
        GUI.backgroundColor = Color.white;
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.LabelField(selectedDatabase.Name, new GUIStyle(BoldStyle) { fontSize = 18 });

        EditorGUILayout.LabelField("Description: " + selectedDatabase.Description, DescriptionStyle);

        EditorGUILayout.Space();

        //Create Registry
        EditorGUILayout.BeginVertical("HelpBox");

        GUI.backgroundColor = Color.black;
        EditorGUILayout.BeginHorizontal("HelpBox");
        EditorGUILayout.LabelField("Create Registry:", new GUIStyle(BoldStyle) { fontSize = 13 });
        EditorGUILayout.EndHorizontal();
        GUI.backgroundColor = Color.white;

        newRegistryIdentifier = EditorGUILayout.TextField("Identifier:", newRegistryIdentifier);

        EditorGUI.BeginChangeCheck();
        createNewCategory = EditorGUILayout.Toggle("New Category", createNewCategory);
        if(EditorGUI.EndChangeCheck())
        {
            newRegistryCategory = string.Empty;
        }

        if (createNewCategory)
        {
            newRegistryCategory = EditorGUILayout.TextField("Category:", newRegistryCategory);
        }
        else
        {
            newRegistryCategory = FilterDefaultElements(GetPopupElementByString(newRegistryCategory, selectedDatabase.GetCategories("<Empty>"), "Category:"), categoryDefaultValues);
        }

        EditorGUILayout.LabelField("NOTE: Category is only to sort on the Editor and doesn't have impact on the final ID.", DescriptionStyle);

        EditorGUILayout.Space();

        if (GUILayout.Button("Create Registry"))
        {
            TryCreateRegistry();
        }
        EditorGUILayout.EndVertical();
        EditorGUILayout.Space();

        DrawRegistries();
    }

    private void DrawRegistries()
    {
        GUI.backgroundColor = Color.black;
        EditorGUILayout.BeginVertical("Box");
        GUI.backgroundColor = Color.white;
        EditorGUILayout.LabelField("Registries:", new GUIStyle(EditorStyles.label) { fontSize = 14, fontStyle = FontStyle.Bold });

        if (selectedDatabase.Registries.Count == 0)
        {
            EditorGUILayout.BeginHorizontal("HelpBox");
            EditorGUILayout.LabelField("There's no registers on this database.");
            EditorGUILayout.EndHorizontal();
        }
        else
        {
            GUI.backgroundColor = Color.gray;
            EditorGUILayout.BeginHorizontal("HelpBox");
            toSearch = EditorGUILayout.TextField("Search", toSearch);
            registriesCategorySearch = FilterDefaultElements(GetPopupElementByString(registriesCategorySearch, selectedDatabase.GetCategories("All"), labelWidth: 120), categoryDefaultValues);

            EditorGUILayout.EndHorizontal();
            GUI.backgroundColor = Color.white;

            EditorGUILayout.BeginVertical("HelpBox");

            registriesScroll = EditorGUILayout.BeginScrollView(registriesScroll);
            for (int i = 0; i < selectedDatabase.Registries.Count; i++)
            {
                IdentifierRegistry data = selectedDatabase.Registries[i];
                if ((!string.IsNullOrEmpty(toSearch) && !data.Identifier.ToLower().Contains(toSearch.ToLower())) && !data.GUID.ToLower().Contains(toSearch.ToLower()) || (!string.IsNullOrEmpty(registriesCategorySearch) && !data.Category.Equals(registriesCategorySearch)))
                {
                    continue;
                }

                GUI.backgroundColor = existingSearchIndex.Value == i && sidebarSearchType == SidebarSearchType.ID ? Color.yellow : Color.white;
                EditorGUILayout.BeginHorizontal("Box");

                EditorGUI.BeginChangeCheck();
                data.Identifier = EditorGUILayout.TextField(data.Identifier);
                if(EditorGUI.EndChangeCheck())
                {
                    if(string.IsNullOrEmpty(data.Identifier))
                    {
                        data.Identifier = data.GUID;
                    }
                }

                data.Category = FilterDefaultElements(GetPopupElementByString(data.Category, selectedDatabase.GetCategories("<No Label>"), labelWidth: 120), categoryDefaultValues);
                EditorGUILayout.LabelField("ID: " + data.GUID, ItalicStyle);

                //Buttons
                if (GUILayout.Button("Copy", EditorStyles.miniButtonLeft, GUILayout.Width(70)))
                {
                    GUIUtility.systemCopyBuffer = data.GUID;
                }
                if (GUILayout.Button("Delete", EditorStyles.miniButtonMid, GUILayout.Width(70)))
                {
                    TryDeleteRegistry(data);
                }
                if (GUILayout.Button("Save", EditorStyles.miniButtonRight, GUILayout.Width(70)))
                {
                    SaveAssets();
                }
                EditorGUILayout.EndHorizontal();
            }
            EditorGUILayout.EndScrollView();
            EditorGUILayout.EndVertical();
        }
        EditorGUILayout.EndVertical();
    }

    #endregion

    #region Creation Methods
    private void CreateNewDatabase()
    {
        selectedDatabase = databaseWrapper.CreateDatabase(newDatabaseName, newDatabaseDesc, DATABASES_PATH);
        SaveAssets();
        ChangeTab(Tabs.RegistryEditor);
    }
    private void TryCreateRegistry()
    {
        if (string.IsNullOrEmpty(newRegistryIdentifier))
        {
            EditorUtility.DisplayDialog("Empty Identifier", "The Register you are trying to create has an empty Identifier. Add a name to continue.", "Continue");
        }
        else if (selectedDatabase.IdentifierExists(newRegistryIdentifier))
        {
            EditorUtility.DisplayDialog("Identifier Registered", "The identifier you want to create already exists. Try a different name and try again.", "Continue");
        }
        else
        {
            CreateRegistry();
        }
    }

    private void CreateRegistry()
    {
        selectedDatabase.CreateRegistry(newRegistryIdentifier, FilterDefaultElements(newRegistryCategory, categoryDefaultValues));
        newRegistryIdentifier = null;
        SaveAssets();
    }
    #endregion

    #region Deletion Methods
    private void DeleteDatabase()
    {
        int answer = EditorUtility.DisplayDialogComplex("Erase Database?", "All registries will be lost.", "Erase", "Cancel", string.Empty);

        if (answer == 0)
        {
            databaseWrapper.RemoveDatabase(selectedDatabase);
            if (databaseWrapper.Databases.Count == 0)
            {
                ChangeTab(Tabs.NewDatabase);
            }
            SaveAssets();
            ChangeTab(Tabs.NewDatabase);
            selectedDatabase = null;
        }
    }

    private void TryDeleteRegistry(IdentifierRegistry toDelete)
    {
        int answer = EditorUtility.DisplayDialogComplex("Delete Registry?", "There's no turning back after deleting. Do you want to continue?", "Delete", "Cancel", string.Empty);
        if (answer == 0)
        {
            DeleteRegistry(toDelete);
        }
    }

    private void DeleteRegistry(IdentifierRegistry toDelete)
    {
        selectedDatabase.DeleteRegistry(toDelete);
        SaveAssets();
    }
    #endregion

    #region Utils
    private void FindWrapper()
    {
        databaseWrapper = GetIdentifiersDatabases();
    }

    private string GetPopupElementByString(string currentSelection, string[] elements, string label = "", int labelWidth = 0)
    {
        if (elements.Length == 0)
        {
            return string.Empty;
        }

        int selection = 0;
        if (!string.IsNullOrEmpty(currentSelection))
        {
            for (int i = 0; i < elements.Length; i++)
            {
                if (elements[i] == currentSelection)
                {
                    selection = i;
                    break;
                }
            }
        }

        if(labelWidth > 0)
        {
            return elements[EditorGUILayout.Popup(label, selection, elements, GUILayout.Width(labelWidth))];
        }
        else
        {
            return elements[EditorGUILayout.Popup(label, selection, elements)];
        }
    }

    private void ResetNewElementVariables()
    {
        newRegistryIdentifier = string.Empty;
        newRegistryCategory = string.Empty;
        newDatabaseName = string.Empty;
        newDatabaseDesc = string.Empty;
        newRegistryCategory = string.Empty;
        existingSearchIndex = new KeyValuePair<int, int>(-1, -1);
        registriesCategorySearch = string.Empty;
    }

    private void PingObject(Object toPing)
    {
        Selection.activeObject = toPing;
        EditorGUIUtility.PingObject(toPing);
    }

    private string FilterDefaultElements(string toCheck, string[] defaultValues)
    {
        for(int i = 0; i < defaultValues.Length; i++)
        {
            if(toCheck == defaultValues[i])
            {
                return string.Empty;
            }
        }
        return toCheck;
    }

    private void SaveAssets()
    {
        if (selectedDatabase != null)
        {
            EditorUtility.SetDirty(selectedDatabase);
        }

        EditorUtility.SetDirty(databaseWrapper);
        AssetDatabase.SaveAssets();
    }

    private void GenerateAllDatabasesCSV()
    {
        if(databaseWrapper == null)
        {
            EditorUtility.DisplayDialog("Wrapper Doesn't Exist", "There's no Wrapper containing Databases to generate a CSV File.", "Continue");
            return;
        }
        if(databaseWrapper.Databases.Count == 0)
        {
            EditorUtility.DisplayDialog("There's no Databases", "There's no Databases on the wrapper to generate a CSV File.", "Continue");
            return;
        }

        string folderPath = EditorUtility.SaveFolderPanel("Save Identifiers CSV", "", "IdentifiersCSVBackup");
        string fileName = CSV_FILE_NAME.Replace("{0}", "All");
        
        CSVGenerator.GenerateCSVFile(folderPath, fileName, databaseWrapper);
    }
    private void GenerateDatabaseCSV(IdentifiersDatabase database)
    {
        if(database == null)
        {
            EditorUtility.DisplayDialog("Database doesn't exist", "Select a Database first to create a CSV File.", "Continue");
            return;
        }
        if(database.Registries.Count == 0)
        {
            EditorUtility.DisplayDialog("There's no Registries", "The Database you want to generate doesn't have registries.", "Continue");
            return;
        }
        
        string folderPath = EditorUtility.SaveFolderPanel("Save Identifiers CSV", "", "IdentifiersCSVBackup");
        string fileName = CSV_FILE_NAME.Replace("{0}", database.Name);
        
        CSVGenerator.GenerateCSVFile(folderPath, fileName, database);
    }

    #endregion

    #region Styles
    
    private GUIStyle DescriptionStyle { get; set; }
    private GUIStyle BoldStyle { get; set; }
    private GUIStyle ItalicStyle { get; set; }

    private void InitializeStyles()
    {
        DescriptionStyle = new GUIStyle(EditorStyles.label) {fontSize = 10};
        BoldStyle = new GUIStyle(EditorStyles.label) {fontStyle = FontStyle.Bold};
        ItalicStyle = new GUIStyle(EditorStyles.label) {fontStyle = FontStyle.Italic};
    }

    #endregion

    public class RegistriesDropdown
    {
        private IdentifiersDatabaseWrapper wrapper = null;
        private string[] identifiersPaths = null;
        private string[] identifiersGUID = null;

        private int selectedIndex = 0;

        public string SelectedRegistry => identifiersPaths[selectedIndex];
        public string SelectedGUID => identifiersGUID[selectedIndex];

        public RegistriesDropdown()
        {
            wrapper = GetIdentifiersDatabases();
            wrapper.GetRegistriesPathsAndGUID(out identifiersPaths, out identifiersGUID);
        }
        

        public string DrawGUI(Rect rect, string label, string currentGUID)
        {
            if (identifiersPaths.Length == 0)
            {
                EditorGUI.LabelField(rect, "There's no Identifiers registered.");
                return string.Empty;
            }

            if (currentGUID != SelectedGUID)
            {
                selectedIndex = GetGUIDIndex(currentGUID);
            }

            selectedIndex = EditorGUI.Popup(rect, label, selectedIndex, identifiersPaths);

            return SelectedGUID;
        }

        public string DrawGUILayout(string label, string currentGUID)
        {
            if (identifiersPaths.Length == 0)
            {
                EditorGUILayout.LabelField("There's no Identifiers registered.");
                return string.Empty;
            }

            if (currentGUID != SelectedGUID)
            {
                selectedIndex = GetGUIDIndex(currentGUID);
            }

            selectedIndex = EditorGUILayout.Popup(label, selectedIndex, identifiersPaths);

            return SelectedGUID;
        }

        private int GetGUIDIndex(string element)
        {
            for (int i = 0; i < identifiersGUID.Length; i++)
            {
                if (identifiersGUID[i] == element)
                {
                    return i;
                }
            }

            return -1;
        }
    }
}
