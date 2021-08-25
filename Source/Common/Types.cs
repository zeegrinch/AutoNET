using System;
using System.Collections.Generic;
using System.Reflection;


namespace AutoNet.Common
{
    /// <summary>
    /// encapsulation of a few pertinent CLS Type's attributes to pas around (store in session, etc.)
    /// </summary>
    public struct ClassInfo
    {
        public string SourceFile;
        public string AssemblyFile;
        public string TypeName;
        public string TypeFQName;
        public string ArtifactType;
        public Assembly AssemblyContainer;
        public Type CLSType;
    }

    /// <summary>
    /// commands understood by the Dispatcher (invocation handler)
    /// </summary>
    public enum Verb
    {
        Unknown,
        Get,
        Set,
        Invoke,
        Callback
    }

    /// <summary>
    /// simple encapsulation of a command together with its arguments
    /// $TODO$: enhance to allow for Invoke() and 'ref' and 'out' parameters
    /// </summary>
    public class Command
    {
        public Verb Verb { get; set; }
        public Stack<Object> arguments = new Stack<Object>();

        public bool IsValid { get; set; }
        public bool IsSession { get; set; }

        public string SesionCommand { get; set; }
    }
}