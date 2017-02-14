using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SlavApp.Microservice.Log
{
    public interface ILogger
    {
        void Info(string message);
        void Info(Exception exception);
        void Debug(string message);
        void Debug(Exception exception);
        void Warning(string message);
        void Warning(Exception exception);
        void Error(string message);
        void Error(Exception exception);
        void Fatal(string message);
        void Fatal(Exception exception);
    }
}
