using System;

namespace EmsPlus.UI
{
    public abstract class WebMenu
    {
        public abstract string HtmlFileName { get; }
        public virtual int DefaultWidth => 900;
        public virtual int DefaultHeight => 580;
        public virtual float Scale => 1.0f;

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