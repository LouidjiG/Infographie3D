using UnityEngine;

public class Billboard : MonoBehaviour
{
    [SerializeField]
    private Camera _mainCamera;

    private void LateUpdate()
    {
        Vector3 cameraPosition = _mainCamera.transform.position;
        cameraPosition.y = transform.position.y;
        transform.LookAt(cameraPosition);
        transform.Rotate(0, 180, 0);
    }
}
