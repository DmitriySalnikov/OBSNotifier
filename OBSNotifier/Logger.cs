﻿using System;
using System.Globalization;
using System.IO;
using System.Linq;

namespace OBSNotifier
{
    internal class Logger : IDisposable
    {
        readonly string saveFile;
        TextWriter logWriter = null;
        DeferredAction flushFileAction = null;
        readonly object dataLock = new object();

        public Logger(string saveFile)
        {
            this.saveFile = Path.Combine(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), App.AppName), saveFile);

            try
            {
                if (File.Exists(this.saveFile))
                    File.Delete(this.saveFile);
                logWriter = new StreamWriter(File.OpenWrite(this.saveFile));
                flushFileAction = new DeferredAction(() => logWriter.Flush(), 500);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"The log file (\"{this.saveFile}\") cannot be opened for writing. {ex.Message}");
            }

            Write($"// Time format - {CultureInfo.InvariantCulture.DateTimeFormat.ShortDatePattern} {CultureInfo.InvariantCulture.DateTimeFormat.LongTimePattern}");
        }

        public void Dispose()
        {
            flushFileAction?.Dispose();
            flushFileAction = null;

            if (logWriter != null)
            {
                try
                {
                    logWriter.Flush();
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }

                logWriter.Close();
                logWriter.Dispose();
            }
            logWriter = null;
        }

        public void Write(string txt)
        {
            txt = string.Join("\n",
                txt.Split('\n').
                Select((t) => $"[{DateTime.Now.ToString(CultureInfo.InvariantCulture)}] {t}"));

            lock (dataLock)
            {
                Console.WriteLine(txt);
                if (logWriter != null)
                {
                    logWriter.WriteLine(txt);
                    flushFileAction?.CallDeferred();
                }
            }
        }

        public void Write(Exception ex)
        {
            Write($"Exception:\n{ex.Message}\nStackTrace:\n{ex.StackTrace}");
        }
    }
}
