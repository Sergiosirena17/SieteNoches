using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Estado compartido de toda la partida. Es la UNICA forma en que las escenas se pasan datos.
/// Reglas y numeros: ver el GDD. Si cambias un numero aqui, cambialo tambien en el GDD.
/// </summary>
public static class GameState
{
    public const int SolDelLanzamiento = 7;

    public static int Sol = 1;
    public static int MisionesHoy = 0;

    public static int Oxigeno = 70;
    public static int Agua = 70;
    public static int Comida = 70;
    public static int Energia = 70;
    public static int Nave = 20;
    public static int Moral = 70;

    public static int NikolasGanadas = 0;
    public static readonly HashSet<string> Marcas = new HashSet<string>();

    /// <summary>Misiones ya jugadas en el sol actual (no se pueden repetir el mismo sol).</summary>
    public static readonly List<string> MisionesHechasHoy = new List<string>();

    /// <summary>Cuantas veces se jugo cada mision en toda la partida. Sirve para subir la dificultad.</summary>
    public static readonly Dictionary<string, int> VecesJugado = new Dictionary<string, int>();

    /// <summary>Historial para la pantalla de resumen del final.</summary>
    public static readonly List<string> Historial = new List<string>();

    /// <summary>Donde estaba el jugador antes de entrar a un minijuego. Null = posicion inicial.</summary>
    public static Vector2? PosicionJugador = null;

    /// <summary>Misiones disponibles en cada sol, segun la tabla del GDD. El indice 0 no se usa.</summary>
    public static readonly string[][] MisionesPorSol =
    {
        new string[0],
        new[] { Misiones.Respira, Misiones.Tormenta, Misiones.Insulta },
        new[] { Misiones.Huerto, Misiones.Respira, Misiones.Roca },
        new[] { Misiones.Tormenta, Misiones.Refugio, Misiones.Insulta },
        new[] { Misiones.Roca, Misiones.Huerto, Misiones.Refugio },
        new[] { Misiones.Respira, Misiones.Tormenta, Misiones.Insulta },
        new[] { Misiones.Refugio, Misiones.Huerto, Misiones.Roca },
        new string[0],
    };

    /// <summary>
    /// El proyecto entra a Play sin recargar el codigo (Enter Play Mode Options), asi que las variables
    /// static sobreviven entre una prueba y otra. Esto las limpia cada vez que se presiona Play.
    /// </summary>
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    static void AlPresionarPlay()
    {
        Reiniciar();
        JugadorMovimiento.Bloqueado = false;
    }

    public static void Reiniciar()
    {
        Sol = 1; MisionesHoy = 0; NikolasGanadas = 0;
        Oxigeno = 70; Agua = 70; Comida = 70; Energia = 70;
        Nave = 20; Moral = 70;
        Marcas.Clear();
        MisionesHechasHoy.Clear();
        VecesJugado.Clear();
        Historial.Clear();
        PosicionJugador = null;
    }

    public static void Sumar(int oxigeno = 0, int agua = 0, int comida = 0,
                             int energia = 0, int nave = 0, int moral = 0)
    {
        Oxigeno = Mathf.Clamp(Oxigeno + oxigeno, 0, 100);
        Agua = Mathf.Clamp(Agua + agua, 0, 100);
        Comida = Mathf.Clamp(Comida + comida, 0, 100);
        Energia = Mathf.Clamp(Energia + energia, 0, 100);
        Nave = Mathf.Clamp(Nave + nave, 0, 100);
        Moral = Mathf.Clamp(Moral + moral, 0, 100);
    }

    public static int MisionesPermitidas()
    {
        return Moral == 0 ? 1 : 2;
    }

    public static bool RocaDecidida()
    {
        return Marcas.Contains("roca_analizada") || Marcas.Contains("roca_sin_analizar") || Marcas.Contains("roca_dejada");
    }

    public static List<string> MisionesDisponibles()
    {
        var lista = new List<string>();
        if (Sol < 1 || Sol >= SolDelLanzamiento) return lista;
        foreach (var id in MisionesPorSol[Sol])
        {
            if (MisionesHechasHoy.Contains(id)) continue;
            if (id == Misiones.Roca && RocaDecidida()) continue;
            lista.Add(id);
        }
        return lista;
    }

    public static bool DebeLlegarLaNoche()
    {
        if (Sol >= SolDelLanzamiento) return false;
        return MisionesHoy >= MisionesPermitidas() || MisionesDisponibles().Count == 0;
    }

    public static int Dificultad(string idMision)
    {
        return VecesJugado.TryGetValue(idMision, out var veces) ? veces : 0;
    }

    public static void RegistrarMision(string idMision, string resumen)
    {
        MisionesHoy++;
        MisionesHechasHoy.Add(idMision);
        VecesJugado[idMision] = Dificultad(idMision) + 1;
        Historial.Add("Sol " + Sol + ": " + resumen);
    }

    /// <summary>Lo llama cada minijuego al terminar, despues de aplicar su resultado.</summary>
    public static void TerminarMinijuego(string idMision, string resumen)
    {
        RegistrarMision(idMision, resumen);
        SceneManager.LoadScene(Escenas.Mapa);
    }

    public static void PasarNoche()
    {
        int penalizacionMoral = 0;
        Sumar(oxigeno: -15, agua: -10, comida: -10, energia: -10);
        if (Agua == 0) penalizacionMoral -= 15;
        if (Comida == 0) penalizacionMoral -= 15;
        if (Energia == 0) penalizacionMoral -= 15;
        Sumar(moral: penalizacionMoral);
        MisionesHoy = 0;
        MisionesHechasHoy.Clear();
        Sol++;
    }
}
