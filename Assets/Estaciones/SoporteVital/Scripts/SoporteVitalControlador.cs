using System.Text;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

/// <summary>
/// Estacion 2: Soporte Vital. Izquierda: reactor MOXIE (una vez por noche). Derecha: a que sector le das calor
/// y ventilacion. Apagar cosas ahorra bateria, pero congela plantas y deja CO2 en los camarotes.
/// Controles: mantener ESPACIO (o el boton) para calentar, P para purgar.
/// </summary>
public class SoporteVitalControlador : EstacionBase
{
    [Header("Reactor MOXIE")]
    public RectTransform rellenoTemperatura;
    public RectTransform zonaProduccion;
    public RectTransform zonaFusion;
    public RectTransform rellenoProduccion;
    public RectTransform rellenoHollin;
    public Text textoTemperatura;
    public Text textoEstado;
    public Text textoTiempo;
    public BotonMantener botonCalentar;
    public GameObject grupoControles;

    [Header("Sectores: 0 calefaccion invernadero, 1 calefaccion camarotes, 2 ventiladores")]
    public Image[] botonesSector;
    public Text[] textosSector;
    public Text textoCargas;

    MoxieLogica logica;
    bool empezo;

    static readonly Color Encendido = new Color(0.3f, 0.69f, 0.48f), Apagado = new Color(0.35f, 0.18f, 0.2f), Bloqueado = new Color(0.25f, 0.27f, 0.33f);

    public MoxieLogica Logica => logica;

    protected override void Awake()
    {
        base.Awake();
        if (string.IsNullOrEmpty(idEstacion)) idEstacion = Estaciones.SoporteVital;
    }

    void Start()
    {
        if (!GameState.MoxieUsado)
        {
            bool marcoVerde = GameState.Trip(TripId.Marco).Estado == EstadoTrip.Verde;
            logica = new MoxieLogica(GameState.Noche, GameState.CodigoReactor, marcoVerde, GameState.TomaCongelada,
                                     GameState.HollinInicial, System.Environment.TickCount);
        }
        Franja(zonaProduccion, MoxieLogica.Normalizar(MoxieLogica.ZonaMin), MoxieLogica.Normalizar(MoxieLogica.ZonaMax));
        Franja(zonaFusion, MoxieLogica.Normalizar(MoxieLogica.Fusion), 1f);
        MostrarTutorial();
    }

    protected override void Update()
    {
        base.Update();
        if (logica != null && !logica.Terminado && !Saliendo && !Tutorial.Abierto)
        {
            bool calentando = Teclas.Mantiene(Key.Space) || (botonCalentar != null && botonCalentar.Presionado);
            if (Teclas.Pulsa(Key.P)) Purgar();
            if (calentando) empezo = true;
            if (empezo)
            {
                GameState.MoxieUsado = true;
                logica.Avanzar(Time.deltaTime, calentando);
                if (logica.Terminado) Guardar();
            }
        }
        DibujarReactor();
        DibujarSectores();
    }

    // ------------------------------------------------------------------ Acciones

    public void Purgar()
    {
        if (logica == null || logica.Terminado) return;
        logica.Purgar();
    }

    public void ApagarReactor()
    {
        if (logica == null || logica.Terminado) return;
        logica.Apagar();
        Guardar();
    }

    /// <summary>Solo para la prueba automatica: deja el reactor con esa produccion y lo apaga.</summary>
    public void ForzarProduccion(float produccion)
    {
        if (logica == null || logica.Terminado) return;
        empezo = true;
        logica.Produccion = Mathf.Clamp01(produccion);
        ApagarReactor();
    }

    /// <summary>0 calefaccion invernadero, 1 calefaccion camarotes, 2 ventiladores.</summary>
    public void AlternarSector(int i)
    {
        if (Bloqueo(i)) return;
        if (i == 0) GameState.CalefInvernadero = !GameState.CalefInvernadero;
        else if (i == 1) GameState.CalefCamarotes = !GameState.CalefCamarotes;
        else GameState.Ventiladores = !GameState.Ventiladores;
    }

    void Guardar()
    {
        if (!empezo) return;
        GameState.MoxieUsado = true;
        GameState.MoxieProduccion = logica.Fundido ? 0f : logica.Produccion;
        if (logica.Fundido) GameState.MoxieDanado = true;
    }

    protected override string ResumenSalida()
    {
        if (logica != null && !logica.Terminado && empezo) { logica.Apagar(); Guardar(); }
        string moxie = GameState.MoxieDanado ? "MOXIE melted" : GameState.MoxieUsado ? "MOXIE " + Mathf.RoundToInt(GameState.MoxieProduccion * 100f) + "%" : "MOXIE off";
        return "Life Support: " + moxie + ", greenhouse heat " + (GameState.CalefInvernadero ? "on" : "off") +
               ", quarters heat " + (GameState.CalefCamarotes ? "on" : "off") + ", fans " + (GameState.Ventiladores ? "on" : "off");
    }

