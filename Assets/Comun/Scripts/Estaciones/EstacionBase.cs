using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Base para las 4 estaciones (Invernadero, Soporte Vital, Biomédica, Antena).
/// Contrato: la estacion cambia GameState mientras el jugador la usa y, al salir,
/// muestra la tarjeta de dato real y vuelve al mapa con GameState.TerminarEstacion (el reloj avanza 2 h 30).
/// </summary>
public abstract class EstacionBase : MonoBehaviour
{
    [Tooltip("Id de Estaciones: invernadero, soporte, biomedica o antena")]
    public string idEstacion;

    [Header("Barra superior")]
    public Text textoHora;
    public Text textoBateria;

    [Header("Tarjeta de dato real (se muestra al salir)")]
    public GameObject panelTarjeta;
    public Text textoResultado;
    public Text textoDato;
    public Text textoFuente;

    protected bool Saliendo { get; private set; }
    string resumen;

    static readonly Color ColorBien = new Color(0.96f, 0.95f, 0.92f), ColorMal = new Color(0.9f, 0.28f, 0.3f);

    protected virtual void Awake()
    {
        if (panelTarjeta != null) panelTarjeta.SetActive(false);
    }

    protected virtual void Update()
    {
        ActualizarBarra();
    }

    protected void ActualizarBarra()
    {
        if (textoHora != null) textoHora.text = "Noche " + GameState.Noche + " · " + GameState.Hora;
        if (textoBateria != null)
        {
            int consumo = SimulacionNoche.ConsumoProyectado();
            textoBateria.text = "Batería: gasto " + consumo + " % de " + GameState.Bateria + " %";
            textoBateria.color = consumo > GameState.Bateria ? ColorMal : ColorBien;
        }
    }

    /// <summary>Texto corto para el informe del final. Cada estacion dice que hizo.</summary>
    protected virtual string ResumenSalida() => Estaciones.Nombre[idEstacion];

    /// <summary>Lo llama el boton "Salir".</summary>
    public void Salir()
    {
        if (Saliendo) return;
        Saliendo = true;
        resumen = ResumenSalida();
        DatosReales.PorEstacion.TryGetValue(idEstacion, out var tarjeta);
        if (textoResultado != null) textoResultado.text = resumen;
        if (textoDato != null) textoDato.text = "Dato real: " + tarjeta.Texto;
        if (textoFuente != null) textoFuente.text = "Fuente: " + tarjeta.Fuente;
        if (panelTarjeta != null) panelTarjeta.SetActive(true);
        else Continuar();
    }

    /// <summary>Lo llama el boton "Continuar" de la tarjeta.</summary>
    public void Continuar()
    {
        if (!Saliendo) return;
        GameState.TerminarEstacion(idEstacion, resumen);
    }

    protected static void Barra(RectTransform r, float valor, bool vertical)
    {
        if (r == null) return;
        valor = Mathf.Clamp01(valor);
        r.anchorMin = Vector2.zero;
        r.anchorMax = vertical ? new Vector2(1f, valor) : new Vector2(valor, 1f);
        r.offsetMin = r.offsetMax = Vector2.zero;
    }

    protected static void Franja(RectTransform r, float desde, float hasta)
    {
        if (r == null) return;
        r.anchorMin = new Vector2(0f, Mathf.Clamp01(desde));
        r.anchorMax = new Vector2(1f, Mathf.Clamp01(hasta));
        r.offsetMin = r.offsetMax = Vector2.zero;
    }
}
