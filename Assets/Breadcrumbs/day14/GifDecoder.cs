using UnityEngine;
using System;
using System.IO;
using System.Collections.Generic;

namespace Breadcrumbs.day14 {
    public class GifDecoder {
        // GIF 파일 헤더 구조체
        private struct GifHeader {
            public string Signature;
            public string Version;
            public ushort Width;
            public ushort Height;
            public byte PackedField;
            public byte BackgroundColorIndex;
            public byte PixelAspectRatio;
        }

        // 로컬 이미지 디스크립터
        private struct ImageDescriptor {
            public ushort Left;
            public ushort Top;
            public ushort Width;
            public ushort Height;
            public byte PackedField;
        }

        // 색상 팔레트
        public struct ColorPalette {
            public Color[] Colors;
        }

        // GIF 프레임 정보
        public class GifFrame {
            public Texture2D Texture;
            public int Delay; // 밀리초 단위
        }

        // GIF 디코딩 메인 메서드
        public List<GifFrame> DecodeGif(string filePath) {
            List<GifFrame> frames = new List<GifFrame>();
            byte transparencyIndex = 0;

            try {
                using (FileStream fs = new FileStream(filePath, FileMode.Open, FileAccess.Read))
                using (BinaryReader reader = new BinaryReader(fs)) {
                    // 기존 헤더 읽기 코드 유지
                    GifHeader header = ReadGifHeader(reader);
                    ColorPalette globalPalette = ReadColorPalette(reader, header);

                    while (true) {
                        byte blockIdentifier = reader.ReadByte();

                        if (blockIdentifier == 0x2C) // 이미지 구분자
                        {
                            ImageDescriptor imageDescriptor = ReadImageDescriptor(reader);
                            ColorPalette localPalette = ReadColorPalette(reader, imageDescriptor.PackedField);
                            ColorPalette activePalette = localPalette.Colors != null ? localPalette : globalPalette;

                            // LZW 최소 코드 크기
                            byte lzwMinCodeSize = reader.ReadByte();
                            byte[] lzwData = new byte[lzwMinCodeSize];
                            reader.Read(lzwData, 0, lzwData.Length);
                            // LZW 압축 해제
                            byte[] imageData = DecodeLZW(lzwData,
                                imageDescriptor.Width,
                                imageDescriptor.Height,
                                lzwMinCodeSize);

                            // 텍스처 생성
                            try {
                                Texture2D frameTexture = CreateTextureFromImageData(
                                    imageData,
                                    imageDescriptor.Width,
                                    imageDescriptor.Height,
                                    activePalette,
                                    transparencyIndex);

                                // 프레임 지연 시간 읽기
                                int delay = ReadGraphicControlExtension(reader, ref transparencyIndex);

                                frames.Add(new GifFrame {
                                    Texture = frameTexture,
                                    Delay = delay
                                });
                            }
                            catch (Exception ex) {
                                Debug.Log("텍스처 파싱중 예외 발생: " + ex.Message);
                                continue;
                            }
                        }
                        else if (blockIdentifier == 0x21) // 확장 블록
                        {
                            byte extensionType = reader.ReadByte();
                            if (extensionType == 0xF9) {
                                // 그래픽 컨트롤 확장 블록은 이미 처리됨
                                SkipBlock(reader);
                            }
                            else {
                                SkipBlock(reader);
                            }
                        }
                        else if (blockIdentifier == 0x3B) // GIF 종료
                        {
                            break;
                        }
                    }
                }

                return frames;
            }
            catch (Exception e) {
                Debug.LogError($"GIF 디코딩 중 오류 발생: {e.Message}");
                return frames;
            }
        }

        // GIF 헤더 읽기
        private GifHeader ReadGifHeader(BinaryReader reader) {
            GifHeader header = new GifHeader {
                Signature = new string(reader.ReadChars(3)),
                Version = new string(reader.ReadChars(3)),
                Width = reader.ReadUInt16(),
                Height = reader.ReadUInt16(),
                PackedField = reader.ReadByte(),
                BackgroundColorIndex = reader.ReadByte(),
                PixelAspectRatio = reader.ReadByte()
            };

            return header;
        }

