using Engine.Junk.Confidence;
using Engine.Junk.Finders;

namespace Engine.Junk.Containers
{
    internal abstract class JunkResultBase : IJunkResult
    {
        public ApplicationUninstallerEntry Application { get; }

        public ConfidenceCollection Confidence { get; }

        public IJunkCreator Source { get; }

        protected JunkResultBase(ApplicationUninstallerEntry application, IJunkCreator source) : this(application, source, new ConfidenceCollection())
        {
        }

        protected JunkResultBase(ApplicationUninstallerEntry application, IJunkCreator source, ConfidenceCollection confidence)
        {
            Application = application;
            Source = source;
            Confidence = confidence;
        }

        public abstract string GetDisplayName();

        public virtual string ToLongString() => $"{Application} - JunkRemover_Confidence: {Confidence.GetConfidence()} - {GetDisplayName()}";

        public override string ToString() => GetType().Name + " - " + GetDisplayName();
    }
}