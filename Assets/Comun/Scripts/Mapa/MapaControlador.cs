using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.Universal;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>
/// Controla el mapa: barra de recursos, avisos, la pantalla de las 18:30, los dilemas (Noches 3 y 6),
/// el amanecer de las 06:00 y la carrera de 60 segundos de la Noche 7.
/// </summary>
public class MapaControlador : MonoBehaviour
{
    public static MapaControlador Instancia { get; private set; }

    [Header("Escena")]
    public Transform jugador;
    public Light2D luzGlobal;

    [Header("Barra superior")]
    public Text textoNoche;
    public Text textoObjetivo;
    public Text textoBateria, textoOxigeno, textoAgua, textoCo2, textoComida, textoRescate, textoDomo;
    public Text[] textosTripulacion;

    [Header("Avisos y pantallas")]
    public Text textoAviso;
    public GameObject panelPantalla;
    public Text textoPantallaTitulo;
    public Text textoPantallaCuerpo;
    public GameObject botonContinuar;
    public GameObject botonOpcionA, botonOpcionB;
    public Text textoOpcionA, textoOpcionB;

    [Header("Luz")]
    public Color colorNoche = new Color(0.55f, 0.62f, 0.9f);
    public Color colorAmanecer = new Color(1f, 0.85f, 0.7f);
    public Color colorEmergencia = new Color(1f, 0.25f, 0.2f);

    public const float SegundosNoche7 = 60f;

    EstacionMapa estacionCerca;
    bool continuar;
    int eleccion = -1;
    bool carreraFinal;
    float tiempoRestante;

    static readonly Color Blanco = new Color(0.96f, 0.95f, 0.92f), Rojo = new Color(0.9f, 0.28f, 0.3f),
                          Amarillo = new Color(0.95f, 0.76f, 0.31f), Verde = new Color(0.3f, 0.69f, 0.48f);

    public bool PantallaAbierta => panelPantalla != null && panelPantalla.activeSelf;
    public float TiempoRestante => tiempoRestante;

    void Awake()
    {
        Instancia = this;
    }

    void OnDestroy()
    {
        if (Instancia == this) Instancia = null;
    }

    void Start()
    {
        AmbienteAudio.EntrarMapa();
        if (GameState.PosicionJugador.HasValue && jugador != null)
            jugador.position = GameState.PosicionJugador.Value;
        if (textoAviso != null) textoAviso.text = "";
        if (panelPantalla != null) panelPantalla.SetActive(false);
        PonerLuz(colorNoche, 0.8f);
        ActualizarBarra();

        if (GameState.Oxigeno <= 0) { SceneManager.LoadScene(Escenas.Final); return; }
        if (GameState.Noche >= GameState.UltimaNoche) StartCoroutine(HoraMasOscura());
        else if (GameState.DebeAmanecer()) StartCoroutine(Amanecer());
        else if (!GameState.PantallaInicioMostrada) StartCoroutine(InicioDeNoche());
    }

    void Update()
    {
        ActualizarBarra();
        if (!carreraFinal || PantallaAbierta) return;
        tiempoRestante -= Time.deltaTime;
        if (tiempoRestante <= 0f)
        {
            carreraFinal = false;
            GameState.TiempoAgotado = true;
            SceneManager.LoadScene(Escenas.Final);
        }
    }

    // ------------------------------------------------------------------ Barra superior

