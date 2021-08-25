using System;
using System.Reflection;
using System.Collections.Generic;
using log4net;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using AutoNet.Utils;

namespace AutoNet.Common
{
   
    /// <summary>
    /// Simple abstraction that maintains state: current class & instance
    /// Enumerates source-files and|or assemblies; compiles & parses the metadata 
    /// Caches some CLR/CTS attributes (the type-objects) up front
    /// </summary>
    public class SessionManager : IDisposable
    {
        private readonly ILog _logger;
        private readonly string _sourceFolder;
        private CancellationTokenSource _cancelTokeSrc;
        private Dictionary<string, ClassInfo> _types = new Dictionary<string, ClassInfo>();

        private readonly CLIHelper _cliHelper;

        private Type _currObjectType = null;
        private Object _currObject = null;

        
        /// $TODO$: refactor (move these constants into a common struct)
        private const string NOCONTEXTPROMPT = "[Ready:No context] >";
        
        public SessionManager(ILog logger, string sourceFolder, CLIHelper cli)
        {
            _logger = logger;
            _sourceFolder = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + "/" + sourceFolder;
            _cliHelper = cli;
            _cancelTokeSrc = new CancellationTokenSource();
            
        }

        public string CurrentType => _currObjectType?.ToString();
        public Object CurrentObject => _currObject;


        #region Interactions
        /// <summary>
        /// simple (primitive?) command interpreter for the session object
        /// 
        /// $Set "Type_Identifier"  :swithces the current object
        /// $Refresh                :refresh its type map
        /// $Quit                   :quit 
        /// </summary>
        /// <returns>true/false if execution succeeds</returns>
        public bool Command(Command cmd)
        {
            switch (cmd.SesionCommand)
            {
                case "QUIT":
                    _cliHelper.ShowMessage("\tBye !", CLIHelper.Feedback.Echo);
                    return false;

                case "MAP":
                    DumpMap();
                    break;

                case "SET":
                    SwitchContext(cmd.arguments.Pop() as string);
                    break;

                default:
                    _cliHelper.ShowMessage($"\tInvalid Session command [{cmd.SesionCommand}] !", CLIHelper.Feedback.Error);
                    break;
            }
            return true;
        }


        /// <summary>
        /// writes out all the types loaded from available assemblies, and -in this version- all available public properties for each type
        /// </summary>
        public void DumpMap()
        {
            foreach( var assemblyKey in _types.Keys)
            {
                _cliHelper.ShowMessage($"[{assemblyKey}] - {_types[assemblyKey].ArtifactType}", CLIHelper.Feedback.Success);
                PropertyInfo[] arrProperties;
                //get all public properties
                arrProperties = _types[assemblyKey].AssemblyContainer.GetType(assemblyKey).GetProperties();
                foreach( var prop in arrProperties)
                {
                    _cliHelper.ShowMessage("\t" + prop.ToString(), CLIHelper.Feedback.Dull);
                }
            }
        }

        /// <summary>
        /// loads an instance of the desired type, creates an instance and switches the 'context' (the subject Object for all GET/SET/INVOKE commands)
        /// </summary>
        /// <param name=""></param>
        /// <returns></returns>
        private bool SwitchContext(string newCurrentType) 
        {
            _logger.Info("Attempting to switch context to a new instance: " + newCurrentType);
            if(!_types.ContainsKey(newCurrentType) )
            {
                //failed to find by FQN, try by short-name (class name) ?
                _cliHelper.ShowMessage($"Unable to locate the type [{newCurrentType}] in current session ! Please use $Map to verify loaded types.", CLIHelper.Feedback.Warning);
                return false;
            }
            _currObjectType = (_types[newCurrentType]).CLSType;
            _cliHelper.Prompter = $"[{_currObjectType.Name}] >";
            _cliHelper.RefreshPrompt(withNewLine:true);

            try
            {
                _currObject =  Activator.CreateInstance(_currObjectType);
            }
            catch (Exception ex)
            {
                _logger.Error($"Unable to create an instance of type[{ newCurrentType}]", ex);
                _cliHelper.ShowMessage($"Unable to create an instance of type [{newCurrentType}]! Please inspect the logs.", CLIHelper.Feedback.Error);
                return false;
            }

            return true;
        }

