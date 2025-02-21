namespace InventoryEngine.Junk.Confidence
{
    /// <summary>
    ///     Universal confidence pieces
    /// </summary>
    internal static class ConfidenceRecords
    {
        public static readonly ConfidenceRecord AllSubdirsMatched = new ConfidenceRecord(4, "ConfidencePart_AllSubdirsMatched");

        public static readonly ConfidenceRecord CompanyNameDidNotMatch = new ConfidenceRecord(-2, "ConfidencePart_CompanyNameDidNotMatch");

        public static readonly ConfidenceRecord CompanyNameMatch = new ConfidenceRecord(4, "ConfidencePart_CompanyNameMatch");

        public static readonly ConfidenceRecord DirectoryStillUsed = new ConfidenceRecord(-7, "ConfidencePart_DirectoryStillUsed");

        public static readonly ConfidenceRecord ExecutablesArePresent = new ConfidenceRecord(-4, "Confidence_PF_ExecsPresent");

        public static readonly ConfidenceRecord ExplicitConnection = new ConfidenceRecord(4, "ConfidencePart_ExplicitConnection");

        public static readonly ConfidenceRecord FilesArePresent = new ConfidenceRecord(0, "Confidence_PF_FilesPresent");

        public static readonly ConfidenceRecord FolderHasNoSubdirectories = new ConfidenceRecord(2, "Confidence_PF_NoSubdirs");

        public static readonly ConfidenceRecord IsEmptyFolder = new ConfidenceRecord(4, "Confidence_PF_EmptyFolder");

        public static readonly ConfidenceRecord IsStoreApp = new ConfidenceRecord(-10, "ConfidencePart_IsStoreApp");

        public static readonly ConfidenceRecord IsUninstallerRegistryKey = new ConfidenceRecord(20, "ConfidencePart_IsUninstallerRegistryKey");

        public static readonly ConfidenceRecord ItemNameEqualsCompanyName = new ConfidenceRecord(-2, "ConfidencePart_ItemNameEqualsCompanyName");

        public static readonly ConfidenceRecord ManyFilesArePresent = new ConfidenceRecord(-2, "Confidence_PF_ManyFilesPresent");

        public static readonly ConfidenceRecord ProductNameDodgyMatch = new ConfidenceRecord(-2, "ConfidencePart_ProductNameDodgyMatch");

        public static readonly ConfidenceRecord ProductNamePerfectMatch = new ConfidenceRecord(2, "ConfidencePart_ProductNamePerfectMatch");

        public static readonly ConfidenceRecord ProgramNameIsStillUsed = new ConfidenceRecord(-4, "Confidence_PF_NameIsUsed");

        public static readonly ConfidenceRecord PublisherIsStillUsed = new ConfidenceRecord(-4, "Confidence_PF_PublisherIsUsed");

        public static readonly ConfidenceRecord QuestionableDirectoryName = new ConfidenceRecord(-3, "ConfidencePart_QuestionableDirectoryName");

        public static readonly ConfidenceRecord UsedBySimilarNamedApp = new ConfidenceRecord(-2, "Confidence_UsedBySimilarNamedApp");
    }
}