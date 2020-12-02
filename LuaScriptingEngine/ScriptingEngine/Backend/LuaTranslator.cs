using System;

using System.Collections.Generic;
using System.Text;

namespace ScriptingEngine
{
    [LuaClass("LuaTranslator")]
    public class LuaTranslator
    {
        Object obj = "";
        String function = "";

        [LuaFunction(typeof(object), typeof(String))]
        public static Object Register(Object obj, String function)
        {
            return new LuaTranslator(obj, function);
        }

        public LuaTranslator(Object objP, String functionP)
        {
            function = functionP;
            obj = objP;
        }

        public void CallIn(params Object[] args)
        {
            LuaEngine.Instance.OnGuiEvent(obj, function, args);
        }

        public void Call(Object a, Object b)
        {
            CallIn(a, b);
        }

        public void Call(Object a, int b)
        {
            CallIn(a, b);
        }
    }
}
