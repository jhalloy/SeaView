using System;
using System.Collections;
using DG.Tweening;
using TMPro;
using UnityEngine;

public class UIManager : MonoBehaviour
{
    [Header("UI Elements")]
    [SerializeField] private TextMeshProUGUI loadingText;

    [Header("Loading")]
    [SerializeField] private string loadingMessage = "Loading data";
    [SerializeField] private float loadingInterval = .3f;
    [SerializeField] private float loadingFadeTime = .3f;
    private bool isLoading = false;
    private Tween loadingFadeTween;
    private Coroutine loadingCoroutine;

    private VisusClient visusClient => VisusClient.Instance;

    #region Mono

    void Start()
    {
        loadingText.alpha = 0;

        // Bind to client load event for loading text.
        visusClient.onLoadingStateChanged += OnLoadingStateChanged;
    }

    #endregion

    #region Loading

    private void OnLoadingStateChanged(bool newState)
    {
        if (isLoading == newState) return;

        Debug.Log($"Loading = {newState}");

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
}
