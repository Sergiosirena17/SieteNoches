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
        Efecto = "With its light on, it provides +15% O₂ every night.",
        Riesgo = "If you turn off its light or lower humidity, it stops producing oxygen immediately." } },
        { Especie.Papa, new InfoSemilla {
            Nombre = "Papa", Cientifico = "Solanum tuberosum var. 'Unique'", Noches = 3, Humedad = 45,
            Rojo = true, ToleraUnaNocheOscura = true, Comestible = true,
        Efecto = "Provides 3 rations. One ration calms (-20 BPM) and warms.",
        Riesgo = "At 90% water in 0.38 g it rots: root hypoxia. It survives 1 night in darkness." } },
        { Especie.Lechuga, new InfoSemilla {
        Nombre = "Red lettuce", Cientifico = "Lactuca sativa 'Outredgeous'", Noches = 2, Humedad = 50,
            Azul = true, Uv = true, Comestible = true,
        Efecto = "Provides 1 radioprotective, calming dose (-40 BPM).",
        Riesgo = "Blue and UV light stress the leaf: it turns red and makes anthocyanins against radiation." } },
        { Especie.Arabidopsis, new InfoSemilla {
        Nombre = "Sentinel cress", Cientifico = "Arabidopsis thaliana Col-0", Noches = 1, Humedad = 30,
            Azul = true, Continua = true,
        Efecto = "Not edible. Once grown, it warns of freezes, radiation, and leaks before the alarms.",
        Riesgo = "It is the model organism used by space laboratories to measure stress." } },
        { Especie.Dechloromonas, new InfoSemilla {
            Nombre = "Dechloromonas", Cientifico = "Cultivo biorremediador (bacteria y micelio)", Noches = 1, Humedad = 60,
        Efecto = "In darkness, it destroys perchlorate (ClO₄⁻) in its pod and returns +20% water.",
        Riesgo = "While working, it consumes 8% O₂. Keep LEDs off." } },
    };

    public static string Leds(InfoSemilla s)
    {
        var partes = new List<string>();
        if (s.Rojo) partes.Add("red");
        if (s.Azul) partes.Add("blue");
        if (s.Uv) partes.Add("UV");
        return partes.Count == 0 ? "no light" : "light " + string.Join(" + ", partes);
    }

    /// <summary>Lo que dice el manual de cada planta: solo lo necesario para cuidarla.</summary>
    static readonly Dictionary<Especie, string> Resumen = new Dictionary<Especie, string>
    {
        { Especie.Espirulina, "Provides oxygen every night while it has light." },
        { Especie.Papa, "Provides 3 food rations. Survives 1 night without light." },
        { Especie.Lechuga, "Provides 1 dose that strongly calms and protects from radiation." },
        { Especie.Arabidopsis, "Not edible. Once grown, it warns you about nighttime hazards." },
        { Especie.Dechloromonas, "Cleans toxic soil in its pod and provides water." },
    };

    static string Ficha(Especie e)
    {
        var s = Info[e];
        string ciclo = s.Continua ? "" : "  ·  " + s.Noches + (s.Noches == 1 ? " night" : " nights");
        return "<b>" + s.Nombre + "</b>" + ciclo + "\n" +
        "Humidity " + s.Humedad + "%  ·  " + Leds(s) + "\n" +
               Resumen[e] + "\n";
    }

    /// <summary>Las 2 paginas del manual que se ven en el invernadero.</summary>
    public static readonly string[] PaginasManual =
    {
        "<b>PLANT MANUAL</b>\n\n" + Ficha(Especie.Espirulina) + "\n" + Ficha(Especie.Papa) + "\n" + Ficha(Especie.Lechuga),
        "<b>PLANT MANUAL</b>\n\n" + Ficha(Especie.Arabidopsis) + "\n" + Ficha(Especie.Dechloromonas) + "\n" +
        "<b>Rules</b>\n" +
        "• Every active light uses battery.\n" +
        "• Without light, a plant consumes a little oxygen.\n" +
        "• Too much water drowns roots.\n" +
        "• High humidity without heating freezes.\n" +
        "• Toxic soil (ClO₄⁻): clean it before planting food.\n" +
        "• 2 neglected nights: it rots.",
    };


    public const string LineaAlucinacion = "“No matter how much light you give them. Outside, it is still night.”";
}
