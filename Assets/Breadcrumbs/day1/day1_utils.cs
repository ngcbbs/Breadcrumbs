using System;
using UnityEngine;

public static class day1_utils {
    /*
    public struct BC1Block {
        public ushort color0; // 565 format
        public ushort color1; // 565 format
        public uint lookupTable; // 2 bits per pixel lookup
    }
    // */

    /*
    사용 예시:
    var compressedData = texture.GetRawTextureData();
    var pixels = ReadBC1(compressedData, texture.width, texture.height);
    // */

    public static Color32[] ReadBC1(byte[] compressedData, int width, int height) {
        int blocksX = (width + 3) / 4;
        int blocksY = (height + 3) / 4;
        Color32[] pixels = new Color32[width * height];

        for (int blockY = 0; blockY < blocksY; blockY++) {
            for (int blockX = 0; blockX < blocksX; blockX++) {
                // 블록 데이터 읽기
                int blockIndex = (blockY * blocksX + blockX) * 8; // 각 블록은 8바이트
                ushort color0 = BitConverter.ToUInt16(compressedData, blockIndex);
                ushort color1 = BitConverter.ToUInt16(compressedData, blockIndex + 2);
                uint lookupTable = BitConverter.ToUInt32(compressedData, blockIndex + 4);

                // 색상 팔레트 생성
                Color32[] palette = new Color32[4];
                palette[0] = RGB565ToColor32(color0);
                palette[1] = RGB565ToColor32(color1);

                if (color0 > color1) // 4색상 모드
                {
                    palette[2] = InterpolateColor(palette[0], palette[1], 2, 3);
                    palette[3] = InterpolateColor(palette[0], palette[1], 1, 3);
                }
                else // 3색상 + 투명 모드
                {
                    palette[2] = InterpolateColor(palette[0], palette[1], 1, 2);
                    palette[3] = new Color32(0, 0, 0, 0);
                }

                // 4x4 블록의 각 픽셀 처리
                for (int y = 0; y < 4; y++) {
                    for (int x = 0; x < 4; x++) {
                        int pixelX = blockX * 4 + x;
                        int pixelY = blockY * 4 + y;

                        if (pixelX >= width || pixelY >= height)
                            continue;

                        // 2비트 컬러 인덱스 추출
                        int shift = (y * 4 + x) * 2;
                        int colorIndex = (int)((lookupTable >> shift) & 0x3);

                        pixels[pixelY * width + pixelX] = palette[colorIndex];
                    }
                }
            }
        }

        return pixels;
    }

    private static Color32 RGB565ToColor32(ushort color) {
        byte r = (byte)((color >> 11) * 255 / 31);
        byte g = (byte)(((color >> 5) & 0x3F) * 255 / 63);
        byte b = (byte)((color & 0x1F) * 255 / 31);
        return new Color32(r, g, b, 255);
    }

    private static Color32 InterpolateColor(Color32 c0, Color32 c1, int i, int denom) {
        return new Color32(
            (byte)((c0.r * (denom - i) + c1.r * i) / denom),
            (byte)((c0.g * (denom - i) + c1.g * i) / denom),
            (byte)((c0.b * (denom - i) + c1.b * i) / denom),
            255
        );
    }
}