    void ActualizarBarra()
    {
        if (textoNoche != null)
            textoNoche.text = carreraFinal
            ? "Night 7 · 05:" + Mathf.Clamp(Mathf.FloorToInt(SegundosNoche7 - tiempoRestante), 0, 59).ToString("00")
            : "Night " + GameState.Noche + " of 7 · " + GameState.Hora;
        if (textoObjetivo != null) textoObjetivo.text = Objetivo();

        int consumo = SimulacionNoche.ConsumoProyectado();
        Poner(textoBateria, GameState.Noche >= GameState.UltimaNoche ? GameState.Bateria + " %" : consumo + "/" + GameState.Bateria + " %", consumo > GameState.Bateria && GameState.Noche < GameState.UltimaNoche);
        Poner(textoOxigeno, GameState.Oxigeno + " %", GameState.Oxigeno <= 30);
        Poner(textoAgua, GameState.Agua + " %", GameState.Agua <= 20);
        float co2 = GameState.Co2Maximo();
        Poner(textoCo2, co2.ToString("0.0"), co2 > Tripulante.LimiteHipercapnia);
        Poner(textoComida, GameState.Raciones + "·" + GameState.Lechugas, false);
        Poner(textoRescate, GameState.ProgresoRescate + "/4", false);
        Poner(textoDomo, GameState.AutosuficienciaDomo + "/4", false);

        if (textosTripulacion == null) return;
        for (int i = 0; i < textosTripulacion.Length && i < GameState.Tripulacion.Length; i++)
        {
            var t = GameState.Tripulacion[i];
            if (textosTripulacion[i] == null) continue;
            textosTripulacion[i].text = t.Nombre.Split(' ')[t.Id == TripId.Elena || t.Id == TripId.Aris ? 1 : 0] + " " + Mathf.RoundToInt(t.Bpm);
            textosTripulacion[i].color = t.Estado == EstadoTrip.Verde ? Verde : t.Estado == EstadoTrip.Amarillo ? Amarillo : Rojo;
        }
    }

    string Objetivo()
    {
        if (carreraFinal)
            return GameState.ValvulasCerradas ? "Get to the Bridge! " + Mathf.CeilToInt(tiempoRestante) + " s" : "Close the Life Support valves. " + Mathf.CeilToInt(tiempoRestante) + " s";
        if (GameState.VentilacionPrometidaPendiente()) return "Turn on the Life Support fans!";
        if (SimulacionNoche.HabraApagon) return "The battery is not enough! Shut down systems in Life Support.";
        return "Visits " + GameState.Visitas + "/" + GameState.VisitasPorNoche + " · Crew Quarters: end the watch";
    }

    static void Poner(Text t, string valor, bool alerta)
    {
        if (t == null) return;
        t.text = valor;
        t.color = alerta ? Rojo : Blanco;
    }

    // ------------------------------------------------------------------ Estaciones

    public void MostrarAviso(EstacionMapa e)
    {
        estacionCerca = e;
        if (textoAviso == null) return;
        string nombre = NombreEnIngles(e);
        if (string.IsNullOrEmpty(e.idEstacion)) { textoAviso.text = nombre; return; }
        if (carreraFinal)
        {
        textoAviso.text = e.idEstacion == Estaciones.SoporteVital ? "E: Close valves"
            : e.idEstacion == Estaciones.Puente ? "E: Go to Bridge" : nombre + ": no time left";
            return;
        }
        if (e.idEstacion == Estaciones.Camarotes) { textoAviso.text = "E: End watch (advance to 06:00)"; return; }
        textoAviso.text = e.Disponible ? "E: " + nombre + " (2 h 30)" : nombre + ": " + e.Motivo;
    }

    static string NombreEnIngles(EstacionMapa e)
    {
        if (!string.IsNullOrEmpty(e.idEstacion) && Estaciones.Nombre.TryGetValue(e.idEstacion, out string nombre))
            return nombre;

        switch (e.nombreVisible)
        {
            case "Zona de hielo (próximamente)": return "Ice Zone (coming soon)";
            case "Paneles solares: 0 W de noche": return "Solar panels: 0 W at night";
            case "Sonda antigua (próximamente)": return "Ancient probe (coming soon)";
            default: return e.nombreVisible;
        }
    }

    public void OcultarAviso(EstacionMapa e)
    {
        if (estacionCerca != e) return;
        estacionCerca = null;
        if (textoAviso != null) textoAviso.text = "";
    }

    public void Entrar(EstacionMapa e)
    {
        if (e == null || PantallaAbierta) return;
        if (carreraFinal) { EntrarNoche7(e); return; }
        if (!e.Disponible) return;
        if (e.idEstacion == Estaciones.Camarotes)
        {
            GameState.GuardiaTerminada = true;
            StartCoroutine(Amanecer());
            return;
        }
        if (!Estaciones.Escena.TryGetValue(e.idEstacion, out var escena)) return;
        if (jugador != null) GameState.PosicionJugador = jugador.position;
        SceneManager.LoadScene(escena);
    }

