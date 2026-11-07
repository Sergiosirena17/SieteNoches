using System.Collections.Generic;

/// <summary>Elige el final segun la tabla del GDD (de arriba hacia abajo, gana la primera condicion).</summary>
public static class EvaluadorFinal
{
    public struct Final { public string Titulo; public string Tipo; public string Texto; }

    public static string Calcular()
    {
        if (GameState.Oxigeno <= 0) return "SinAire";

        bool naveLista = GameState.Nave >= 70;
        bool energiaLista = GameState.Energia >= 30;

        if (naveLista && energiaLista)
        {
            if (GameState.NikolasGanadas >= 3) return "UltimaPalabra";
            if (GameState.Marcas.Contains("roca_dejada")
                && !GameState.Marcas.Contains("escotilla_abierta")
                && !GameState.Marcas.Contains("sonda_desarmada")
                && GameState.Moral >= 50) return "PrimerContacto";
            if (GameState.Marcas.Contains("roca_sin_analizar")) return "Pasajero";
            if (GameState.Marcas.Contains("roca_analizada")) return "RegresoHeroe";
            return "VueltaJusta";
        }

        if (naveLista) return "DerivaSilenciosa";
        if (GameState.Comida >= 30 && GameState.Oxigeno >= 30) return "ColonosALaFuerza";
        return "FalloEnLaRampa";
    }

    public static readonly Dictionary<string, Final> Finales = new Dictionary<string, Final>
    {
        { "SinAire", new Final { Titulo = "Sin aire", Tipo = "Malo", Texto = "La alarma de oxígeno deja de sonar. Wayra sigue hablando, pero ya nadie la escucha." } },
        { "UltimaPalabra", new Final { Titulo = "La última palabra", Tipo = "Secreto", Texto = "Vuelves a la Tierra. Nikolas te espera con la respuesta a tu último insulto, y esta vez no hay retraso." } },
        { "PrimerContacto", new Final { Titulo = "Primer contacto", Tipo = "Secreto", Texto = "Antes de despegar, ves que todas las huellas forman un círculo alrededor de la nave. Algo te dejó ir." } },
        { "Pasajero", new Final { Titulo = "Pasajero", Tipo = "Malo", Texto = "Ya en órbita, la bodega está en silencio. Entonces algo golpea desde adentro de la caja de la roca." } },
        { "RegresoHeroe", new Final { Titulo = "Regreso héroe", Tipo = "El mejor", Texto = "Vuelves con la nave entera y una muestra analizada. La respuesta tardará años, pero empezó contigo." } },
        { "VueltaJusta", new Final { Titulo = "Vuelta justa", Tipo = "Bueno", Texto = "Vuelves a casa. Nadie sabrá nunca qué había en esa roca." } },
        { "DerivaSilenciosa", new Final { Titulo = "Deriva silenciosa", Tipo = "Malo", Texto = "La nave despega, pero no hay energía para cambiar de rumbo. Marte se aleja. La Tierra también." } },
        { "ColonosALaFuerza", new Final { Titulo = "Colonos a la fuerza", Tipo = "Mixto", Texto = "La nave no despega. El huerto crece y el aire alcanza. Quedan 26 meses de espera." } },
        { "FalloEnLaRampa", new Final { Titulo = "Fallo en la rampa", Tipo = "Malo", Texto = "El cohete enciende. Lo último que oyes es a Wayra diciendo que ya te lo había advertido." } },
    };
}
