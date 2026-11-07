using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Audio de ambiente (direccion de tono "efecto Undertale"):
/// - Normal: musica lo-fi y zumbido de ventiladores.
/// - Si apagas ventiladores Y calefaccion de los camarotes: silencio total, solo crujidos del casco.
/// - Oxigeno bajo 30 %: la musica baja a 0.75x y suena arrastrada.
/// - En la antena y en la Noche 7 la musica se apaga.
/// Se crea solo al dar Play; no hace falta ponerlo en ninguna escena.
/// </summary>
public class AmbienteAudio : MonoBehaviour
{
    public static AmbienteAudio Instancia { get; private set; }

    /// <summary>Lo pone la Antena mientras buscas la senal.</summary>
    public static bool MusicaApagada;

    AudioSource musica, zumbido, efectos, siseo;
    float proximoCrujido;

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
        musica = Fuente(SonidoProcedural.Musica(), true, 0.5f);
        zumbido = Fuente(SonidoProcedural.Zumbido(), true, 0.25f);
        siseo = Fuente(SonidoProcedural.Siseo(), true, 0f);
        efectos = Fuente(null, false, 1f);
    }

    AudioSource Fuente(AudioClip clip, bool loop, float volumen)
    {
        var s = gameObject.AddComponent<AudioSource>();
        s.clip = clip; s.loop = loop; s.volume = volumen; s.playOnAwake = false;
        if (clip != null && loop) s.Play();
        return s;
    }

    void Update()
    {
        if (GameState.Tripulacion == null) return;
        string escena = SceneManager.GetActiveScene().name;
        bool noche7 = GameState.Noche >= GameState.UltimaNoche && escena == Escenas.Mapa;
        bool silencioTension = !GameState.Ventiladores && !GameState.CalefCamarotes;

        // Se corta en seco, sin fundido: el silencio es el susto.
        musica.volume = (MusicaApagada || silencioTension || noche7) ? 0f : 0.5f;
        zumbido.volume = silencioTension ? 0f : 0.25f;
        musica.pitch = GameState.Oxigeno < 30 ? 0.75f : 1f;

        if (silencioTension && Time.time > proximoCrujido)
        {
            efectos.PlayOneShot(SonidoProcedural.Crujido(), 0.7f);
            proximoCrujido = Time.time + Random.Range(5f, 11f);
        }
    }

    /// <summary>Volumen del siseo de la antena (0 a 1).</summary>
    public static void Siseo(float volumen)
    {
        if (Instancia == null) return;
        Instancia.siseo.volume = volumen * 0.6f;
    }

    public static void Efecto(AudioClip clip, float volumen = 1f)
    {
        if (Instancia == null || clip == null) return;
        Instancia.efectos.PlayOneShot(clip, volumen);
    }
}
