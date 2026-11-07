using UnityEngine;

/// <summary>
/// Electrocardiograma dibujado con barras finas que se desplazan. Pita en cada latido.
/// Las barras las crea el constructor de escenas; aqui solo se cambia su alto.
/// </summary>
public class MonitorCardiaco : MonoBehaviour
{
    public RectTransform[] barras;
    public float bpm = 75f;
    public bool sonido = true;
    public float altoMaximo = 44f;

    float fase;
    float[] muestras;
    int cabeza;
    float acumulado;

    void Update()
    {
        if (barras == null || barras.Length == 0) return;
        if (muestras == null || muestras.Length != barras.Length) muestras = new float[barras.Length];

        float latidosPorSegundo = Mathf.Max(0.5f, bpm / 60f);
        acumulado += Time.deltaTime * 30f; // 30 muestras por segundo
        while (acumulado >= 1f)
        {
            acumulado -= 1f;
            float antes = fase;
            fase = (fase + latidosPorSegundo / 30f) % 1f;
            if (fase < antes && sonido) AmbienteAudio.Efecto(SonidoProcedural.Pitido(), 0.35f);
            muestras[cabeza] = Onda(fase);
            cabeza = (cabeza + 1) % muestras.Length;
        }
        for (int i = 0; i < barras.Length; i++)
        {
            float v = muestras[(cabeza + i) % muestras.Length];
            barras[i].sizeDelta = new Vector2(barras[i].sizeDelta.x, 2f + Mathf.Abs(v) * altoMaximo);
            barras[i].anchoredPosition = new Vector2(barras[i].anchoredPosition.x, v * altoMaximo * 0.5f);
        }
    }

    /// <summary>Forma aproximada de un latido (P, QRS, T).</summary>
    static float Onda(float f)
    {
        if (f < 0.05f) return 0.12f * Mathf.Sin(f / 0.05f * Mathf.PI);
        if (f < 0.10f) return 0f;
        if (f < 0.12f) return -0.2f;
        if (f < 0.15f) return 1f;
        if (f < 0.18f) return -0.35f;
        if (f < 0.30f) return 0f;
        if (f < 0.40f) return 0.25f * Mathf.Sin((f - 0.30f) / 0.10f * Mathf.PI);
        return 0f;
    }
}
