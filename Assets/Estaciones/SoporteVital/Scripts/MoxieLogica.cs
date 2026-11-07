using UnityEngine;

/// <summary>
/// Reglas del reactor MOXIE (sin nada visual: se prueba sola).
/// - Mantener "Calentar" sube la temperatura; si sueltas, el frio de la noche la baja.
/// - Solo produce O2 entre 780 y 830 °C. Sobre 870 °C durante 1.5 s se funden las celdas.
/// - Mientras produce, el hollin (carbono solido) tapa los filtros. "Purgar" lo limpia pero enfria 200 °C.
/// - Termina al llenar la produccion, al apagarlo o a los 45 s.
/// </summary>
public class MoxieLogica
{
    public const float TempMin = -60f, TempMax = 900f, ZonaMin = 780f, ZonaMax = 830f, Fusion = 870f;
    public const float Duracion = 45f, SegundosParaLlenar = 20f, CalorPorSegundo = 90f;

    public float Temperatura = TempMin;
    public float Produccion;
    public float Hollin;
    public float Tiempo;
    public bool Terminado;
    public bool Fundido;

    readonly float calor, frio, ritmoHollin, factorToma;
    readonly System.Random azar;
    float tiempoFusion;

    public MoxieLogica(int noche, bool codigoReactor, bool marcoVerde, bool tomaCongelada, float hollinInicial, int semilla)
    {
        calor = CalorPorSegundo * (codigoReactor ? 1.3f : 1f);
        frio = (noche == 3 || noche >= 7) ? 45f : 35f;
        ritmoHollin = 0.04f * (marcoVerde ? 0.5f : 1f);
        factorToma = tomaCongelada ? 0.5f : 1f;
        Hollin = hollinInicial;
        azar = new System.Random(semilla);
    }

    public bool EnZona => Temperatura >= ZonaMin && Temperatura <= ZonaMax;
    public bool Tapado => Hollin >= 1f;
    public bool Produciendo => EnZona && !Tapado && !Terminado;

    /// <summary>Temperatura de 0 a 1 para dibujar la barra.</summary>
    public static float Normalizar(float grados) => Mathf.InverseLerp(TempMin, TempMax, grados);

    public void Avanzar(float dt, bool calentando)
    {
        if (Terminado) return;
        Tiempo += dt;
        float ruido = ((float)azar.NextDouble() - 0.5f) * 20f;
        Temperatura = Mathf.Clamp(Temperatura + ((calentando ? calor : -frio) + ruido) * dt, TempMin, TempMax);

        if (Produciendo)
        {
            Produccion = Mathf.Min(1f, Produccion + dt / SegundosParaLlenar * factorToma);
            Hollin = Mathf.Min(1f, Hollin + ritmoHollin * dt);
        }

        tiempoFusion = Temperatura >= Fusion ? tiempoFusion + dt : 0f;
        if (tiempoFusion >= 1.5f) { Fundido = true; Terminado = true; }
        if (Produccion >= 1f || Tiempo >= Duracion) Terminado = true;
    }

    public void Purgar()
    {
        if (Terminado) return;
        Hollin = 0f;
        Temperatura = Mathf.Max(TempMin, Temperatura - 200f);
    }

    public void Apagar() { Terminado = true; }
}
