using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Estacion 3: Terminal Biomédica. Solo hay tiempo para hablar con 2 tripulantes por noche.
/// Mira el monitor (BPM y ppCO2) para saber si la crisis es psicologica o ambiental antes de responder.
/// En crisis (CO2 &gt; 5.5 mmHg o &gt; 115 BPM) el personaje pierde la voz, habla 3 veces mas lento y mira fijo.
/// </summary>
public class BiomedicaControlador : EstacionBase
{
    [Header("Tarjetas de la tripulacion (4)")]
    public Image[] retratosTarjeta;
    public Text[] textosTarjeta;
    public Image[] fondosTarjeta;

    [Header("Monitor")]
    public MonitorCardiaco monitor;
    public Text textoMonitor;

    [Header("Dialogo")]
    public Image retratoDialogo;
    public Text textoNombre;
    public Text textoDialogo;
    public GameObject[] botonesOpcion;
    public Text[] textosOpcion;
    public GameObject botonOtro;

    [Header("Retratos (orden: Elena, Marco, Aris, Jonas)")]
    public Sprite[] retratos;
    public Sprite[] retratosMirada;

    public const float LetrasPorSegundo = 40f;

    enum Paso { Elegir, Saludo, Opciones, Respuesta, Fin }
    Paso paso = Paso.Elegir;
    Tripulante actual;
    string textoCompleto = "";
    float letras;
    bool crisis;
    int ultimoBlip;

    static readonly Color Verde = new Color(0.3f, 0.69f, 0.48f), Amarillo = new Color(0.95f, 0.76f, 0.31f), Rojo = new Color(0.9f, 0.28f, 0.3f);

    public Tripulante Actual => actual;
    public bool Escribiendo => letras < textoCompleto.Length;

    protected override void Awake()
    {
        base.Awake();
        if (string.IsNullOrEmpty(idEstacion)) idEstacion = Estaciones.Biomedica;
    }

    void Start()
    {
        MostrarEleccion();
        MostrarTutorial();
    }

    protected override void Update()
    {
        base.Update();
        DibujarTarjetas();
        DibujarMonitor();
        Escribir();
    }

    // ------------------------------------------------------------------ Acciones

    /// <summary>Elegir con quien hablar: 0 Elena, 1 Marco, 2 Aris, 3 Jonas.</summary>
    public void Elegir(int i)
    {
        if (paso != Paso.Elegir || Saliendo) return;
        if (GameState.Charlas >= GameState.CharlasPorNoche) return;
        var t = GameState.Tripulacion[i];
        if (t.HabloEstaNoche) return;
        actual = t;
        t.HabloEstaNoche = true;
        GameState.Charlas++;
        crisis = t.EnCrisis;
        if (retratoDialogo != null)
        {
            retratoDialogo.sprite = crisis && retratosMirada != null && i < retratosMirada.Length ? retratosMirada[i] : (retratos != null && i < retratos.Length ? retratos[i] : null);
            retratoDialogo.enabled = retratoDialogo.sprite != null;
        }
        if (textoNombre != null) textoNombre.text = t.Nombre;
        paso = Paso.Saludo;
        Decir(Dialogos.Saludo(t, GameState.Noche));
    }

    /// <summary>0 A diagnostico, 1 B promesa, 2 C papa, 3 C lechuga.</summary>
    public void Opcion(int n)
    {
        if (paso != Paso.Opciones || actual == null) return;
        if (!Dialogos.OpcionDisponible(actual, n)) return;
        string respuesta = Dialogos.Responder(actual, n);
        if (string.IsNullOrEmpty(respuesta)) return;
        paso = Paso.Respuesta;
        Decir(respuesta);
        OcultarOpciones();
    }

    /// <summary>Volver a elegir otro tripulante.</summary>
    public void HablarConOtro()
    {
        if (paso != Paso.Fin) return;
        MostrarEleccion();
    }

    /// <summary>Muestra todo el texto de golpe (lo usa la prueba automatica y Espacio).</summary>
    public void CompletarTexto()
    {
        letras = textoCompleto.Length;
        if (textoDialogo != null) textoDialogo.text = textoCompleto;
        AlTerminarTexto();
    }

    // ------------------------------------------------------------------ Flujo

    void MostrarEleccion()
    {
        paso = Paso.Elegir;
        actual = null;
        OcultarOpciones();
        if (botonOtro != null) botonOtro.SetActive(false);
        if (retratoDialogo != null) retratoDialogo.enabled = false;
        if (textoNombre != null) textoNombre.text = "";
        Decir(GameState.Charlas >= GameState.CharlasPorNoche
            ? "Ya hablaste con 2 tripulantes. Vuelve mañana por la noche."
            : "Elige con quién hablar (te quedan " + (GameState.CharlasPorNoche - GameState.Charlas) + "). Mira su pulso y su CO₂ antes de responder.");
        CompletarTexto();
    }

