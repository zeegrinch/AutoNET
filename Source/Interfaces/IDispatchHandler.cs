using System;
using System.Collections.Generic;
using AutoNet.Common;

namespace AutoNet.Interfaces
{
    /// <summary>
    /// a command-handler class must implement this interface, to be able to support most standard interactions with a .NET object
    /// </summary>
    public interface IDispatchHandler
    {
        bool Dispatch(Object instance, Command cmd);
        Object GetProperty(string propertyName);
        List<(string, string)> GetAllProperties();
        bool SetProperty(string propertyName, Object value);
        bool Invoke(string methodName, Object retValue, params object[] args);
    }
}
