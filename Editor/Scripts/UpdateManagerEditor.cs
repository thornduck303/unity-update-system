#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;

namespace ThornDuck.UpdateSystem.Editor{
    [CustomEditor(typeof(UpdateManager))]
    public class UpdateManagerEditor : UnityEditor.Editor
    {
        private SerializedProperty runningProp;

        private void OnEnable()
            => runningProp = serializedObject.FindProperty("isRunning");

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            serializedObject.Update();

            if (EditorApplication.isPlaying)
            {
                if (runningProp.boolValue)
                {
                    if (GUILayout.Button(
                            new GUIContent(
                            "Pause",
                            "Pause manager execution"
                            )
                        ))
                        ((UpdateManager)target).Pause();
                }
                else
                {
                    if (GUILayout.Button(
                            new GUIContent(
                            "Resume",
                            "Resume manager execution"
                            )
                        ))
                        ((UpdateManager)target).Resume();
                }

                if (GUILayout.Button(
                        new GUIContent(
                            "Log",
                            "Log the current start of the manager"
                        )
                    ))
                    ((UpdateManager)target).LogCurrentState();
            }
            else
            {
                EditorGUILayout.PropertyField(
                    runningProp,
                    new GUIContent(
                        "Start running",
                        "Start running instance update queue"
                    )
                );
            }

            serializedObject.ApplyModifiedProperties();
        }
    }
}
#endif