using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelLoader : MonoBehaviour
{
    public static LevelLoader Instance { get; private set; }

    [SerializeField] private Animator transitionAnimator;
    // [SerializeField] private float fadeOutDuration = 1f;

    private static readonly int StartTrig = Animator.StringToHash("Start");
    private static readonly int EndTrig   = Animator.StringToHash("End");

    private bool isLoading;

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDestroy()
    {
        if (Instance == this)
            SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void Start()
    {
        if (transitionAnimator != null)
        {
            transitionAnimator.ResetTrigger(StartTrig);
            transitionAnimator.SetTrigger(EndTrig);
        }
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (!isLoading) return;

        transitionAnimator.ResetTrigger(StartTrig);
        transitionAnimator.SetTrigger(EndTrig);

        isLoading = false;
    }


    public void LoadScene(string sceneName)
    {
        if (isLoading || transitionAnimator == null) return;
        StartCoroutine(LoadRoutine(sceneName));
    }

    private IEnumerator LoadRoutine(string sceneName)
    {
        isLoading = true;

        transitionAnimator.ResetTrigger(EndTrig);
        transitionAnimator.SetTrigger(StartTrig);

        while (!transitionAnimator.GetCurrentAnimatorStateInfo(0).IsName("Black"))
            yield return null;

        transitionAnimator.ResetTrigger(EndTrig);

        SceneManager.LoadScene(sceneName);
    }

}
