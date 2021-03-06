﻿using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using Ghostscript.NET;
using Ghostscript.NET.Processor;
using PrinterAgentServer.Util;

namespace PrinterAgentServer.PrintingHandler
{
    public class GhostScriptPrintingHandler : PrintingHandler
    {
        private static readonly string[] GsCommandSwitches = ConfigurationManager.AppSettings["GhostScriptSwitches"].Split(',');

        protected override void Print(string printerName, string filePath)
        {
            byte[] buffer = File.ReadAllBytes(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)+"\\gsdll32.dll");

            using (GhostscriptProcessor processor = new GhostscriptProcessor(buffer))
            {
                var switches = GsCommandSwitches.Select(s=> s.Replace("{printerName}", printerName).Replace("{fileName}", filePath)).ToArray();                
                
                Logger.LogInfo(string.Join(" ", switches));

                var callback = new CallbackStdIO();
                processor.StartProcessing(switches, callback);

                Logger.LogInfo("GS StdOut:\n" + callback.OutLog);
                if (!string.IsNullOrEmpty(callback.ErrorLog))
                    Logger.LogError("GS StdError:\n" + callback.ErrorLog);
                
                
            }
        }


        private class CallbackStdIO : GhostscriptStdIO
        {
            public string OutLog = "";
            public string ErrorLog = "";
            public CallbackStdIO() : base(false, true, true)
            {
            }

            public override void StdIn(out string input, int count)
            {
                throw new NotImplementedException();
            }

            public override void StdOut(string output)
            {
                OutLog += output+Environment.NewLine;
            }

            public override void StdError(string error)
            {
                ErrorLog += error + Environment.NewLine;
                
            }
        }

    }
}