        #endregion 

        #region Lifecycle
        public async Task Init()
        {
            await Refresh(bWithPurge: false);
            _cliHelper.Prompter = NOCONTEXTPROMPT;
            _cliHelper.RefreshPrompt();
        }

        public void Dispose()
        {
            //cleanup & release resource
            
        }

        /// <summary>
        /// purges the current state: clears the map wih all available types
        /// 
        /// Note:   there is no way to "unload" assemblies in .Net - that's why a separate APP domain for a more serious app
        ///         is the way to go (one cleans-up by tearing that App Domain down)
        /// </summary>
        public void Recycle()
        {
            _logger.Info("Initializing a new session (reycle)...");
            _currObjectType = null;
            _currObject = null;  
            _types.Clear();
        }
        
                
        /// <summary>
        /// Reloads the object-type map; this may be a slow/long running operation 
        /// Each source file will be (sequentially) compiled on a separate thread (from the CLR thread pool)
        /// </summary>
        public async Task Refresh(bool bWithPurge = true)
        {
            if (bWithPurge)
                Recycle();

            try
            {
                foreach(var srcFile in ScanSources())
                {
                    //attempt a compile (best effort: no retries or attempt to correct anything)
                    Task<bool> compileTask = Task.Run(() => CompileSource(_cancelTokeSrc.Token, srcFile), _cancelTokeSrc.Token);

                    await compileTask;
                    if(compileTask.Result != true)
                    {
                        _logger.WarnFormat("Compiling {0} has failed. Please inspect the log for additional details.", srcFile);
                        continue;
                    }

                    //update the type-map: load all available DLLs
                    Task<bool> reflectionTask = Task.Run(() => ParseBinary(_cancelTokeSrc.Token, srcFile), _cancelTokeSrc.Token);
                    await reflectionTask;
                    if (reflectionTask.Result != true)
                    {
                        _logger.ErrorFormat("Loading/Parsing assembly for {0} has failed. Please inspect the log for additional details.", srcFile);
                        continue;
                    }
                }
            }
            catch(Exception ex)
            {
                _logger.Error(ex);
            }
        }
        #endregion

        #region Internal_Ops
        /// <summary>
        /// enumerates source file(s) in the specified folder (caller will attempt to compile)
        /// </summary>
        private IEnumerable<string> ScanSources()
        {
            _logger.Info($"Scanning the {_sourceFolder} folder for source files...");
            DirectoryInfo dirInfo = new DirectoryInfo(_sourceFolder);
            var srcFiles = dirInfo.GetFiles("*.cs");

            foreach ( var fi in srcFiles)
            {
                _logger.Info(fi.Name);
                yield return fi.FullName;
            }
        }

        /// <summary>
        /// Takes a source file (C#) and attemps to compile into an assembly
        /// </summary>
        /// <param name="sourceFile">a C# compile unit (class definition file)</param>
        public bool CompileSource(CancellationToken ct, string sourceFile)
        {
            _logger.DebugFormat("Attempt to compile source file {0}", sourceFile);
            CompileHelper compiler = new CompileHelper(_logger, sourceFile);
            if (!compiler.Compile(skipIfExists:true))  
            {
                //decide if we want to alert the user or rely just on logs/traces
                return false;
            }
            return true;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="ct">a cancellation token to abort the task</param>
        /// <param name="binaryFile"></param>
        /// <returns></returns>
        public bool ParseBinary(CancellationToken ct, string srcFile)
        {
            string binFile = string.Format(@"{0}\\{1}.dll", Path.GetDirectoryName(srcFile), Path.GetFileNameWithoutExtension(srcFile));
            _logger.DebugFormat("Attempt to open & parse metadata for binary file {0}", binFile);
            
            ReflectionHelper reflHelper = new ReflectionHelper(_logger, binFile);
            reflHelper.SourceFile = srcFile;

            reflHelper.LoadAssembly(_types);

            return true;
        }

        #endregion

    }
}
