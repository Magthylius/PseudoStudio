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
            if (previewSource == null)
                previewSource = EditorUtility
                    .CreateGameObjectWithHideFlags("Audio previewer", HideFlags.HideAndDontSave, typeof(AudioSource))
                    .GetComponent<AudioSource>();
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
                if (Application.isPlaying)
                    ((AudioEventData)target).Play(Vector3.zero);
                else
                    PlayOnEditor();
            }
            EditorGUI.EndDisabledGroup();
        }

        private void PlayOnEditor()
        {
            
            
        }
    }
}
