﻿using System;
using System.Configuration;
using System.IO;
using System.Threading;
using PrinterAgentServer.Cache;

namespace PrinterAgentServer.PrintingHandler
{
    public abstract class PrintingHandler
    {
        private static readonly string TempFilePrefix = ConfigurationManager.AppSettings["TempFilePrefix"];

        public void Print(string printerName, byte[] document, string id)
        {
            var filePath = StoreFile(document, id);
            new Thread(() =>{
                try
                {
                    Print(printerName, filePath);
                }
                finally
                {
                    RemoveFile(filePath);
                    PrintRequestsCache.RemoveRequestInProgress(id);
                }
            }).Start();
        }

        protected abstract void Print(string printerName, string filePath);
        
        private string StoreFile(byte[] document, string id)
        {
            var tempPath = Path.GetTempPath();
            var filePath = tempPath + TempFilePrefix + id + ".pdf";

            File.WriteAllBytes(filePath, document);
            return filePath;
        }

        private void RemoveFile(string filePath)
        {
            try
            {
                if (File.Exists(filePath))
                    File.Delete(filePath);
            }
            catch
            {
                // ignored
            }
        }



    }
}
