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
            EditorGUILayout.LabelField(string.Empty);
            EditorGUILayout.HelpBox(((AudioEventData)target).Description, MessageType.Info, true);
            EditorGUILayout.LabelField(string.Empty);

            DrawDefaultInspector();

            EditorGUILayout.Space(20);

            //! it will be chaotic if we allowed multiple previews to trigger at once, composite audio events are made specifically for this
            EditorGUI.BeginDisabledGroup(serializedObject.isEditingMultipleObjects);
            if (GUILayout.Button("Preview Sound"))
            {
                CreateAudioSourceIfNull();
                ((AudioEventData)target).Play(previewSource);
            }
            EditorGUI.EndDisabledGroup();
        }

        protected void CreateAudioSourceIfNull()
        {
            if (previewSource == null)
                previewSource = EditorUtility
                    .CreateGameObjectWithHideFlags("Audio previewer", HideFlags.HideAndDontSave, typeof(AudioSource))
                    .GetComponent<AudioSource>();
        }
    }
}
