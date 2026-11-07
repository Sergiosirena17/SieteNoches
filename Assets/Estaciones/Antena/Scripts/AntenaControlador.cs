using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

/// <summary>
/// Estacion 4: Comunicaciones y Antena Banda X. Solo hay bateria para 1 paquete por noche (25 %).
/// 1) Eliges el paquete (SOS, domo o ciencia). 2) Alineas la onda con el satelite compensando el Doppler.
/// 3) Si llega, Nikolas responde desde Houston con 12 minutos de retraso.
/// Controles: A/D o flechas izquierda/derecha = frecuencia; W/S o arriba/abajo = amplitud.
/// </summary>
public class AntenaControlador : EstacionBase
{
    [Header("Grupos (uno visible a la vez)")]
    public GameObject grupoPaquetes;
    public GameObject grupoOscilo;
    public GameObject grupoMensaje;

    [Header("Osciloscopio")]
    public RectTransform[] puntosObjetivo;
    public RectTransform[] puntosJugador;
    public Vector2 tamanoOscilo = new Vector2(420f, 130f);
    public RectTransform marcaJugador;
    public RectTransform marcaObjetivo;
    public float anchoRegla = 420f;
    public RectTransform rellenoBloqueo;
    public Text textoTiempo;
    public Text textoEstado;
    public Text textoVacio;
    public BotonMantener botonFrecMenos, botonFrecMas, botonAmpMas, botonAmpMenos;

    [Header("Mensaje de Nikolas")]
    public Text textoMensaje;
    public GameObject[] botonesRespuesta;

    AntenaLogica logica;
    string paqueteElegido = "";
    bool respondio;
    readonly System.Random azar = new System.Random();

    public AntenaLogica Logica => logica;

    protected override void Awake()
    {
        base.Awake();
        if (string.IsNullOrEmpty(idEstacion)) idEstacion = Estaciones.Antena;
    }

    void Start()
    {
        Mostrar(grupoPaquetes);
        bool elenaVerde = GameState.Trip(TripId.Elena).Estado == EstadoTrip.Verde;
        if (marcaObjetivo != null) marcaObjetivo.gameObject.SetActive(elenaVerde);
    }

    void OnDestroy()
    {
        AmbienteAudio.MusicaApagada = false;
        AmbienteAudio.Siseo(0f);
    }

    protected override void Update()
    {
        base.Update();
        if (logica == null || Saliendo) return;
        if (!logica.Terminado)
        {
            int f = 0, a = 0;
            if (Teclas.Mantiene(Key.A) || Teclas.Mantiene(Key.LeftArrow) || Mantenido(botonFrecMenos)) f -= 1;
            if (Teclas.Mantiene(Key.D) || Teclas.Mantiene(Key.RightArrow) || Mantenido(botonFrecMas)) f += 1;
            if (Teclas.Mantiene(Key.W) || Teclas.Mantiene(Key.UpArrow) || Mantenido(botonAmpMas)) a += 1;
            if (Teclas.Mantiene(Key.S) || Teclas.Mantiene(Key.DownArrow) || Mantenido(botonAmpMenos)) a -= 1;
            logica.Avanzar(Time.deltaTime, f, a);
            if (logica.Terminado) Resultado();
        }
        DibujarOscilo();
    }

    static bool Mantenido(BotonMantener b) => b != null && b.Presionado;

    // ------------------------------------------------------------------ Acciones

    /// <summary>0 SOS a Houston, 1 mejoras del domo, 2 datos cientificos.</summary>
    public void ElegirPaquete(int i)
    {
        if (logica != null || Saliendo) return;
        paqueteElegido = i == 0 ? "sos" : i == 1 ? "domo" : "ciencia";
        bool elenaVerde = GameState.Trip(TripId.Elena).Estado == EstadoTrip.Verde;
        logica = new AntenaLogica(elenaVerde, System.Environment.TickCount);
        AmbienteAudio.MusicaApagada = true;
        Mostrar(grupoOscilo);
    }

    /// <summary>Salir sin gastar bateria.</summary>
    public void NoTransmitir()
    {
        if (logica != null) return;
        Salir();
    }

    /// <summary>Para la prueba automatica: termina la alineacion con el resultado dado.</summary>
    public void TerminarAlineacion(bool exito)
    {
        if (logica == null || logica.Terminado) return;
        logica.Terminado = true;
        logica.Exito = exito;
        Resultado();
    }