    void EntrarNoche7(EstacionMapa e)
    {
        if (e.idEstacion == Estaciones.SoporteVital)
        {
            GameState.ValvulasCerradas = true;
        if (textoAviso != null) textoAviso.text = "Valves closed. Now get to the Bridge!";
        }
        else if (e.idEstacion == Estaciones.Puente)
        {
        if (!GameState.ValvulasCerradas) { if (textoAviso != null) textoAviso.text = "Close the Life Support valves first."; return; }
            carreraFinal = false;
            SceneManager.LoadScene(Escenas.Lanzamiento);
        }
    }

    // ------------------------------------------------------------------ Pantallas de la noche

    IEnumerator InicioDeNoche()
    {
        GameState.PantallaInicioMostrada = true;
        int n = GameState.Noche;
        var cuerpo = new List<string>
        {
            TextosNoches.Titulo[n],
            TextosNoches.Telemetria[n] + " | Battery at nightfall: " + GameState.Bateria + "%",
            "",
            TextosNoches.Incidente[n],
        };
        if (n == 4 && GameState.CapsulaContaminada >= 0)
            cuerpo.Add("Contaminated pod: " + GameState.Capsulas[GameState.CapsulaContaminada].Nombre + ".");
        if (GameState.CentinelaActiva() && TextosNoches.Centinela[n] != "") cuerpo.Add(TextosNoches.Centinela[n]);

        PonerLuz(colorNoche, 0.8f);
        if (n == 3 && GameState.TomaCongelada)
        {
            if (GameState.CentinelaActiva() || GameState.BiorremediacionHecha)
            {
                GameState.TomaCongelada = false;
        if (!GameState.CentinelaActiva()) cuerpo.Add("Dechloromonas left the intakes clean: dry ice did not stick.");
        yield return Pantalla("Night 3 · 18:30 MTC", string.Join("\n", cuerpo));
            }
            else
            {
        cuerpo.Add("\nTo thaw the intakes, you must remove heat from one sector tonight:");
        yield return Pantalla("Night 3 · 18:30 MTC", string.Join("\n", cuerpo),
            "Cool the Greenhouse (risk: wet roots freeze)", "Cool the Crew Quarters (+25 BPM to Marco and Jonas)");
                GameState.TomaCongelada = false;
                if (eleccion == 0) GameState.SectorSinCalor = Estaciones.Invernadero;
                else
                {
                    GameState.SectorSinCalor = Estaciones.Camarotes;
                    GameState.Trip(TripId.Marco).SumarBpm(25f);
                    GameState.Trip(TripId.Jonas).SumarBpm(25f);
                }
            }
        }
        else if (n == 6)
        {
        cuerpo.Add("\nPoint of no return. Which system do you sacrifice?");
        yield return Pantalla("Night 6 · 18:30 MTC", string.Join("\n", cuerpo),
            "Shut down the Greenhouse and charge the launch capacitors", "Shut down the Antenna and seal the dome as a colony");
            if (eleccion == 0)
            {
                GameState.InvernaderoSacrificado = true;
                GameState.Condensadores = 30;
                GameState.CalefInvernadero = false;
                foreach (var c in GameState.Capsulas) { c.Vaciar(); c.LedRojo = c.LedAzul = c.LedUv = false; }
                GameState.Trip(TripId.Aris).SumarBpm(25f);
        GameState.Informe.Add("Night 6: you sacrificed the greenhouse to charge the launch capacitors.");
            }
            else
            {
                GameState.AntenaApagada = true;
                GameState.SumarDomo(1);
                GameState.Trip(TripId.Elena).SumarBpm(25f);
        GameState.Informe.Add("Night 6: you shut down the antenna and sealed the dome as a colony.");
            }
        }
        else yield return Pantalla("Night " + n + " · 18:30 MTC", string.Join("\n", cuerpo));

        if (!GameState.TutorialesVistos.Contains("mapa"))
        {
            GameState.TutorialesVistos.Add("mapa");
            yield return Pantalla("How to Play", string.Join("\n", ComoSeJuega));
        }
    }

