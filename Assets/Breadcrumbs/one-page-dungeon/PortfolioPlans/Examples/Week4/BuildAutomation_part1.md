# 빌드 자동화 스크립트 예제 - Part 1

## BuildManager 클래스

```csharp
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;

/// <summary>
/// 포트폴리오 빌드 자동화 관리 클래스
/// 데모 빌드 생성 및 최적화를 담당합니다.
/// </summary>
public class BuildManager : EditorWindow
{
    // 빌드 설정
    private string _buildPath = "Builds";
    private string _buildName = "DungeonCrawlerDemo";
    private string _version = "0.1.0";
    private BuildTarget _buildTarget = BuildTarget.StandaloneWindows64;
    private BuildOptions _buildOptions = BuildOptions.None;
    
    // 씬 설정
    private List<EditorBuildSettingsScene> _scenesToBuild = new List<EditorBuildSettingsScene>();
    private bool _useDefaultScenes = true;
    
    // 빌드 최적화 설정
    private bool _enableOptimizations = true;
    private bool _stripDebugSymbols = true;
    private bool _enableLightmapCompression = true;
    private bool _enableTextureCompression = true;
    private bool _enableMeshCompression = true;
    private bool _enableCodeStripping = true;
    
    // 빌드 전/후 작업 설정
    private bool _runPreBuildTasks = true;
    private bool _runPostBuildTasks = true;
    private bool _generateVersionInfo = true;
    private bool _createInstaller = false;
    
    // 배포 설정
    private bool _createZipArchive = true;
    private bool _copyToPortfolioFolder = true;
    private string _portfolioFolderPath = "PortfolioSubmission";
    
    [MenuItem("Portfolio/Build Manager")]
    public static void ShowWindow()
    {
        GetWindow<BuildManager>("Build Manager");
    }
    
    private void OnEnable()
    {
        // 기본 씬 로드
        if (_useDefaultScenes)
        {
            LoadDefaultScenes();
        }
    }
    
    private void OnGUI()
    {
        GUILayout.Label("Dungeon Crawler Demo Build Manager", EditorStyles.boldLabel);
        EditorGUILayout.Space();
        
        DrawBuildSettings();
        EditorGUILayout.Space();
        
        DrawSceneSettings();
        EditorGUILayout.Space();
        
        DrawOptimizationSettings();
        EditorGUILayout.Space();
        
        DrawBuildTaskSettings();
        EditorGUILayout.Space();
        
        DrawDeploymentSettings();
        EditorGUILayout.Space();
        
        DrawBuildButtons();
    }
    
    private void DrawBuildSettings()
    {
        GUILayout.Label("Build Settings", EditorStyles.boldLabel);
        
        _buildPath = EditorGUILayout.TextField("Build Path", _buildPath);
        _buildName = EditorGUILayout.TextField("Build Name", _buildName);
        _version = EditorGUILayout.TextField("Version", _version);
        _buildTarget = (BuildTarget)EditorGUILayout.EnumPopup("Build Target", _buildTarget);
        _buildOptions = (BuildOptions)EditorGUILayout.EnumFlagsField("Build Options", _buildOptions);
    }
    
    private void DrawSceneSettings()
    {
        GUILayout.Label("Scene Settings", EditorStyles.boldLabel);
        
        _useDefaultScenes = EditorGUILayout.Toggle("Use Default Scenes", _useDefaultScenes);
        
        if (!_useDefaultScenes)
        {
            EditorGUILayout.HelpBox("Custom scene selection is not implemented in this demo.", MessageType.Info);
        }
    }
    
    private void DrawOptimizationSettings()
    {
        GUILayout.Label("Optimization Settings", EditorStyles.boldLabel);
        
        _enableOptimizations = EditorGUILayout.Toggle("Enable Optimizations", _enableOptimizations);
        
        if (_enableOptimizations)
        {
            EditorGUI.indentLevel++;
            _stripDebugSymbols = EditorGUILayout.Toggle("Strip Debug Symbols", _stripDebugSymbols);
            _enableLightmapCompression = EditorGUILayout.Toggle("Lightmap Compression", _enableLightmapCompression);
            _enableTextureCompression = EditorGUILayout.Toggle("Texture Compression", _enableTextureCompression);
            _enableMeshCompression = EditorGUILayout.Toggle("Mesh Compression", _enableMeshCompression);
            _enableCodeStripping = EditorGUILayout.Toggle("Code Stripping", _enableCodeStripping);
            EditorGUI.indentLevel--;
        }
    }
```
