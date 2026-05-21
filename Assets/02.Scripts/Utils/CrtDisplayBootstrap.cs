using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.Universal;
using UnityEngine.UI;

[DisallowMultipleComponent]
public sealed class CrtDisplayBootstrap : MonoBehaviour
{
    [SerializeField] private Shader displayShader;
    [SerializeField] private Camera sourceCamera;
    [SerializeField] private Vector2Int referenceResolution = new Vector2Int(1920, 1080);
    [SerializeField] private int displaySortingOrder = 1000;
    [SerializeField] private float displayCameraDepthOffset = 1.0f;
    [SerializeField] private Canvas[] capturedScreenSpaceOverlayCanvases;
    [SerializeField] private float capturedCanvasPlaneDistance = 10.0f;
    [SerializeField] private float displayCanvasPlaneDistance = 1.0f;
    [SerializeField] private Color displayTint = new Color(0.0f, 1.0f, 0.78f, 1.0f);
    [SerializeField, Range(0.0f, 1.0f)] private float monochrome = 0.9f;
    [SerializeField, Range(0.0f, 1.0f)] private float scanlineStrength = 0.34f;
    [SerializeField, Range(0.0f, 1.0f)] private float maskStrength = 0.42f;
    [SerializeField, Range(0.0f, 1.0f)] private float noiseStrength = 0.055f;
    [SerializeField, Range(0.0f, 1.0f)] private float distortion = 0.045f;
    [SerializeField, Range(0.0f, 0.02f)] private float chromaticOffset = 0.0035f;
    [SerializeField, Range(0.0f, 1.0f)] private float vignetteStrength = 0.48f;
    [SerializeField, Range(0.0f, 2.0f)] private float glowStrength = 0.65f;

    private RenderTexture displayTexture;
    private Material displayMaterial;
    private Camera displayCamera;
    private GameObject displayCanvasObject;
    private RawImage displayImage;
    private readonly List<CanvasState> capturedCanvasStates = new List<CanvasState>();
    private RenderTexture originalSourceCameraTargetTexture;
    private bool hasSourceCameraState;
    private int textureWidth;
    private int textureHeight;

    private void Awake()
    {
        EnsureSourceCamera();
    }

    private void OnEnable()
    {
        EnsureSourceCamera();
        BuildDisplayCamera();
        BuildDisplay();
    }

    private void Update()
    {
        if (sourceCamera == null)
        {
            return;
        }

        if (Screen.width != textureWidth || Screen.height != textureHeight)
        {
            RebuildRenderTexture();
        }

        ApplyMaterialSettings();
    }

    private void OnDisable()
    {
        CleanupDisplayPath();
    }

    private void EnsureSourceCamera()
    {
        if (sourceCamera == null)
            Debug.LogWarning("CRT Display Bootstrap requires an explicit source camera reference.", this);
    }

    private void BuildDisplay()
    {
        if (sourceCamera == null)
        {
            Debug.LogWarning("CRT Display Bootstrap requires a source camera.", this);
            return;
        }

        if (displayMaterial == null)
        {
            Shader shader = displayShader;
            if (shader == null)
            {
                Debug.LogWarning("CRT display shader was not found.", this);
                return;
            }

            displayMaterial = new Material(shader)
            {
                name = "Runtime CRT Display Material"
            };
        }

        BuildDisplayCamera();
        CaptureScreenSpaceOverlayCanvases();

        if (displayTexture == null)
        {
            RebuildRenderTexture();
        }

        if (displayCanvasObject != null)
        {
            ApplyMaterialSettings();
            return;
        }

        int uiLayer = GetUiLayer();
        displayCanvasObject = new GameObject("CRT Display Canvas");
        displayCanvasObject.layer = uiLayer;
        displayCanvasObject.transform.SetParent(transform, false);

        Canvas canvas = displayCanvasObject.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceCamera;
        canvas.worldCamera = displayCamera;
        canvas.planeDistance = displayCanvasPlaneDistance;
        canvas.sortingOrder = displaySortingOrder;

        CanvasScaler scaler = displayCanvasObject.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = referenceResolution;
        scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
        scaler.matchWidthOrHeight = 0.5f;

        displayCanvasObject.AddComponent<GraphicRaycaster>();

        GameObject imageObject = new GameObject("CRT Display RawImage");
        imageObject.layer = uiLayer;
        imageObject.transform.SetParent(displayCanvasObject.transform, false);

        displayImage = imageObject.AddComponent<RawImage>();
        displayImage.texture = displayTexture;
        displayImage.material = displayMaterial;
        displayImage.raycastTarget = false;

        RectTransform rectTransform = displayImage.rectTransform;
        rectTransform.anchorMin = Vector2.zero;
        rectTransform.anchorMax = Vector2.one;
        rectTransform.offsetMin = Vector2.zero;
        rectTransform.offsetMax = Vector2.zero;

        ApplyMaterialSettings();
    }

