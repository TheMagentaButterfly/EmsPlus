using Rage;
using System.Drawing;

namespace EmsPlus.UI.Helpers
{
    public static class RenderHelper
    {
        public static void Draw3DPrompt(Rage.Graphics g, string text, Vector3 worldPosition, Color color, float scale = 0.35f, int alpha = 255)
        {
            Vector2 screenPos = World.ConvertWorldPositionToScreenPosition(worldPosition);

            if (screenPos == Vector2.Zero || screenPos.X < 0 || screenPos.X > Game.Resolution.Width ||
                screenPos.Y < 0 || screenPos.Y > Game.Resolution.Height) return;

            float textWidth = NativeUITools.MeasureNativeTextWidth(text, scale);
            float bgPadding = 12f;
            float bgHeight = 28f;
            float bgY = screenPos.Y - bgHeight / 2;

            // Background panel with inspection menu style
            NativeUITools.DrawNativeRect(
                screenPos.X - textWidth / 2 - bgPadding,
                bgY,
                textWidth + bgPadding * 2,
                bgHeight,
                Color.FromArgb(Helpers.MathHelper.Clamp((int)(alpha * 0.86f), 0, 255), 12, 18, 28)
            );

            // Accent border (left edge)
            NativeUITools.DrawNativeRect(
                screenPos.X - textWidth / 2 - bgPadding,
                bgY,
                2,
                bgHeight,
                Color.FromArgb(alpha, color.R, color.G, color.B)
            );

            // Subtle top highlight
            NativeUITools.DrawNativeRect(
                screenPos.X - textWidth / 2 - bgPadding,
                bgY,
                textWidth + bgPadding * 2,
                1,
                Color.FromArgb(Helpers.MathHelper.Clamp(alpha / 3, 0, 255), 255, 255, 255)
            );

            float textY = bgY;

            // Draw shadow for text depth
            NativeUITools.DrawNativeText(
                text,
                screenPos.X + 1,
                textY + 1,
                scale,
                Color.FromArgb(Helpers.MathHelper.Clamp(alpha / 2, 0, 255), 0, 0, 0),
                true
            );

            // Draw main text
            NativeUITools.DrawNativeText(
                text,
                screenPos.X,
                textY,
                scale,
                Color.FromArgb(alpha, 255, 255, 255),
                true
            );
        }
    }
}