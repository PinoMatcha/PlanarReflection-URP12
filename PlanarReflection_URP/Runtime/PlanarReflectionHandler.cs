using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using RenderPipeline = UnityEngine.Rendering.RenderPipelineManager;

namespace PMP {
    public class PlanarReflectionHandler : MonoBehaviour {

        // �����_�[�e�N�X�`��
        private RenderTexture refTexture;
        // ���������J����
        private GameObject objRefCamera;
        // ���˗p�p�l���̃}�e���A��
        private Material matRefPalne;

        public RenderTexture GetRenderTexture() { return refTexture; }

        const string REFLECTION_TEXTURE_PROPERTY_NAME = "_ReflectionTex";

        bool isStopped = false;

        [Header("References")]
        public Camera mainCameraOverride = null;
        Camera mainCamera;
        Camera refCamera;

        Transform mainCameraTrns;
        Transform refCameraTrns;

        // ���˗p�p�l���I�u�W�F�N�g
        public GameObject reflectionPanel;
        public Transform reflectionPanelUpOverride;

        public Transform GetPanelTransform() => reflectionPanelUpOverride ? reflectionPanelUpOverride : transform;

        [Header("Settings")]
        public float renderTextureResolutionMultiplier = 1;
        public LayerMask cullingMask;
        public int refCamRenderInterval = 30;
        int tmpFrameCount = 0;
        PlanarReflectionResolutionUpdater resolutionUpdater;
        public bool checkResolutionChangeEveryFrame = false;

        public float GetResolutionMultiplier() => renderTextureResolutionMultiplier;

        [Header("FrameBlend Settings")]
        public bool useFrameBlendBlur = false;
        const int FRAME_BLEND_SAMPLE_COUNT = 3;
        RenderTexture[] frameBlendRtList;
        int fbRtIndex = 0;
        public Shader frameBlendShader;
        Material frameBlendMaterial;

        [Header("extra blur")]
        public bool useExtraBlur = false;
        public bool extraBlurHighQuality = false;
        public float extraBlurStrength = 0.5f;
        public Shader kernelBlurShader;
        Material exBlurMaterial;

        private void OnEnable() {
            RenderPipeline.beginCameraRendering += BeginCameraRendering;
            RenderPipeline.endCameraRendering += EndCameraRendering;
        }

        private void OnDisable() {
            RenderPipeline.beginCameraRendering -= BeginCameraRendering;
            RenderPipeline.endCameraRendering -= EndCameraRendering;
        }

        private void Start() => Initialize();

        void Update() {
            if (isStopped) return;

            if (checkResolutionChangeEveryFrame)
                resolutionUpdater.Check();
        }

        void Initialize() {
            // �����_�[�e�N�X�`������
            refTexture = CreateRenderTexture();

            {   // ���C���J����
                if (!mainCamera) {
                    if (mainCameraOverride)
                        mainCamera = Camera.main;
                    else
                        mainCamera = Camera.main;
                }

                if (mainCamera) {
                    mainCameraTrns = Camera.main.transform;
                } else {
                    Debug.LogError("���C���J������������܂���ł���");
                    enabled = false;
                    return;
                }
            }

            {   // ���˗p�J��������
                if (!objRefCamera) {
                    objRefCamera = new GameObject();
                    objRefCamera.name = "Reflection Camera";
                }

                if (objRefCamera) {
                    refCamera = objRefCamera.AddComponent<Camera>();
                    refCamera.enabled = false;
                    refCameraTrns = objRefCamera.transform;
                    refCamera.targetTexture = refTexture;
                    refCamera.cullingMask = cullingMask;
                    refCamera.fieldOfView = mainCamera.fieldOfView;
                }
            }

            var _obj = reflectionPanel ? reflectionPanel : gameObject;
            matRefPalne = _obj.GetComponent<Renderer>().sharedMaterial;
            if (matRefPalne) {
                if (matRefPalne.HasProperty(REFLECTION_TEXTURE_PROPERTY_NAME)) {
                    matRefPalne.SetTexture(REFLECTION_TEXTURE_PROPERTY_NAME, refTexture);
                } else {
                    Debug.LogError("�}�e���A���v���p�e�B��������܂���ł���");
                    enabled = false;
                    return;
                }
            } else {
                Debug.LogError("�}�e���A����������܂���ł���");
                enabled = false;
                return;
            }

            tmpFrameCount = 0;

            {   // �t���[���u�����h
                frameBlendRtList = new RenderTexture[FRAME_BLEND_SAMPLE_COUNT];
                for (int i = 0; i < frameBlendRtList.Length; i++) {
                    int width = Mathf.RoundToInt(Screen.width * 0.5f);
                    int height = Mathf.RoundToInt(Screen.height * 0.5f);
                    frameBlendRtList[i] = new RenderTexture(width, height, 0, RenderTextureFormat.ARGB32);
                }

                fbRtIndex = 0;

                frameBlendMaterial = new Material(frameBlendShader);

                for (int i = 0; i < frameBlendRtList.Length; i++) {
                    // �V�F�[�_�Ƀ����_�[�e�N�X�`�����Z�b�g
                    //matRefPalne.SetTexture("_MBTex" + i, motionBlurRtList[i]);
                    frameBlendMaterial.SetTexture("_Tex" + i, frameBlendRtList[i]);
                }
            }

            {   // �u���[
                exBlurMaterial = new Material(kernelBlurShader);
            }

            resolutionUpdater = new PlanarReflectionResolutionUpdater(this);

            isStopped= false;
        }

