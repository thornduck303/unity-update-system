#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;

namespace ThornDuck.UpdateSystem.Editor{
    [CustomEditor(typeof(UpdateProfile)), CanEditMultipleObjects]
    public class UpdateProfileEditor : UnityEditor.Editor
    {
        private bool isUsingFPS;

        private SerializedProperty priorityProp;
        private SerializedProperty deltaFrameProp;
        private SerializedProperty updateModeProp;
        private SerializedProperty frameOffsetProp;
        private SerializedProperty targetDeltaTimeProp;

        private void OnEnable()
        {
            priorityProp = serializedObject.FindProperty("priority");
            deltaFrameProp = serializedObject.FindProperty("deltaFrame");
            updateModeProp = serializedObject.FindProperty("updateMode");
            frameOffsetProp = serializedObject.FindProperty("frameOffset");
            targetDeltaTimeProp = serializedObject.FindProperty("targetDeltaTimeSeconds");
        }

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            serializedObject.Update();

            if(Application.isPlaying)
                GUI.enabled = false;
            EditorGUILayout.PropertyField(
                priorityProp,
                new GUIContent(
                    "Priority",
                    "Entity execution priority (descending order)"
                )
            );
            GUI.enabled = true;
            
            EditorGUILayout.PropertyField(
                updateModeProp,
                new GUIContent(
                    "Update Mode",
                    "How this entity should update over time"
                )
            );

            UpdateMode mode = (UpdateMode)updateModeProp.enumValueIndex;
            
            EditorGUILayout.Space();
            switch (mode)
            {
                case UpdateMode.FixedTime:
                    EditorGUILayout.LabelField("Fixed frame duration update", EditorStyles.boldLabel);
                
                    isUsingFPS = EditorGUILayout.Toggle(
                        new GUIContent(
                            "Use FPS",
                            "Use frames per second measure"
                        ),
                        isUsingFPS
                    );

                    if (isUsingFPS)
                    {
                        float fps = 1f / targetDeltaTimeProp.floatValue;
                        fps = EditorGUILayout.FloatField(
                            new GUIContent("Target FPS", "Target frame rate to execute"),
                            fps
                        );
                        targetDeltaTimeProp.floatValue = 1f / Mathf.Clamp(
                            fps,
                            1f/UpdateSettings.MAX_DELTA_TIME_SECONDS,
                            1f/UpdateSettings.MIN_DELTA_TIME_SECONDS);
                    }
                    else
                    {
                        EditorGUILayout.PropertyField(
                            targetDeltaTimeProp,
                            new GUIContent(
                                "Target duration",
                                "Target time elapsed between frames, in seconds, for execution"
                            )
                        );
                        targetDeltaTimeProp.floatValue = Mathf.Clamp(
                            targetDeltaTimeProp.floatValue,
                            UpdateSettings.MIN_DELTA_TIME_SECONDS,
                            UpdateSettings.MAX_DELTA_TIME_SECONDS);
                    }
                    break;

                case UpdateMode.FixedFrame:
                    EditorGUILayout.LabelField("Fixed frame count update", EditorStyles.boldLabel);
                    EditorGUILayout.PropertyField(
                        deltaFrameProp,
                        new GUIContent(
                            "Frame count",
                            "Number of frames between updates, including the current frame"
                        )
                    );
                    deltaFrameProp.intValue = Mathf.Max(deltaFrameProp.intValue, 1);
                    EditorGUILayout.PropertyField(
                        frameOffsetProp,
                        new GUIContent(
                            "Frame offset",
                            "Number of frames to start first update call"
                        )
                    );
                    frameOffsetProp.intValue = Mathf.Max(frameOffsetProp.intValue, 0);
                    break;
            }

            serializedObject.ApplyModifiedProperties();
        }
    }
}
#endif