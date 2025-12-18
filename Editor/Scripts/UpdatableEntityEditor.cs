#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;

namespace ThornDuck.UpdateSystem.Editor{
    [CustomEditor(typeof(UpdatableEntity), true), CanEditMultipleObjects]
    public class UpdatableEntityEditor : UnityEditor.Editor
    {
        private bool showUpdateSettings;
        private SerializedProperty updateProfileProp;

        private void OnEnable()
            => updateProfileProp = serializedObject.FindProperty("updateProfile");

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            serializedObject.Update();

            GUIStyle boldFoldoutStyle = new GUIStyle(EditorStyles.foldout)
            {
                fontStyle = FontStyle.Bold
            };

            showUpdateSettings = EditorGUILayout.Foldout(
                showUpdateSettings,
                "Update Settings",
                true,
                boldFoldoutStyle
            );

            if (showUpdateSettings)
            {
                if(Application.isPlaying)
                    GUI.enabled = false;
                EditorGUILayout.PropertyField(
                    updateProfileProp,
                    new GUIContent(
                        "Profile Data",
                        "Settings for updatable entities"
                    )
                );
                GUI.enabled = true;
            }

            serializedObject.ApplyModifiedProperties();
        }
    }
}
#endif