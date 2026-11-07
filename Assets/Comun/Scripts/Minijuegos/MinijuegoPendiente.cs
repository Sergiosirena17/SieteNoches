using UnityEngine.UI;

/// <summary>
/// Minijuego TEMPORAL para probar la partida completa mientras se programa el de verdad.
/// Tiene tres botones que simulan el resultado. Quien programe este minijuego borra este
/// componente de su escena y pone su propio script (heredando de MinijuegoBase).
/// </summary>
public class MinijuegoPendiente : MinijuegoBase
{
    public Text textoTitulo;
    public Text textoBoton1, textoBoton2, textoBoton3;

    void Start()
    {
        bool roca = idMision == Misiones.Roca;
        if (textoTitulo != null) textoTitulo.text = Misiones.Nombre[idMision] + "\n(en construcción)";
        if (textoBoton1 != null) textoBoton1.text = roca ? "Analizarla" : "Simular: ganar";
        if (textoBoton2 != null) textoBoton2.text = roca ? "Subirla sin analizar" : "Simular: a medias";
        if (textoBoton3 != null) textoBoton3.text = roca ? "Dejarla" : "Simular: perder";
    }

    public void Boton1() { if (idMision == Misiones.Roca) TerminarRoca(DecisionRoca.Analizar, "Analizaste la roca."); else Terminar(Resultado.Gano, "(simulado)"); }
    public void Boton2() { if (idMision == Misiones.Roca) TerminarRoca(DecisionRoca.SubirSinAnalizar, "Subiste la roca sin analizar."); else Terminar(Resultado.AMedias, "(simulado)"); }
    public void Boton3() { if (idMision == Misiones.Roca) TerminarRoca(DecisionRoca.Dejar, "Dejaste la roca donde estaba."); else Terminar(Resultado.Perdio, "(simulado)"); }
}