        // 색상 팔레트 읽기
        private ColorPalette ReadColorPalette(BinaryReader reader, GifHeader header) {
            // 색상 팔레트 존재 여부 확인
            bool hasPalette = (header.PackedField & 0x80) != 0;
            if (!hasPalette) return new ColorPalette();

            int colorCount = 1 << ((header.PackedField & 0x07) + 1);
            Color[] colors = new Color[colorCount];

            for (int i = 0; i < colorCount; i++) {
                byte r = reader.ReadByte();
                byte g = reader.ReadByte();
                byte b = reader.ReadByte();
                colors[i] = new Color(r / 255f, g / 255f, b / 255f);
            }

            return new ColorPalette { Colors = colors };
        }

        // 오버로드된 색상 팔레트 읽기 메서드 (로컬 이미지 디스크립터용)
        private ColorPalette ReadColorPalette(BinaryReader reader, byte packedField) {
            bool hasPalette = (packedField & 0x80) != 0;
            if (!hasPalette) return new ColorPalette();

            int colorCount = 1 << ((packedField & 0x07) + 1);
            Color[] colors = new Color[colorCount];

            for (int i = 0; i < colorCount; i++) {
                byte r = reader.ReadByte();
                byte g = reader.ReadByte();
                byte b = reader.ReadByte();
                colors[i] = new Color(r / 255f, g / 255f, b / 255f);
            }

            return new ColorPalette { Colors = colors };
        }

        // 이미지 디스크립터 읽기
        private ImageDescriptor ReadImageDescriptor(BinaryReader reader) {
            return new ImageDescriptor {
                Left = reader.ReadUInt16(),
                Top = reader.ReadUInt16(),
                Width = reader.ReadUInt16(),
                Height = reader.ReadUInt16(),
                PackedField = reader.ReadByte()
            };
        }

        public static byte[] DecodeLZW(byte[] compressedData, int width, int height, byte lzwMinCodeSize) {
            List<List<byte>> dictionary = new List<List<byte>>();
            List<byte> decompressedData = new List<byte>();

            int clearCode = 1 << lzwMinCodeSize;
            int endCode = clearCode + 1;
            int nextCode = clearCode + 2;
            int currentCodeSize = lzwMinCodeSize + 1;

            // 초기 딕셔너리 설정
            for (int i = 0; i < clearCode; i++) {
                dictionary.Add(new List<byte> { (byte)i });
            }

            int currentBit = 0;
            int previousCode = -1;

            while (currentBit < compressedData.Length * 8) {
                // 현재 코드 읽기
                int currentCode = ReadBits(compressedData, currentBit, currentCodeSize);
                currentBit += currentCodeSize;

                // 종료 코드 확인
                if (currentCode == endCode)
                    break;

                // 클리어 코드 처리
                if (currentCode == clearCode) {
                    dictionary.Clear();
                    for (int i = 0; i < clearCode; i++) {
                        dictionary.Add(new List<byte> { (byte)i });
                    }

                    currentCodeSize = lzwMinCodeSize + 1;
                    nextCode = clearCode + 2;
                    previousCode = -1;
                    continue;
                }

                List<byte> currentSequence;

                // 새로운 코드 처리
                if (currentCode < dictionary.Count) {
                    currentSequence = dictionary[currentCode];
                    decompressedData.AddRange(currentSequence);

                    // 이전 코드가 유효한 경우에만 새 항목 추가
                    if (previousCode != -1 && previousCode < dictionary.Count) {
                        List<byte> newEntry = new List<byte>(dictionary[previousCode]);
                        newEntry.Add(currentSequence[0]);
                        dictionary.Add(newEntry);
                    }
                }
                else {
                    // 이전 코드가 유효한 경우에만 처리
                    if (previousCode != -1 && previousCode < dictionary.Count) {
                        currentSequence = new List<byte>(dictionary[previousCode]);
                        currentSequence.Add(currentSequence[0]);

                        decompressedData.AddRange(currentSequence);
                        dictionary.Add(currentSequence);
                    }
                    else {
                        // 예외적인 상황 처리
                        break;
                    }
                }

                previousCode = currentCode;

                // 코드 크기 조정
                if (nextCode < 4096 && dictionary.Count == (1 << currentCodeSize)) {
                    currentCodeSize++;
                }
            }

            // 결과 데이터를 2차원 픽셀 배열로 변환
            return ResizeImageData(decompressedData.ToArray(), width, height);
        }

