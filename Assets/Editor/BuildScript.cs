using UnityEditor;
using UnityEngine;
using GameByte.Automation;

public static class BuildScript {
    /// <summary>
    /// WebGL build'i otomatik automation ile birlikte çalıştırır
    /// Command line: -executeMethod BuildScript.BuildWebGL
    /// </summary>
    public static void BuildWebGL() {
        Debug.Log("Starting WebGL build with automation...");
        
        // Pre-build automation
        RunPreBuildAutomation();
        
        PlayerSettings.WebGL.compressionFormat = WebGLCompressionFormat.Gzip;
        var options = new BuildPlayerOptions {
            scenes = new[] { "Assets/Scenes/GameByteScene.unity" },
            locationPathName = "Builds/WebGL",
            target = BuildTarget.WebGL,
            options = BuildOptions.None
        };
        
        BuildPipeline.BuildPlayer(options);
        
        Debug.Log("WebGL build completed successfully.");
    }
    
    /// <summary>
    /// WebGL build'i full automation ile çalıştırır
    /// Command line: -executeMethod BuildScript.BuildWebGLWithFullAutomation
    /// </summary>
    public static void BuildWebGLWithFullAutomation() {
        Debug.Log("Starting WebGL build with full automation...");
        
        // Full automation pipeline
        BatchModeAutomation.AutomateAllInspectorOperations();
        
        // Build after automation
        BuildWebGL();
    }
    
    /// <summary>
    /// Build öncesi temel automation'lar
    /// </summary>
    private static void RunPreBuildAutomation() {
        Debug.Log("Running pre-build automation...");
        
        try {
            // Script referanslarını kontrol et ve bağla
            BatchModeAutomation.LinkAllScriptReferences();
            
            // GameHeader'ları optimize et
            BatchModeAutomation.ConfigureGameHeaders();
            
            // UI component'larını optimize et
            BatchModeAutomation.ConfigureUIComponents();
            
            Debug.Log("Pre-build automation completed.");
        }
        catch (System.Exception e) {
            Debug.LogError($"Pre-build automation failed: {e.Message}");
            throw;
        }
    }
    
    /// <summary>
    /// Development build ile automation testi
    /// Command line: -executeMethod BuildScript.BuildWebGLDevelopment
    /// </summary>
    public static void BuildWebGLDevelopment() {
        Debug.Log("Starting development WebGL build...");
        
        // Full automation for development
        BatchModeAutomation.AutomateAllInspectorOperations();
        
        PlayerSettings.WebGL.compressionFormat = WebGLCompressionFormat.Gzip;
        var options = new BuildPlayerOptions {
            scenes = new[] { "Assets/Scenes/GameByteScene.unity" },
            locationPathName = "Builds/WebGL-Dev",
            target = BuildTarget.WebGL,
            options = BuildOptions.Development | BuildOptions.ConnectWithProfiler
        };
        
        BuildPipeline.BuildPlayer(options);
        
        Debug.Log("Development WebGL build completed.");
    }
}