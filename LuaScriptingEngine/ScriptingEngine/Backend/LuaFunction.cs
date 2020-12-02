using System;

using System.Collections.Generic;
using System.Text;
using LuaCSharp;
using System.Reflection;

namespace ScriptingEngine
{
    [AttributeUsage(AttributeTargets.Method)]
    public class LuaFunction : Attribute
    {
        private List<Type> arguments;
        private bool manual;
        private MethodInfo mi;
        public LuaFunction() //void
        {
            arguments = new List<Type>();
            manual = true;
        }

        public LuaFunction(params Type[] args)
        {
            arguments = new List<Type>();
            arguments.AddRange(args);
            manual = false;
        }

        public LuaFunction(bool manual)
        {
            arguments = new List<Type>();
            this.manual = manual;
        }

        public List<Type> GetArguments()
        {
            return arguments;
        }

        public bool GetManual() { return manual; }

        public MethodInfo GetMethodInfo() { return mi; }
        public void SetMethodInfo(MethodInfo val) { mi = val; }
    }
}
