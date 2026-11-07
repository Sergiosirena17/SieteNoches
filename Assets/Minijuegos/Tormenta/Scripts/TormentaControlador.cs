using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

/// <summary>
/// Pantalla de "Tormenta de polvo". Controles: clic en un panel (o teclas 1 a 6) para limpiarlo.
/// Cada panel es un boton; su capa de polvo es una imagen cuya transparencia muestra la suciedad.
/// </summary>
public class TormentaControlador : MinijuegoBase
{
    [Header("Paneles (en orden 1 a 6)")]
    public Image[] capasPolvo = new Image[TormentaLogica.CantidadPaneles];
    [Header("Barra de energia (relleno horizontal)")]
    public RectTransform rellenoEnergia;
    [Header("Oscuridad de la tormenta (imagen negra encima del fondo)")]
    public Image oscuridad;
    [Header("Textos")]
    public Text textoTiempo;
    public Text textoEstado;

    TormentaLogica logica;
    bool empezo;
    float avisoRafaga;

    static readonly Key[] teclas = { Key.Digit1, Key.Digit2, Key.Digit3, Key.Digit4, Key.Digit5, Key.Digit6 };

    protected override void Awake()
    {
        base.Awake();
        if (string.IsNullOrEmpty(idMision)) idMision = Misiones.Tormenta;
    }

    void Start()
    {
        logica = new TormentaLogica(Dificultad, System.Environment.TickCount);
        if (textoEstado != null) textoEstado.text = "Limpia un panel para empezar";
        Dibujar();
    }

    void Update()
    {
        if (Terminado || logica == null) return;
        for (int i = 0; i < teclas.Length; i++) if (Teclas.Pulsa(teclas[i])) LimpiarPanel(i);
        if (!empezo) return;

        if (logica.Avanzar(Time.deltaTime)) avisoRafaga = 1.5f;
        avisoRafaga -= Time.deltaTime;
        Dibujar();

        if (logica.Terminado)
            Terminar(logica.ResultadoFinal, "Energía guardada: " + Mathf.RoundToInt(logica.Energia * 100f) + "%");
    }

    /// <summary>Lo llama cada boton de panel con su numero (0 a 5).</summary>
    public void LimpiarPanel(int indice)
    {
        if (logica == null || Terminado) return;
        empezo = true;
        logica.Limpiar(indice);
        Dibujar();
    }

    void Dibujar()
    {
        for (int i = 0; i < capasPolvo.Length && i < TormentaLogica.CantidadPaneles; i++)
        {
            if (capasPolvo[i] == null) continue;
            var c = capasPolvo[i].color; c.a = logica.Polvo[i]; capasPolvo[i].color = c;
        }
        if (rellenoEnergia != null)
        {
            rellenoEnergia.anchorMin = Vector2.zero; rellenoEnergia.anchorMax = new Vector2(logica.Energia, 1f);
            rellenoEnergia.offsetMin = Vector2.zero; rellenoEnergia.offsetMax = Vector2.zero;
        }
        if (oscuridad != null) { var c = oscuridad.color; c.a = (1f - logica.Luz) * 0.75f; oscuridad.color = c; }
        if (textoTiempo != null) textoTiempo.text = Mathf.CeilToInt(TormentaLogica.Duracion - logica.Tiempo) + " s";
        if (textoEstado != null && empezo)
            textoEstado.text = avisoRafaga > 0f ? "¡Ráfaga de viento!" : logica.Luz < 0.4f ? "Se va la luz..." : "Mantén los paneles limpios";
    }
}
