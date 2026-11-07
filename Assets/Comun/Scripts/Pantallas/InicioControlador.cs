using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>Pantalla de titulo: el boton Jugar reinicia la partida y pasa a la intro.</summary>
public class InicioControlador : MonoBehaviour
{
    public void Jugar()
    {
        GameState.Reiniciar();
        SceneManager.LoadScene(Escenas.Intro);
    }
}