        /// <summary>
        /// �����_�[�e�N�X�`���𐶐�����B
        /// </summary>
        /// <returns></returns>
        RenderTexture CreateRenderTexture() {
            int width = Mathf.RoundToInt(Screen.width * renderTextureResolutionMultiplier);
            int height = Mathf.RoundToInt(Screen.height * renderTextureResolutionMultiplier);
            return new RenderTexture(width, height, 0, RenderTextureFormat.ARGB32);
        }

        /// <summary>
        /// �����_�[�e�N�X�`���̉𑜓x��ύX����B
        /// </summary>
        /// <param name="resolutionMultiplier">�{���i0~1�j</param>
        public void ResizeRenderTexture(float resolutionMultiplier = -1) {
            float min = 0.01f;
            float _mul = 1;
            if (resolutionMultiplier < min) {
                _mul = renderTextureResolutionMultiplier;
            } else {
                _mul = Mathf.Clamp(resolutionMultiplier, min, 1f);
            }

            int width = Mathf.RoundToInt(Screen.width * _mul);
            int height = Mathf.RoundToInt(Screen.height * _mul);
            if (refTexture && (width > 0 && height > 0)) {
                refTexture.Release();
                refTexture.width = width;
                refTexture.height = height;
            }
        }

        /// <summary>
        /// ���˗p�J�����̌v�Z
        /// </summary>
        private void SetReflectionCamera() {
            Vector3 normal = reflectionPanelUpOverride ? reflectionPanelUpOverride.up : transform.up;
            Vector3 pos = transform.position;
            Matrix4x4 mainCamMatrix = mainCamera.worldToCameraMatrix;

            float d = -Vector3.Dot(normal, pos);
            Matrix4x4 refMatrix = CalcReflectionMatrix(new Vector4(normal.x, normal.y, normal.z, d));

            refCamera.worldToCameraMatrix = mainCamera.worldToCameraMatrix * refMatrix;

            Vector3 cpos = refCamera.worldToCameraMatrix.MultiplyPoint(pos);
            Vector3 cnormal = refCamera.worldToCameraMatrix.MultiplyVector(normal).normalized;
            Vector4 clipPlane = new Vector4(cnormal.x, cnormal.y, cnormal.z, -Vector3.Dot(cpos, cnormal));

            refCamera.projectionMatrix = mainCamera.CalculateObliqueMatrix(clipPlane);
        }

