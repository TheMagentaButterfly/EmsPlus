using System.IO;
using System.Windows.Forms;

namespace EmsPlus.Core
{
    public static class Assets
    {
        public static string GetPath(string fileName)
        {
            return Path.Combine(Application.StartupPath, "Plugins", "EmsPlus", "Assets", fileName);
        }
    }
}