using UnityEngine;
using UnityEditor;

namespace Hadal.AudioSystem.Editorial
{
    [CustomEditor(typeof(AudioEventData), true)]
    public class AudioEventDataEditor : Editor
    {
        private AudioSource previewSource;

        public void OnEnable()
        {
            CreateAudioSourceIfNull();
        }
        public void OnDisable()
        {
            DestroyImmediate(previewSource.gameObject);
        }

        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();

            EditorGUI.BeginDisabledGroup(serializedObject.isEditingMultipleObjects);
            if (GUILayout.Button("Preview"))
            {
                CreateAudioSourceIfNull();
                ((AudioEventData)target).Play(previewSource);
            }
            EditorGUI.EndDisabledGroup();
        }

        private void CreateAudioSourceIfNull()
        {
            if (previewSource == null)
                previewSource = EditorUtility
                    .CreateGameObjectWithHideFlags("Audio previewer", HideFlags.HideAndDontSave, typeof(AudioSource))
                    .GetComponent<AudioSource>();
        }
    }
}
