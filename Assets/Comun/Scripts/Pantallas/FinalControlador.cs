using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>Muestra el final elegido por EvaluadorFinal y el Informe Científico de Misión.</summary>
public class FinalControlador : MonoBehaviour
{
    public Text textoTitulo;
    public Text textoTipo;
    public Text textoCuerpo;
    public Text textoInforme;

    public string IdFinal { get; private set; }

    void Start()
    {
        IdFinal = EvaluadorFinal.Calcular();
        var f = EvaluadorFinal.Finales[IdFinal];
        if (textoTitulo != null) textoTitulo.text = f.Titulo;
        if (textoTipo != null) textoTipo.text = "Final: " + f.Tipo;
        if (textoCuerpo != null)
            textoCuerpo.text = f.Texto + (IdFinal == EvaluadorFinal.Hipoxia ? "\n\n" + EvaluadorFinal.MotivoHipoxia() : "");
        if (textoInforme != null)
            textoInforme.text = "INFORME CIENTÍFICO DE MISIÓN\n• " + string.Join("\n• ", EvaluadorFinal.InformeCientifico());
    }

    public void JugarDeNuevo()
    {
        SceneManager.LoadScene(Escenas.Inicio);
    }
}