    static readonly string[] ComoSeJuega =
    {
        "• Move with WASD and enter stations with E. Each visit lasts 2 h 30.",
        "• Anything you leave on drains the battery. At 6:00 AM you see what happened to oxygen, plants, and crew.",
        "• Your resources and each crew member's pulse are at the top: green is good, red is bad.",
        "• When you want to end the night, go to the Crew Quarters.",
        "",
        "To begin: plant in the Greenhouse, turn on MOXIE in Life Support, and talk to Jonas at the Biomedical Terminal (he is nervous).",
    };

    /// <summary>Guiño a FNAF: a las 6:00 AM se resuelve todo lo que dejaste configurado.</summary>
    IEnumerator Amanecer()
    {
        int noche = GameState.Noche;
        PonerLuz(colorAmanecer, 1f);
        var reporte = SimulacionNoche.Amanecer();
        yield return Pantalla("6:00 AM", "You survived Night " + noche + ".\n\n" + string.Join("\n", reporte), null, null, 11);
        if (GameState.Oxigeno <= 0) { SceneManager.LoadScene(Escenas.Final); yield break; }
        if (GameState.Noche >= GameState.UltimaNoche) yield return HoraMasOscura();
        else yield return InicioDeNoche();
    }

    IEnumerator HoraMasOscura()
    {
        PonerLuz(colorEmergencia, 0.6f);
        yield return Pantalla("Night 7 · 05:00 AM", TextosNoches.Titulo[7] + "\n" + TextosNoches.Telemetria[7] + " | Battery: " + GameState.Bateria + "%\n\n" + TextosNoches.Incidente[7] +
            "\n\nRescue " + GameState.ProgresoRescate + "/4 · Dome " + GameState.AutosuficienciaDomo + "/4 · O₂ " + GameState.Oxigeno + "%");
        tiempoRestante = SegundosNoche7;
        carreraFinal = true;
        AmbienteAudio.Efecto(SonidoProcedural.Pitido(), 0.8f);
    }

    /// <summary>
    /// Muestra una pantalla y espera. Sin opciones: Espacio, clic o el boton Continuar.
    /// Con opciones: espera a que elija A o B (queda en "eleccion").
    /// </summary>
    IEnumerator Pantalla(string titulo, string cuerpo, string opcionA = null, string opcionB = null, int tamano = 13)
    {
        JugadorMovimiento.Bloqueado = true;
        continuar = false;
        eleccion = -1;
        bool conOpciones = !string.IsNullOrEmpty(opcionA);
        if (panelPantalla != null) panelPantalla.SetActive(true);
        if (textoPantallaTitulo != null) textoPantallaTitulo.text = titulo;
        if (textoPantallaCuerpo != null)
        {
            textoPantallaCuerpo.fontSize = tamano;
        textoPantallaCuerpo.text = cuerpo + (conOpciones ? "" : "\n\n(Space or Continue button)");
        }
        if (botonContinuar != null) botonContinuar.SetActive(!conOpciones);
        if (botonOpcionA != null) botonOpcionA.SetActive(conOpciones);
        if (botonOpcionB != null) botonOpcionB.SetActive(conOpciones);
        if (textoOpcionA != null) textoOpcionA.text = opcionA ?? "";
        if (textoOpcionB != null) textoOpcionB.text = opcionB ?? "";
        yield return null;
        if (conOpciones) while (eleccion < 0) yield return null;
        else while (!continuar && !Teclas.AvanzarTeclado()) yield return null;
        if (panelPantalla != null) panelPantalla.SetActive(false);
        yield return null;
        JugadorMovimiento.Bloqueado = false;
    }

    /// <summary>Boton "Continuar" de la pantalla (y la prueba automatica).</summary>
    public void ContinuarPantalla() { continuar = true; }

    /// <summary>Botones de los dilemas: 0 = opcion A, 1 = opcion B.</summary>
    public void Elegir(int opcion) { eleccion = opcion; }

    void PonerLuz(Color c, float intensidad)
    {
        if (luzGlobal == null) return;
        luzGlobal.color = c;
        luzGlobal.intensity = intensidad;
    }
}
