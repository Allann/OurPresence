﻿// Copyright (c)  Allan Nielsen.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;

namespace OurPresence.Modeller.Interfaces
{
    /// <summary>
    /// Defines the files to be used as a project
    /// </summary>
    public interface IProject : IOutput
    {
        /// <summary>
        /// The files within the project that should be generated
        /// </summary>
        IEnumerable<IFileGroup> FileGroups { get; }

        /// <summary>
        /// The files within the project that should be copied
        /// </summary>
        IEnumerable<IFolderCopy> Folders { get; }

        /// <summary>
        /// Relative path if used within a <see cref="ISolution"/> otherwise a full path
        /// </summary>
        string Path { get; set; }

        /// <summary>
        /// Project Id as used by Visual Studio
        /// </summary>
        Guid Id { get; }

        IFileGroup AddFileGroup(IFileGroup file);

        void AddFolder(IFolderCopy folder);
    }

}
