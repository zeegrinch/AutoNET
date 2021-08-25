using System;
using System.Collections.Generic;
using System.Reflection;
using log4net;
using AutoNet.Common;

namespace AutoNet.Utils
{
    class ReflectionHelper
    {
        private readonly ILog _logger;
        private readonly string _binFile; //a DLL or EXE assembly

        public ReflectionHelper(ILog logger, string binFile)
        {
            _binFile = binFile;
            _logger = logger;
        }

        public string SourceFile { get; set; }

        /// <summary>
        /// loads a specific assembly from its binary image on disk (in the current App domain)
        /// </summary>
        public void LoadAssembly(Dictionary<string, ClassInfo> typesMap)
        {
            try
            {
                Assembly assembly = Assembly.LoadFrom(_binFile);

                foreach( Type t in assembly.ExportedTypes)
                {
                    _logger.Info("Loading type:" + t.FullName);

                    if( !typesMap.ContainsKey(t.FullName) )
                    {
                        string type = (t.IsValueType && !t.IsEnum) ? " Struct " : " Class ";
                        if (t.IsEnum)
                            type = " Enum ";

                        typesMap.Add(t.FullName, new ClassInfo
                        {
                            SourceFile = this.SourceFile,
                            AssemblyFile = _binFile,
                            TypeName = t.Name,
                            TypeFQName = t.FullName,
                            ArtifactType = type,
                            AssemblyContainer = assembly,
                            CLSType = t
                        });
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.Error("There has been an error loading the assembly image:" + _binFile, ex);
            }
        }



    }
}
