namespace PSOK.Kernel
{
    /// <summary>
    /// Specifies an operation performed on a bucket.
    /// </summary>
    public enum BucketOperation
    {
        /// <summary>
        /// Specifies that a contact has been updated in a bucket.
        /// </summary>
        Update = 0,

        /// <summary>
        /// Specifies that a contact has been added to a bucket.
        /// </summary>
        Add = 100,

        /// <summary>
        /// Specifies that a contact has been removed from a bucket.
        /// </summary>
        Remove = 200,

        /// <summary>
        /// Specifies that a contact was discarded and not inserted in a bucket.
        /// </summary>
        Discard = 300
    }
}
