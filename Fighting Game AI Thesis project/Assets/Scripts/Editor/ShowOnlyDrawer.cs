using System;
using UnityEditor;
using UnityEngine;
//NickGlenn. (n.d.). Unity3D [ShowOnly] Inspector Attribute. GitHub. https://gist.github.com/NickGlenn/84b8b43004a642b96ce9b6fef0bbcc8d
[CustomPropertyDrawer(typeof(ShowOnlyAttribute))]
public class ShowOnlyDrawer : PropertyDrawer
{
    public override void OnGUI(Rect position, SerializedProperty prop, GUIContent label)
    {
        string valueStr;

        switch (prop.propertyType)
        {
            case SerializedPropertyType.Integer:
                valueStr = prop.intValue.ToString();
                break;
            case SerializedPropertyType.Boolean:
                valueStr = prop.boolValue.ToString();
                break;
            case SerializedPropertyType.Float:
                valueStr = prop.floatValue.ToString("0.00000");
                break;
            case SerializedPropertyType.String:
                valueStr = prop.stringValue;
                break;
            case SerializedPropertyType.Enum:
                 valueStr = prop.enumNames[prop.enumValueIndex];
                break;
            case SerializedPropertyType.ObjectReference:
                 try { valueStr = prop.objectReferenceValue.ToString ();
                 } catch (NullReferenceException) {
                    valueStr = "None (Game Object)"; 
                 }
                 break;
            default:
                valueStr = "(not supported)";
                break;
        }

        EditorGUI.LabelField(position,label.text, valueStr);
    }
}