        /// <summary>
        /// ���˗p�̃}�g���b�N�X���v�Z
        /// </summary>
        private Matrix4x4 CalcReflectionMatrix(Vector4 n) {
            Matrix4x4 refMatrix = new Matrix4x4();

            refMatrix.m00 = 1f - 2f * n.x * n.x;
            refMatrix.m01 = -2f * n.x * n.y;
            refMatrix.m02 = -2f * n.x * n.z;
            refMatrix.m03 = -2f * n.x * n.w;

            refMatrix.m10 = -2f * n.x * n.y;
            refMatrix.m11 = 1f - 2f * n.y * n.y;
            refMatrix.m12 = -2f * n.y * n.z;
            refMatrix.m13 = -2f * n.y * n.w;

            refMatrix.m20 = -2f * n.x * n.z;
            refMatrix.m21 = -2f * n.y * n.z;
            refMatrix.m22 = 1f - 2f * n.z * n.z;
            refMatrix.m23 = -2f * n.z * n.w;

            refMatrix.m30 = 0F;
            refMatrix.m31 = 0F;
            refMatrix.m32 = 0F;
            refMatrix.m33 = 1F;

            return refMatrix;
        }

        void BeginCameraRendering(ScriptableRenderContext SRC, Camera camera) {
            if (isStopped) return;

            var _frameCount = Time.frameCount;
            refCamRenderInterval = Mathf.Max(0, refCamRenderInterval);
            if (_frameCount - tmpFrameCount < refCamRenderInterval) return;

            SetReflectionCamera();
            GL.invertCulling = true;   // �J�����O���[�h���]
            UniversalRenderPipeline.RenderSingleCamera(SRC, refCamera);   // Camera.Render()��URP��
            GL.invertCulling = false;

            var src = refTexture;
            src.MarkRestoreExpected();

            {   // �t���[���u�����h����
                if (useFrameBlendBlur) {
                    RenderTexture frameBlendBuffer = frameBlendRtList[fbRtIndex];
                    // 1�t���[�����炵����ʂ������_�[�e�N�X�`���ɃR�s�[
                    Graphics.Blit(src, frameBlendBuffer);

                    if (fbRtIndex < FRAME_BLEND_SAMPLE_COUNT - 1) fbRtIndex++; else fbRtIndex = 0;

                    frameBlendMaterial.SetTexture("_MainTex", frameBlendBuffer);
                    frameBlendMaterial.SetInt("_SampleCount", FRAME_BLEND_SAMPLE_COUNT);

                    // Render the image using the shader
                    Graphics.Blit(src, frameBlendBuffer);
                    Graphics.Blit(frameBlendBuffer, src, frameBlendMaterial);
                }
            }

            if (useExtraBlur) {
                // �u���[�o�b�t�@�ɃR�s�[
                var blurBuffer = RenderTexture.GetTemporary(src.width, src.height, 0, src.format);
                Graphics.Blit(src, blurBuffer);

                exBlurMaterial.SetTexture("_MainTex", blurBuffer);
                exBlurMaterial.SetFloat("_BlurStrength", extraBlurStrength);

                // �u���[�o�b�t�@����V�F�[�_�[��ʂ���src�ɃR�s�[
                Graphics.Blit(blurBuffer, src, exBlurMaterial);

                RenderTexture.ReleaseTemporary(blurBuffer);

                if (extraBlurHighQuality) {
                    var blurBufferH1 = RenderTexture.GetTemporary(Screen.width / 4, Screen.height / 4, 0, src.format);
                    var blurBufferH2 = RenderTexture.GetTemporary(Screen.width / 8, Screen.height / 8, 0, src.format);

                    Graphics.Blit(src, blurBufferH1);
                    Graphics.Blit(blurBufferH1, blurBufferH2);
                    Graphics.Blit(blurBufferH2, src);

                    RenderTexture.ReleaseTemporary(blurBufferH1);
                    RenderTexture.ReleaseTemporary(blurBufferH2);
                }
            } else {
                matRefPalne.SetFloat("_BlurStrength", 0);
            }

            tmpFrameCount = _frameCount;   // �t���[���̃J�E���g�����Z�b�g
        }

        void EndCameraRendering(ScriptableRenderContext SRC, Camera camera) {
            if (isStopped) return;
        }

        /// <summary>
        /// ���ˌv�Z���~����
        /// </summary>
        public void StopReflectionRendering() {
            isStopped= true;
        }

        /// <summary>
        /// ���ˌv�Z���ĊJ����
        /// </summary>
        public void ResumeReflectionRendering() {
            isStopped= false;
        }
    }
}