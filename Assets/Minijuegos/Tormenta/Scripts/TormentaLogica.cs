using UnityEngine;

/// <summary>
/// Reglas de "Tormenta de polvo" (inspirado en Opportunity). Sin nada visual: se puede probar sola.
/// - 6 paneles se ensucian solos y con rafagas de viento.
/// - La luz del sol baja durante 75 s porque se acerca la tormenta.
/// - La energia sube segun lo limpios que esten los paneles y la luz que queda.
/// Gana al llenar la energia. A medias si llega a la mitad.
/// </summary>
public class TormentaLogica
{
    public const int CantidadPaneles = 6;
    public const float Duracion = 75f;
    public const float LuzFinal = 0.15f;
    public const float LimpiezaPorClic = 0.2f;

    public readonly float[] Polvo = new float[CantidadPaneles];
    public float Luz = 1f;
    public float Energia;
    public float Tiempo;
    public bool Terminado;
    public int Rafagas;

    readonly float acumulacion;
    readonly float ganancia = 0.034f;
    readonly System.Random azar;
    float proximaRafaga = 6f;

    public TormentaLogica(int dificultad, int semilla)
    {
        azar = new System.Random(semilla);
        acumulacion = 0.035f + 0.01f * Mathf.Clamp(dificultad, 0, 3);
        for (int i = 0; i < CantidadPaneles; i++) Polvo[i] = 0.2f + (float)azar.NextDouble() * 0.3f;
    }

    public void Limpiar(int panel)
    {
        if (Terminado || panel < 0 || panel >= CantidadPaneles) return;
        Polvo[panel] = Mathf.Max(0f, Polvo[panel] - LimpiezaPorClic);
    }

    /// <summary>Devuelve true si en este paso hubo una rafaga (para mostrar un aviso).</summary>
    public bool Avanzar(float dt)
    {
        if (Terminado) return false;
        Tiempo += dt;
        Luz = Mathf.Lerp(1f, LuzFinal, Tiempo / Duracion);
        for (int i = 0; i < CantidadPaneles; i++) Polvo[i] = Mathf.Min(1f, Polvo[i] + acumulacion * dt);

        bool rafaga = false;
        if (Tiempo >= proximaRafaga)
        {
            rafaga = true;
            Rafagas++;
            for (int k = 0; k < 3; k++)
            {
                int p = azar.Next(CantidadPaneles);
                Polvo[p] = Mathf.Min(1f, Polvo[p] + 0.35f);
            }
            proximaRafaga += 7f + (float)azar.NextDouble() * 3f;
        }

        float salida = 0f;
        for (int i = 0; i < CantidadPaneles; i++) salida += 1f - Polvo[i];
        salida /= CantidadPaneles;
        Energia = Mathf.Min(1f, Energia + salida * Luz * ganancia * dt);

        if (Energia >= 1f || Tiempo >= Duracion) Terminado = true;
        return rafaga;
    }

    public int PanelMasSucio()
    {
        int mejor = 0;
        for (int i = 1; i < CantidadPaneles; i++) if (Polvo[i] > Polvo[mejor]) mejor = i;
        return mejor;
    }

    public Resultado ResultadoFinal => Energia >= 1f ? Resultado.Gano : Energia >= 0.5f ? Resultado.AMedias : Resultado.Perdio;
}
