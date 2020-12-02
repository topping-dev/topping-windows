using System;

using System.Collections.Generic;
using System.Text;

namespace ScriptingEngine
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct)]
    public class LuaClass : Attribute
    {
        private String staticName;
        private bool loadAll;

        public LuaClass(String staticNameP)
        {
            staticName = staticNameP;
            loadAll = false;
        }

        public LuaClass(String staticNameP, bool loadAllP)
        {
            staticName = staticNameP;
            loadAll = loadAllP;
        }

        public String GetName() { return staticName; }
        public bool LoadAll() { return loadAll; }
    }
}
