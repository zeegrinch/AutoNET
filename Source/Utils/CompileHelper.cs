using System;
using System.IO;
using log4net;
using System.CodeDom.Compiler;
using System.Text;
using System.Threading.Tasks;

namespace AutoNet.Utils
{
    /// <summary>
    /// light wrapper around System.CodeDom.Compiler utilities; attempts to compile a (single-file) C# class into an assembly
    /// to allow for reflection and dynamic interactions with the object (instance of that class)
    /// </summary>
    class CompileHelper
    {
        private readonly ILog _logger;
        private readonly string _sourceFile;

        public CompileHelper(ILog logger, string sourceFile)
        {
            _sourceFile = sourceFile;
            _logger = logger;
        }


        /// <summary>
        /// Invokes the System.CodeDom compilation for C#, intention is to produce an assmebly and subsequently 
        /// use reflection for interacting with object instances dynamically.
        /// </summary>
        /// <returns>true/false if compilation succeeds</returns>
        public bool Compile(bool skipIfExists = false)
        {
            //output file name (assembly)
            string outputFileName = string.Format(@"{0}\\{1}.dll", Path.GetDirectoryName(_sourceFile), Path.GetFileNameWithoutExtension(_sourceFile));
            string errorFileName  = string.Format(@"{0}\\{1}.error.txt", Path.GetDirectoryName(_sourceFile), Path.GetFileNameWithoutExtension(_sourceFile));

            if(File.Exists(errorFileName))  //from a previous attempt
            {
                File.Delete(errorFileName);
            }

            if(skipIfExists && File.Exists(outputFileName))
            {
                _logger.Info("Skipping compilation for existing binary:" + outputFileName);
                return true;
            }

            CodeDomProvider provider = CodeDomProvider.CreateProvider("CSharp");
            CompilerParameters options = new CompilerParameters
            {
                GenerateExecutable = false, // compile as library (dll)
                OutputAssembly = outputFileName,
                GenerateInMemory = false,   // file on disk
            };

            try
            {
                CompilerResults results = provider.CompileAssemblyFromFile(options, _sourceFile);
                if (results.Errors.Count > 0)
                {
                    StringBuilder errorDetails = new StringBuilder();
                    errorDetails.AppendFormat("Errors building {0} into {1}", _sourceFile, outputFileName);
                    foreach (CompilerError error in results.Errors)
                        errorDetails.Append("  ≡ {0}" + error.ToString());

                    _logger.Error(errorDetails.ToString());

                    File.WriteAllText(errorFileName, errorDetails.ToString());
                    return false;
                }
                else
                {
                    _logger.InfoFormat("Source {0} built into {1} successfully.", _sourceFile, results.PathToAssembly);
                    return true;
                }
            }
            catch(Exception ex)
            {
                _logger.Error(ex);

                File.WriteAllText(errorFileName, ex.ToString());
                return false;
            }
}
    }
}
