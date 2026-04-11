using Rage;
using System;
using System.Drawing;
using System.IO;
using EmsPlus.UI.Helpers;
using EmsPlus.Core;

namespace EmsPlus.Custom.TaskMenu
{
    public class UIElement : IDisposable
    {
        private Texture _texture;
        private float _hoverScale = 1f;
        private float _dragAlpha = 1f;

        public string Name { get; }
        public RectangleF Bounds { get; set; }
        public bool IsHovered { get; private set; }
        public bool IsBeingDragged { get; set; }
        public bool IsDropZone { get; set; }
        public bool Enabled { get; set; } = true;
        public Color TintColor { get; set; } = Color.White;

        public UIElement(string name, string path, RectangleF bounds)
        {
            Name = name;
            Bounds = bounds;
            ChangeTexture(path);
        }

        public void ChangeTexture(string path)
        {
            _texture = null;
            if (string.IsNullOrEmpty(path)) return;
            string fullPath = Assets.GetPath(path);
            if (File.Exists(fullPath)) _texture = Game.CreateTextureFromFile(fullPath);
        }

        public void Update(PointF mouse)
        {
            if (!Enabled) { IsHovered = false; return; }
            IsHovered = Bounds.Contains(mouse);
            _hoverScale = Lerp(_hoverScale, IsHovered && !IsBeingDragged ? 1.08f : 1f, 0.2f);
            _dragAlpha = IsBeingDragged ? 0.7f : 1f;
        }

        public void DrawNativeShadow()
        {
            if (!Enabled) return;
            RectangleF rect = GetScaledBounds();
            if (rect.Width <= 0) return;

            NativeUITools.DrawNativeRect(rect.X + 4, rect.Y + 4, rect.Width, rect.Height, Color.FromArgb(100, 0, 0, 0));
        }

        public void DrawNativeTexture(Rage.Graphics g)
        {
            if (!Enabled) return;
            RectangleF rect = GetScaledBounds();
            if (rect.Width <= 0) return;

            if (_texture != null)
            {
                g.DrawTexture(_texture, rect);
            }
            else
            {
                NativeUITools.DrawNativeRect(rect.X, rect.Y, rect.Width, rect.Height, Color.FromArgb(150, 45, 50, 60));
            }
        }

        public void DrawNativeOverlays()
        {
            if (!Enabled) return;
            RectangleF rect = GetScaledBounds();
            if (rect.Width <= 0) return;

            int alpha = (int)(255 * _dragAlpha);

            if (_texture != null && TintColor != Color.White)
            {
                NativeUITools.DrawNativeRect(rect.X, rect.Y, rect.Width, rect.Height, Color.FromArgb(alpha / 2, TintColor));
            }

            if (IsHovered || IsDropZone)
            {
                Color c = IsDropZone ? Color.FromArgb(alpha, 0, 180, 255) : Color.FromArgb(alpha, 255, 255, 255);
                float thick = 2.5f;
                NativeUITools.DrawNativeRect(rect.X, rect.Y, rect.Width, thick, c);
                NativeUITools.DrawNativeRect(rect.X, rect.Y + rect.Height - thick, rect.Width, thick, c);
                NativeUITools.DrawNativeRect(rect.X, rect.Y, thick, rect.Height, c);
                NativeUITools.DrawNativeRect(rect.X + rect.Width - thick, rect.Y, thick, rect.Height, c);
            }
        }

        private float Lerp(float a, float b, float t) => a + (b - a) * t;
        public void Dispose() => _texture = null;
        private RectangleF GetScaledBounds() => _hoverScale == 1f ? Bounds :
            new RectangleF(Bounds.X - (Bounds.Width * (_hoverScale - 1) / 2), Bounds.Y - (Bounds.Height * (_hoverScale - 1) / 2),
                           Bounds.Width * _hoverScale, Bounds.Height * _hoverScale);
    }
}