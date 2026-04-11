using Rage;
using Rage.Native;
using System.Drawing;

namespace EmsPlus.UI.Helpers
{
    public static class NativeUITools
    {
        public static void DrawNativeRect(float x, float y, float w, float h, Color col)
        {
            float resX = Game.Resolution.Width;
            float resY = Game.Resolution.Height;

            float finalX = (x + (w / 2f)) / resX;
            float finalY = (y + (h / 2f)) / resY;
            float finalW = w / resX;
            float finalH = h / resY;

            NativeFunction.Natives.DRAW_RECT(finalX, finalY, finalW, finalH, col.R, col.G, col.B, col.A);
        }

        public static void DrawNativeText(string text, float x, float y, float scale, Color col, bool centered = false, int fontId = 4)
        {
            float resX = Game.Resolution.Width;
            float resY = Game.Resolution.Height;

            try
            {
                NativeFunction.Natives.SET_TEXT_FONT(fontId);
                NativeFunction.Natives.SET_TEXT_SCALE(scale, scale);
                NativeFunction.Natives.SET_TEXT_COLOUR(col.R, col.G, col.B, col.A);

                if (centered)
                    NativeFunction.Natives.SET_TEXT_CENTRE(true);

                NativeFunction.CallByHash<uint>(0x25FBB336DF1804CB, "STRING");
                NativeFunction.Natives.ADD_TEXT_COMPONENT_SUBSTRING_PLAYER_NAME(text);
                NativeFunction.CallByHash<uint>(0xCD015E5BB0D96A57, x / resX, y / resY);
            }
            catch (System.Exception ex)
            {
                Game.Console.Print($"[EmsPlus] Error: {ex.Message}");
            }
        }

        public static void DrawNativeSprite(string dict, string name, float x, float y, float w, float h, float heading, Color col)
        {
            if (!NativeFunction.Natives.HAS_STREAMED_TEXTURE_DICT_LOADED<bool>(dict))
            {
                NativeFunction.Natives.REQUEST_STREAMED_TEXTURE_DICT(dict, false);
                return;
            }

            float resX = Game.Resolution.Width;
            float resY = Game.Resolution.Height;

            float finalX = (x + (w / 2f)) / resX;
            float finalY = (y + (h / 2f)) / resY;
            float finalW = w / resX;
            float finalH = h / resY;

            NativeFunction.Natives.DRAW_SPRITE(dict, name, finalX, finalY, finalW, finalH, heading, col.R, col.G, col.B, col.A);
        }

        public static float MeasureNativeTextWidth(string text, float scale, int fontId = 4)
        {
            if (string.IsNullOrEmpty(text)) return 0f;

            float baseFactor = 0.014f;

            float estimatedWidth = text.Length * baseFactor * scale;

            return estimatedWidth * Game.Resolution.Width;
        }

        public static void DrawNativeTexture(Texture tex, float x, float y, float w, float h, Color col)
        {
            if (tex == null) return;

            float resX = Game.Resolution.Width;
            float resY = Game.Resolution.Height;

            float finalX = (x + (w / 2f)) / resX;
            float finalY = (y + (h / 2f)) / resY;
            float finalW = w / resX;
            float finalH = h / resY;

            Game.DisplaySubtitle("", 0);
        }
    }
}