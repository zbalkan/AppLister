using System.Runtime.InteropServices;
using System.Text;

namespace InventoryEngine.Tools
{
    internal static class MsiWrapper
    {
        // ------------------------------------------------------------------------- Record object
        // functions -------------------------------------------------------------------------

        // Create a new record object with the requested number of fields Field 0, not included in
        // count, is used for format strings and op codes All fields are initialized to null Returns
        // a handle to the created record, or 0 if memory could not be allocated

        [DllImport("msi.dll", SetLastError = true)]
        internal static extern int MsiEnumProducts(int iProductIndex, StringBuilder lpProductBuf);
    }
}