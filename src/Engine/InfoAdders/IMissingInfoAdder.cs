namespace Engine.InfoAdders
{
    public interface IMissingInfoAdder
    {
        /// <summary>
        ///     Always run this adder if the requirements are met. If false, only run this adder if
        ///     it can produce any missing values.
        /// </summary>
        bool AlwaysRun { get; }

        /// <summary>
        ///     Names of values this InfoAdder can fill in.
        /// </summary>
        string[] CanProduceValueNames { get; }

        /// <summary>
        ///     Higher priority InfoAdders are executed first.
        /// </summary>
        InfoAdderPriority Priority { get; }

        /// <summary>
        ///     Names of values this InfoAdder requires to work.
        /// </summary>
        string[] RequiredValueNames { get; }

        /// <summary>
        ///     True if all Required Values need to be defined. False if only one needs to be defined.
        /// </summary>
        bool RequiresAllValues { get; }

        /// <summary>
        ///     Try to add missing information to the target entry.
        /// </summary>
        void AddMissingInformation(ApplicationUninstallerEntry target);
    }
}