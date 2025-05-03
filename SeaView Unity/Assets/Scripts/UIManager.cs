using System;
using System.Collections;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    [Header("UI Elements")]
    [SerializeField] private TextMeshProUGUI loadingText;
    [SerializeField] private TextMeshProUGUI qualityText;
    [SerializeField] private Slider qualitySlider;
    [SerializeField] private TextMeshProUGUI timeText;
    [SerializeField] private Slider timeSlider;
    [SerializeField] private TextMeshProUGUI xText;
    [SerializeField] private Slider xMinSlider;
    [SerializeField] private Slider xMaxSlider;
    [SerializeField] private TextMeshProUGUI yText;
    [SerializeField] private Slider yMinSlider;
    [SerializeField] private Slider yMaxSlider;
    [SerializeField] private TextMeshProUGUI zText;
    [SerializeField] private Slider zMinSlider;
    [SerializeField] private Slider zMaxSlider;
    [SerializeField] private Button visualizeButton;
    [SerializeField] private GameObject legendRootObject;
    [SerializeField] private RawImage legendImage;
    [SerializeField] private TextMeshProUGUI legendMinText;
    [SerializeField] private TextMeshProUGUI legendMaxText;

    [Header("Loading")]
    [SerializeField] private string loadingMessage = "Loading data";
    [SerializeField] private float loadingInterval = .3f;
    [SerializeField] private float loadingFadeTime = .3f;
    private bool isLoading = false;
    private Tween loadingFadeTween;
    private Coroutine loadingCoroutine;

    [Header("Legend")]
    [SerializeField] private int textureSize = 512;
    private Gradient gradient => visualizer.velocityGradient;
    private float maxSpeed => visualizer.maxSpeed;

    private Visualizer visualizer => Visualizer.Instance;

    #region Mono

    void Start()
    {
        legendRootObject.SetActive(false);

        loadingText.alpha = 0;

        // Bind to client load event for loading text.
        visualizer.onLoadingStateChanged += OnLoadingStateChanged;

        visualizer.onUpdatedVisuals += UpdateLegend;

        // Quality stuff
        UpdateQuality();
        qualitySlider.onValueChanged.AddListener(delegate {UpdateQuality();});

        // Generation command
        visualizeButton.onClick.AddListener(delegate {OnVisualize();});

        // Time
        UpdateTime();
        timeSlider.onValueChanged.AddListener(delegate {UpdateTime();});

        // Ranges
        UpdateX();
        xMinSlider.onValueChanged.AddListener(delegate {UpdateX();});
        xMaxSlider.onValueChanged.AddListener(delegate {UpdateX();});
        UpdateY();
        yMinSlider.onValueChanged.AddListener(delegate {UpdateY();});
        yMaxSlider.onValueChanged.AddListener(delegate {UpdateY();});
        UpdateZ();
        zMinSlider.onValueChanged.AddListener(delegate {UpdateZ();});
        zMaxSlider.onValueChanged.AddListener(delegate {UpdateZ();});
    }

    #endregion

    #region Loading

    private void OnLoadingStateChanged(bool newState)
    {
        if (isLoading == newState) return;

        // Debug.Log($"Loading = {newState}");

        isLoading = newState;

        loadingFadeTween?.Kill();

        loadingFadeTween = loadingText.DOFade(isLoading ? 1 : 0, loadingFadeTime);

        // Update the text
        if (isLoading)
        {
            loadingCoroutine = StartCoroutine(LoadingText_Coroutine());
        }
        else if (loadingCoroutine != null) StopCoroutine(loadingCoroutine);

        return;
    }

    private IEnumerator LoadingText_Coroutine()
    {
        string[] ellipses = {".", "..", "..."};
        int i = 0;

        while (true)
        {
            loadingText.text = $"{loadingMessage}{ellipses[i]}";
            i = (i + 1) % ellipses.Length;
            yield return new WaitForSeconds(loadingInterval);
        }
    }

    #endregion

    #region Quality

    private void UpdateQuality()
    {
        int newQuality = (int)qualitySlider.value;
        qualityText.text = $"Quality: {newQuality}";

        // Update the visualizer's quality argument.
        visualizer.quality = newQuality;
    }

    #endregion

    #region Visualization

    private void OnVisualize()
    {
        visualizer.Visualize();
    }

    #endregion

    #region Time

    private void UpdateTime()
    {
        int newTime = (int)timeSlider.value;
        timeText.text = $"Time: {newTime}";

        visualizer.time = newTime;
    }

    #endregion

    #region Ranges

    private void UpdateX()
    {
        ResolveRanges(xMinSlider, xMaxSlider);

        int min = (int)xMinSlider.value;
        int max = (int)xMaxSlider.value;

        // Update the visualizer
        visualizer.xRange = new int[] { min, max };

        xText.text = $"Lat Range: {min}, {max}";
    }

    private void UpdateY()
    {
        ResolveRanges(yMinSlider, yMaxSlider);

        int min = (int)yMinSlider.value;
        int max = (int)yMaxSlider.value;

        // Update the visualizer
        visualizer.yRange = new int[] { min, max };

        yText.text = $"Lon Range: {min}, {max}";
    }

    private void UpdateZ()
    {
        ResolveRanges(zMinSlider, zMaxSlider);

        int min = (int)zMinSlider.value;
        int max = (int)zMaxSlider.value;

        // Update the visualizer
        visualizer.z = new int[] { min, max };

        zText.text = $"Depth Range: {min}, {max}";
    }

    private void ResolveRanges(Slider minSlider, Slider maxSlider)
    {
        int min = (int)minSlider.value;
        int max = (int)maxSlider.value;

        if (min >= max)
        {
            max = min + 1;
        }

        minSlider.maxValue = max - 1;
        maxSlider.minValue = min + 1;

        maxSlider.value = max;
    }

    #endregion

    #region Legend

    private void UpdateLegend()
    {
        legendRootObject.SetActive(true);

        // Show/update the legend
        legendMinText.text = "0";
        legendMaxText.text = $"{maxSpeed:F2}";

        Texture2D tex = new Texture2D(1, textureSize, TextureFormat.RGBA32, false);
        tex.wrapMode = TextureWrapMode.Clamp;

        for (int y = 0; y < textureSize; y++)
        {
            Color color = gradient.Evaluate((float)y / (textureSize - 1));
            tex.SetPixel(0, y, color);
        }

        tex.Apply();
        legendImage.texture = tex;
    }

    #endregion
}
