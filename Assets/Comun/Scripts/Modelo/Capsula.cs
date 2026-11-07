using System;

public enum EstadoPlanta { Vacia, Creciendo, Estresada, Madura, Podrida }

/// <summary>
/// Una de las 3 capsulas de cultivo del invernadero. Guarda su configuracion entre noches
/// (las estaciones tienen memoria): si no vuelves a tocarla, sigue igual la noche siguiente.
/// </summary>
[Serializable]
public class Capsula
{
    public string Nombre;
    public bool Regolito;
    public bool Perclorato;
    public Especie Especie;
    public EstadoPlanta Estado;
    public int Humedad = 50;
    public bool LedRojo, LedAzul, LedUv;
    public int Progreso;
    public int ErroresSeguidos;
    public int NochesOscuras;
    /// <summary>Comida sembrada en suelo con percloratos: la cosecha se descarta.</summary>
    public bool Toxica;
    /// <summary>Murio porque el agua se congelo (solo cambia el dibujo).</summary>
    public bool Congelada;
    /// <summary>Ya sumo autosuficiencia al limpiarse de percloratos.</summary>
    public bool Descontaminada;

    public Capsula(string nombre, bool regolito)
    {
        Nombre = nombre;
        Regolito = regolito;
        Perclorato = regolito;
        Estado = EstadoPlanta.Vacia;
    }

    public bool Vacia => Especie == Especie.Ninguna;
    public bool Viva => !Vacia && Estado != EstadoPlanta.Podrida;
    public int CanalesLed => (LedRojo ? 1 : 0) + (LedAzul ? 1 : 0) + (LedUv ? 1 : 0);
    public InfoSemilla Info => Semillas.Info[Especie];

    public void Sembrar(Especie e)
    {
        Especie = e;
        Estado = EstadoPlanta.Creciendo;
        Progreso = 0;
        ErroresSeguidos = 0;
        NochesOscuras = 0;
        Congelada = false;
        Toxica = Perclorato && Semillas.Info[e].Comestible;
    }

    public void Vaciar()
    {
        Especie = Especie.Ninguna;
        Estado = EstadoPlanta.Vacia;
        Progreso = 0;
        ErroresSeguidos = 0;
        NochesOscuras = 0;
        Toxica = false;
        Congelada = false;
    }

    public bool HumedadCorrecta => !Vacia && Math.Abs(Humedad - Info.Humedad) <= Semillas.Tolerancia;

    /// <summary>
    /// True si la capsula esta bien configurada para su especie. Con ledsForzadosApagados (apagon)
    /// se evalua como si los LEDs estuvieran apagados. Tambien cuenta la noche a oscuras de la papa.
    /// </summary>
    public bool ConfiguracionCorrecta(bool ledsForzadosApagados, out bool usoNocheOscura)
    {
        usoNocheOscura = false;
        if (Vacia) return false;
        var s = Info;
        bool r = LedRojo && !ledsForzadosApagados, a = LedAzul && !ledsForzadosApagados, u = LedUv && !ledsForzadosApagados;
        if (!HumedadCorrecta) return false;
        if (r == s.Rojo && a == s.Azul && u == s.Uv) return true;
        if (s.ToleraUnaNocheOscura && !r && !a && !u && NochesOscuras == 0)
        {
            usoNocheOscura = true;
            return true;
        }
        return false;
    }
}
