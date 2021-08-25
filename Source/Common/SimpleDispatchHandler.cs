using System;
using System.Collections.Generic;
using System.Reflection;
using log4net;
using AutoNet.Utils;
using AutoNet.Interfaces;

namespace AutoNet.Common
{
    
    /// <summary>
    /// Simple command handler that only supports invocation of GET-er/SET-ers for properties of an object.
    /// For setters, all values received from the user input are passed in as strings, conversion to the proper type is required 
    /// 
    /// </summary>
    class SimpleDispatchHandler : IDispatchHandler
    {
        private readonly ILog _logger;
        private readonly CLIHelper _cliHelper;
        //private readonly SessionManager _sessionMgr; //don't think we need this coupling...
        
        private Object _instance = null;
        
        public SimpleDispatchHandler(ILog logger, CLIHelper cli/*SessionManager sessionMgr*/)
        {
            _logger = logger;
            _cliHelper = cli;
            //_sessionMgr = sessionMgr;
        }


        /// <summary>
        /// main command dispatching routine
        /// </summary>
        /// <param name="instance">the current object (in session)</param>
        /// <param name="cmd">VERB + params/values</param>
        /// <returns>boolean: invocation succeeded or not</returns>
        public bool Dispatch(Object instance, Command cmd)
        {
            _instance = instance;
            switch(cmd.Verb)
            {
                case Verb.Get:
                    //* | specificProp
                    string property = cmd.arguments.Pop() as string;
                    if( property.Equals("*"))
                    {
                        try
                        {
                            var allPropsValues = GetAllProperties();
                            _cliHelper.ShowTabularData<(string,string)>(allPropsValues, 2);
                        }
                        catch (Exception ex)
                        {
                            _logger.Error("There has been an error reading all properties [GET *] !", ex);
                            _cliHelper.ShowMessage("There has been an error reading all properties [GET *]. Please inspect the logs.", CLIHelper.Feedback.Warning);
                            return false;
                        }
                    }
                    else
                    {
                        try
                        {
                            var result = GetProperty(property);
                            if (result == null)
                                result = "null";
                            _cliHelper.ShowMessage($"\t{property}={result}", CLIHelper.Feedback.Echo);
                            return true;
                        }
                        catch(Exception ex)
                        {
                            _cliHelper.ShowMessage(ex.Message, CLIHelper.Feedback.Warning);
                            return false;
                        }
                        
                    }
                    break;
                
                case Verb.Set:
                    string propName = cmd.arguments.Pop() as string;
                    string value = cmd.arguments.Pop() as string;
                    try
                    {
                        SetProperty(propName, value);
                    }
                    //specialized exception here
                    catch(ArgumentException ex)
                    {
                        _cliHelper.ShowMessage($"Error setting [{propName}]: {ex.Message}.", CLIHelper.Feedback.Warning);
                        return false;
                    }
                    
                    catch (Exception ex)
                    {
                        _logger.Error($"There has been an error SET-ing a value for property [{propName}] !", ex);
                        _cliHelper.ShowMessage($"Error setting [{propName}]: {ex.Message}. Please inspect the logs for additional details." , CLIHelper.Feedback.Warning);
                        return false;
                    }
                    _cliHelper.ShowMessage("√ Ok", CLIHelper.Feedback.Confirmation);

                    break;
                default:
                    _cliHelper.ShowMessage($"Command [{cmd.Verb}] not supported by SimpleDispatchHandler handler class.", CLIHelper.Feedback.Warning);
                    return false;
            }
                        
            return true;
        }

        #region Dispatcher
        public Object GetProperty(string propertyName)
        {
            TypeInfo typeInfo = _instance.GetType().GetTypeInfo();
            _logger.Debug($"Attempt to read property value [{propertyName}] for current instance of {typeInfo}.");

            PropertyInfo propInfo = typeInfo.GetDeclaredProperty(propertyName);
            if (propInfo == null)
                throw new ArgumentException($"Invalid property[{ propertyName}] for class {typeInfo}.");

            Object value = propInfo.GetValue(_instance, null);
            if (typeInfo.Name.Equals("System.String") && value != null)
                return $"\"{value}\"";
            return value;
        }

        /// <summary>
        /// returns an 'opaque' list of tuples (propname,value) as strings (for display purposes)
        /// </summary>
        /// <returns></returns>
        public List<(string, string)> GetAllProperties()
        {
            TypeInfo typeInfo = _instance.GetType().GetTypeInfo();
            _logger.Debug($"Attempt to read all properties values for current instance of {typeInfo}.");
            
            var allProperties = new List<(string, string)>();

            PropertyInfo[] arrProperties = typeInfo.GetProperties();
            
            foreach (var propInfo in arrProperties)
            {
                string displayValue = "null";
                Object value = propInfo.GetValue(_instance, null);
                if (typeInfo.Name.Equals("System.String") && value != null)
                    displayValue = $"\"{value}\"";
                else if( value != null)
                    displayValue = value.ToString();
                allProperties.Add((propInfo.Name, displayValue));
            }
            return allProperties;
        }

        /// <summary>
        /// Sets a new value for a specific property. All inputs from the user will be passed in as strings (in this version)
        /// </summary>
        /// <param name="propertyName">a string specifying a property to set</param>
        /// <param name="value">new value (as string)</param>
        /// <returns></returns>
        public bool SetProperty(string propertyName, Object value)
        {
            TypeInfo typeInfo = _instance.GetType().GetTypeInfo();
            _logger.Debug($"Attempt to set property value [{propertyName}] for current instance of {typeInfo}.");

            PropertyInfo propInfo = typeInfo.GetDeclaredProperty(propertyName);
            if (propInfo == null)
                throw new ArgumentException($"Invalid property[{ propertyName}] for class {typeInfo}.");

            //try coercion 
            Type t = propInfo.PropertyType;
            if (t.IsGenericType && t.GetGenericTypeDefinition().Equals(typeof(Nullable<>)))
            {
                t = Nullable.GetUnderlyingType(t);
            }
            var coercedValue = Convert.ChangeType(value, t);

            propInfo.SetValue(_instance, coercedValue, null );

            return true;
        }


        /// <summary>
        /// Method invocation not implemented in this implementation
        /// </summary>
        /// <param name="methodName"></param>
        /// <param name="retValue"></param>
        /// <param name="args"></param>
        /// <returns></returns>
        public bool Invoke(string methodName, Object retValue, params object[] args)
        {
            throw new NotImplementedException("This operation is not currently supported.");
        }
        #endregion
    }
}
