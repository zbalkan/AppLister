using System.Runtime.InteropServices;
using System.Text;

namespace InventoryEngine.Tools
{
    internal static class MsiWrapper
    {
        [DllImport("msi.dll", SetLastError = true)]
        internal static extern int MsiEnumProducts(int iProductIndex, StringBuilder lpProductBuf);
    }
}