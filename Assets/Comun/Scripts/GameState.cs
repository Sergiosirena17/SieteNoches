using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Estado compartido de toda la partida. Es la UNICA forma en que las escenas se pasan datos.
/// Las 4 estaciones leen y cambian este mismo estado: lo que haces en una cambia lo que pasa en las otras.
/// Reglas y numeros: documento "Ideas de misiones" + Docs/CambiosParaDocs (lo que el documento no fijaba).
/// </summary>
public static class GameState
{
    public const int UltimaNoche = 7;
    /// <summary>Cada visita a una estacion dura 2 h 30. Despues de 4 visitas son las 06:00.</summary>
    public const int VisitasPorNoche = 4;
    public const int MinutosPorVisita = 150;
    public const int CharlasPorNoche = 2;

    // ---------- Noche y reloj ----------
    public static int Noche;
    public static int Visitas;
    public static bool GuardiaTerminada;
    public static bool PantallaInicioMostrada;

    // ---------- Recursos (0 a 100) ----------
    /// <summary>Carga con la que empezo esta noche, en %. Se gasta al amanecer segun lo que dejaste encendido.</summary>
    public static int Bateria;
    public static int Oxigeno;
    public static int Agua;
    public static int Raciones;
    public static int Lechugas;
    /// <summary>0 a 4. Sube con cada SOS que llega a Houston.</summary>
    public static int ProgresoRescate;
    /// <summary>0 a 4. Sube con mejoras del domo, suelo descontaminado y cosechas limpias.</summary>
    public static int AutosuficienciaDomo;

    // ---------- Sistemas (persisten entre noches) ----------
    public static bool CalefInvernadero;
    public static bool CalefCamarotes;
    public static bool Ventiladores;
    /// <summary>Noche 3: sector al que le quitaste el calor para descongelar las tomas ("invernadero" o "camarotes").</summary>
    public static string SectorSinCalor;
    public static bool TomaCongelada;

    // ---------- MOXIE ----------
    public static bool MoxieUsado;
    public static float MoxieProduccion;
    public static bool MoxieDanado;
    public static int NocheMoxieDanado;
    public static float HollinInicial;
    public static bool CodigoReactor;
    public static bool CodigoReactorManana;

    // ---------- Antena ----------
    /// <summary>Paquete enviado esta noche: "", "sos", "domo" o "ciencia".</summary>
    public static string Paquete;
    public static bool AntenaUsada;
    public static bool AntenaFallo;
    public static int NikolasDiscusiones;
    public static int NikolasCordiales;
    public static bool PronosticoTau;

    // ---------- Biomedica ----------
    public static int Charlas;
    public static int PromesasRotas;

    // ---------- Invernadero y tripulacion ----------
    public static Capsula[] Capsulas;
    public static Tripulante[] Tripulacion;
    public static int CapsulaContaminada = -1;

    // ---------- Decisiones grandes ----------
    public static bool InvernaderoSacrificado;
    public static bool AntenaApagada;
    public static bool CosechaLimpia;
    public static bool BiorremediacionHecha;
    public static int Condensadores;

    // ---------- Noche 7 ----------
    public static bool ValvulasCerradas;
    public static bool TiempoAgotado;
    /// <summary>"despegar" o "sellar". Vacio mientras no se decide en el Puente.</summary>
    public static string DecisionFinal;

    // ---------- Para el informe del final ----------
    public static int NochesMoxie, NochesApagon, NochesSinVentilacion, PlantasMuertas, Cosechas, SosEnviados, CosechasToxicas;
    public static readonly List<string> Informe = new List<string>();
    public static readonly List<string> UltimoReporte = new List<string>();

    /// <summary>Donde estaba el jugador antes de entrar a una estacion. Null = posicion inicial.</summary>
    public static Vector2? PosicionJugador;

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
        Oxigeno = 70; Agua = 60; Raciones = 0; Lechugas = 0;
        ProgresoRescate = 0; AutosuficienciaDomo = 0;
        CalefInvernadero = true; CalefCamarotes = true; Ventiladores = true;
        NocheMoxieDanado = 0;
        NikolasDiscusiones = 0; NikolasCordiales = 0; PronosticoTau = false; CodigoReactorManana = false;
        PromesasRotas = 0;
        InvernaderoSacrificado = false; AntenaApagada = false; CosechaLimpia = false; BiorremediacionHecha = false;
        Condensadores = 0; CapsulaContaminada = -1;
        ValvulasCerradas = false; TiempoAgotado = false; DecisionFinal = "";
        NochesMoxie = NochesApagon = NochesSinVentilacion = PlantasMuertas = Cosechas = SosEnviados = CosechasToxicas = 0;
        Informe.Clear();
        UltimoReporte.Clear();
        PosicionJugador = null;

