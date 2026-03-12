#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using System.IO;

public class BuildScript
{
    [MenuItem("Tools/Build WebGL")]
    public static void BuildWebGL()
    {
        // Créer la scène d'abord
        SceneBuilder.BuildScene();

        string buildPath = Path.Combine(Application.dataPath, "../../docs");
        buildPath = Path.GetFullPath(buildPath);

        Directory.CreateDirectory(buildPath);

        var options = new BuildPlayerOptions
        {
            scenes = new[] { "Assets/Scenes/Main.unity" },
            locationPathName = buildPath,
            target = BuildTarget.WebGL,
            options = BuildOptions.None
        };

        var report = BuildPipeline.BuildPlayer(options);
        Debug.Log($"Build result: {report.summary.result}");

        if (report.summary.result == UnityEditor.Build.Reporting.BuildResult.Succeeded)
        {
            // Créer le fichier .nojekyll pour GitHub Pages
            File.WriteAllText(Path.Combine(buildPath, ".nojekyll"), "");
            Debug.Log($"Build WebGL réussi dans : {buildPath}");
        }
        else
        {
            Debug.LogError($"Build échoué : {report.summary.totalErrors} erreurs");
        }
    }

    // Appelé en ligne de commande : Unity -batchmode -executeMethod BuildScript.BuildWebGLCLI
    public static void BuildWebGLCLI()
    {
        // S'assurer que la scène existe
        if (!System.IO.File.Exists("Assets/Scenes/Main.unity"))
        {
            CreateAndSaveScene();
        }

        string buildPath = System.IO.Path.GetFullPath("docs");
        System.IO.Directory.CreateDirectory(buildPath);

        var options = new BuildPlayerOptions
        {
            scenes = new[] { "Assets/Scenes/Main.unity" },
            locationPathName = buildPath,
            target = BuildTarget.WebGL,
            options = BuildOptions.None
        };

        PlayerSettings.WebGL.compressionFormat = WebGLCompressionFormat.Disabled;
        PlayerSettings.WebGL.template = "APPLICATION:Default";

        var report = BuildPipeline.BuildPlayer(options);

        if (report.summary.result == UnityEditor.Build.Reporting.BuildResult.Succeeded)
        {
            System.IO.File.WriteAllText(System.IO.Path.Combine(buildPath, ".nojekyll"), "");
            Debug.Log("BUILD_SUCCESS");
        }
        else
        {
            Debug.LogError("BUILD_FAILED");
            EditorApplication.Exit(1);
        }

        EditorApplication.Exit(0);
    }

    static void CreateAndSaveScene()
    {
        var scene = UnityEditor.SceneManagement.EditorSceneManager.NewScene(
            UnityEditor.SceneManagement.NewSceneSetup.EmptyScene,
            UnityEditor.SceneManagement.NewSceneMode.Single);

        SceneBuilder.BuildScene();

        System.IO.Directory.CreateDirectory("Assets/Scenes");
        UnityEditor.SceneManagement.EditorSceneManager.SaveScene(scene, "Assets/Scenes/Main.unity");
        AssetDatabase.SaveAssets();
    }
}
#endif
