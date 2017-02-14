using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SlavApp.Microservice.Log
{
    public class TraceLogger: ILogger
    {
        public TraceLogger()
        {
            Trace.Listeners.Add(new TextWriterTraceListener("error.log"));
            Trace.AutoFlush = true;
            Trace.IndentSize = 4;
        }

        public void Info(string message)
        {
            Trace.TraceInformation(message);
        }

        public void Info(Exception exception)
        {
            Trace.TraceInformation(exception.ToString());
        }

        public void Debug(string message)
        {
#if DEBUG
            Trace.TraceInformation(message);
#endif
        }

        public void Debug(Exception exception)
        {
#if DEBUG
            Trace.TraceInformation(exception.ToString());
#endif
        }

        public void Warning(string message)
        {
            Trace.TraceWarning(message);
        }

        public void Warning(Exception exception)
        {
            Trace.TraceWarning(exception.ToString());
        }

        public void Error(string message)
        {
            Trace.TraceError(message);
        }

        public void Error(Exception exception)
        {
            Trace.TraceError(exception.ToString());
        }

        public void Fatal(string message)
        {
            throw new NotImplementedException();
        }

        public void Fatal(Exception exception)
        {
            throw new NotImplementedException();
        }
    }
}
