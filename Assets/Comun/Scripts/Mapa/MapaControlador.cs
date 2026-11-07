using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.Universal;
using UnityEngine.SceneManagement;
using UnityEngine.Serialization;
using UnityEngine.UI;

/// <summary>
/// Controla el mapa: barra de recursos, avisos de estaciones, el inicio de cada noche y el amanecer (6:00 AM).
/// </summary>
public class MapaControlador : MonoBehaviour
{
    public static MapaControlador Instancia { get; private set; }

    [Header("Escena")]
    public Transform jugador;
    public Light2D luzGlobal;

    [Header("Barra superior")]
    [FormerlySerializedAs("textoSol")] public Text textoNoche;
    public Text textoMisiones;
    public Text textoOxigeno, textoAgua, textoComida, textoEnergia, textoNave, textoMoral;

    [Header("Avisos y pantallas")]
    public Text textoAviso;
    public GameObject panelPantalla;
    public Text textoPantallaTitulo;
    public Text textoPantallaCuerpo;

    [Header("Luz")]
    [Tooltip("Luz mientras juegas la noche")]
    public Color colorNoche = new Color(0.55f, 0.62f, 0.9f);
    [Tooltip("Luz de la pantalla de las 6:00 AM")]
    [FormerlySerializedAs("colorDia")] public Color colorAmanecer = new Color(1f, 0.85f, 0.7f);

    EstacionMision estacionCerca;

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
        PonerLuz(colorNoche, 0.8f);
        ActualizarBarra();

        if (GameState.Oxigeno <= 0) { SceneManager.LoadScene(Escenas.Final); return; }
        if (GameState.DebeAmanecer()) StartCoroutine(Amanecer());
        else if (GameState.MisionesEstaNoche == 0) StartCoroutine(Pantalla("Noche " + GameState.Noche, TextosNoches.Inicio[Mathf.Clamp(GameState.Noche, 0, 7)] + "\n\n" + ResumenMisiones()));
    }

    void Update()
    {
        ActualizarBarra();
    }

    string ResumenMisiones()
    {
        if (GameState.Noche >= GameState.NocheDelLanzamiento) return "Ve al Puente para despegar.";
        var nombres = new List<string>();
        foreach (var id in GameState.MisionesDisponibles()) nombres.Add(Misiones.Nombre[id]);
        return "Misiones de esta noche (elige " + GameState.MisionesPermitidas() + "): " + string.Join(", ", nombres);
    }

    void ActualizarBarra()
    {
        if (textoNoche != null) textoNoche.text = "Noche " + GameState.Noche + " de " + GameState.NocheDelLanzamiento;
        if (textoMisiones != null)
            textoMisiones.text = GameState.Noche >= GameState.NocheDelLanzamiento
                ? "Ve al Puente"
                : "Misiones: " + GameState.MisionesEstaNoche + " de " + GameState.MisionesPermitidas();
        Poner(textoOxigeno, GameState.Oxigeno);
        Poner(textoAgua, GameState.Agua);
        Poner(textoComida, GameState.Comida);
        Poner(textoEnergia, GameState.Energia);
        Poner(textoNave, GameState.Nave);
        Poner(textoMoral, GameState.Moral);
    }

    static void Poner(Text t, int valor)
    {
        if (t == null) return;
        t.text = valor.ToString();
        t.color = valor <= 20 ? new Color(0.9f, 0.28f, 0.3f) : new Color(0.96f, 0.95f, 0.92f);
    }

    public void MostrarAviso(EstacionMision e)
    {
        estacionCerca = e;
        if (textoAviso == null) return;
        if (e.Disponible)
            textoAviso.text = GameState.Noche >= GameState.NocheDelLanzamiento ? "E: Despegar" : "E: " + e.nombreVisible;
        else
            textoAviso.text = string.IsNullOrEmpty(e.idMision) ? e.nombreVisible + " (próximamente)" : e.nombreVisible + ": esta noche no hay misión aquí";
    }

    public void OcultarAviso(EstacionMision e)
    {
        if (estacionCerca != e) return;
        estacionCerca = null;
        if (textoAviso != null) textoAviso.text = "";
    }

    public void Entrar(EstacionMision e)
    {
        if (!e.Disponible) return;
        if (jugador != null) GameState.PosicionJugador = jugador.position;
        if (GameState.Noche >= GameState.NocheDelLanzamiento) { SceneManager.LoadScene(Escenas.Lanzamiento); return; }
        SceneManager.LoadScene(Misiones.Escena[e.idMision]);
    }

    /// <summary>Guiño a FNAF: al terminar las misiones de la noche suenan las 6:00 AM.</summary>
    IEnumerator Amanecer()
    {
        int noche = GameState.Noche;
        PonerLuz(colorAmanecer, 1f);
        yield return Pantalla("6:00 AM", "Sobreviviste la Noche " + noche + ".\n\n" + TextosNoches.Amanecer[Mathf.Clamp(noche, 0, 7)]);
        GameState.PasarAlAmanecer();
        if (GameState.Oxigeno <= 0) { SceneManager.LoadScene(Escenas.Final); yield break; }
        PonerLuz(colorNoche, 0.8f);
        yield return Pantalla("Noche " + GameState.Noche, TextosNoches.Inicio[Mathf.Clamp(GameState.Noche, 0, 7)] + "\n\n" + ResumenMisiones());
    }

    IEnumerator Pantalla(string titulo, string cuerpo)
    {
        JugadorMovimiento.Bloqueado = true;
        if (panelPantalla != null) panelPantalla.SetActive(true);
        if (textoPantallaTitulo != null) textoPantallaTitulo.text = titulo;
        if (textoPantallaCuerpo != null) textoPantallaCuerpo.text = cuerpo + "\n\n(Espacio o clic para continuar)";
        yield return null;
        while (!Teclas.Avanzar()) yield return null;
        if (panelPantalla != null) panelPantalla.SetActive(false);
        yield return null;
        JugadorMovimiento.Bloqueado = false;
    }

    void PonerLuz(Color c, float intensidad)
    {
        if (luzGlobal == null) return;
        luzGlobal.color = c;
        luzGlobal.intensity = intensidad;
    }
}