    void Decir(string texto)
    {
        textoCompleto = texto ?? "";
        letras = 0f;
        ultimoBlip = 0;
        if (textoDialogo != null) textoDialogo.text = "";
    }

    void Escribir()
    {
        if (letras >= textoCompleto.Length) return;
        if (Teclas.AvanzarTeclado() && paso != Paso.Elegir) { CompletarTexto(); return; }
        float velocidad = crisis ? LetrasPorSegundo / 3f : LetrasPorSegundo;
        letras += velocidad * Time.deltaTime;
        int visibles = Mathf.Min(textoCompleto.Length, (int)letras);
        if (textoDialogo != null) textoDialogo.text = textoCompleto.Substring(0, visibles);
        if (!crisis && actual != null && visibles / 2 > ultimoBlip)
        {
            ultimoBlip = visibles / 2;
            AmbienteAudio.Efecto(SonidoProcedural.Blip(Dialogos.Voz(actual.Id)), 0.5f);
        }
        if (visibles >= textoCompleto.Length) AlTerminarTexto();
    }

    void AlTerminarTexto()
    {
        if (paso == Paso.Saludo) { paso = Paso.Opciones; MostrarOpciones(); }
        else if (paso == Paso.Respuesta)
        {
            paso = Paso.Fin;
            if (botonOtro != null) botonOtro.SetActive(GameState.Charlas < GameState.CharlasPorNoche);
        }
    }

    void MostrarOpciones()
    {
        for (int i = 0; i < botonesOpcion.Length; i++)
        {
            bool visible = actual != null && Dialogos.OpcionDisponible(actual, i);
            if (botonesOpcion[i] != null) botonesOpcion[i].SetActive(visible);
            if (visible && i < textosOpcion.Length && textosOpcion[i] != null) textosOpcion[i].text = Dialogos.TextoOpcion(actual, i);
        }
    }

    void OcultarOpciones()
    {
        foreach (var b in botonesOpcion) if (b != null) b.SetActive(false);
    }

    // ------------------------------------------------------------------ Dibujo

    void DibujarTarjetas()
    {
        var tri = GameState.Tripulacion;
        for (int i = 0; i < tri.Length; i++)
        {
            var t = tri[i];
            if (i < textosTarjeta.Length && textosTarjeta[i] != null)
            {
                string nombre = t.Nombre.Split(' ')[t.Id == TripId.Elena || t.Id == TripId.Aris ? 1 : 0];
                textosTarjeta[i].text = nombre + "\n" + Mathf.RoundToInt(t.Bpm) + " BPM" + (t.HabloEstaNoche ? "\n(ya hablaron)" : "");
            }
            if (i < fondosTarjeta.Length && fondosTarjeta[i] != null)
            {
                var c = t.Estado == EstadoTrip.Verde ? Verde : t.Estado == EstadoTrip.Amarillo ? Amarillo : Rojo;
                fondosTarjeta[i].color = t.HabloEstaNoche ? c * 0.45f : c;
            }
            if (i < retratosTarjeta.Length && retratosTarjeta[i] != null && retratos != null && i < retratos.Length)
                retratosTarjeta[i].sprite = retratos[i];
        }
    }

    void DibujarMonitor()
    {
        if (actual == null)
        {
            if (monitor != null) { monitor.bpm = 60f; monitor.sonido = false; }
            if (textoMonitor != null) textoMonitor.text = "Sin paciente conectado.   Verde < 85 BPM · Rojo > 115 BPM · Hipercapnia > 5.0 mmHg";
            return;
        }
        if (monitor != null) { monitor.bpm = actual.Bpm; monitor.sonido = true; }
        if (textoMonitor == null) return;
        int deuda = GameState.Noche * 39;
        string co2 = actual.Co2.ToString("0.0") + " mmHg";
        if (actual.Hipercapnia) co2 = "<color=#E5484D>" + co2 + "</color>";
        textoMonitor.text = "♥ " + Mathf.RoundToInt(actual.Bpm) + " BPM   ppCO₂ " + co2 + "   Deuda de sueño +" + (deuda / 60) + " h " + (deuda % 60) + " min";
    }

    protected override string ResumenSalida()
    {
        return "Biomédica: hablaste con " + GameState.Charlas + " tripulante(s)";
    }
}