        Capsulas = new[]
        {
            new Capsula("Cápsula 1 (hidropónica)", false),
            new Capsula("Cápsula 2 (regolito)", true),
            new Capsula("Cápsula 3 (regolito)", true),
        };
        Tripulacion = new[]
        {
            new Tripulante(TripId.Elena, "Cmdt. Elena Vance", "Piloto principal", 88f, 2.0f),
            new Tripulante(TripId.Marco, "Marco \"Tornillo\" Silva", "Ingeniero de soporte vital", 78f, 2.5f),
            new Tripulante(TripId.Aris, "Dra. Aris Thorne", "Astrobióloga", 92f, 2.0f),
            new Tripulante(TripId.Jonas, "Jonas Miller", "Especialista de carga", 100f, 2.0f),
        };
        SimulacionNoche.EmpezarNoche(1, 0);
    }

    public static Tripulante Trip(TripId id) => Tripulacion[(int)id];

    public static void Sumar(int oxigeno = 0, int agua = 0)
    {
        Oxigeno = Mathf.Clamp(Oxigeno + oxigeno, 0, 100);
        Agua = Mathf.Clamp(Agua + agua, 0, 100);
    }

    public static void SumarRescate(int n) { ProgresoRescate = Mathf.Clamp(ProgresoRescate + n, 0, 4); }
    public static void SumarDomo(int n) { AutosuficienciaDomo = Mathf.Clamp(AutosuficienciaDomo + n, 0, 4); }

    /// <summary>Hora de la noche: empieza a las 18:30 y cada visita suma 2 h 30.</summary>
    public static string Hora
    {
        get
        {
            if (Noche >= UltimaNoche) return "05:00";
            int minutos = (18 * 60 + 30 + Visitas * MinutosPorVisita) % (24 * 60);
            return (minutos / 60).ToString("00") + ":" + (minutos % 60).ToString("00");
        }
    }

    /// <summary>Despues de las 02:00 es "madrugada": cambian algunos dialogos.</summary>
    public static bool Madrugada => Visitas >= 3;

    public static bool DebeAmanecer()
    {
        return Noche < UltimaNoche && (GuardiaTerminada || Visitas >= VisitasPorNoche);
    }

    /// <summary>Si se puede entrar a la estacion ahora. Si no, devuelve el motivo para mostrarlo.</summary>
    public static bool EstacionDisponible(string id, out string motivo)
    {
        motivo = "";
        if (Noche >= UltimaNoche)
        {
            if (id == Estaciones.SoporteVital || id == Estaciones.Puente) return true;
            motivo = "No hay tiempo: Soporte Vital y luego el Puente.";
            return false;
        }
        switch (id)
        {
            case Estaciones.Invernadero:
                if (InvernaderoSacrificado) { motivo = "Apagado para cargar los condensadores"; return false; }
                return true;
            case Estaciones.SoporteVital:
            case Estaciones.Camarotes:
                return true;
            case Estaciones.Biomedica:
                if (Charlas >= CharlasPorNoche) { motivo = "Ya hablaste con 2 tripulantes esta noche"; return false; }
                return true;
            case Estaciones.Antena:
                if (AntenaApagada) { motivo = "Apagada para sellar el domo"; return false; }
                if (Noche < 2) { motivo = "Se activa en la Noche 2, cuando pase el orbitador"; return false; }
                if (AntenaUsada) { motivo = "Solo hay batería para 1 paquete por noche"; return false; }
                return true;
            case Estaciones.Puente:
                motivo = "La ventana orbital se abre en la Noche 7";
                return false;
        }
        motivo = "Próximamente";
        return false;
    }

    public static bool EstacionDisponible(string id) => EstacionDisponible(id, out _);

    /// <summary>Lo llama cada estacion al salir: avanza el reloj y vuelve al mapa.</summary>
    public static void TerminarEstacion(string id, string resumen)
    {
        Visitas++;
        if (!string.IsNullOrEmpty(resumen)) Informe.Add("Noche " + Noche + " " + Hora + ": " + resumen);
        SceneManager.LoadScene(Escenas.Mapa);
    }

    public static int ContarEnRojo()
    {
        int n = 0;
        foreach (var t in Tripulacion) if (t.Estado == EstadoTrip.Rojo) n++;
        return n;
    }

    public static float Co2Maximo()
    {
        float m = 0f;
        foreach (var t in Tripulacion) m = Mathf.Max(m, t.Co2);
        return m;
    }

    /// <summary>Las 3 capsulas con plantas sanas y sin percloratos (condicion de Raíces Rojas).</summary>
    public static bool InvernaderoAlCien()
    {
        if (InvernaderoSacrificado) return false;
        foreach (var c in Capsulas)
            if (!c.Viva || c.Perclorato || c.Toxica || c.Estado == EstadoPlanta.Estresada) return false;
        return true;
    }

    public static bool CentinelaActiva()
    {
        foreach (var c in Capsulas)
            if (c.Especie == Especie.Arabidopsis && c.Estado == EstadoPlanta.Madura) return true;
        return false;
    }

    /// <summary>Promesa de "voy a encender la ventilacion" pendiente: el mapa lo muestra como urgente.</summary>
    public static bool VentilacionPrometidaPendiente()
    {
        if (Ventiladores) return false;
        foreach (var t in Tripulacion) if (t.Promesa == Promesa.Ventilacion) return true;
        return false;
    }
}
