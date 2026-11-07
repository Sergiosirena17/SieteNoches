using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

/// <summary>
/// Guarda una imagen de cada escena (camara principal) en Docs/Capturas. Util para revisar sin abrir cada escena.
/// Menu: SieteSoles > Capturar escenas.
/// </summary>
public static class Capturas
{
    [MenuItem("SieteSoles/Capturar escenas")]
    public static void Capturar()
    {
        string carpeta = Path.GetFullPath(Path.Combine(Application.dataPath, "../Docs/Capturas"));
        Directory.CreateDirectory(carpeta);
        foreach (var e in EditorBuildSettings.scenes)
        {
            var escena = EditorSceneManager.OpenScene(e.path, OpenSceneMode.Single);
            foreach (var a in Object.FindObjectsByType<AnimadorHojaSprites>()) a.Preparar();
            var cam = Camera.main;
            if (cam == null) continue;
            var seguir = cam.GetComponent<CamaraSeguir>();
            if (seguir != null && seguir.objetivo != null)
                cam.transform.position = new Vector3(seguir.objetivo.position.x, seguir.objetivo.position.y + 0.75f, -10f);
            var rt = new RenderTexture(1280, 720, 24);
            cam.targetTexture = rt;
            Canvas.ForceUpdateCanvases();
            cam.Render();
            RenderTexture.active = rt;
            var tex = new Texture2D(1280, 720, TextureFormat.RGB24, false);
            tex.ReadPixels(new Rect(0, 0, 1280, 720), 0, 0);
            tex.Apply();
            File.WriteAllBytes(Path.Combine(carpeta, escena.name + ".png"), tex.EncodeToPNG());
            cam.targetTexture = null;
            RenderTexture.active = null;
            Object.DestroyImmediate(rt);
            Object.DestroyImmediate(tex);
            Debug.Log("[Capturas] " + escena.name);
        }
    }

    public static void CapturarDesdeConsola()
    {
        Capturar();
        EditorApplication.Exit(0);
    }
}
