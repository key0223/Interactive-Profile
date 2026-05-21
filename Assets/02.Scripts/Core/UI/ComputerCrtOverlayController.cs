using UnityEngine;
using UnityEngine.UI;

public sealed class ComputerCrtOverlayController : MonoBehaviour
{
    private static readonly int ScanlineIntensityId = Shader.PropertyToID("_ScanlineIntensity");
    private static readonly int ScanlineDensityId = Shader.PropertyToID("_ScanlineDensity");
    private static readonly int NoiseIntensityId = Shader.PropertyToID("_NoiseIntensity");
    private static readonly int FlickerIntensityId = Shader.PropertyToID("_FlickerIntensity");
    private static readonly int VignetteIntensityId = Shader.PropertyToID("_VignetteIntensity");
    private static readonly int TintColorId = Shader.PropertyToID("_TintColor");
    private static readonly int TimeScaleId = Shader.PropertyToID("_TimeScale");
    private static readonly int OverlayTimeId = Shader.PropertyToID("_OverlayTime");

    [SerializeField] private GameObject _overlayRoot;
    [SerializeField] private Graphic _overlayGraphic;
    [SerializeField] private Material _overlayMaterialTemplate;
    [SerializeField, Range(0f, 1f)] private float _scanlineIntensity = 0.28f;
    [SerializeField] private float _scanlineDensity = 220f;
    [SerializeField, Range(0f, 1f)] private float _noiseIntensity = 0.06f;
    [SerializeField, Range(0f, 1f)] private float _flickerIntensity = 0.035f;
    [SerializeField, Range(0f, 1f)] private float _vignetteIntensity = 0.42f;
    [SerializeField] private Color _tintColor = new Color(0.46f, 1f, 0.78f, 0.18f);
    [SerializeField] private float _timeScale = 1f;

    private Material _runtimeMaterial;

    private void Awake()
    {
        EnsureRuntimeMaterial();
        ApplyStaticProperties();
    }

    private void OnEnable()
    {
        EnsureRuntimeMaterial();
        ApplyStaticProperties();
    }

    private void Update()
    {
        if (_runtimeMaterial == null)
            return;

        _runtimeMaterial.SetFloat(OverlayTimeId, Time.unscaledTime);
        _runtimeMaterial.SetFloat(FlickerIntensityId, _flickerIntensity);
    }

    private void OnDestroy()
    {
        if (_overlayGraphic != null && _overlayGraphic.material == _runtimeMaterial)
            _overlayGraphic.material = null;

        if (_runtimeMaterial != null)
        {
            Destroy(_runtimeMaterial);
            _runtimeMaterial = null;
        }
    }

    public void SetVisible(bool visible)
    {
        if (_overlayRoot != null)
            _overlayRoot.SetActive(visible);
        else
            gameObject.SetActive(visible);

        if (visible)
        {
            EnsureRuntimeMaterial();
            ApplyStaticProperties();
        }
    }

    private void EnsureRuntimeMaterial()
    {
        if (_runtimeMaterial != null || _overlayGraphic == null)
            return;

        Material sourceMaterial = _overlayMaterialTemplate != null ? _overlayMaterialTemplate : _overlayGraphic.material;
        if (sourceMaterial == null)
            return;

        _runtimeMaterial = new Material(sourceMaterial)
        {
            name = $"{sourceMaterial.name} Runtime"
        };

        _overlayGraphic.material = _runtimeMaterial;
    }

    private void ApplyStaticProperties()
    {
        if (_runtimeMaterial == null)
            return;

        _runtimeMaterial.SetFloat(ScanlineIntensityId, _scanlineIntensity);
        _runtimeMaterial.SetFloat(ScanlineDensityId, _scanlineDensity);
        _runtimeMaterial.SetFloat(NoiseIntensityId, _noiseIntensity);
        _runtimeMaterial.SetFloat(FlickerIntensityId, _flickerIntensity);
        _runtimeMaterial.SetFloat(VignetteIntensityId, _vignetteIntensity);
        _runtimeMaterial.SetColor(TintColorId, _tintColor);
        _runtimeMaterial.SetFloat(TimeScaleId, _timeScale);
    }
}
