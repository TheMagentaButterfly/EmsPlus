using System;

namespace EmsPlus.UI.Helpers
{
    public abstract class WebMenu
    {
        public abstract string HtmlFileName { get; }

        public int CustomWidth { get; set; } = 900;
        public int CustomHeight { get; set; } = 580;
        public virtual float Scale => 1.0f;

        public virtual int DefaultWidth => (int)(CustomWidth * Scale);
        public virtual int DefaultHeight => (int)(CustomHeight * Scale);

        public abstract int OffsetX { get; set; }
        public abstract int OffsetY { get; set; }

        public bool IsActiveMenu => WebUIManager.ActiveMenu == this;
        public bool MouseUnlocked { get; set; } = false;

        public virtual void OnOpen() { }
        public virtual void OnClose() { }
        public virtual void OnProcess() { }
        public virtual void OnMessage(string action) { }
        public virtual void OnSavePosition() { }

        public void ExecuteScript(string script)
        {
            WebUIManager.ExecuteScript(script);
        }

        public void Close()
        {
            if (IsActiveMenu)
            {
                WebUIManager.CloseMenu();
            }
        }
    }
}