using System.Collections.Generic;

public enum Especie { Ninguna, Espirulina, Papa, Lechuga, Arabidopsis, Dechloromonas }

/// <summary>Una ficha del Manual Astrobotánico. Los numeros son la regla del juego.</summary>
public struct InfoSemilla
{
    public string Nombre;
    public string Cientifico;
    /// <summary>Noches que necesita con la configuracion correcta para dar su efecto.</summary>
    public int Noches;
    /// <summary>Humedad correcta en %. Se acepta 5 puntos arriba o abajo.</summary>
    public int Humedad;
    public bool Rojo, Azul, Uv;
    /// <summary>Solo la papa: aguanta 1 noche con los LEDs apagados sin que cuente como error.</summary>
    public bool ToleraUnaNocheOscura;
    /// <summary>Si es comida, sembrarla en suelo con percloratos la vuelve toxica.</summary>
    public bool Comestible;
    /// <summary>No se cosecha: sigue dando su efecto cada noche mientras este bien cuidada.</summary>
    public bool Continua;
    public string Efecto;
    public string Riesgo;
}

/// <summary>Catalogo de las 5 semillas (tabla del documento "Ideas de misiones").</summary>
public static class Semillas
{
    public const int Tolerancia = 5;

    public static readonly Especie[] Todas = { Especie.Espirulina, Especie.Papa, Especie.Lechuga, Especie.Arabidopsis, Especie.Dechloromonas };

    public static readonly Dictionary<Especie, InfoSemilla> Info = new Dictionary<Especie, InfoSemilla>
    {
        { Especie.Espirulina, new InfoSemilla {
            Nombre = "Espirulina", Cientifico = "Arthrospira platensis (fotobiorreactor)", Noches = 1, Humedad = 90,
            Rojo = true, Azul = true, Comestible = true, Continua = true,
            Efecto = "Con su luz encendida da +15 % de O₂ cada noche.",
            Riesgo = "Si le apagas la luz o le bajas la humedad, deja de producir oxígeno al instante." } },
        { Especie.Papa, new InfoSemilla {
            Nombre = "Papa", Cientifico = "Solanum tuberosum var. 'Unique'", Noches = 3, Humedad = 45,
            Rojo = true, ToleraUnaNocheOscura = true, Comestible = true,
            Efecto = "Da 3 raciones. Una ración calma (-20 BPM) y da calor.",
            Riesgo = "Si la riegas al 90 % en 0.38 g se pudre: hipoxia radicular. Aguanta 1 noche a oscuras." } },
        { Especie.Lechuga, new InfoSemilla {
            Nombre = "Lechuga roja", Cientifico = "Lactuca sativa 'Outredgeous'", Noches = 2, Humedad = 50,
            Azul = true, Uv = true, Comestible = true,
            Efecto = "Da 1 dosis radioprotectora y sedante (-40 BPM).",
            Riesgo = "La luz azul y UV estresa la hoja: se vuelve roja y fabrica antocianinas contra la radiación." } },
        { Especie.Arabidopsis, new InfoSemilla {
            Nombre = "Berro centinela", Cientifico = "Arabidopsis thaliana Col-0", Noches = 1, Humedad = 30,
            Azul = true, Continua = true,
            Efecto = "No se come. Crecida, avisa de heladas, radiación y fugas antes que las alarmas.",
            Riesgo = "Es el organismo modelo de los laboratorios espaciales para medir estrés." } },
        { Especie.Dechloromonas, new InfoSemilla {
            Nombre = "Dechloromonas", Cientifico = "Cultivo biorremediador (bacteria y micelio)", Noches = 1, Humedad = 60,
            Efecto = "A oscuras destruye el perclorato (ClO₄⁻) de su cápsula y devuelve +20 % de agua.",
            Riesgo = "Mientras trabaja consume 8 % de O₂. Va con los LEDs apagados." } },
    };

    public static string Leds(InfoSemilla s)
    {
        var partes = new List<string>();
        if (s.Rojo) partes.Add("roja");
        if (s.Azul) partes.Add("azul");
        if (s.Uv) partes.Add("UV");
        return partes.Count == 0 ? "sin luz" : "luz " + string.Join(" + ", partes);
    }

    /// <summary>Lo que dice el manual de cada planta: solo lo necesario para cuidarla.</summary>
    static readonly Dictionary<Especie, string> Resumen = new Dictionary<Especie, string>
    {
        { Especie.Espirulina, "Da oxígeno cada noche mientras tenga luz." },
        { Especie.Papa, "Da 3 raciones de comida. Aguanta 1 noche sin luz." },
        { Especie.Lechuga, "Da 1 dosis que calma mucho y protege de la radiación." },
        { Especie.Arabidopsis, "No se come. Crecida, te avisa de los peligros de la noche." },
        { Especie.Dechloromonas, "Limpia el suelo tóxico de su cápsula y da agua." },
    };

    static string Ficha(Especie e)
    {
        var s = Info[e];
        string ciclo = s.Continua ? "" : "  ·  " + s.Noches + (s.Noches == 1 ? " noche" : " noches");
        return "<b>" + s.Nombre + "</b>" + ciclo + "\n" +
               "Humedad " + s.Humedad + " %  ·  " + Leds(s) + "\n" +
               Resumen[e] + "\n";
    }

    /// <summary>Las 2 paginas del manual que se ven en el invernadero.</summary>
    public static readonly string[] PaginasManual =
    {
        "<b>MANUAL DE PLANTAS</b>\n\n" + Ficha(Especie.Espirulina) + "\n" + Ficha(Especie.Papa) + "\n" + Ficha(Especie.Lechuga),
        "<b>MANUAL DE PLANTAS</b>\n\n" + Ficha(Especie.Arabidopsis) + "\n" + Ficha(Especie.Dechloromonas) + "\n" +
        "<b>Reglas</b>\n" +
        "• Cada luz encendida gasta batería.\n" +
        "• Sin luz, la planta gasta un poco de oxígeno.\n" +
        "• Mucha agua ahoga las raíces.\n" +
        "• Humedad alta sin calefacción: se congela.\n" +
        "• Suelo tóxico (ClO₄⁻): límpialo antes de sembrar comida.\n" +
        "• 2 noches mal cuidada: se pudre.",
    };


    public const string LineaAlucinacion = "«No importa cuánta luz les pongas. Afuera sigue siendo de noche.»";
}