    /// <summary>0 cordial, 1 sarcastico.</summary>
    public void Responder(int i)
    {
        if (respondio || grupoMensaje == null || !grupoMensaje.activeSelf) return;
        respondio = true;
        string respuesta;
        if (i == 0)
        {
            GameState.NikolasCordiales++;
            if (GameState.Noche % 2 == 0)
            {
                GameState.CodigoReactorManana = true;
                respuesta = "Nikolas: Gracias. Te mando un código de desbloqueo: mañana el MOXIE calentará 30 % más rápido.";
            }
            else
            {
                GameState.PronosticoTau = true;
                respuesta = "Nikolas: Te mando el pronóstico de opacidad (Tau) de mañana. Llega con el reporte de las 06:00.";
            }
        }
        else
        {
            GameState.NikolasDiscusiones++;
            respuesta = "Nikolas: ...Ok. Anotado. Con 12 minutos de retraso, pero anotado.";
        }
        if (textoMensaje != null) textoMensaje.text += "\n\nTú: " + (i == 0 ? "Recibido, Houston. Gracias, Nikolas." : "Tú nos mandaste a aterrizar aquí, genio.") + "\n\n" + respuesta;
        foreach (var b in botonesRespuesta) if (b != null) b.SetActive(false);
    }

    // ------------------------------------------------------------------ Flujo

    void Resultado()
    {
        AmbienteAudio.MusicaApagada = false;
        AmbienteAudio.Siseo(0f);
        GameState.AntenaUsada = true;
        if (!logica.Exito)
        {
            GameState.AntenaFallo = true;
            if (textoEstado != null) textoEstado.text = "El orbitador se perdió tras el horizonte. No llegó nada (gastaste 10 % de batería).";
            return;
        }
        GameState.Paquete = paqueteElegido;
        string que;
        if (paqueteElegido == "sos") { GameState.SumarRescate(1); GameState.SosEnviados++; que = "SOS enviado: rescate " + GameState.ProgresoRescate + "/4."; }
        else if (paqueteElegido == "domo") { GameState.SumarDomo(1); GameState.Sumar(agua: 10); que = "Descargaste el esquema de reciclaje: domo " + GameState.AutosuficienciaDomo + "/4 y +10 % de agua."; }
        else que = "Transmitiste datos científicos. Aris está feliz; Elena no tanto.";
        if (textoMensaje != null)
            textoMensaje.text = que + "\n\n[Recibido 12 min 30 s después]\nNikolas: " + TextosNoches.MensajeNikolas[Mathf.Clamp(GameState.Noche, 0, 7)];
        Mostrar(grupoMensaje);
    }

    void Mostrar(GameObject grupo)
    {
        if (grupoPaquetes != null) grupoPaquetes.SetActive(grupo == grupoPaquetes);
        if (grupoOscilo != null) grupoOscilo.SetActive(grupo == grupoOscilo);
        if (grupoMensaje != null) grupoMensaje.SetActive(grupo == grupoMensaje);
    }

    void DibujarOscilo()
    {
        float t = Time.time;
        bool nieve = GameState.Noche == 5; // tormenta de protones: nieve en la pantalla
        Onda(puntosObjetivo, logica.FrecuenciaObjetivo, logica.AmplitudObjetivo, t, nieve);
        Onda(puntosJugador, logica.Frecuencia, logica.Amplitud, t, nieve);
        if (marcaJugador != null) marcaJugador.anchoredPosition = new Vector2((logica.Frecuencia - 0.5f) * anchoRegla, 0f);
        if (marcaObjetivo != null) marcaObjetivo.anchoredPosition = new Vector2((logica.FrecuenciaObjetivo - 0.5f) * anchoRegla, 0f);
        Barra(rellenoBloqueo, logica.Bloqueo, false);
        if (textoTiempo != null) textoTiempo.text = Mathf.CeilToInt(AntenaLogica.Duracion - logica.Tiempo) + " s";

        bool vacio = !logica.Terminado && logica.EnElVacio;
        AmbienteAudio.Siseo(vacio ? 1f : 0f);
        if (textoVacio != null)
            textoVacio.text = vacio ? "Distancia a la Tierra: 225,000,000 km\nRetraso de señal: 12 min 30 s" : "";
        if (textoEstado != null && !logica.Terminado)
            textoEstado.text = logica.Alineado ? "¡Señal alineada! Mantén..." : "Sigue la onda tenue: frecuencia con A/D, amplitud con W/S.";
    }

    void Onda(RectTransform[] puntos, float frecuencia, float amplitud, float t, bool nieve)
    {
        if (puntos == null || puntos.Length < 2) return;
        float ciclos = 1f + frecuencia * 5f;
        for (int i = 0; i < puntos.Length; i++)
        {
            float u = i / (float)(puntos.Length - 1);
            float y = amplitud * tamanoOscilo.y * 0.5f * Mathf.Sin(2f * Mathf.PI * (ciclos * u) - t * 3f);
            if (nieve) y += ((float)azar.NextDouble() - 0.5f) * 14f;
            puntos[i].anchoredPosition = new Vector2((u - 0.5f) * tamanoOscilo.x, y);
        }
    }

    protected override string ResumenSalida()
    {
        if (logica != null && !logica.Terminado) TerminarAlineacion(false);
        if (!GameState.AntenaUsada) return "Antena: no transmitiste";
        if (GameState.AntenaFallo) return "Antena: se perdió el orbitador";
        return "Antena: paquete " + GameState.Paquete.ToUpper() + " enviado";
    }
}
