using UnityEngine;

/// <summary>
/// Un lugar del mapa donde se entra a un minijuego. Se pone sobre el sprite de la estacion.
/// Necesita un Collider2D marcado como "Is Trigger" para detectar al jugador cerca.
/// </summary>
public class EstacionMision : MonoBehaviour
{
    [Tooltip("Id de Misiones (respira, tormenta...). Vacio = lugar extra que todavia no tiene minijuego.")]
    public string idMision;
    public string nombreVisible = "Lugar";
    [Tooltip("Marcar solo en el Puente: la noche 7 desde aqui se despega")]
    public bool esPuente;
    [Tooltip("Flecha o brillo que indica que esta noche hay mision aqui")]
    public SpriteRenderer indicador;

    bool jugadorCerca;

    public bool Disponible
    {
        get
        {
            if (GameState.Noche >= GameState.NocheDelLanzamiento) return esPuente;
            if (string.IsNullOrEmpty(idMision)) return false;
            return GameState.MisionesDisponibles().Contains(idMision);
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
