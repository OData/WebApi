namespace Nuwa.Sdk
{
    /// <summary>
    /// IRunElement defines a factor in RunFrame. The factory could be host strategy, 
    /// trace strategy, security strategy and so or.
    /// 
    /// IRunElement is created by IRunElementPerceiver from Attributes or other clues
    /// regarding to test run configuration. IRunElements are shared between RunFrame
    /// which means the should NOT contains any life cycle sensitve instance, because
    /// it will be initialized multiple times.
    /// 
    /// IRunElement is initialized only once by RunFrame when the first corresponding
    /// test method is about to run. The common pattern of the initialization is a 
    /// run element initialize an object and save the objhect in the given RunFrame.
    /// 
    /// After initialized, for every test method run the Recovery of IRunElement will
    /// be called to recover the test scene, meaning set value back to the test class's
    /// properties.
    /// 
    /// When RunFrame request cleanup, it retrieve the objects from RunFrame and 
    /// uninitialize base on them
    /// </summary>
    public interface IRunElement
    {
        /// <summary>
        /// Discription of this RunFrameElement
        /// </summary>
        string Name { get; }

        /// <summary>
        /// Initialize the run frame.
        /// </summary>
        void Initialize(RunFrame frame);

        /// <summary>
        /// Recover test scene
        /// </summary>
        void Recover(object testClass, NuwaTestCommand testCommand);

        /// <summary>
        /// Clean up the test command and run frame.
        /// </summary>
        /// <param name="frame">the run frame to be initialized</param>
        void Cleanup(RunFrame frame);
    }
}