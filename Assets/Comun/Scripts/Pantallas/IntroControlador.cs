using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>
/// Cinematica de vinetas: una imagen arriba (640 x 240) y el texto debajo, que aparece letra por letra.
/// Para cambiar los dibujos: reemplazar Assets/Intro/Vinetas/spr_intro_XX.png con el mismo nombre.
/// Para cambiar los textos: editar la lista "vinetas" en el Inspector de este objeto.
/// </summary>
public class IntroControlador : MonoBehaviour
{
    [System.Serializable]
    public class Vineta
    {
        public Sprite imagen;
        [TextArea(2, 4)] public string texto;
    }

    public Vineta[] vinetas;
    public Image imagen;
    public Text texto;
    public Text textoAyuda;
    public float letrasPorSegundo = 40f;

    int indice;
    float letrasVisibles;

    void Start()
    {
        Mostrar(0);
    }

    void Update()
    {
        if (vinetas == null || vinetas.Length == 0) { SceneManager.LoadScene(Escenas.Mapa); return; }
        string completo = vinetas[indice].texto ?? "";
        if (letrasVisibles < completo.Length)
        {
            letrasVisibles += letrasPorSegundo * Time.deltaTime;
            texto.text = completo.Substring(0, Mathf.Min(completo.Length, (int)letrasVisibles));
            if (Teclas.Avanzar()) { letrasVisibles = completo.Length; texto.text = completo; }
            return;
        }
        if (Teclas.Avanzar())
        {
            if (indice + 1 >= vinetas.Length) SceneManager.LoadScene(Escenas.Mapa);
            else Mostrar(indice + 1);
        }
    }

    void Mostrar(int i)
    {
        indice = i;
        letrasVisibles = 0f;
        if (vinetas == null || vinetas.Length == 0) return;
        if (imagen != null) { imagen.sprite = vinetas[i].imagen; imagen.enabled = vinetas[i].imagen != null; }
        if (texto != null) texto.text = "";
        if (textoAyuda != null) textoAyuda.text = (i + 1) + " / " + vinetas.Length + "   Espacio o clic";
    }
}
