using UnityEngine;
using UnityEditor;
using UnityEditor.Build.Reporting;

public class Builder : MonoBehaviour
{
    private const string BuildOutput = "Build";
    private static readonly string[] Scenes = new [] {
        "Assets/Isometric/Isometric.unity"
    };

    public static BuildReport Build(BuildTarget target)
    {
        BuildPlayerOptions buildPlayerOptions = new BuildPlayerOptions();
        buildPlayerOptions.scenes = Scenes;
        buildPlayerOptions.locationPathName = target.ToString();
        buildPlayerOptions.target = target;
        buildPlayerOptions.options = BuildOptions.None;

        return BuildPipeline.BuildPlayer(buildPlayerOptions);
    }
    
    public static BuildReport BuildWebGL() {
        return Build(BuildTarget.WebGL);
    }

    [MenuItem("Build/WebGL")]
    public static void BuildWebGLMenu()
    {
        BuildSummary summary = BuildWebGL().summary;

        if (summary.result == BuildResult.Succeeded)
        {
            Debug.Log("Build succeeded: " + summary.totalSize + " bytes");
        }

        if (summary.result == BuildResult.Failed)
        {
            Debug.Log("Build failed");
        }
    }
}
