using System.Collections.Generic;
using Unity.Collections;
using UnityEngine;
using UnityEngine.UI;

// outline 알고리즘 sobel, laplacian, canny...

public class day1 : MonoBehaviour {
    [SerializeField] private Texture2D texture;

    [SerializeField] private RawImage original;

    [SerializeField] private RawImage grayscale;

    [SerializeField] private RawImage result;

    delegate Color32[] GetColorArray(Texture2D texture, int mipLevel = 0);

    private readonly Dictionary<TextureFormat, GetColorArray> _colorArrays = new();

    private void Awake() {
        _colorArrays.Add(TextureFormat.RGB24, GetColorArrayFromRGB24);
        _colorArrays.Add(TextureFormat.DXT1, GetColorArrayFromDxt1);
    }

    void Start() {
        if (texture == null || !IsSupportFormat(texture.format)) {
            Debug.Log("Texture is null or unsupported...");
            return;
        }

        original.texture = texture;
        grayscale.texture = CreateGrayscale(texture); // with shader
        result.texture = CreateGrayscale(texture, true); // without shader
    }

    // TextureFormat.RGB8 픽셀 포멧..
    private struct Color24 {
        public byte r;
        public byte g;
        public byte b;
    }

    private static bool IsSupportFormat(TextureFormat format) {
        return format switch {
            TextureFormat.RGB24 => true,
            TextureFormat.DXT1 => true,
            _ => false, // throw new Exception($"not support format `{format}`")
        };
    }

    private Texture2D CreateGrayscale(Texture2D source, bool outline = false) {
        var mipmapCount = source.mipmapCount;
        Debug.Log("mipmapCount = " + mipmapCount);

        var grayscaleTexture = new Texture2D(source.width, source.height, TextureFormat.R8, mipmapCount > 0) {
            name = "grayscale with red channel"
        };

        for (var mipLevel = 0; mipLevel < mipmapCount; mipLevel++) {
            var colors = _colorArrays[source.format](source, mipLevel);
            var buffer = new NativeArray<byte>(source.width * source.height, Allocator.Temp);

            for (var i = 0; i < colors.Length; i++)
                buffer[i] = ToGrayscale(colors[i]);

            if (outline) {
                var f = mipLevel / (float)mipmapCount;
                var w = (int)(source.width * (1f - f));
                var h = (int)(source.height * (1f - f));
                Debug.Log($"w = {w}, h = {h} / colors.Length = {colors.Length}");
                SobelFilter(buffer, w, h);
            }

            grayscaleTexture.SetPixelData(buffer, mipLevel);

            buffer.Dispose();
        }

        grayscaleTexture.Apply();
        return grayscaleTexture;
    }

    private void SobelFilter(NativeArray<byte> buffer, int width, int height) {
        var bytes = buffer.ToArray();

        int gx, gy;

        for (var i = 0; i < buffer.Length; i++) {
            var row = (i - i % width) / width;

            if (i % width == 0 || i % width == width - 1 || row == 0)
                continue;
            if (row == height - 1)
                break;

            gx = bytes[i + 1] * -2 + bytes[i - 1] * 2 +
                 bytes[i + 1 - width] * -1 + bytes[i - 1 - width] * 1 +
                 bytes[i + 1 + width] * -1 + bytes[i - 1 + width] * 1;

            gy = bytes[i + width] * -2 + bytes[i - width] * 2 +
                 bytes[i - 1 + width] * -1 + bytes[i - 1 - width] * 1 +
                 bytes[i + 1 + width] * -1 + bytes[i + 1 - width] * 1;

            gx *= gx;
            gy *= gy;
            buffer[i] = (byte)Mathf.Sqrt(gx + gy);
        }
    }
    
    private static Color32[] GetColorArrayFromRGB24(Texture2D source, int mipLevel = 0) {
        using var pixels = source.GetPixelData<Color24>(mipLevel);
        var colors = new Color32[pixels.Length];
        for (var i = 0; i < pixels.Length; i++)
            colors[i] = new Color32(pixels[i].r, pixels[i].g, pixels[i].b, 255);
        return colors;
    }

    private static Color32[] GetColorArrayFromDxt1(Texture2D source, int mipLevel = 0) {
        using var pixelData = source.GetPixelData<byte>(mipLevel);
        return day1_utils.ReadBC1(pixelData.ToArray(), source.width, source.height);
    }

    private static byte ToGrayscale(Color32 color) {
        return (byte)(color.r * 0.299f + color.g * 0.587f + color.b * 0.114f);
    }
}
