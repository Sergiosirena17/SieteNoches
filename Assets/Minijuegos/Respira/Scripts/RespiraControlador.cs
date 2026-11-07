using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

/// <summary>
/// Pantalla de "Respira, Marte". Controles: mantener ESPACIO (o el boton Calentar) y pulsar E (o el boton Bombear).
/// Las barras se dibujan cambiando el tamano de una imagen (anchorMax), asi el artista puede cambiar los sprites sin tocar codigo.
/// </summary>
public class RespiraControlador : MinijuegoBase
{
    [Header("Medidores (relleno vertical)")]
    public RectTransform rellenoTemperatura;
    public RectTransform zonaTemperatura;
    public RectTransform rellenoPresion;
    public RectTransform zonaPresion;
    [Header("Barra de oxigeno (relleno horizontal)")]
    public RectTransform rellenoOxigeno;
    [Header("Textos")]
    public Text textoTiempo;
    public Text textoEstado;
    [Header("Botones")]
    public BotonMantener botonCalentar;

    RespiraLogica logica;
    bool empezo;

    protected override void Awake()
    {
        base.Awake();
        if (string.IsNullOrEmpty(idMision)) idMision = Misiones.Respira;
    }

    void Start()
    {
        logica = new RespiraLogica(Dificultad, System.Environment.TickCount);
        Ajustar(zonaTemperatura, logica.ZonaTMin, logica.ZonaTMax, true);
        Ajustar(zonaPresion, logica.ZonaPMin, logica.ZonaPMax, true);
        if (textoEstado != null) textoEstado.text = "Pulsa E o Bombear para empezar";
    }

    void Update()
    {
        if (Terminado || logica == null) return;
        bool bombear = Teclas.Pulsa(Key.E);
        if (!empezo)
        {
            if (!bombear && !Teclas.Mantiene(Key.Space) && (botonCalentar == null || !botonCalentar.Presionado)) { Dibujar(); return; }
            empezo = true;
        }
        if (bombear) logica.Bombear();
        bool calentando = Teclas.Mantiene(Key.Space) || (botonCalentar != null && botonCalentar.Presionado);
        logica.Avanzar(Time.deltaTime, calentando);
        Dibujar();

        if (logica.Terminado)
        {
            string msg = logica.Quemada ? "La máquina se sobrecalentó."
                : "Oxígeno producido: " + Mathf.RoundToInt(logica.Oxigeno * 100f) + "%";
            Terminar(logica.ResultadoFinal, msg);
        }
    }

    /// <summary>Lo llama el boton Bombear.</summary>
    public void BotonBombear()
    {
        if (logica == null || Terminado) return;
        empezo = true;
        logica.Bombear();
    }

    void Dibujar()
    {
        Ajustar(rellenoTemperatura, 0f, logica.Temperatura, true);
        Ajustar(rellenoPresion, 0f, logica.Presion, true);
        Ajustar(rellenoOxigeno, 0f, logica.Oxigeno, false);
        if (textoTiempo != null) textoTiempo.text = Mathf.CeilToInt(RespiraLogica.Duracion - logica.Tiempo) + " s";
        if (!empezo || textoEstado == null) return;
        if (logica.Temperatura >= 0.92f) textoEstado.text = "¡Muy caliente! Suelta ESPACIO";
        else if (logica.Produciendo) textoEstado.text = "Produciendo oxígeno...";
        else if (!logica.TemperaturaBien) textoEstado.text = logica.Temperatura < logica.ZonaTMin ? "Temperatura baja: mantén ESPACIO" : "Temperatura alta: suelta ESPACIO";
        else textoEstado.text = logica.Presion < logica.ZonaPMin ? "Presión baja: pulsa E" : "Presión alta: espera";
    }

    static void Ajustar(RectTransform r, float desde, float hasta, bool vertical)
    {
        if (r == null) return;
        if (vertical) { r.anchorMin = new Vector2(0f, desde); r.anchorMax = new Vector2(1f, hasta); }
        else { r.anchorMin = new Vector2(desde, 0f); r.anchorMax = new Vector2(hasta, 1f); }
        r.offsetMin = Vector2.zero; r.offsetMax = Vector2.zero;
    }
}
