using System.Collections.Generic;

/// <summary>Scene names. Always use these constants instead of typing a name.</summary>
public static class Escenas
{
    public const string Inicio = "Inicio";
    public const string Intro = "Intro";
    public const string Mapa = "Mapa";
    public const string Lanzamiento = "Lanzamiento";
    public const string Final = "Final";
}

/// <summary>The four main stations and map locations.</summary>
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
        { Invernadero, "Hydroponic Greenhouse" },
        { SoporteVital, "Life Support (MOXIE)" },
        { Biomedica, "Biomedical Terminal" },
        { Antena, "X-Band Antenna" },
        { Camarotes, "Crew Quarters" },
        { Puente, "Bridge" },
    };

    public static readonly Dictionary<string, string> Escena = new Dictionary<string, string>
    {
        { Invernadero, "Est_Invernadero" },
        { SoporteVital, "Est_SoporteVital" },
        { Biomedica, "Est_Biomedica" },
        { Antena, "Est_Antena" },
    };
}

/// <summary>Real-world fact card shown when leaving each station.</summary>
public static class DatosReales
{
    public struct Tarjeta { public string Texto; public string Fuente; }

    public static readonly Dictionary<string, Tarjeta> PorEstacion = new Dictionary<string, Tarjeta>
    {
        { Estaciones.Invernadero, new Tarjeta { Texto = "Plants are grown aboard the International Space Station under LED lights. In low gravity, excess water can drown their roots.", Fuente = "nasa.gov (Growing Plants in Space)" } },
        { Estaciones.SoporteVital, new Tarjeta { Texto = "The Perseverance rover's MOXIE extracted oxygen from Martian air by heating it to 800 °C.", Fuente = "jpl.nasa.gov" } },
        { Estaciones.Biomedica, new Tarjeta { Texto = "A day on Mars lasts 24 hours and 39 minutes. That offset and isolation affect astronauts' sleep and mood.", Fuente = "nasa.gov/hrp" } },
        { Estaciones.Antena, new Tarjeta { Texto = "A message from Earth takes 3 to 22 minutes to reach Mars.", Fuente = "science.nasa.gov" } },
    };
}

/// <summary>Events and telemetry for each night.</summary>
public static class TextosNoches
{
    public static readonly int[] BateriaBase = { 0, 70, 65, 60, 45, 60, 55, 10 };

    public static readonly string[] Titulo =
    {
        "",
        "First night after the Arcadia Planitia impact",
        "Orbiter pass in shadow",
        "Dry ice freeze",
        "Nighttime dust storm",
        "Solar proton storm",
        "Soot and a broken watch",
        "The darkest hour",
    };

    public static readonly string[] Telemetria =
    {
        "",
        "Temp: -75 °C | Pressure: 6.1 hPa | τ = 0.4",
        "Temp: -82 °C | Phobos eclipse | τ = 0.5",
        "Temp: -120 °C | Frost on valves | τ = 0.6",
        "Temp: -90 °C | Opacity τ = 3.1",
        "Temp: -95 °C | Radiation: 45 mSv | No global magnetosphere",
        "Temp: -100 °C | Soot in MOXIE | Sleep debt: +3.9 h",
        "Temp: -125 °C | Critical reserve",
    };

    public static readonly string[] Incidente =
    {
        "",
        "The sun has set and the panels produce 0 W. MOXIE is cold: bring it to 800 °C to extract O₂ from CO₂. Read the Astrobotany Manual and plant your first seeds. Jonas is nervous: the hull creaks as it contracts in the cold.",
        "The X-Band Antenna comes online: the orbiter crosses the sky and its frequency shifts through the Doppler effect. Transmitting costs 25% battery. Send an SOS to Houston or the recycling design requested by Dr. Thorne?",
        "The cold freezes CO₂ from the air as dry ice on MOXIE's intakes.",
        "A storm raged all day: the panels barely charged. Dust also carried perchlorates (ClO₄⁻) into a greenhouse pod.",
        "The Sun released energetic particles. Mars has no global magnetic field, so radiation passes through the dome. Houston reports that the ascent module cannot carry biological samples. Vance and Thorne argue.",
        "MOXIE is building up solid carbon in its valves (2CO₂ → 2CO + O₂). Any crew member in red will carry out their sabotage tonight.",
        "It is 05:00. Mars is coldest just before dawn. You have 60 seconds under emergency lights: close the Life Support valves and get to the Bridge.",
    };

    public static readonly string[] Centinela =
    {
        "", "",
        "The sentinel is calm.",
        "The sentinel closed its leaves hours ago: a freeze is coming. Thanks to the warning, you thawed the intakes in time.",
        "The sentinel's leaves are pale: there is perchlorate dust in the air.",
        "The Arabidopsis twisted and its leaves turned black. It is not a monster: it detected the proton storm before the alarms did.",
        "The sentinel leans toward MOXIE: it smells carbon monoxide from the soot.",
        "",
    };

    public static readonly string[] MensajeNikolas =
    {
        "", "",
        "Can you hear me? Houston here. I received your signal. First of all: who landed the ship? Asking for the report.",
        "The forecast says -120 °C tonight. Bundle up. And do not blame the forecast.",
        "We can see the storm from here: Tau 3.1. Ration the battery. Yes, I know you already know.",
        "Bad news: the ascent module has a mass issue. It cannot carry biological samples. I am sorry.",
        "Last window tomorrow. If you launch, launch well. If not... I still want to know.",
        "",
    };
}
