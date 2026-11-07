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
        if (s.Rojo) partes.Add("Rojo 660 nm");
        if (s.Azul) partes.Add("Azul 450 nm");
        if (s.Uv) partes.Add("UV-A 380 nm");
        return partes.Count == 0 ? "Apagado (oscuridad total)" : string.Join(" + ", partes);
    }

    static string Ficha(Especie e)
    {
        var s = Info[e];
        return "<b>" + s.Nombre + "</b>  <i>" + s.Cientifico + "</i>\n" +
               "Ciclo: " + s.Noches + (s.Noches == 1 ? " noche" : " noches") + (s.Continua ? " (y sigue)" : "") + "\n" +
               "Humedad: " + s.Humedad + " %   Luz: " + Leds(s) + "\n" +
               s.Efecto + "\n" + s.Riesgo + "\n";
    }

    /// <summary>Las 3 paginas del Manual Astrobotánico que se ven en el invernadero.</summary>
    public static readonly string[] PaginasManual =
    {
        "MANUAL ASTROBOTÁNICO  (1/3)\n\n" + Ficha(Especie.Espirulina) + "\n" + Ficha(Especie.Papa),
        "MANUAL ASTROBOTÁNICO  (2/3)\n\n" + Ficha(Especie.Lechuga) + "\n" + Ficha(Especie.Arabidopsis),
        "MANUAL ASTROBOTÁNICO  (3/3)\n\n" + Ficha(Especie.Dechloromonas) + "\n" +
        "<b>Reglas de la noche</b>\n" +
        "• Cada canal LED encendido gasta 3 % de batería.\n" +
        "• Una planta a oscuras respira: consume 2 % de O₂.\n" +
        "• En 0.38 g el agua forma una película en la raíz. Mucha agua la ahoga.\n" +
        "• Con 60 % de humedad o más y sin calefacción, el agua se congela.\n" +
        "• El regolito tiene de 0.5 % a 1 % de percloratos: límpialo antes de sembrar comida.\n" +
        "• 1 noche mal configurada: deja de crecer. 2 seguidas: biomasa podrida (-5 % O₂).",
    };

    public const string LineaAlucinacion = "«No importa cuánta luz les pongas. Afuera sigue siendo de noche.»";
}