        // 이전 비트 읽기와 ResizeImageData 메서드는 동일하게 유지

        private static int ReadBits(byte[] data, int startBit, int bitCount) {
            int result = 0;
            int endBit = startBit + bitCount;

            for (int i = startBit; i < endBit; i++) {
                int byteIndex = i / 8;
                int bitIndex = i % 8;

                if (byteIndex < data.Length) {
                    int bit = (data[byteIndex] >> bitIndex) & 1;
                    result |= (bit << (i - startBit));
                }
            }

            return result;
        }

        private static byte[] ResizeImageData(byte[] imageData, int width, int height) {
            byte[] resizedData = new byte[width * height];
            Array.Fill(resizedData, (byte)0);

            int sourceIndex = 0;
            for (int y = 0; y < height; y++) {
                for (int x = 0; x < width; x++) {
                    if (sourceIndex < imageData.Length) {
                        resizedData[y * width + x] = imageData[sourceIndex];
                        sourceIndex++;
                    }
                }
            }

            return resizedData;
        }

        // 픽셀 데이터 변환 로직
        private Texture2D CreateTextureFromImageData(byte[] imageData, int width, int height, ColorPalette palette,
            byte transparencyIndex) {
            Texture2D texture = new Texture2D(width, height, TextureFormat.RGBA32, false);
            Color[] pixels = new Color[width * height];

            for (int y = 0; y < height; y++) {
                for (int x = 0; x < width; x++) {
                    int index = y * width + x;
                    byte colorIndex = imageData[index];

                    if (colorIndex == transparencyIndex) {
                        // 투명 픽셀
                        pixels[index] = Color.clear;
                    }
                    else if (colorIndex < palette.Colors.Length) {
                        // 팔레트 색상 사용
                        pixels[index] = palette.Colors[colorIndex];
                    }
                    else {
                        // 잘못된 인덱스 처리
                        pixels[index] = Color.magenta;
                    }
                }
            }

            texture.SetPixels(pixels);
            texture.Apply();

            return texture;
        }

        // 그래픽 컨트롤 확장 메서드 업데이트 (투명도 처리 추가)
        private int ReadGraphicControlExtension(BinaryReader reader, ref byte transparencyIndex) {
            // 블록 크기
            reader.ReadByte();

            // 그래픽 컨트롤 플래그
            byte flags = reader.ReadByte();

            // 지연 시간 (1/100초 단위)
            int delayTime = reader.ReadUInt16() * 10; // 밀리초로 변환

            // 투명 색상 인덱스
            transparencyIndex = reader.ReadByte();

            // 블록 종료
            reader.ReadByte();

            // 투명도 처리 여부 확인
            bool hasTransparency = (flags & 0x01) != 0;
            if (!hasTransparency) {
                transparencyIndex = 0xFF; // 투명도 없음
            }

            return delayTime;
        }

        // 블록 건너뛰기
        private void SkipBlock(BinaryReader reader) {
            byte blockSize;
            while ((blockSize = reader.ReadByte()) != 0) {
                reader.ReadBytes(blockSize);
            }
        }

        // 팔레트 텍스처 저장
        public void SavePaletteTexture(ColorPalette palette, string fileName) {
            if (palette.Colors == null || palette.Colors.Length == 0) return;

            Texture2D paletteTexture = new Texture2D(palette.Colors.Length, 1, TextureFormat.RGBA32, false);
            paletteTexture.SetPixels(palette.Colors);
            paletteTexture.Apply();

            byte[] pngBytes = paletteTexture.EncodeToPNG();
            File.WriteAllBytes(
                Path.Combine(Application.persistentDataPath, fileName),
                pngBytes
            );
        }
    }

    /*
    // 사용 예시
    public class GifProcessorExample : MonoBehaviour {
        void Start() {
            string gifPath = Path.Combine(Application.streamingAssetsPath, "example.gif");

            GifDecoder decoder = new GifDecoder();
            List<GifDecoder.GifFrame> frames = decoder.DecodeGif(gifPath);

            foreach (var frame in frames) {
                Debug.Log($"Frame Texture: {frame.Texture}, Delay: {frame.Delay}ms");
            }
        }
    }
    // */
}
