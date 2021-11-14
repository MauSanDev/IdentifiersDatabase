using UnityEngine;
using UnityEditor;

[CustomPropertyDrawer(typeof(IdentifierReference))]
public class IdentifierReferencePropertyDrawer : PropertyDrawer
{
    private IdentifiersDatabaseWrapper wrapper = null;
    private IdentifiersDatabaseEditor.RegistriesDropdown registriesDropdown = new IdentifiersDatabaseEditor.RegistriesDropdown();

    public IdentifierReferencePropertyDrawer() : base()
    {
        wrapper = IdentifiersDatabaseEditor.GetIdentifiersDatabases();
    }

    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        SerializedProperty assignedIdProperty = property.FindPropertyRelative("assignedId");

        EditorGUI.BeginProperty(position, label, property);

        Rect labelRect = new Rect(position.x + 3, position.y, position.width / 2, EditorGUI.GetPropertyHeight(property));
        Rect assignedIdRect = new Rect(position.x + position.width / 3, position.y, position.width, EditorGUI.GetPropertyHeight(property));
        Rect dropdownRect = new Rect(position.x, position.y + 20, position.width, EditorGUI.GetPropertyHeight(property));
        Rect copyButtonRect = new Rect(position.x + position.width * 0.65f, position.y, position.width / 6, EditorGUI.GetPropertyHeight(property));
        Rect openEditorButtonRect = new Rect(position.x + position.width * 0.80f, position.y, position.width / 6, EditorGUI.GetPropertyHeight(property));

        //Top Row
        GUI.backgroundColor = wrapper.Databases.Count == 0 ? Color.red : Color.white;
        EditorGUI.HelpBox(position, string.Empty, MessageType.None);
        EditorGUI.LabelField(labelRect, $"{property.displayName} (Identifier Reference):", EditorStyles.boldLabel);

        EditorGUI.LabelField(assignedIdRect, $"Assigned ID: {(string.IsNullOrEmpty(assignedIdProperty.stringValue) ? "Empty" : assignedIdProperty.stringValue)}", new GUIStyle(EditorStyles.label) { fontStyle = FontStyle.Italic, normal = { textColor = string.IsNullOrEmpty(assignedIdProperty.stringValue) ? Color.red : Color.gray } });
        GUI.backgroundColor = Color.gray;
        if (GUI.Button(copyButtonRect, "Copy ID", EditorStyles.miniButtonLeft))
        {
            GUIUtility.systemCopyBuffer = assignedIdProperty.stringValue;
        }
        GUI.backgroundColor = Color.white;
        GUI.backgroundColor = Color.gray;
        if (GUI.Button(openEditorButtonRect, "Open Editor", EditorStyles.miniButtonRight))
        {
            EditorWindow.GetWindow<IdentifiersDatabaseEditor>().Show();
        }
        GUI.backgroundColor = Color.white;

        assignedIdProperty.stringValue = registriesDropdown.DrawGUI(dropdownRect, string.Empty, assignedIdProperty.stringValue);
    
        EditorGUI.EndProperty();
    }
    public override float GetPropertyHeight(SerializedProperty property, GUIContent label) => 45f;
}