using InventoryEngine.Junk.Confidence;
using InventoryEngine.Junk.Finders;

namespace InventoryEngine.Junk.Containers
{
    public interface IJunkResult
    {
        /// <summary>
        ///     Uninstaller this entry belongs to
        /// </summary>
        ApplicationUninstallerEntry Application { get; }

        /// <summary>
        ///     Confidence that this entry is safe to remove
        /// </summary>
        ConfidenceCollection Confidence { get; }

        /// <summary>
        ///     Origin of this junk
        /// </summary>
        IJunkCreator Source { get; }

        string GetDisplayName();

        /// <summary>
        ///     Get extended information with overall confidence information.
        /// </summary>
        string ToLongString();
    }
}