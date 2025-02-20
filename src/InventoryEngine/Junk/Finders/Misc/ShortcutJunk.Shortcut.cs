namespace InventoryEngine.Junk.Finders.Misc
{
    internal partial class ShortcutJunk
    {
        private sealed class Shortcut
        {
            public string LinkFilename { get; }

            public string LinkTarget { get; }

            public Shortcut(string linkFilename, string linkTarget)
            {
                LinkFilename = linkFilename;
                LinkTarget = linkTarget;
            }

            public override string ToString() => LinkTarget;
        }
    }
}