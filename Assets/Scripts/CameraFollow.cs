using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    // Cible à suivre (votre objet Player)
    public Transform target;

    [Header("Paramètres de Suivi")]
    // Vitesse à laquelle la caméra se déplace vers la cible (pour un suivi doux)
    public float smoothSpeed = 5f; 
    // Décalage (offset) de la caméra par rapport à la cible
    private Vector3 offset;

    void Start()
    {
        // Calculer automatiquement l'offset au démarrage
        if (target != null)
        {
            offset = transform.position - target.position;
        }
        else
        {
            Debug.LogWarning("Target is not assigned to CameraFollow script.");
        }
    }

    void LateUpdate()
    {
        // On s'assure que la cible est bien définie
        if (target == null) return;

        // Position désirée : la position de la cible plus le décalage (offset)
        Vector3 desiredPosition = target.position + offset;

        // Interpolation linéaire (Lerp) pour un mouvement doux
        // Time.deltaTime permet un mouvement indépendant du framerate
        Vector3 smoothedPosition = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed * Time.deltaTime);

        // Appliquer la position lissée
        transform.position = smoothedPosition;

        // Optionnel : Faire en sorte que la caméra regarde toujours la cible
        // transform.LookAt(target); 
    }
}