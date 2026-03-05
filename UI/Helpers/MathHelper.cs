using System;

namespace EmsPlus.UI.Helpers
{
    public static class MathHelper
    {
        private static readonly Random _random = new Random();

        public static float Lerp(float a, float b, float t) => a + (b - a) * t;
        public static float EaseOutCubic(float t) => 1f - (float)Math.Pow(1f - t, 3);

        public static int Clamp(int v, int min, int max) => v < min ? min : (v > max ? max : v);
        public static float ClampFloat(float v, float min, float max) => v < min ? min : (v > max ? max : v);

        public static int GetRandomInteger(int max) => _random.Next(max);
        public static int GetRandomInteger(int min, int max) => _random.Next(min, max);
    }
}