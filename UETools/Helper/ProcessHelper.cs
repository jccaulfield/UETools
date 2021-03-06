﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace UETools.Helper
{
    public static class ProcessHelper
    {
        public const int DefaultTimeoutMS = 0;

        public static int RunProcess(string filename, string arguments, string workingDirectory, out string output, int timeoutMS, CancellationToken ct)
        {
            int returnCode = 0;
            using (var process = new System.Diagnostics.Process())
            {
                process.StartInfo.UseShellExecute = false;
                process.StartInfo.CreateNoWindow = true;
                process.StartInfo.RedirectStandardOutput = true;
                process.StartInfo.RedirectStandardError = true;
                process.StartInfo.FileName = filename;
                process.StartInfo.Arguments = arguments;
                if (workingDirectory != null)
                {
                    process.StartInfo.WorkingDirectory = workingDirectory;
                }

                StringBuilder outputBuilder = new StringBuilder();
                StringBuilder errorBuilder = new StringBuilder();

                using (AutoResetEvent outputWaitHandle = new AutoResetEvent(false))
                using (AutoResetEvent errorWaitHandle = new AutoResetEvent(false))
                {
                    process.OutputDataReceived += (sender, e) => { if (e.Data == null) { outputWaitHandle.Set(); } else { outputBuilder.AppendLine(e.Data); } };
                    process.ErrorDataReceived += (sender, e) => { if (e.Data == null) { errorWaitHandle.Set(); } else { errorBuilder.AppendLine(e.Data); } };

                    process.Start();
                    process.BeginOutputReadLine();
                    process.BeginErrorReadLine();

                    DateTime timeoutTime = DateTime.Now + TimeSpan.FromMilliseconds(timeoutMS);
                    while ( true )
                    {
                        if ( process.WaitForExit(100) && outputWaitHandle.WaitOne(100) && errorWaitHandle.WaitOne(100) )
                        {
                            output = (process.ExitCode == 0) ? outputBuilder.ToString() : errorBuilder.ToString();
                            returnCode = process.ExitCode;
                            break;
                        }

                        if (ct.IsCancellationRequested)
                        {
                            process.Kill();
                            returnCode = 1;
                            output = "Cancellation Requested";
                            ct.ThrowIfCancellationRequested();
                            break;
                        }

                        if ((timeoutMS > 0 ) && (DateTime.Now > timeoutTime) )
                        {
                            process.Kill();
                            returnCode = 1;
                            output = string.Format("Failed to run process '{0}' before we timed out", filename);
                            break;
                        }
                    }
                }
            }
            return returnCode;
        }

        public static int RunProcess(string filename, string arguments, string workingDirectory, out string output, int timeoutMS)
        {
            return RunProcess(filename, arguments, workingDirectory, out output, timeoutMS, new CancellationToken());
        }

        public static int RunProcess(string filename, string arguments, string workingDirectory, out string output)
        {
            return RunProcess(filename, arguments, workingDirectory, out output, DefaultTimeoutMS, new CancellationToken());
        }

        public static int RunProcess(string filename, string arguments, string workingDirectory)
        {
            string dummy;
            return RunProcess(filename, arguments, workingDirectory, out dummy, DefaultTimeoutMS, new CancellationToken());
        }

        public static int RunProcess(string filename, string arguments, out string output, int timeoutMS, CancellationToken ct)
        {
            return RunProcess(filename, arguments, null, out output, timeoutMS, ct);
        }

        public static int RunProcess(string filename, string arguments, out string output, int timeoutMS)
        {
            return RunProcess(filename, arguments, null, out output, timeoutMS);
        }

        public static int RunProcess(string filename, string arguments, out string output)
        {
            return RunProcess(filename, arguments, out output, DefaultTimeoutMS, new CancellationToken());
        }

        public static int RunProcess(string filename, string arguments)
        {
            string dummy;
            return RunProcess(filename, arguments, out dummy, DefaultTimeoutMS, new CancellationToken());
        }
    }
}
