using UnityEngine;

public class BlueRoom : MonoBehaviour
{
    [SerializeField] private KeyCode key = KeyCode.B;
    [SerializeField] private string combatSceneName = "blue scene";

    private void Update()
    {
        if (Input.GetKeyDown(key))
        {
            if (LevelLoader.Instance != null)
                LevelLoader.Instance.LoadScene(combatSceneName);
        }
    }
}
