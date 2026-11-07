using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>Muestra el final elegido por EvaluadorFinal y el resumen de decisiones.</summary>
public class FinalControlador : MonoBehaviour
{
    public Text textoTitulo;
    public Text textoTipo;
    public Text textoCuerpo;
    public Text textoResumen;

    void Start()
    {
        var id = EvaluadorFinal.Calcular();
        var f = EvaluadorFinal.Finales[id];
        if (textoTitulo != null) textoTitulo.text = f.Titulo;
        if (textoTipo != null) textoTipo.text = "Final " + f.Tipo.ToLower();
        if (textoCuerpo != null) textoCuerpo.text = f.Texto;
        if (textoResumen != null)
        {
            var lineas = GameState.Historial.Count == 0 ? "No completaste misiones." : string.Join("\n", GameState.Historial);
            textoResumen.text = "Tus decisiones:\n" + lineas;
        }
    }

    public void JugarDeNuevo()
    {
        SceneManager.LoadScene(Escenas.Inicio);
    }
}