    // ------------------------------------------------------------------ Dibujo

    static bool Bloqueo(int i)
    {
        if (i == 0) return GameState.SectorSinCalor == Estaciones.Invernadero;
        if (i == 1) return GameState.SectorSinCalor == Estaciones.Camarotes;
        return false;
    }

    void DibujarReactor()
    {
        if (logica == null)
        {
            if (textoTemperatura != null) textoTemperatura.text = "—";
            if (textoEstado != null) textoEstado.text = GameState.MoxieDanado
                ? "The cells melted tonight."
                : "MOXIE already ran tonight (" + Mathf.RoundToInt(GameState.MoxieProduccion * 100f) + "%). You can change sectors here.";
            if (grupoControles != null) grupoControles.SetActive(false);
            Barra(rellenoProduccion, GameState.MoxieProduccion, false);
            return;
        }
        Barra(rellenoTemperatura, MoxieLogica.Normalizar(logica.Temperatura), true);
        Barra(rellenoProduccion, logica.Produccion, false);
        Barra(rellenoHollin, logica.Hollin, false);
        if (textoTemperatura != null)
        {
            textoTemperatura.text = Mathf.RoundToInt(logica.Temperatura) + " °C";
            textoTemperatura.color = logica.Temperatura >= MoxieLogica.Fusion ? new Color(0.9f, 0.28f, 0.3f) : logica.EnZona ? new Color(0.3f, 0.69f, 0.48f) : new Color(0.96f, 0.95f, 0.92f);
        }
        if (textoTiempo != null) textoTiempo.text = Mathf.CeilToInt(MoxieLogica.Duracion - logica.Tiempo) + " s";
        if (grupoControles != null) grupoControles.SetActive(!logica.Terminado);
        if (textoEstado == null) return;
        if (logica.Fundido) textoEstado.text = "Cells melted! MOXIE will not provide oxygen tonight.";
        else if (logica.Terminado) textoEstado.text = "Reactor off. Production: " + Mathf.RoundToInt(logica.Produccion * 100f) + "% (+" + Mathf.RoundToInt(SimulacionNoche.O2Moxie * logica.Produccion) + "% O₂ at dawn).";
        else if (!empezo) textoEstado.text = GameState.TomaCongelada
            ? "The intakes have dry ice: it produces at half speed. Hold SPACE to heat."
            : "Hold SPACE to heat into the green band (780-830 °C).";
        else if (logica.Temperatura >= MoxieLogica.Fusion) textoEstado.text = "Too hot! Release SPACE.";
        else if (logica.Tapado) textoEstado.text = "Filters clogged with soot: press PURGE (P).";
        else if (logica.Produciendo) textoEstado.text = "Splitting CO₂ into O₂...";
        else textoEstado.text = logica.Temperatura < MoxieLogica.ZonaMin ? "Cold: keep heating." : "You passed the band: release and let it cool.";
    }

    void DibujarSectores()
    {
        bool[] estado = { GameState.CalefInvernadero, GameState.CalefCamarotes, GameState.Ventiladores };
        string[] nombres = { "Greenhouse heating", "Crew quarters heating", "Crew quarters fans" };
        int[] costos = { SimulacionNoche.CostoCalefaccion, SimulacionNoche.CostoCalefaccion, SimulacionNoche.CostoVentiladores };
        for (int i = 0; i < 3; i++)
        {
            bool bloqueado = Bloqueo(i);
            if (i < botonesSector.Length && botonesSector[i] != null)
                botonesSector[i].color = bloqueado ? Bloqueado : estado[i] ? Encendido : Apagado;
            if (i < textosSector.Length && textosSector[i] != null)
                textosSector[i].text = nombres[i] + ": " + (bloqueado ? "NO HEAT (intakes)" : estado[i] ? "ON · " + costos[i] + "%" : "OFF");
        }
        if (textoCargas == null) return;
        var sb = new StringBuilder("Tonight's power use:\n");
        foreach (var c in SimulacionNoche.Cargas()) sb.Append("  ").Append(c.Nombre).Append(": ").Append(c.Costo).Append(" %\n");
        int total = SimulacionNoche.ConsumoProyectado();
        sb.Append("TOTAL: ").Append(total).Append("% of ").Append(GameState.Bateria).Append("%");
        if (total > GameState.Bateria) sb.Append("\n<color=#E5484D>Not enough! There will be a blackout at 03:00.</color>");
        if (!GameState.Ventiladores) sb.Append("\nWithout fans, CO₂ builds up in the crew quarters.");
        if (!GameState.CalefInvernadero) sb.Append("\nWithout heating, plants at 60% humidity or higher freeze.");
        textoCargas.text = sb.ToString();
    }
}
