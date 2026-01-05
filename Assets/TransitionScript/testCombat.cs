using UnityEngine;

public class LoadCombatOnKey : MonoBehaviour
{
    [SerializeField] private KeyCode key = KeyCode.C;
    [SerializeField] private string combatSceneName = "Combat scene";

    private void Update()
    {
        if (Input.GetKeyDown(key))
        {
            if (LevelLoader.Instance != null)
                LevelLoader.Instance.LoadScene(combatSceneName);
        }
    }
}
