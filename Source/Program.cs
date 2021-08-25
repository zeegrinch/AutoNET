using System;
using System.Configuration;
using log4net;
using AutoNet.Utils;
using AutoNet.Common;
using AutoNet.Interfaces;

namespace AutoNet
{
    class Program
    {
        //singleton-instance of the Log4Net looger; passed around to other instances (DI style)
        private static readonly ILog _logger = LogManager.GetLogger("DebugAppender");

        private static readonly string _sourceFolder = String.IsNullOrEmpty(ConfigurationManager.AppSettings["SourceFiles"]) ? "Sources" :
                                                                            ConfigurationManager.AppSettings["SourceFiles"];

        
        
        
        
        
        static void Main(string[] args)
        {
            CLIHelper cmdHelper = new CLIHelper("TEST-RIG");

            SessionManager sessionMgr = new SessionManager(_logger, _sourceFolder, cmdHelper);

            //in this version, only a simple Dispatcher is provided
            IDispatchHandler simpleInterpreter = new SimpleDispatchHandler(_logger, cmdHelper);

            sessionMgr.Init();

            while (true)
            {
                Command cmd = cmdHelper.GetUserInput();

                if( cmd == null || !cmd.IsValid )
                {
                    //warning/feedback displayed by CLIHelper
                    continue;
                }

                if(cmd.IsSession)
                {
                    if (!sessionMgr.Command(cmd)) //quit
                    {
                        sessionMgr.Dispose();
                        break;
                    }
                }
                else
                {
                    if(sessionMgr.CurrentType == null  || sessionMgr.CurrentObject == null)
                    {
                        cmdHelper.ShowMessage("ERROR: no current object in context.", CLIHelper.Feedback.Error);
                        continue;
                    }
                    //simply pass-in the current Object instance and the command
                    bool bOK = simpleInterpreter.Dispatch(sessionMgr.CurrentObject, cmd);
                }
            }

        }
    }
}
