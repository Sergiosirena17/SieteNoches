using UnityEngine;

/// <summary>
/// Reglas de "Respira, Marte" (inspirado en MOXIE). Sin nada visual: se puede probar sola.
/// La maquina solo produce oxigeno cuando la temperatura Y la presion estan en su zona segura.
/// - Temperatura: sube mientras calientas y baja sola. Si llega al maximo 1.5 s, la maquina se quema.
/// - Presion: se escapa sola; cada bombeo la sube.
/// Gana al llenar el oxigeno antes de 60 s. A medias si llega a la mitad.
/// </summary>
public class RespiraLogica
{
    public const float Duracion = 60f;
    public const float SegundosParaLlenar = 40f;

    public float Temperatura = 0.40f;
    public float Presion = 0.60f;
    public float Oxigeno;
    public float Tiempo;
    public bool Terminado;
    public bool Quemada;

    public readonly float ZonaTMin, ZonaTMax, ZonaPMin, ZonaPMax;
    readonly float fugaPresion;
    readonly System.Random azar;
    float tiempoAlMaximo;

    public RespiraLogica(int dificultad, int semilla)
    {
        float estrechar = 0.03f * Mathf.Clamp(dificultad, 0, 3);
        ZonaTMin = 0.55f + estrechar; ZonaTMax = 0.80f - estrechar;
        ZonaPMin = 0.45f + estrechar; ZonaPMax = 0.75f - estrechar;
        fugaPresion = 0.07f + 0.015f * Mathf.Clamp(dificultad, 0, 3);
        azar = new System.Random(semilla);
    }

    public bool TemperaturaBien => Temperatura >= ZonaTMin && Temperatura <= ZonaTMax;
    public bool PresionBien => Presion >= ZonaPMin && Presion <= ZonaPMax;
    public bool Produciendo => TemperaturaBien && PresionBien && !Terminado;

    public void Bombear()
    {
        if (Terminado) return;
        Presion = Mathf.Min(1f, Presion + 0.12f);
    }

    public void Avanzar(float dt, bool calentando)
    {
        if (Terminado) return;
        Tiempo += dt;
        float ruido = ((float)azar.NextDouble() - 0.5f) * 0.12f;
        Temperatura = Mathf.Clamp01(Temperatura + ((calentando ? 0.30f : -0.12f) + ruido) * dt);
        Presion = Mathf.Clamp01(Presion - fugaPresion * dt);

        if (TemperaturaBien && PresionBien) Oxigeno = Mathf.Min(1f, Oxigeno + dt / SegundosParaLlenar);

        tiempoAlMaximo = Temperatura >= 0.97f ? tiempoAlMaximo + dt : 0f;
        if (tiempoAlMaximo >= 1.5f) { Quemada = true; Terminado = true; }
        if (Oxigeno >= 1f || Tiempo >= Duracion) Terminado = true;
    }

    public Resultado ResultadoFinal
    {
        get
        {
            if (Quemada) return Resultado.Perdio;
            if (Oxigeno >= 1f) return Resultado.Gano;
            return Oxigeno >= 0.5f ? Resultado.AMedias : Resultado.Perdio;
        }
    }
}
