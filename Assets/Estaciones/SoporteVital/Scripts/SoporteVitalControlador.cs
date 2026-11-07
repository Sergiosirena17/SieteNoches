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
    }

    protected override void Update()
    {
        base.Update();
        if (logica != null && !logica.Terminado && !Saliendo)
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
        string moxie = GameState.MoxieDanado ? "MOXIE fundido" : GameState.MoxieUsado ? "MOXIE " + Mathf.RoundToInt(GameState.MoxieProduccion * 100f) + " %" : "MOXIE apagado";
        return "Soporte vital: " + moxie + ", calefacción inv. " + (GameState.CalefInvernadero ? "sí" : "no") +
               ", camarotes " + (GameState.CalefCamarotes ? "sí" : "no") + ", ventiladores " + (GameState.Ventiladores ? "sí" : "no");
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
                ? "Las celdas se fundieron esta noche."
                : "El MOXIE ya trabajó esta noche (" + Mathf.RoundToInt(GameState.MoxieProduccion * 100f) + " %). Aquí puedes cambiar los sectores.";
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
        if (logica.Fundido) textoEstado.text = "¡Celdas fundidas! El MOXIE no dará oxígeno esta noche.";
        else if (logica.Terminado) textoEstado.text = "Reactor apagado. Producción: " + Mathf.RoundToInt(logica.Produccion * 100f) + " % (+" + Mathf.RoundToInt(SimulacionNoche.O2Moxie * logica.Produccion) + " % O₂ al amanecer).";
        else if (!empezo) textoEstado.text = GameState.TomaCongelada
            ? "Las tomas tienen hielo seco: produce a mitad de ritmo. Mantén ESPACIO para calentar."
            : "Mantén ESPACIO para calentar hasta la franja verde (780-830 °C).";
        else if (logica.Temperatura >= MoxieLogica.Fusion) textoEstado.text = "¡Demasiado caliente! Suelta ESPACIO.";
        else if (logica.Tapado) textoEstado.text = "Filtros tapados de hollín: pulsa PURGAR (P).";
        else if (logica.Produciendo) textoEstado.text = "Rompiendo CO₂ en O₂...";
        else textoEstado.text = logica.Temperatura < MoxieLogica.ZonaMin ? "Frío: sigue calentando." : "Pasaste la franja: suelta y deja enfriar.";
    }

    void DibujarSectores()
    {
        bool[] estado = { GameState.CalefInvernadero, GameState.CalefCamarotes, GameState.Ventiladores };
        string[] nombres = { "Calefacción del invernadero", "Calefacción de los camarotes", "Ventiladores de los camarotes" };
        int[] costos = { SimulacionNoche.CostoCalefaccion, SimulacionNoche.CostoCalefaccion, SimulacionNoche.CostoVentiladores };
        for (int i = 0; i < 3; i++)
        {
            bool bloqueado = Bloqueo(i);
            if (i < botonesSector.Length && botonesSector[i] != null)
                botonesSector[i].color = bloqueado ? Bloqueado : estado[i] ? Encendido : Apagado;
            if (i < textosSector.Length && textosSector[i] != null)
                textosSector[i].text = nombres[i] + ": " + (bloqueado ? "SIN CALOR (tomas)" : estado[i] ? "ENCENDIDA · " + costos[i] + " %" : "APAGADA");
        }
        if (textoCargas == null) return;
        var sb = new StringBuilder("Consumo de esta noche:\n");
        foreach (var c in SimulacionNoche.Cargas()) sb.Append("  ").Append(c.Nombre).Append(": ").Append(c.Costo).Append(" %\n");
        int total = SimulacionNoche.ConsumoProyectado();
        sb.Append("TOTAL: ").Append(total).Append(" % de ").Append(GameState.Bateria).Append(" %");
        if (total > GameState.Bateria) sb.Append("\n<color=#E5484D>¡No alcanza! A las 03:00 habrá apagón.</color>");
        if (!GameState.Ventiladores) sb.Append("\nSin ventiladores el CO₂ se acumula en los camarotes.");
        if (!GameState.CalefInvernadero) sb.Append("\nSin calefacción, las plantas con 60 % de humedad o más se congelan.");
        textoCargas.text = sb.ToString();
    }
}
