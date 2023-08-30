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
            this.saveFile = Path.Combine(App.AppDataFolder, saveFile);

            try
            {
                if (File.Exists(this.saveFile))
                    File.Delete(this.saveFile);
                if (!Directory.Exists(Path.GetDirectoryName(this.saveFile)))
                    Directory.CreateDirectory(Path.GetDirectoryName(this.saveFile));

                logWriter = new StreamWriter(File.Open(this.saveFile, FileMode.CreateNew, FileAccess.Write, FileShare.Read));
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
            var isFirst = true;
            txt = string.Join("\n",
                txt.Split('\n').
                Select((t) =>
                {
                    var res = $"[{DateTime.Now.ToString(CultureInfo.InvariantCulture)}]{(isFirst ? "\t" : "-\t")}{t}";
                    isFirst = false;
                    return res;
                }));

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
