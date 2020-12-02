using System;

using System.Collections.Generic;
using System.Text;

namespace ScriptingEngine
{
    public interface LuaInterface
    {
        void RegisterEventFunction(String var, LuaTranslator lt);
        String GetId();
    }
}
