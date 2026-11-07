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
                ? "Noche 7 · 05:" + Mathf.Clamp(Mathf.FloorToInt(SegundosNoche7 - tiempoRestante), 0, 59).ToString("00")
                : "Noche " + GameState.Noche + " de 7 · " + GameState.Hora;
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
            return GameState.ValvulasCerradas ? "¡Al Puente! " + Mathf.CeilToInt(tiempoRestante) + " s" : "Cierra las válvulas en Soporte Vital. " + Mathf.CeilToInt(tiempoRestante) + " s";
        if (GameState.VentilacionPrometidaPendiente()) return "¡Enciende los ventiladores en Soporte Vital!";
        if (SimulacionNoche.HabraApagon) return "¡La batería no alcanza! Apaga sistemas en Soporte Vital.";
        return "Visitas " + GameState.Visitas + "/" + GameState.VisitasPorNoche + " · Camarotes: terminar la guardia";
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
        if (string.IsNullOrEmpty(e.idEstacion)) { textoAviso.text = e.nombreVisible; return; }
        if (carreraFinal)
        {
            textoAviso.text = e.idEstacion == Estaciones.SoporteVital ? "E: Cerrar válvulas"
                : e.idEstacion == Estaciones.Puente ? "E: Ir a la cabina" : e.nombreVisible + ": no hay tiempo";
            return;
        }
        if (e.idEstacion == Estaciones.Camarotes) { textoAviso.text = "E: Terminar la guardia (pasar a las 06:00)"; return; }
        textoAviso.text = e.Disponible ? "E: " + e.nombreVisible + " (2 h 30)" : e.nombreVisible + ": " + e.Motivo;
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
            if (textoAviso != null) textoAviso.text = "Válvulas cerradas. ¡Ahora al Puente!";
        }
        else if (e.idEstacion == Estaciones.Puente)
        {
            if (!GameState.ValvulasCerradas) { if (textoAviso != null) textoAviso.text = "Primero cierra las válvulas en Soporte Vital."; return; }
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
            TextosNoches.Telemetria[n] + " | Batería al anochecer: " + GameState.Bateria + " %",
            "",
            TextosNoches.Incidente[n],
        };
        if (n == 4 && GameState.CapsulaContaminada >= 0)
            cuerpo.Add("Cápsula contaminada: " + GameState.Capsulas[GameState.CapsulaContaminada].Nombre + ".");
        if (GameState.CentinelaActiva() && TextosNoches.Centinela[n] != "") cuerpo.Add(TextosNoches.Centinela[n]);

        PonerLuz(colorNoche, 0.8f);
        if (n == 3 && GameState.TomaCongelada)
        {
            if (GameState.CentinelaActiva() || GameState.BiorremediacionHecha)
            {
                GameState.TomaCongelada = false;
                if (!GameState.CentinelaActiva()) cuerpo.Add("La Dechloromonas dejó las tomas limpias: el hielo seco no se pegó.");
                yield return Pantalla("Noche 3 · 18:30 MTC", string.Join("\n", cuerpo));
            }
            else
            {
                cuerpo.Add("\nPara descongelar las tomas hay que quitarle el calor a un sector esta noche:");
                yield return Pantalla("Noche 3 · 18:30 MTC", string.Join("\n", cuerpo),
                    "Enfriar el Invernadero (riesgo: raíces húmedas se congelan)", "Enfriar los Camarotes (+25 BPM a Marco y Jonas)");
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
            cuerpo.Add("\nPunto de no retorno. ¿Qué sistema sacrificas?");
            yield return Pantalla("Noche 6 · 18:30 MTC", string.Join("\n", cuerpo),
                "Apagar el Invernadero y cargar los condensadores de despegue", "Apagar la Antena y sellar el domo como colonia");
            if (eleccion == 0)
            {
                GameState.InvernaderoSacrificado = true;
                GameState.Condensadores = 30;
                GameState.CalefInvernadero = false;
                foreach (var c in GameState.Capsulas) { c.Vaciar(); c.LedRojo = c.LedAzul = c.LedUv = false; }
                GameState.Trip(TripId.Aris).SumarBpm(25f);
                GameState.Informe.Add("Noche 6: sacrificaste el invernadero para cargar los condensadores de despegue.");
            }
            else
            {
                GameState.AntenaApagada = true;
                GameState.SumarDomo(1);
                GameState.Trip(TripId.Elena).SumarBpm(25f);
                GameState.Informe.Add("Noche 6: apagaste la antena y sellaste el domo como colonia.");
            }
        }
        else yield return Pantalla("Noche " + n + " · 18:30 MTC", string.Join("\n", cuerpo));

        if (!GameState.TutorialesVistos.Contains("mapa"))
        {
            GameState.TutorialesVistos.Add("mapa");
            yield return Pantalla("Cómo se juega", string.Join("\n", ComoSeJuega));
        }
    }

    static readonly string[] ComoSeJuega =
    {
        "• Camina con WASD y entra a las estaciones con E. Cada visita dura 2 h 30.",
        "• Lo que dejas encendido gasta batería. A las 6:00 AM ves qué pasó con el oxígeno, las plantas y la tripulación.",
        "• Arriba están tus recursos y el pulso de cada tripulante: verde está bien, rojo está mal.",
        "• Cuando quieras terminar la noche, ve a los Camarotes.",
        "",
        "Para empezar: siembra en el Invernadero, enciende el MOXIE en Soporte Vital y habla con Jonas en la Terminal Biomédica (está nervioso).",
    };

    /// <summary>Guiño a FNAF: a las 6:00 AM se resuelve todo lo que dejaste configurado.</summary>
    IEnumerator Amanecer()
    {
        int noche = GameState.Noche;
        PonerLuz(colorAmanecer, 1f);
        var reporte = SimulacionNoche.Amanecer();
        yield return Pantalla("6:00 AM", "Sobreviviste la Noche " + noche + ".\n\n" + string.Join("\n", reporte), null, null, 11);
        if (GameState.Oxigeno <= 0) { SceneManager.LoadScene(Escenas.Final); yield break; }
        if (GameState.Noche >= GameState.UltimaNoche) yield return HoraMasOscura();
        else yield return InicioDeNoche();
    }

    IEnumerator HoraMasOscura()
    {
        PonerLuz(colorEmergencia, 0.6f);
        yield return Pantalla("Noche 7 · 05:00 AM", TextosNoches.Titulo[7] + "\n" + TextosNoches.Telemetria[7] + " | Batería: " + GameState.Bateria + " %\n\n" + TextosNoches.Incidente[7] +
                              "\n\nRescate " + GameState.ProgresoRescate + "/4 · Domo " + GameState.AutosuficienciaDomo + "/4 · O₂ " + GameState.Oxigeno + " %");
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
            textoPantallaCuerpo.text = cuerpo + (conOpciones ? "" : "\n\n(Espacio o botón Continuar)");
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
