using UnityEngine;

/// <summary>
/// Reglas del osciloscopio de la Antena Banda X (sin nada visual: se prueba sola).
/// El orbitador cruza el cielo y su frecuencia se corre de derecha a izquierda por el efecto Doppler.
/// Hay que seguirla con la frecuencia y la amplitud hasta llenar la barra de bloqueo antes de 40 s.
/// Frecuencia y amplitud van de 0 a 1.
/// </summary>
public class AntenaLogica
{
    public const float Duracion = 40f;
    public const float ToleranciaFrecuencia = 0.05f, ToleranciaAmplitud = 0.10f;
    public const float VelocidadFrecuencia = 0.35f, VelocidadAmplitud = 0.5f;
    public const float SegundosParaBloquear = 6f;

    public float Frecuencia = 0.5f;
    public float Amplitud = 0.5f;
    public float Bloqueo;
    public float Tiempo;
    public bool Terminado;
    public bool Exito;
    public readonly float AmplitudObjetivo;
    readonly float ritmoDoppler;

    public AntenaLogica(bool elenaVerde, int semilla)
    {
        var azar = new System.Random(semilla);
        AmplitudObjetivo = 0.3f + (float)azar.NextDouble() * 0.45f;
        // Elena en verde calcula las efemerides: la onda se mueve 30 % mas lento.
        ritmoDoppler = elenaVerde ? 0.7f : 1f;
    }

    /// <summary>Frecuencia del satelite ahora: empieza a la derecha (0.8) y termina a la izquierda (0.2).</summary>
    public float FrecuenciaObjetivo => 0.5f + 0.3f * Mathf.Cos(Mathf.PI * Mathf.Clamp01(Tiempo * ritmoDoppler / Duracion));

    public float ErrorFrecuencia => Mathf.Abs(Frecuencia - FrecuenciaObjetivo);
    public bool Alineado => ErrorFrecuencia < ToleranciaFrecuencia && Mathf.Abs(Amplitud - AmplitudObjetivo) < ToleranciaAmplitud;
    /// <summary>Muy lejos de la senal: se oye el vacio.</summary>
    public bool EnElVacio => ErrorFrecuencia > 0.2f;

    /// <summary>dirFrecuencia y dirAmplitud: -1, 0 o 1.</summary>
    public void Avanzar(float dt, int dirFrecuencia, int dirAmplitud)
    {
        if (Terminado) return;
        Tiempo += dt;
        Frecuencia = Mathf.Clamp01(Frecuencia + dirFrecuencia * VelocidadFrecuencia * dt);
        Amplitud = Mathf.Clamp01(Amplitud + dirAmplitud * VelocidadAmplitud * dt);
        Bloqueo = Alineado ? Mathf.Min(1f, Bloqueo + dt / SegundosParaBloquear) : Mathf.Max(0f, Bloqueo - dt / (SegundosParaBloquear * 2f));
        if (Bloqueo >= 1f) { Exito = true; Terminado = true; }
        else if (Tiempo >= Duracion) Terminado = true;
    }
}
