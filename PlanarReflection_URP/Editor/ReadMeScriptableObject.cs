using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace PMP {
    public class ReadMeScriptableObject : ScriptableObject { }

    [CustomEditor(typeof(ReadMeScriptableObject))]
    public class ReadMeScriptableObjectEditor : Editor {

        public override void OnInspectorGUI() {
            // base.OnInspectorGUI();

            GUILayout.Space(20);

            EditorGUILayout.LabelField("Read Me", new GUIStyle() {
                alignment = TextAnchor.MiddleCenter,
                fontStyle = FontStyle.Bold,
                fontSize = 20,
            });

            GUILayout.Space(15);

            EditorGUILayout.LabelField("利用規約やドキュメント、その他なんやかんやはGitHubに置いてあります。\n一度目を通すようお願いします。", new GUIStyle() {
                alignment = TextAnchor.MiddleCenter,
                fontSize = 14,
            });

            GUILayout.Space(10);

            var url = "https://github.com/PinoMatcha/PlanarReflection_URP";

            LayoutUtility.LinkButton(url, url, new GUIStyle() {
                alignment = TextAnchor.MiddleCenter,
                fontSize = 14,
            });

            GUILayout.Space(15);

            EditorGUILayout.LabelField("© 2023 ピノまっちゃ", new GUIStyle() {
                alignment = TextAnchor.MiddleCenter,
                fontSize = 12,
            });
        }
    }
}