using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.Universal;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>
/// Controla el mapa: barra de recursos, avisos de estaciones, la noche y el paso de soles.
/// </summary>
public class MapaControlador : MonoBehaviour
{
    public static MapaControlador Instancia { get; private set; }

    [Header("Escena")]
    public Transform jugador;
    public Light2D luzGlobal;

    [Header("Barra superior")]
    public Text textoSol;
    public Text textoMisiones;
    public Text textoOxigeno, textoAgua, textoComida, textoEnergia, textoNave, textoMoral;

    [Header("Avisos y pantallas")]
    public Text textoAviso;
    public GameObject panelPantalla;
    public Text textoPantallaTitulo;
    public Text textoPantallaCuerpo;

    [Header("Luz")]
    public Color colorDia = new Color(1f, 0.93f, 0.85f);
    public Color colorNoche = new Color(0.35f, 0.45f, 0.8f);

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
        PonerLuz(colorDia, 1f);
        ActualizarBarra();

        if (GameState.Oxigeno <= 0) { SceneManager.LoadScene(Escenas.Final); return; }
        if (GameState.DebeLlegarLaNoche()) StartCoroutine(Noche());
        else if (GameState.MisionesHoy == 0) StartCoroutine(Pantalla("Sol " + GameState.Sol, TextosSoles.Dia[Mathf.Clamp(GameState.Sol, 0, 7)] + "\n\n" + ResumenMisiones()));
    }

    void Update()
    {
        ActualizarBarra();
    }

    string ResumenMisiones()
    {
        if (GameState.Sol >= GameState.SolDelLanzamiento) return "Ve al Puente para despegar.";
        var nombres = new List<string>();
        foreach (var id in GameState.MisionesDisponibles()) nombres.Add(Misiones.Nombre[id]);
        return "Misiones de hoy (elige " + GameState.MisionesPermitidas() + "): " + string.Join(", ", nombres);
    }

    void ActualizarBarra()
    {
        if (textoSol != null) textoSol.text = "Sol " + GameState.Sol + " de " + GameState.SolDelLanzamiento;
        if (textoMisiones != null)
            textoMisiones.text = GameState.Sol >= GameState.SolDelLanzamiento
                ? "Ve al Puente"
                : "Misiones: " + GameState.MisionesHoy + " de " + GameState.MisionesPermitidas();
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
            textoAviso.text = GameState.Sol >= GameState.SolDelLanzamiento ? "E: Despegar" : "E: " + e.nombreVisible;
        else
            textoAviso.text = string.IsNullOrEmpty(e.idMision) ? e.nombreVisible + " (próximamente)" : e.nombreVisible + ": hoy no hay misión aquí";
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
        if (GameState.Sol >= GameState.SolDelLanzamiento) { SceneManager.LoadScene(Escenas.Lanzamiento); return; }
        SceneManager.LoadScene(Misiones.Escena[e.idMision]);
    }

    IEnumerator Noche()
    {
        int sol = GameState.Sol;
        PonerLuz(colorNoche, 0.45f);
        yield return Pantalla("Noche " + sol, TextosSoles.Noche[Mathf.Clamp(sol, 0, 7)]);
        GameState.PasarNoche();
        if (GameState.Oxigeno <= 0) { SceneManager.LoadScene(Escenas.Final); yield break; }
        PonerLuz(colorDia, 1f);
        yield return Pantalla("Sol " + GameState.Sol, TextosSoles.Dia[Mathf.Clamp(GameState.Sol, 0, 7)] + "\n\n" + ResumenMisiones());
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
