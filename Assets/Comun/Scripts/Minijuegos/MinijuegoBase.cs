using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

/// <summary>
/// Base para TODOS los minijuegos. Cumple el contrato del documento tecnico:
/// 1) arranca solo, 2) aplica su resultado una sola vez, 3) muestra la tarjeta de dato real,
/// 4) vuelve al mapa con GameState.TerminarMinijuego.
/// Para hacer un minijuego nuevo: heredar de esta clase y llamar a Terminar(resultado, mensaje).
/// </summary>
public abstract class MinijuegoBase : MonoBehaviour
{
    [Tooltip("Id de Misiones: respira, tormenta, huerto, refugio, insulta o roca")]
    public string idMision;

    [Header("Tarjeta de dato real (se muestra al terminar)")]
    public GameObject panelTarjeta;
    public Text textoResultado;
    public Text textoDato;
    public Text textoFuente;

    protected bool Terminado { get; private set; }
    Resultado resultado;
    string resumen;
    bool esDecisionRoca;
    DecisionRoca decision;

    protected int Dificultad => GameState.Dificultad(idMision);

    protected virtual void Awake()
    {
        if (panelTarjeta != null) panelTarjeta.SetActive(false);
    }

    /// <summary>Termina el minijuego normal (gano, a medias o perdio).</summary>
    protected void Terminar(Resultado r, string mensaje)
    {
        if (Terminado) return;
        Terminado = true;
        resultado = r;
        string palabra = r == Resultado.Gano ? "¡Lo lograste!" : r == Resultado.AMedias ? "A medias" : "Fallaste";
        resumen = Misiones.Nombre[idMision] + ": " + palabra;
        MostrarTarjeta(palabra + "\n" + mensaje);
    }

    /// <summary>Solo para la roca: es una decision, no se gana ni se pierde.</summary>
    protected void TerminarRoca(DecisionRoca d, string mensaje)
    {
        if (Terminado) return;
        Terminado = true;
        esDecisionRoca = true;
        decision = d;
        resumen = Misiones.Nombre[Misiones.Roca] + ": " + mensaje;
        MostrarTarjeta(mensaje);
    }

    void MostrarTarjeta(string titulo)
    {
        DatosReales.PorMision.TryGetValue(idMision, out var tarjeta);
        if (textoResultado != null) textoResultado.text = titulo;
        if (textoDato != null) textoDato.text = "Dato real: " + tarjeta.Texto;
        if (textoFuente != null) textoFuente.text = "Fuente: " + tarjeta.Fuente;
        if (panelTarjeta != null) panelTarjeta.SetActive(true);
        else Continuar();
    }

    /// <summary>Lo llama el boton "Continuar" de la tarjeta.</summary>
    public void Continuar()
    {
        if (!Terminado) return;
        if (esDecisionRoca) Efectos.DecidirRoca(decision);
        else Efectos.Aplicar(idMision, resultado);
        GameState.TerminarMinijuego(idMision, resumen);
    }
}
