# 빌드 자동화 스크립트 예제 - Part 3

## BuildManager 클래스 (계속)

```csharp
    /// <summary>
    /// 빌드 최적화 설정 적용
    /// </summary>
    private void ApplyBuildOptimizations()
    {
        // 디버그 심볼 제거 설정
        if (_stripDebugSymbols)
        {
            EditorUserBuildSettings.stripDebugSymbols = true;
        }
        
        // 라이트맵 압축 설정
        if (_enableLightmapCompression)
        {
            LightmapEditorSettings.enableCompression = true;
        }
        
        // 텍스처 압축 설정
        if (_enableTextureCompression)
        {
            // 텍스처 압축 설정은 직접 Asset 설정에 적용해야 함
            ApplyTextureCompression();
        }
        
        // 메시 압축 설정
        if (_enableMeshCompression)
        {
            // 메시 압축 설정은 직접 Asset 설정에 적용해야 함
            ApplyMeshCompression();
        }
        
        // 코드 스트리핑 설정
        if (_enableCodeStripping)
        {
            PlayerSettings.stripEngineCode = true;
            PlayerSettings.stripUnusedMeshComponents = true;
            
            // IL2CPP 설정
            if (_buildTarget == BuildTarget.StandaloneWindows64 || 
                _buildTarget == BuildTarget.StandaloneWindows)
            {
                PlayerSettings.SetScriptingBackend(BuildTargetGroup.Standalone, ScriptingImplementation.IL2CPP);
                PlayerSettings.SetIl2CppCompilerConfiguration(BuildTargetGroup.Standalone, Il2CppCompilerConfiguration.Release);
            }
        }
    }
    
    /// <summary>
    /// 텍스처 압축 설정 적용
    /// </summary>
    private void ApplyTextureCompression()
    {
        string[] textureGuids = AssetDatabase.FindAssets("t:texture", new[] { "Assets" });
        int count = 0;
        
        foreach (string guid in textureGuids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            TextureImporter importer = AssetImporter.GetAtPath(path) as TextureImporter;
            
            if (importer != null)
            {
                // UI 텍스처는 건너뜀
                if (importer.textureType == TextureImporterType.Sprite)
                {
                    continue;
                }
                
                TextureImporterPlatformSettings settings = importer.GetPlatformTextureSettings("Standalone");
                
                // 이미 압축이 적용되어 있으면 건너뜀
                if (settings != null && settings.overridden && 
                    settings.format != TextureImporterFormat.Automatic)
                {
                    continue;
                }
                
                // 새 압축 설정 적용
                settings = new TextureImporterPlatformSettings
                {
                    name = "Standalone",
                    overridden = true,
                    maxTextureSize = 2048,
                    format = TextureImporterFormat.DXT5Crunched,
                    compressionQuality = 50,
                    textureCompression = TextureImporterCompression.CompressedHQ
                };
                
                importer.SetPlatformTextureSettings(settings);
                
                // 변경사항 저장
                AssetDatabase.ImportAsset(path);
                count++;
            }
        }
        
        Debug.Log($"Applied texture compression to {count} textures");
    }
    
    /// <summary>
    /// 메시 압축 설정 적용
    /// </summary>
    private void ApplyMeshCompression()
    {
        string[] meshGuids = AssetDatabase.FindAssets("t:model", new[] { "Assets" });
        int count = 0;
        
        foreach (string guid in meshGuids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            ModelImporter importer = AssetImporter.GetAtPath(path) as ModelImporter;
            
            if (importer != null)
            {
                // 이미 압축이 적용되어 있으면 건너뜀
                if (importer.meshCompression != ModelImporterMeshCompression.Off)
                {
                    continue;
                }
                
                // 압축 설정 적용
                importer.meshCompression = ModelImporterMeshCompression.Medium;
                
                // 기타 최적화 설정
                importer.optimizeMeshPolygons = true;
                importer.optimizeMeshVertices = true;
                
                // 변경사항 저장
                AssetDatabase.ImportAsset(path);
                count++;
            }
        }
        
        Debug.Log($"Applied mesh compression to {count} models");
    }
```
