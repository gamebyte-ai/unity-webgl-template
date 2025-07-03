using UnityEditor;

public static class BuildScript {
    public static void BuildWebGL() {
        PlayerSettings.WebGL.compressionFormat = WebGLCompressionFormat.Gzip;
        var options = new BuildPlayerOptions {
            scenes = new[] { "Assets/Scenes/GameByteScene.unity" },
            locationPathName = "Builds/WebGL",
            target = BuildTarget.WebGL,
            options = BuildOptions.None
        };
        BuildPipeline.BuildPlayer(options);
    }
}