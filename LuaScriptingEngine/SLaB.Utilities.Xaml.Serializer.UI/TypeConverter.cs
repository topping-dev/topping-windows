#if NETFX_CORE

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace System.ComponentModel
{
    public class TypeConverter
    {
        public virtual object ConvertFrom(ITypeDescriptorContext context, Globalization.CultureInfo culture, string val)
        {
            return null;
        }
    }
}

#endif