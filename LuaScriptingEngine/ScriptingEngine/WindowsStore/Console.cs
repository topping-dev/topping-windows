using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace System
{
    class Console
    {
        public static void WriteLine(Object val)
        {
            Debug.WriteLine(val);
        }

        public static void Write(Object val)
        {
            Debug.WriteLine(val);
        }

        public static void Write(Object val, Object target)
        {
            Debug.WriteLine(val);
        }

        public static void Write(params Object[] args)
        {
            foreach (Object o in args)
                Debug.WriteLine(o);
        }

        public static void Write(String format, params Object[] args)
        {
            Debug.WriteLine(format, args);
        }

        public static void WriteLine()
        {
            Debug.WriteLine("");
        }
    }
}
