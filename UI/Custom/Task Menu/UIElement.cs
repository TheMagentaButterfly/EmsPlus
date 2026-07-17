using Rage;
using System;
using System.Drawing;
using System.IO;
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
            if (_texture != null) { _texture = null; }
            if (string.IsNullOrEmpty(path)) return;
            string fullPath = Assets.GetPath(path);
            if (File.Exists(fullPath)) _texture = Game.CreateTextureFromFile(fullPath);
        }

        public void Update(PointF mouse)
        {
            if (!Enabled) { IsHovered = false; return; }
            IsHovered = Bounds.Contains(mouse);
            _hoverScale = UI.Helpers.MathHelper.Lerp(_hoverScale, IsHovered && !IsBeingDragged ? 1.08f : 1f, 0.2f);
            _dragAlpha = IsBeingDragged ? 0.7f : 1f;
        }

        public void Draw(Rage.Graphics g)
        {
            if (!Enabled) return;
            RectangleF rect = GetScaledBounds();
            if (rect.Width <= 0) return;

            int alpha = (int)(255 * _dragAlpha);

            g.DrawRectangle(new RectangleF(rect.X + 4, rect.Y + 4, rect.Width, rect.Height), Color.FromArgb(100, 0, 0, 0));

            if (_texture != null)
            {
                g.DrawTexture(_texture, rect);
            }
            else
            {
                g.DrawRectangle(rect, Color.FromArgb(alpha, 45, 50, 60));
            }

            if (_texture != null && TintColor != Color.White)
            {
                g.DrawRectangle(rect, Color.FromArgb(alpha / 2, TintColor));
            }

            if (IsHovered || IsDropZone)
            {
                Color c = IsDropZone ? Color.FromArgb(alpha, 0, 180, 255) : Color.FromArgb(alpha, 255, 255, 255);
                float thick = 3f;
                g.DrawRectangle(new RectangleF(rect.X, rect.Y, rect.Width, thick), c);
                g.DrawRectangle(new RectangleF(rect.X, rect.Y + rect.Height - thick, rect.Width, thick), c);
                g.DrawRectangle(new RectangleF(rect.X, rect.Y, thick, rect.Height), c);
                g.DrawRectangle(new RectangleF(rect.X + rect.Width - thick, rect.Y, thick, rect.Height), c);
            }
        }

        public void Dispose() { if (_texture != null) { _texture = null; } }
        private RectangleF GetScaledBounds() => _hoverScale == 1f ? Bounds :
            new RectangleF(Bounds.X - (Bounds.Width * (_hoverScale - 1) / 2), Bounds.Y - (Bounds.Height * (_hoverScale - 1) / 2),
                           Bounds.Width * _hoverScale, Bounds.Height * _hoverScale);
    }
}