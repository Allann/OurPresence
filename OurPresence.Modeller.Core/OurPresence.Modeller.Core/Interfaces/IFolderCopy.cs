﻿namespace OurPresence.Modeller.Interfaces
{
    /// <summary>
    /// An output type that represents a folder to copy.  No changes will be made to the files.
    /// </summary>
    public interface IFolderCopy : IOutput
    {
        /// <summary>
        /// The source folder name
        /// </summary>
        string Source { get; }

        /// <summary>
        /// The destination folder name
        /// </summary>
        string Destination { get; }

        /// <summary>
        /// Flag whether to include sub folders
        /// </summary>
        bool IncludeSubFolders { get; }

        void SetPath(string path);
    }

}
