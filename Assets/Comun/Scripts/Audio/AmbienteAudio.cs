using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Audio de ambiente (direccion de tono "efecto Undertale"):
/// - Inicio: la narracion se reproduce una vez; el ambiente empieza cuando aparece el jugador en el mapa.
/// - Estaciones: la musica de minijuegos reemplaza temporalmente a la musica principal.
/// - Normal: musica lo-fi y zumbido de ventiladores.
/// - Si apagas ventiladores, el casco cruje cada 10 segundos.
/// - Oxigeno bajo 30 %: la musica baja a 0.75x y suena arrastrada.
/// - En la antena y en la Noche 7 la musica se apaga.
/// Se crea solo al dar Play; no hace falta ponerlo en ninguna escena.
/// </summary>
public class AmbienteAudio : MonoBehaviour
{
    const float VolumenMusica = 0.5f;
    const float VolumenMinijuegos = 0.45f;
    const float VolumenZumbido = 0.12f;
    const float VolumenNarracion = 0.9f;
    const float IntervaloCrujido = 10f;

    public static AmbienteAudio Instancia { get; private set; }

    /// <summary>Lo pone la Antena mientras buscas la senal.</summary>
    public static bool MusicaApagada;

    AudioSource musica, minijuegos, zumbido, efectos, siseo, narracion;
    float proximoCrujido;
    bool ventiladoresDetenidos;
    bool ambienteActivo;
    bool enMinijuego;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    static void Crear()
    {
        MusicaApagada = false;
        if (Instancia != null) return;
        var go = new GameObject("Ambiente (audio)");
        DontDestroyOnLoad(go);
        Instancia = go.AddComponent<AmbienteAudio>();
    }

    void Awake()
    {
        musica = Fuente(SonidoProcedural.Musica(), true, VolumenMusica);
        minijuegos = Fuente(SonidoProcedural.Minijuegos(), true, VolumenMinijuegos);
        zumbido = Fuente(SonidoProcedural.Zumbido(), true, VolumenZumbido);
        siseo = Fuente(SonidoProcedural.Siseo(), true, 0f);
        narracion = Fuente(SonidoProcedural.Narracion(), false, VolumenNarracion);
        efectos = Fuente(null, false, 1f);
    }

    AudioSource Fuente(AudioClip clip, bool loop, float volumen)
    {
        var s = gameObject.AddComponent<AudioSource>();
        s.clip = clip; s.loop = loop; s.volume = volumen; s.playOnAwake = false;
        return s;
    }

    void Update()
    {
        if (GameState.Tripulacion == null) return;
        string escena = SceneManager.GetActiveScene().name;
        bool noche7 = GameState.Noche >= GameState.UltimaNoche && escena == Escenas.Mapa;
        bool silencioTension = !GameState.Ventiladores && !GameState.CalefCamarotes;

        // Se corta en seco, sin fundido: el silencio es el susto.
        musica.volume = ambienteActivo && !enMinijuego && !MusicaApagada && !silencioTension && !noche7 ? VolumenMusica : 0f;
        minijuegos.volume = ambienteActivo && enMinijuego ? VolumenMinijuegos : 0f;
        zumbido.volume = ambienteActivo && !silencioTension ? VolumenZumbido : 0f;
        musica.pitch = GameState.Oxigeno < 30 ? 0.75f : 1f;

        if (ambienteActivo && !GameState.Ventiladores)
        {
            if (!ventiladoresDetenidos)
            {
                ventiladoresDetenidos = true;
                proximoCrujido = Time.time + IntervaloCrujido;
            }
            else if (Time.time >= proximoCrujido)
            {
                efectos.PlayOneShot(SonidoProcedural.Crujido(), 0.7f);
                proximoCrujido = Time.time + IntervaloCrujido;
            }
        }
        else
        {
            ventiladoresDetenidos = false;
        }
    }

    /// <summary>Volumen del siseo de la antena (0 a 1).</summary>
    public static void Siseo(float volumen)
    {
        if (Instancia == null) return;
        if (volumen > 0f) Instancia.ReproducirLoop(Instancia.siseo);
        Instancia.siseo.volume = Mathf.Clamp01(volumen) * 0.6f;
    }

    public static void Efecto(AudioClip clip, float volumen = 1f)
    {
        if (Instancia == null || clip == null) return;
        Instancia.efectos.PlayOneShot(clip, volumen);
    }

    /// <summary>Deja el titulo sin ambiente. La narracion empieza solo al pulsar Jugar.</summary>
    public static void PrepararInicio()
    {
        if (Instancia == null) return;
        Instancia.ambienteActivo = false;
        Instancia.enMinijuego = false;
        Instancia.DetenerLoops();
        if (Instancia.narracion != null) Instancia.narracion.Stop();
    }

    /// <summary>Reproduce la narracion de apertura una vez, mientras se muestran las vinetas.</summary>
    public static void NarrarInicio()
    {
        if (Instancia == null) return;
        Instancia.ambienteActivo = false;
        Instancia.enMinijuego = false;
        Instancia.DetenerLoops();
        if (Instancia.narracion != null)
        {
            Instancia.narracion.Stop();
            Instancia.narracion.Play();
        }
    }

    /// <summary>Activa musica principal y zumbido al llegar al mapa con el personaje.</summary>
    public static void EntrarMapa()
    {
        if (Instancia == null) return;
        Instancia.ambienteActivo = true;
        Instancia.enMinijuego = false;
        if (Instancia.narracion != null) Instancia.narracion.Stop();
        if (Instancia.minijuegos != null) Instancia.minijuegos.Stop();
        Instancia.ReproducirLoop(Instancia.musica);
        Instancia.ReproducirLoop(Instancia.zumbido);
    }

    /// <summary>Reemplaza temporalmente la musica principal por el loop de una estacion.</summary>
    public static void EntrarMinijuego()
    {
        if (Instancia == null) return;
        Instancia.ambienteActivo = true;
        Instancia.enMinijuego = true;
        if (Instancia.narracion != null) Instancia.narracion.Stop();
        if (Instancia.musica != null) Instancia.musica.Stop();
        Instancia.ReproducirLoop(Instancia.minijuegos);
        Instancia.ReproducirLoop(Instancia.zumbido);
    }

    void ReproducirLoop(AudioSource fuente)
    {
        if (fuente != null && fuente.clip != null && !fuente.isPlaying) fuente.Play();
    }

    void DetenerLoops()
    {
        if (musica != null) musica.Stop();
        if (minijuegos != null) minijuegos.Stop();
        if (zumbido != null) zumbido.Stop();
        if (siseo != null) siseo.Stop();
    }
}
