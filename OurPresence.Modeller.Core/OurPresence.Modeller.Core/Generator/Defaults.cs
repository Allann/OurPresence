﻿using System;
using System.IO;

namespace OurPresence.Modeller.Generator
{
    public static class Defaults
    {
        public static string OutputFolder => Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "Visual Studio 2017", "Projects");

        public static string LocalFolder => Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Company", "Generators");

        public static string ServerFolder => Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Resources), "Company", "Generators");

        public static string Target => Targets.Default;

        public static GeneratorVersion Version => new GeneratorVersion();
    }

}
