namespace RabbitRPC.ServiceHost.Filters
{
    /// <summary>
    /// Contains constant values for known filter scopes.
    /// </summary>
    /// <remarks>
    /// Scope defines the ordering of filters that have the same order. Scope is by-default defined by how a filter is registered.
    /// </remarks>
    public static class FilterScope
    {
        /// <summary>
        /// First filter scope.
        /// </summary>
        public static readonly int First;

        /// <summary>
        /// Global filter scope.
        /// </summary>
        public static readonly int Global = 10;

        /// <summary>
        /// Service filter scope.
        /// </summary>
        public static readonly int Service = 20;

        /// <summary>
        /// Action filter scope.
        /// </summary>
        public static readonly int Action = 30;

        /// <summary>
        /// Last filter scope.
        /// </summary>
        public static readonly int Last = 100;
    }
}
