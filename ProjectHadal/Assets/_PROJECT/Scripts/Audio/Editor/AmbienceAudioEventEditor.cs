using UnityEngine;
using UnityEditor;

namespace Hadal.AudioSystem.Editorial
{
    [CustomEditor(typeof(AmbienceAudioEvent), true)]
    public class AmbienceAudioEventEditor : AudioEventDataEditor
    {
        private bool isPaused = false;

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            EditorGUI.BeginDisabledGroup(serializedObject.isEditingMultipleObjects);
            if (GUILayout.Button("Toggle Pause"))
            {
                CreateAudioSourceIfNull();
                isPaused = !isPaused;
                ((AudioEventData)target).Pause(isPaused);
            }
            if (GUILayout.Button("Stop Preview"))
            {
                CreateAudioSourceIfNull();
                ((AudioEventData)target).Stop(true);
            }
            EditorGUI.EndDisabledGroup();
        }
    }
}
