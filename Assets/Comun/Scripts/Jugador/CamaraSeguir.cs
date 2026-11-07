using UnityEngine;

/// <summary>La camara sigue al jugador y se ajusta a la rejilla de pixeles (32 px por unidad) para que no tiemble.</summary>
public class CamaraSeguir : MonoBehaviour
{
    public Transform objetivo;
    public float suavizado = 10f;
    public float pixelesPorUnidad = 32f;
    [Tooltip("Limites del mapa en unidades (x min, y min, x max, y max). Cero = sin limites.")]
    public Vector4 limites;

    Vector3 posicionSuave;

    void Start()
    {
        if (objetivo != null) posicionSuave = new Vector3(objetivo.position.x, objetivo.position.y, transform.position.z);
    }

    void LateUpdate()
    {
        if (objetivo == null) return;
        var meta = new Vector3(objetivo.position.x, objetivo.position.y + 0.75f, transform.position.z);
        posicionSuave = Vector3.Lerp(posicionSuave, meta, 1f - Mathf.Exp(-suavizado * Time.deltaTime));

        var cam = GetComponent<Camera>();
        var p = posicionSuave;
        if (cam != null && limites != Vector4.zero)
        {
            float alto = cam.orthographicSize, ancho = alto * cam.aspect;
            p.x = Mathf.Clamp(p.x, limites.x + ancho, Mathf.Max(limites.x + ancho, limites.z - ancho));
            p.y = Mathf.Clamp(p.y, limites.y + alto, Mathf.Max(limites.y + alto, limites.w - alto));
        }
        p.x = Mathf.Round(p.x * pixelesPorUnidad) / pixelesPorUnidad;
        p.y = Mathf.Round(p.y * pixelesPorUnidad) / pixelesPorUnidad;
        transform.position = p;
    }
}
