using UnityEngine;
using UnityEngine.UI;

public class FullscreenBlurRenderer : MonoBehaviour
{
    private static readonly int BlurIntensityId = Shader.PropertyToID("_BlurIntensity");
    private static readonly int BlurTexelSizeId = Shader.PropertyToID("_BlurTexelSize");
    private static readonly int SampleCountId = Shader.PropertyToID("_SampleCount");
    private static readonly int TintColorId = Shader.PropertyToID("_TintColor");

    [SerializeField] private Camera _sourceCamera;
    [SerializeField] private RawImage _targetImage;
    [SerializeField] private Material _blurMaterialTemplate;
    [SerializeField, Range(0f, 1f)] private float _blurIntensity = 1f;
    [SerializeField, Range(1f, 8f)] private float _blurRadius = 3f;
    [SerializeField, Range(1, 9)] private int _sampleCount = 9;
    [SerializeField, Range(1, 4)] private int _downsample = 2;
    [SerializeField] private Vector2Int _textureSize = new Vector2Int(960, 540);
    [SerializeField] private bool _captureEveryFrame;
    [SerializeField] private Color _tintColor = new Color(0f, 0f, 0f, 0.2f);

    private Material _runtimeMaterial;
    private RenderTexture _captureTexture;
    private int _lastWidth;
    private int _lastHeight;
    private bool _referencesResolved;

    private void Awake()
    {
        ResolveReferences();
        EnsureRuntimeMaterial();
        EnsureCaptureTexture();
        ApplyMaterialProperties();
        Capture();
    }

    private void OnEnable()
    {
        ResolveReferences();
        EnsureRuntimeMaterial();
        EnsureCaptureTexture();
        ApplyMaterialProperties();
        Capture();
    }

    private void LateUpdate()
    {
        if (_captureEveryFrame)
            Capture();
    }

    private void OnDestroy()
    {
        if (_targetImage != null && _targetImage.material == _runtimeMaterial)
            _targetImage.material = null;

        if (_runtimeMaterial != null)
        {
            Destroy(_runtimeMaterial);
            _runtimeMaterial = null;
        }

        ReleaseCaptureTexture();
    }

    public void SetVisible(bool visible)
    {
        ResolveReferences();

        if (_targetImage != null)
        {
            _targetImage.gameObject.SetActive(visible);
            _targetImage.enabled = visible;
        }
    }

    public void SetBlurIntensity(float intensity)
    {
        SetIntensity(intensity);

        if (_blurIntensity > 0.001f)
            CaptureOnce();
    }

    public void SetIntensity(float intensity)
    {
        ResolveReferences();
        EnsureRuntimeMaterial();
        EnsureCaptureTexture();

        _blurIntensity = Mathf.Clamp01(intensity);
        ApplyMaterialProperties();
    }

    public void Prepare()
    {
        ResolveReferences();
        EnsureRuntimeMaterial();
        EnsureCaptureTexture();

        if (_targetImage != null)
        {
            _targetImage.texture = _captureTexture;
            _targetImage.material = _runtimeMaterial;
        }

        ApplyMaterialProperties();
    }

    public void CaptureOnce()
    {
        Prepare();

        if (_sourceCamera == null || _captureTexture == null || _targetImage == null)
            return;

        RenderTexture previousTarget = _sourceCamera.targetTexture;
        _sourceCamera.targetTexture = _captureTexture;
        _sourceCamera.Render();
        _sourceCamera.targetTexture = previousTarget;

        _targetImage.texture = _captureTexture;
    }

    public void Capture()
    {
        CaptureOnce();
    }

    private void ResolveReferences()
    {
        if (_referencesResolved)
            return;

        if (_targetImage == null)
            _targetImage = GetComponent<RawImage>();

        _referencesResolved = true;
    }

    private void EnsureRuntimeMaterial()
    {
        if (_runtimeMaterial != null || _targetImage == null)
            return;

        Material sourceMaterial = _blurMaterialTemplate != null ? _blurMaterialTemplate : _targetImage.material;
        if (sourceMaterial == null)
            return;

        _runtimeMaterial = new Material(sourceMaterial)
        {
            name = $"{sourceMaterial.name} Runtime"
        };

        _targetImage.material = _runtimeMaterial;
    }

    private void EnsureCaptureTexture()
    {
        int width = Mathf.Max(16, _textureSize.x / Mathf.Max(1, _downsample));
        int height = Mathf.Max(16, _textureSize.y / Mathf.Max(1, _downsample));

        if (_captureTexture != null && _lastWidth == width && _lastHeight == height)
            return;

        ReleaseCaptureTexture();

        _captureTexture = new RenderTexture(width, height, 16, RenderTextureFormat.ARGB32)
        {
            name = "Fullscreen_Blur_Capture_RT",
            filterMode = FilterMode.Bilinear,
            wrapMode = TextureWrapMode.Clamp,
            useMipMap = false,
            autoGenerateMips = false
        };

        _captureTexture.Create();
        _lastWidth = width;
        _lastHeight = height;

        if (_targetImage != null)
            _targetImage.texture = _captureTexture;
    }

    private void ReleaseCaptureTexture()
    {
        if (_captureTexture == null)
            return;

        if (_targetImage != null && _targetImage.texture == _captureTexture)
            _targetImage.texture = null;

        _captureTexture.Release();
        Destroy(_captureTexture);
        _captureTexture = null;
    }

    private void ApplyMaterialProperties()
    {
        if (_runtimeMaterial == null)
            return;

        float width = _captureTexture != null ? _captureTexture.width : Mathf.Max(1, _textureSize.x);
        float height = _captureTexture != null ? _captureTexture.height : Mathf.Max(1, _textureSize.y);
        float radius = Mathf.Max(0f, _blurRadius) * _blurIntensity;

        _runtimeMaterial.SetFloat(BlurIntensityId, _blurIntensity);
        _runtimeMaterial.SetVector(BlurTexelSizeId, new Vector4(radius / width, radius / height, 0f, 0f));
        _runtimeMaterial.SetFloat(SampleCountId, Mathf.Clamp(_sampleCount, 1, 9));
        _runtimeMaterial.SetColor(TintColorId, _tintColor);
    }
}
