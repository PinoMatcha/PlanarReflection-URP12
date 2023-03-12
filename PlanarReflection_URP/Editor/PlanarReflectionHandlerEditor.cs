using UnityEngine;
using UnityEditor;

namespace PMP {
    [CanEditMultipleObjects]
    [CustomEditor(typeof(PlanarReflectionHandler))]
    public class PlanarReflectionHandlerEditor : Editor {

        public override void OnInspectorGUI() {
            PMP.LayoutUtility.TitleAndCredit("鏡面反射", "© 2023 ピノまっちゃ");
            PlanarReflectionHandler handler = target as PlanarReflectionHandler;
            if (handler != null) {
                serializedObject.Update();
                Properties(handler);
                serializedObject.ApplyModifiedProperties();
            }
        }

        void Properties(PlanarReflectionHandler handler) {
            handler.mainCameraOverride = EditorGUILayout.ObjectField("Main Camera", handler.mainCameraOverride, typeof(Camera), true) as Camera;

            handler.reflectionPanel = EditorGUILayout.ObjectField("反射用オブジェクト", handler.reflectionPanel, typeof(GameObject), true) as GameObject;
            handler.reflectionPanelUpOverride = EditorGUILayout.ObjectField("反射面上書き", handler.reflectionPanelUpOverride, typeof(Transform), true) as Transform;

            using (new GUILayout.VerticalScope(EditorStyles.helpBox)) {
                LayoutUtility.HeaderField(Color.gray, "描画設定", Color.white);

                float resMin = 0.01f;
                float resMax = 1f;
                handler.renderTextureResolutionMultiplier = EditorGUILayout.Slider("解像度倍率", handler.renderTextureResolutionMultiplier, resMin, resMax);

                handler.checkResolutionChangeEveryFrame = EditorGUILayout.Toggle("解像度をリアルタイムに変更する", handler.checkResolutionChangeEveryFrame);

                EditorGUILayout.PropertyField(serializedObject.FindProperty("cullingMask"), new GUIContent("カリングマスク"));

                handler.refCamRenderInterval = EditorGUILayout.IntField("描画間隔（Frame）", Mathf.Max(0, handler.refCamRenderInterval));

                GUILayout.Space(5);
            }

            using (new GUILayout.VerticalScope(EditorStyles.helpBox)) {
                LayoutUtility.HeaderField(Color.gray, "フレームブレンド設定", Color.white);

                handler.useFrameBlendBlur = EditorGUILayout.Toggle("フレームブレンドを使用", handler.useFrameBlendBlur);

                EditorGUI.BeginDisabledGroup(!handler.useFrameBlendBlur);
                EditorGUILayout.PropertyField(serializedObject.FindProperty("frameBlendShader"), new GUIContent("フレームブレンドシェーダー"));
                EditorGUI.EndDisabledGroup();

                GUILayout.Space(5);
            }

            using (new GUILayout.VerticalScope(EditorStyles.helpBox)) {
                LayoutUtility.HeaderField(Color.gray, "ぼかし設定", Color.white);

                handler.useExtraBlur = EditorGUILayout.Toggle("ぼかしを使用", handler.useExtraBlur);

                EditorGUI.BeginDisabledGroup(!handler.useExtraBlur);
                {
                    float bStrMin = 0f;
                    float bStrMax = 5f;
                    handler.extraBlurStrength = EditorGUILayout.Slider("ぼかしの強さ", handler.extraBlurStrength, bStrMin, bStrMax);
                    handler.extraBlurHighQuality = EditorGUILayout.Toggle("高品質", handler.extraBlurHighQuality);

                    EditorGUILayout.PropertyField(serializedObject.FindProperty("kernelBlurShader"), new GUIContent("ぼかしシェーダー"));
                }
                EditorGUI.EndDisabledGroup();

                GUILayout.Space(5);
            }

            if (GUILayout.Button("停止", GUILayout.Height(30))) {
                handler.StopReflectionRendering();
            }

            if (GUILayout.Button("再開", GUILayout.Height(30))) {
                handler.ResumeReflectionRendering();
            }
        }

        private void OnSceneGUI() {

            PlanarReflectionHandler handler = target as PlanarReflectionHandler;

            var pTrns = handler.GetPanelTransform();

            Handles.DrawSolidRectangleWithOutline(
                new[] {
                    pTrns.position + (-pTrns.right + pTrns.forward) / 2,
                    pTrns.position + (pTrns.right + pTrns.forward) / 2,
                    pTrns.position + (pTrns.right - pTrns.forward) / 2,
                    pTrns.position + (-pTrns.right - pTrns.forward) / 2,
                },
                new Color(0.52f, 0.80f, 0.92f, 0.50f),
                new Color(1.00f, 1.00f, 1.00f, 1.00f));
        }
    }
}