    private void BuildDisplayCamera()
    {
        if (displayCamera != null)
        {
            return;
        }

        GameObject cameraObject = new GameObject("CRT Display Camera");
        cameraObject.transform.SetParent(transform, false);

        displayCamera = cameraObject.AddComponent<Camera>();
        displayCamera.enabled = true;
        displayCamera.clearFlags = CameraClearFlags.SolidColor;
        displayCamera.backgroundColor = Color.black;
        displayCamera.cullingMask = 1 << GetUiLayer();
        displayCamera.orthographic = true;
        displayCamera.orthographicSize = 5.0f;
        displayCamera.nearClipPlane = 0.1f;
        displayCamera.farClipPlane = 10.0f;
        displayCamera.depth = sourceCamera != null ? sourceCamera.depth + displayCameraDepthOffset : 100.0f;
        displayCamera.targetTexture = null;
        displayCamera.targetDisplay = sourceCamera != null ? sourceCamera.targetDisplay : 0;
        displayCamera.allowHDR = false;
        displayCamera.allowMSAA = false;

        UniversalAdditionalCameraData cameraData = cameraObject.AddComponent<UniversalAdditionalCameraData>();
        cameraData.renderType = CameraRenderType.Base;
        cameraData.renderPostProcessing = false;
        cameraData.requiresColorOption = CameraOverrideOption.Off;
        cameraData.requiresDepthOption = CameraOverrideOption.Off;
    }

    private int GetUiLayer()
    {
        int uiLayer = LayerMask.NameToLayer("UI");
        return uiLayer >= 0 ? uiLayer : 5;
    }

    private void CaptureScreenSpaceOverlayCanvases()
    {
        if (capturedScreenSpaceOverlayCanvases == null)
        {
            return;
        }

        for (int i = 0; i < capturedScreenSpaceOverlayCanvases.Length; i++)
        {
            Canvas canvas = capturedScreenSpaceOverlayCanvases[i];
            if (canvas == null || canvas.transform.IsChildOf(transform))
            {
                continue;
            }

            if (canvas.renderMode != RenderMode.ScreenSpaceOverlay)
            {
                continue;
            }

            capturedCanvasStates.Add(new CanvasState(canvas));

            canvas.renderMode = RenderMode.ScreenSpaceCamera;
            canvas.worldCamera = sourceCamera;
            canvas.planeDistance = capturedCanvasPlaneDistance;
        }
    }

    private void RestoreCapturedCanvases()
    {
        for (int i = 0; i < capturedCanvasStates.Count; i++)
        {
            capturedCanvasStates[i].Restore();
        }

        capturedCanvasStates.Clear();
    }

    private void RebuildRenderTexture()
    {
        textureWidth = Mathf.Max(1, Screen.width);
        textureHeight = Mathf.Max(1, Screen.height);

        if (displayTexture != null)
        {
            displayTexture.Release();
            Destroy(displayTexture);
            displayTexture = null;
        }

        displayTexture = new RenderTexture(textureWidth, textureHeight, 24, RenderTextureFormat.ARGB32)
        {
            name = "CRT_Display_Capture_RT",
            filterMode = FilterMode.Bilinear,
            wrapMode = TextureWrapMode.Clamp,
            useMipMap = false,
            autoGenerateMips = false
        };
        displayTexture.Create();

        CaptureSourceCameraState();
        sourceCamera.targetTexture = displayTexture;

        if (displayImage != null)
        {
            displayImage.texture = displayTexture;
        }
    }

    private void CleanupDisplayPath()
    {
        if (sourceCamera != null && sourceCamera.targetTexture == displayTexture)
        {
            sourceCamera.targetTexture = hasSourceCameraState ? originalSourceCameraTargetTexture : null;
        }

        hasSourceCameraState = false;
        originalSourceCameraTargetTexture = null;

        RestoreCapturedCanvases();

        if (displayCanvasObject != null)
        {
            Destroy(displayCanvasObject);
            displayCanvasObject = null;
            displayImage = null;
        }

        if (displayTexture != null)
        {
            displayTexture.Release();
            Destroy(displayTexture);
            displayTexture = null;
        }

        if (displayMaterial != null)
        {
            Destroy(displayMaterial);
            displayMaterial = null;
        }

        if (displayCamera != null)
        {
            Destroy(displayCamera.gameObject);
            displayCamera = null;
        }

        textureWidth = 0;
        textureHeight = 0;
    }

    private void CaptureSourceCameraState()
    {
        if (hasSourceCameraState || sourceCamera == null)
        {
            return;
        }

        originalSourceCameraTargetTexture = sourceCamera.targetTexture;
        hasSourceCameraState = true;
    }

    private void ApplyMaterialSettings()
    {
        if (displayMaterial == null)
        {
            return;
        }

        displayMaterial.SetColor("_DisplayTint", displayTint);
        displayMaterial.SetFloat("_Monochrome", monochrome);
        displayMaterial.SetFloat("_ScanlineStrength", scanlineStrength);
        displayMaterial.SetFloat("_MaskStrength", maskStrength);
        displayMaterial.SetFloat("_NoiseStrength", noiseStrength);
        displayMaterial.SetFloat("_Distortion", distortion);
        displayMaterial.SetFloat("_ChromaticOffset", chromaticOffset);
        displayMaterial.SetFloat("_VignetteStrength", vignetteStrength);
        displayMaterial.SetFloat("_GlowStrength", glowStrength);
    }

    private readonly struct CanvasState
    {
        private readonly Canvas canvas;
        private readonly RenderMode renderMode;
        private readonly Camera worldCamera;
        private readonly float planeDistance;

        public CanvasState(Canvas canvas)
        {
            this.canvas = canvas;
            renderMode = canvas.renderMode;
            worldCamera = canvas.worldCamera;
            planeDistance = canvas.planeDistance;
        }

        public void Restore()
        {
            if (canvas == null)
            {
                return;
            }

            canvas.renderMode = renderMode;
            canvas.worldCamera = worldCamera;
            canvas.planeDistance = planeDistance;
        }
    }
}
