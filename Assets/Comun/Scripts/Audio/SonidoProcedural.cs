using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Sonidos de BOCETO generados por codigo, para no depender de archivos de audio todavia.
/// Para poner el audio real: guardar un archivo con el mismo nombre en Assets/Resources/Audio/
/// (musica, zumbido, crujido, siseo, pitido). Si existe, se usa ese en lugar del generado.
/// </summary>
public static class SonidoProcedural
{
    const int Frec = 22050;
    static readonly Dictionary<string, AudioClip> cache = new Dictionary<string, AudioClip>();

    public static AudioClip Musica() => Obtener("musica", GenerarMusica);
    public static AudioClip Zumbido() => Obtener("zumbido", GenerarZumbido);
    public static AudioClip Crujido() => Obtener("crujido", GenerarCrujido);
    public static AudioClip Siseo() => Obtener("siseo", GenerarSiseo);
    public static AudioClip Pitido() => Obtener("pitido", () => Tono("pitido", 1000f, 0.07f, 0.25f, false));

    /// <summary>Sonido corto de voz (voice blip) para los dialogos. Cada personaje tiene su tono.</summary>
    public static AudioClip Blip(float hz) => Obtener("blip" + Mathf.RoundToInt(hz), () => Tono("blip", hz, 0.035f, 0.12f, true));

    static AudioClip Obtener(string nombre, System.Func<AudioClip> generar)
    {
        if (cache.TryGetValue(nombre, out var c) && c != null) return c;
        c = Resources.Load<AudioClip>("Audio/" + nombre);
        if (c == null) c = generar();
        cache[nombre] = c;
        return c;
    }

    static AudioClip Crear(string nombre, float[] datos)
    {
        var clip = AudioClip.Create(nombre, datos.Length, 1, Frec, false);
        clip.SetData(datos, 0);
        return clip;
    }

    static AudioClip Tono(string nombre, float hz, float segundos, float volumen, bool cuadrada)
    {
        int n = (int)(Frec * segundos);
        var d = new float[n];
        for (int i = 0; i < n; i++)
        {
            float t = i / (float)Frec;
            float fase = Mathf.Sin(2f * Mathf.PI * hz * t);
            float onda = cuadrada ? Mathf.Sign(fase) : fase;
            float env = Mathf.Min(1f, i / 80f) * Mathf.Min(1f, (n - i) / 200f);
            d[i] = onda * volumen * env;
        }
        return Crear(nombre, d);
    }

    /// <summary>Piano electrico tranquilo (lo-fi acogedor): 4 acordes en 8 segundos que se repiten.</summary>
    static AudioClip GenerarMusica()
    {
        float[][] acordes =
        {
            new[] { 130.81f, 329.63f, 392.00f, 493.88f },  // Do maj7
            new[] { 110.00f, 261.63f, 329.63f, 392.00f },  // La m7
            new[] { 87.31f, 220.00f, 261.63f, 329.63f },   // Fa maj7
            new[] { 98.00f, 246.94f, 293.66f, 349.23f },   // Sol 7
        };
        int n = Frec * 8;
        var d = new float[n];
        var azar = new System.Random(7);
        for (int a = 0; a < 4; a++)
        {
            float inicio = a * 2f;
            Nota(d, acordes[a][0], inicio, 1.9f, 0.16f);
            for (int k = 1; k < 4; k++) Nota(d, acordes[a][k], inicio, 1.6f, 0.06f);
            Nota(d, acordes[a][1] * 2f, inicio + 0.75f, 0.9f, 0.05f);
            Nota(d, acordes[a][2] * 2f, inicio + 1.0f, 0.8f, 0.045f);
            Nota(d, acordes[a][3] * 2f, inicio + 1.5f, 0.5f, 0.04f);
        }
        for (int i = 0; i < n; i++)
            if (azar.NextDouble() < 0.0006) d[i] += ((float)azar.NextDouble() - 0.5f) * 0.08f; // crujido de vinilo
        return Crear("musica", d);
    }

    static void Nota(float[] d, float hz, float inicio, float duracion, float volumen)
    {
        int i0 = (int)(inicio * Frec), largo = (int)(duracion * Frec);
        for (int i = 0; i < largo && i0 + i < d.Length; i++)
        {
            float t = i / (float)Frec;
            float env = Mathf.Exp(-t * 2.2f) * Mathf.Min(1f, i / 60f);
            float s = Mathf.Sin(2f * Mathf.PI * hz * t) + 0.25f * Mathf.Sin(4f * Mathf.PI * hz * t);
            d[i0 + i] += s * env * volumen;
        }
    }

    /// <summary>Zumbido de ventiladores: ruido grave que se repite sin cortes (frecuencias multiplos de 1/3 Hz).</summary>
    static AudioClip GenerarZumbido()
    {
        int n = Frec * 3;
        var d = new float[n];
        var azar = new System.Random(3);
        for (int k = 120; k < 1200; k += 7)
        {
            float hz = k / 3f;
            float amp = 0.5f / Mathf.Sqrt(hz);
            float fase = (float)azar.NextDouble() * 6.283f;
            for (int i = 0; i < n; i++) d[i] += amp * Mathf.Sin(2f * Mathf.PI * hz * i / Frec + fase);
        }
        for (int i = 0; i < n; i++) d[i] = d[i] * 0.35f + 0.05f * Mathf.Sin(2f * Mathf.PI * 120f * i / Frec);
        return Crear("zumbido", d);
    }

    /// <summary>Crujido metalico del casco contrayendose por el frio.</summary>
    static AudioClip GenerarCrujido()
    {
        int n = (int)(Frec * 0.8f);
        var d = new float[n];
        var azar = new System.Random(11);
        float fase = 0f;
        for (int i = 0; i < n; i++)
        {
            float t = i / (float)Frec;
            float hz = Mathf.Lerp(190f, 70f, t / 0.8f) * (1f + 0.08f * Mathf.Sin(t * 90f));
            fase += 2f * Mathf.PI * hz / Frec;
            float env = Mathf.Exp(-t * 4f) * Mathf.Min(1f, i / 300f);
            float ruido = ((float)azar.NextDouble() - 0.5f) * (azar.NextDouble() < 0.02 ? 1f : 0.15f);
            d[i] = (Mathf.Sign(Mathf.Sin(fase)) * 0.25f + ruido) * env * 0.6f;
        }
        return Crear("crujido", d);
    }

    /// <summary>Siseo de la antena fuera de frecuencia, con un silbido que sube y baja (ionosfera).</summary>
    static AudioClip GenerarSiseo()
    {
        int n = Frec * 2;
        var d = new float[n];
        var azar = new System.Random(5);
        float fase = 0f;
        for (int i = 0; i < n; i++)
        {
            float t = i / (float)Frec;
            float hz = 2400f + 700f * Mathf.Sin(2f * Mathf.PI * 0.5f * t);
            fase += 2f * Mathf.PI * hz / Frec;
            d[i] = ((float)azar.NextDouble() - 0.5f) * 0.18f + Mathf.Sin(fase) * 0.025f;
        }
        return Crear("siseo", d);
    }
}
