using System.Collections.Generic;

/// <summary>Nombres de escenas. Usa siempre estas constantes, nunca escribas el nombre a mano.</summary>
public static class Escenas
{
    public const string Inicio = "Inicio";
    public const string Intro = "Intro";
    public const string Mapa = "Mapa";
    public const string Lanzamiento = "Lanzamiento";
    public const string Final = "Final";
}

/// <summary>Las 4 estaciones principales y los lugares del mapa (documento "Ideas de misiones").</summary>
public static class Estaciones
{
    public const string Invernadero = "invernadero";
    public const string SoporteVital = "soporte";
    public const string Biomedica = "biomedica";
    public const string Antena = "antena";
    public const string Camarotes = "camarotes";
    public const string Puente = "puente";

    public static readonly Dictionary<string, string> Nombre = new Dictionary<string, string>
    {
        { Invernadero, "Invernadero hidropónico" },
        { SoporteVital, "Soporte Vital (MOXIE)" },
        { Biomedica, "Terminal Biomédica" },
        { Antena, "Antena Banda X" },
        { Camarotes, "Camarotes" },
        { Puente, "Puente" },
    };

    /// <summary>Escena de cada estacion. Los Camarotes y el Puente no tienen escena propia.</summary>
    public static readonly Dictionary<string, string> Escena = new Dictionary<string, string>
    {
        { Invernadero, "Est_Invernadero" },
        { SoporteVital, "Est_SoporteVital" },
        { Biomedica, "Est_Biomedica" },
        { Antena, "Est_Antena" },
    };
}

/// <summary>Tarjeta de dato real que aparece al salir de cada estacion.</summary>
public static class DatosReales
{
    public struct Tarjeta { public string Texto; public string Fuente; }

    public static readonly Dictionary<string, Tarjeta> PorEstacion = new Dictionary<string, Tarjeta>
    {
        { Estaciones.Invernadero, new Tarjeta { Texto = "En la Estación Espacial cultivan plantas con luces LED. En poca gravedad, el exceso de agua ahoga las raíces.", Fuente = "nasa.gov (Growing Plants in Space)" } },
        { Estaciones.SoporteVital, new Tarjeta { Texto = "El MOXIE del rover Perseverance sacó oxígeno del aire de Marte calentándolo a 800 °C.", Fuente = "jpl.nasa.gov" } },
        { Estaciones.Biomedica, new Tarjeta { Texto = "Un día en Marte dura 24 h 39 min. Ese desfase y el aislamiento afectan el sueño y el ánimo de los astronautas.", Fuente = "nasa.gov/hrp" } },
        { Estaciones.Antena, new Tarjeta { Texto = "Un mensaje de la Tierra tarda de 3 a 22 minutos en llegar a Marte.", Fuente = "science.nasa.gov" } },
    };
}

/// <summary>Eventos y telemetria de cada noche (tabla "Planificación de eventos y crisis").</summary>
public static class TextosNoches
{
    /// <summary>Bateria con la que empieza cada noche, en %. El indice 0 no se usa.</summary>
    public static readonly int[] BateriaBase = { 0, 70, 65, 60, 45, 60, 55, 10 };

    public static readonly string[] Titulo =
    {
        "",
        "Primera noche tras el impacto en Arcadia Planitia",
        "Paso del orbitador en sombra",
        "Helada de hielo seco",
        "Tormenta nocturna de polvo",
        "Tormenta de protones solares",
        "Hollín y guardia rota",
        "La hora más oscura",
    };

    public static readonly string[] Telemetria =
    {
        "",
        "Temp: -75 °C | Presión: 6.1 hPa | τ = 0.4",
        "Temp: -82 °C | Eclipse de Fobos | τ = 0.5",
        "Temp: -120 °C | Escarcha en válvulas | τ = 0.6",
        "Temp: -90 °C | Opacidad τ = 3.1",
        "Temp: -95 °C | Radiación: 45 mSv | Sin magnetosfera global",
        "Temp: -100 °C | Hollín en MOXIE | Deuda de sueño: +3.9 h",
        "Temp: -125 °C | Reserva crítica",
    };

    public static readonly string[] Incidente =
    {
        "",
        "El sol cayó y los paneles dan 0 W. El MOXIE está frío: llévalo a 800 °C para sacar O₂ del CO₂. Lee el Manual Astrobotánico y siembra tus primeras semillas. Jonas está nervioso: el casco cruje al contraerse por el frío.",
        "Se activa la Antena Banda X: el orbitador cruza el cielo y su frecuencia se corre por el efecto Doppler. Transmitir gasta 25 % de batería. ¿SOS a Houston o el esquema de reciclaje que pide la Dra. Thorne?",
        "El frío congela el CO₂ del aire como hielo seco sobre las tomas del MOXIE.",
        "Hubo tormenta todo el día: los paneles casi no cargaron. Además, el polvo metió percloratos (ClO₄⁻) en una cápsula del invernadero.",
        "El Sol lanzó partículas energéticas. Marte no tiene campo magnético global: la radiación atraviesa el domo. Houston avisa que el módulo de ascenso no podrá subir muestras biológicas. Vance y Thorne discuten.",
        "El MOXIE acumula carbono sólido en sus válvulas (2CO₂ → 2CO + O₂). Cualquier tripulante en rojo ejecuta esta noche su sabotaje.",
        "Son las 05:00. El momento más frío de Marte es justo antes del amanecer. Tienes 60 segundos bajo luces de emergencia: cierra las válvulas en Soporte Vital y ve al Puente.",
    };

    /// <summary>Lo que avisa la Arabidopsis centinela al empezar la noche (si esta viva y crecida).</summary>
    public static readonly string[] Centinela =
    {
        "",
        "",
        "La centinela está tranquila.",
        "La centinela cerró sus hojas horas antes: viene una helada. Gracias al aviso descongelaste las tomas a tiempo.",
        "La centinela tiene las hojas pálidas: hay polvo con percloratos en el aire.",
        "La Arabidopsis se retorció y sus hojas están negras. No es un monstruo: detectó la tormenta de protones antes que las alarmas.",
        "La centinela se inclina hacia el MOXIE: huele el monóxido de carbono del hollín.",
        "",
    };

    public static readonly string[] MensajeNikolas =
    {
        "",
        "",
        "¿Me oyen? Aquí Houston. Recibí su señal. Antes que nada: ¿quién aterrizó la nave? Pregunto para el informe.",
        "El pronóstico dice -120 °C esta noche. Abríguense. Y no culpen al pronóstico.",
        "Vemos la tormenta desde aquí: Tau 3.1. Racionen la batería. Sí, ya sé que lo saben.",
        "Malas noticias: el módulo de ascenso tiene una avería de peso. No podrá subir muestras biológicas. Lo siento.",
        "Última ventana mañana. Si despegan, despeguen bien. Si no... también quiero saberlo.",
        "",
    };
}
