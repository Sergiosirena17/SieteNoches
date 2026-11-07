using System.Collections.Generic;

/// <summary>Nombres de escenas. Usa siempre estas constantes, nunca escribas el nombre a mano.</summary>
public static class Escenas
{
    public const string Inicio = "Inicio";
    public const string Intro = "Intro";
    public const string Mapa = "Mapa";
    public const string Lanzamiento = "Lanzamiento";
    public const string Final = "Final";
}

/// <summary>Identificadores de las misiones del nucleo.</summary>
public static class Misiones
{
    public const string Respira = "respira";
    public const string Tormenta = "tormenta";
    public const string Huerto = "huerto";
    public const string Refugio = "refugio";
    public const string Insulta = "insulta";
    public const string Roca = "roca";

    public static readonly Dictionary<string, string> Nombre = new Dictionary<string, string>
    {
        { Respira, "Respira, Marte" },
        { Tormenta, "Tormenta de polvo" },
        { Huerto, "Huerto marciano" },
        { Refugio, "Refugio" },
        { Insulta, "Insulta a Nikolas" },
        { Roca, "¿Roca o señal?" },
    };

    public static readonly Dictionary<string, string> Escena = new Dictionary<string, string>
    {
        { Respira, "Mj_Respira" },
        { Tormenta, "Mj_Tormenta" },
        { Huerto, "Mj_Huerto" },
        { Refugio, "Mj_Refugio" },
        { Insulta, "Mj_Insulta" },
        { Roca, "Mj_Roca" },
    };
}

public enum Resultado { Gano, AMedias, Perdio }

public enum DecisionRoca { Analizar, SubirSinAnalizar, Dejar }

/// <summary>Efecto de cada minijuego en los recursos. Copia exacta de la tabla del GDD.</summary>
public static class Efectos
{
    public static void Aplicar(string id, Resultado r)
    {
        switch (id)
        {
            case Misiones.Respira:
                if (r == Resultado.Gano) GameState.Sumar(oxigeno: 30, nave: 10);
                else if (r == Resultado.AMedias) GameState.Sumar(oxigeno: 15, nave: 5);
                else GameState.Sumar(nave: -5);
                break;
            case Misiones.Tormenta:
                if (r == Resultado.Gano) GameState.Sumar(energia: 30, nave: 10);
                else if (r == Resultado.AMedias) GameState.Sumar(energia: 15, nave: 5);
                else GameState.Sumar(energia: -10);
                break;
            case Misiones.Huerto:
                if (r == Resultado.Gano) GameState.Sumar(comida: 30, agua: -5, nave: 10);
                else if (r == Resultado.AMedias) GameState.Sumar(comida: 15, agua: -5, nave: 5);
                else GameState.Sumar(agua: -10);
                break;
            case Misiones.Refugio:
                if (r == Resultado.Gano) GameState.Sumar(moral: 15, nave: 10);
                else if (r == Resultado.AMedias) GameState.Sumar(nave: 5);
                else GameState.Sumar(moral: -15, nave: -5);
                break;
            case Misiones.Insulta:
                if (r == Resultado.Gano) { GameState.Sumar(moral: 20, nave: 10); GameState.NikolasGanadas++; }
                else if (r == Resultado.AMedias) GameState.Sumar(moral: 5, nave: 5);
                else GameState.Sumar(moral: -5);
                break;
            default:
                UnityEngine.Debug.LogError("Efectos.Aplicar: mision desconocida '" + id + "'. Para la roca usa DecidirRoca.");
                break;
        }
    }

    public static void DecidirRoca(DecisionRoca decision)
    {
        switch (decision)
        {
            case DecisionRoca.Analizar:
                GameState.Sumar(energia: -10, nave: 5); GameState.Marcas.Add("roca_analizada"); break;
            case DecisionRoca.SubirSinAnalizar:
                GameState.Sumar(nave: 5); GameState.Marcas.Add("roca_sin_analizar"); break;
            default:
                GameState.Sumar(moral: 10, nave: 5); GameState.Marcas.Add("roca_dejada"); break;
        }
    }
}

/// <summary>Tarjetas de dato real (texto y fuente de Narrativa y game design).</summary>
public static class DatosReales
{
    public struct Tarjeta { public string Texto; public string Fuente; }

    public static readonly Dictionary<string, Tarjeta> PorMision = new Dictionary<string, Tarjeta>
    {
        { Misiones.Respira, new Tarjeta { Texto = "MOXIE, a bordo de Perseverance, produjo 122 gramos de oxígeno en Marte en 16 sesiones.", Fuente = "jpl.nasa.gov" } },
        { Misiones.Tormenta, new Tarjeta { Texto = "Opportunity dejó de comunicarse en 2018 tras una tormenta de polvo que cubrió Marte.", Fuente = "science.nasa.gov" } },
        { Misiones.Huerto, new Tarjeta { Texto = "Veggie es el huerto de la Estación Espacial y suele tener seis plantas.", Fuente = "nasa.gov" } },
        { Misiones.Refugio, new Tarjeta { Texto = "La radiación espacial es uno de los cinco peligros de los vuelos tripulados.", Fuente = "nasa.gov/hrp" } },
        { Misiones.Insulta, new Tarjeta { Texto = "Una orden enviada desde la Tierra tarda de 3 a 22 minutos en llegar a Perseverance.", Fuente = "science.nasa.gov" } },
        { Misiones.Roca, new Tarjeta { Texto = "Cheyava Falls es una posible biofirma: podría tener origen biológico, pero faltan datos para concluirlo.", Fuente = "nasa.gov" } },
    };
}

/// <summary>Textos de cada sol (de Narrativa y game design).</summary>
public static class TextosSoles
{
    public static readonly string[] Dia =
    {
        "",
        "Wayra explica lo básico y Nikolas ya está discutiendo.",
        "Aparece una roca con manchas raras.",
        "Llega una alerta solar.",
        "Wayra empieza a contradecirse.",
        "El polvo se acerca y Nikolas se pone serio.",
        "Últimas reparaciones.",
        "Sube a la nave y despega. Ve al Puente.",
    };

    public static readonly string[] Noche =
    {
        "",
        "Un ruido suave en el casco. Wayra dice que es el viento.",
        "Los paneles se apagan uno por uno y vuelven.",
        "Hay huellas nuevas junto al invernadero.",
        "Llega un mensaje de radio con tu propia voz.",
        "Golpes en la escotilla.",
        "Silencio total. Los sensores marcan algo a tu lado.",
        "",
    };
}
