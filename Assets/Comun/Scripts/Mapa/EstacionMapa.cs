using UnityEngine;

/// <summary>
/// Un lugar del mapa. Se pone sobre el sprite de la estacion.
/// Necesita un Collider2D marcado como "Is Trigger" para detectar al jugador cerca.
/// </summary>
public class EstacionMapa : MonoBehaviour
{
    [Tooltip("Id de Estaciones (invernadero, soporte, biomedica, antena, camarotes, puente). Vacio = decoracion.")]
    public string idEstacion;
    public string nombreVisible = "Lugar";
    [Tooltip("Flecha que indica que se puede entrar")]
    public SpriteRenderer indicador;

    bool jugadorCerca;

    public bool Disponible => !string.IsNullOrEmpty(idEstacion) && GameState.EstacionDisponible(idEstacion);

    public string Motivo
    {
        get
        {
            if (string.IsNullOrEmpty(idEstacion)) return "";
            GameState.EstacionDisponible(idEstacion, out var m);
            return m;
        }
    }

    void Update()
    {
        if (indicador != null)
        {
            indicador.enabled = Disponible;
            indicador.transform.localPosition = new Vector3(0f, 2.2f + Mathf.Round(Mathf.Sin(Time.time * 4f) * 3f) / 32f, 0f);
        }
        if (jugadorCerca && !JugadorMovimiento.Bloqueado && Teclas.Interactuar())
            MapaControlador.Instancia?.Entrar(this);
    }

    void OnTriggerEnter2D(Collider2D otro)
    {
        if (otro.GetComponent<JugadorMovimiento>() == null) return;
        jugadorCerca = true;
        MapaControlador.Instancia?.MostrarAviso(this);
    }

    void OnTriggerExit2D(Collider2D otro)
    {
        if (otro.GetComponent<JugadorMovimiento>() == null) return;
        jugadorCerca = false;
        MapaControlador.Instancia?.OcultarAviso(this);
    }
}
