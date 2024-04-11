//using UnityEngine;
//using UnityEditor;

//[CustomEditor(typeof(Interactable))]
//public class HideUnwantedVariables : Editor
//{
//    public override void OnInspectorGUI()
//    {
//        serializedObject.Update();

//        SerializedProperty iterator = serializedObject.GetIterator();
//        bool enterChildren = true;

//        while (iterator.NextVisible(enterChildren))
//        {
//            enterChildren = false;

//            if (iterator.propertyType == SerializedPropertyType.ObjectReference && iterator.objectReferenceValue == null)
//                continue;

//            EditorGUILayout.PropertyField(iterator, true);
//        }

//        serializedObject.ApplyModifiedProperties();
//    }
//}
