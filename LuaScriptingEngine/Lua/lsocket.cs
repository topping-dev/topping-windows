using System;
using System.Net;
using System.Windows;
using ScriptingEngine;
using System.Threading;
using System.Text;
using System.Windows.Input;
#if !NETFX_CORE
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
#else
using Windows.UI.Xaml.Controls;
using Windows.Networking;
#endif

namespace LuaCSharp
{
    public partial class Lua
    {
        /******************************************************************
         * **********************Lua Socket Functions**********************
         * ***************************************************************/
	    
	    public static int auxiliar_open(lua_State L)
	    {
		    return 0;
	    }
	
	    public static void auxiliar_newclass(lua_State L, CharPtr classname, luaL_Reg[] func)
	    {
		    luaL_newmetatable(L, classname); /* mt */
	        /* create __index table to place methods */
	        lua_pushstring(L, new CharPtr("__index"));    /* mt,"__index" */
	        lua_newtable(L);                 /* mt,"__index",it */ 
	        /* put class name into class metatable */
	        lua_pushstring(L, "class");      /* mt,"__index",it,"class" */
	        lua_pushstring(L, classname);    /* mt,"__index",it,"class",classname */
	        lua_rawset(L, -3);               /* mt,"__index",it */
	        /* pass all methods that start with _ to the metatable, and all others
	            * to the index table */
	        int reg_num = 0;
	        for (; func[reg_num].name != null; reg_num++) {     /* mt,"__index",it */
	            lua_pushstring(L, func[reg_num].name);
	            lua_pushcfunction(L, func[reg_num].func);
	            lua_rawset(L, func[reg_num].name[0] == '_' ? -5: -3);
	        }
	        lua_rawset(L, -3);               /* mt */
	        lua_pop(L, 1);
	    }
	
	    public static int auxiliar_tostring(lua_State L) 
	    {
	        if (lua_getmetatable(L, 1) == 0)
	        {
	    	    lua_pushstring(L, "invalid object passed to 'auxiliar.c:__tostring'");
		        lua_error(L);
		        return 1;
	        }
	        lua_pushstring(L, "__index");
	        lua_gettable(L, -2);
	        if (!lua_istable(L, -1))
	        {
	    	    lua_pushstring(L, "invalid object passed to 'auxiliar.c:__tostring'");
		        lua_error(L);
		        return 1;
	        }
	        lua_pushstring(L, "class");
	        lua_gettable(L, -2);
	        if (lua_isstring(L, -1) == 0)
	        {
	    	    lua_pushstring(L, "invalid object passed to 'auxiliar.c:__tostring'");
		        lua_error(L);
		        return 1;
	        }
	        Object obj = lua_touserdata(L, 1);
	        lua_pushfstring(L, "%s: %s", lua_tostring(L, -1), obj.ToString());
	        return 1;	    
	    }
	
	    public static void auxiliar_add2group(lua_State L, CharPtr classname, CharPtr groupname) 
	    {
	        luaL_getmetatable(L, classname);
	        lua_pushstring(L, groupname);
	        lua_pushboolean(L, 1);
	        lua_rawset(L, -3);
	        lua_pop(L, 1);
	    }
	
	    public static int auxiliar_checkboolean(lua_State L, int objidx) {
	        if (!lua_isboolean(L, objidx))
	            luaL_typerror(L, objidx, lua_typename(L, LUA_TBOOLEAN));
	        return lua_toboolean(L, objidx);
	    }
	
	    public static Object auxiliar_checkclass(lua_State L, CharPtr classname, int objidx) {
	        Object data = auxiliar_getclassudata(L, classname, objidx);
	        if (data == null) {
	            luaL_argerror(L, objidx, new CharPtr(classname.toString() + " expected"));
	        }
	        return data;
	    }
	
	    public static Object auxiliar_checkgroup(lua_State L, CharPtr groupname, int objidx) {
	        Object data = auxiliar_getgroupudata(L, groupname, objidx);
	        if (data == null) {
	    	    luaL_argerror(L, objidx, new CharPtr(groupname.toString() + " expected"));
	        }
	        return data;
	    }
	
	    public static void auxiliar_setclass(lua_State L, CharPtr classname, int objidx) {
	        luaL_getmetatable(L, classname);
	        if (objidx < 0) objidx--;
	        lua_setmetatable(L, objidx);
	    }
	
	    public static Object auxiliar_getgroupudata(lua_State L, CharPtr groupname, int objidx) {
	        if (lua_getmetatable(L, objidx) == 0)
	            return null;
	        lua_pushstring(L, groupname);
	        lua_rawget(L, -2);
	        if (lua_isnil(L, -1)) {
	            lua_pop(L, 2);
	            return null;
	        } else {
	            lua_pop(L, 2);
	            return lua_touserdata(L, objidx);
	        }
	    }

	    public static Object auxiliar_getclassudata(lua_State L, CharPtr classname, int objidx) {
	        return luaL_checkudata(L, objidx, classname);
	    }
	
	    public static int buffer_open(lua_State L) {
	        return 0;
	    }

	    /*-------------------------------------------------------------------------*\
	    * Initializes C structure 
	    \*-------------------------------------------------------------------------*/
	    public static void buffer_init(pBuffer buf, pIO io, pTimeout tm) {
		    buf.first = buf.last = 0;
	        buf.io = io;
	        buf.tm = tm;
	        buf.received = buf.sent = 0;
	        buf.birthday = timeout_gettime();
	    }

	    /*-------------------------------------------------------------------------*\
	    * object:getstats() interface
	    \*-------------------------------------------------------------------------*/
	    public static int buffer_meth_getstats(lua_State L, pBuffer buf) {
	        lua_pushnumber(L, buf.received);
	        lua_pushnumber(L, buf.sent);
	        lua_pushnumber(L, timeout_gettime() - buf.birthday);
	        return 3;
	    }

	    /*-------------------------------------------------------------------------*\
	    * object:setstats() interface
	    \*-------------------------------------------------------------------------*/
	    public static int buffer_meth_setstats(lua_State L, pBuffer buf) {
	        buf.received = (long) luaL_optnumber(L, 2, buf.received); 
	        buf.sent = (long) luaL_optnumber(L, 3, buf.sent); 
	        if (lua_isnumber(L, 4) > 0) buf.birthday = timeout_gettime() - lua_tonumber(L, 4);
	        lua_pushnumber(L, 1);
	        return 1;
	    }

	    /*-------------------------------------------------------------------------*\
	    * object:send() interface
	    \*-------------------------------------------------------------------------*/
	    public static int buffer_meth_send(lua_State L, pBuffer buf) {
	        int top = lua_gettop(L);
	        int err = pIO.IO_DONE;
	        uint size = 0, sent = 0;
	        CharPtr data = luaL_checklstring(L, 2, out size);
	        long start = (long) luaL_optnumber(L, 3, 1);
	        long end = (long) luaL_optnumber(L, 4, -1);
	        pTimeout tm = timeout_markstart(buf.tm);
	        if (start < 0) start = (long) (size+start+1);
	        if (end < 0) end = (long) (size+end+1);
	        if (start < 1) start = (long) 1;
	        if (end > (long) size) end = (long) size;
	        if (start <= end) err = sendraw(buf, data + ((int)start- 1), ((int)(end-start+1)), out sent);
	        /* check if there was an error */
	        if (err != pIO.IO_DONE) {
	            lua_pushnil(L);
	            lua_pushstring(L, (CharPtr) buf.io.error(buf.io.ctx, err)); 
	            lua_pushnumber(L, sent+start-1);
	        } else {
	            lua_pushnumber(L, sent+start-1);
	            lua_pushnil(L);
	            lua_pushnil(L);
	        }
	        return lua_gettop(L) - top;
	    }

	    /*-------------------------------------------------------------------------*\
	    * object:receive() interface
	    \*-------------------------------------------------------------------------*/
	    public static int buffer_meth_receive(lua_State L, pBuffer buf) {
	        int err = pIO.IO_DONE, top = lua_gettop(L);
	        luaL_Buffer b;
	        uint size = 0;
	        CharPtr part = luaL_optlstring(L, 3, "", out size);
	        pTimeout tm = timeout_markstart(buf.tm);
	        /* initialize buffer with optional extra prefix 
	            * (useful for concatenating previous partial results) */
	        b = new luaL_Buffer();
		    luaL_buffinit(L, b);
	        luaL_addlstring(b, part, size);
	        /* receive new patterns */
	        if (lua_isnumber(L, 2) == 0) {
	            CharPtr p= luaL_optstring(L, 2, "*l");
	            if (p[0] == '*' && p[1] == 'l') err = recvline(buf, b);
	            else if (p[0] == '*' && p[1] == 'a') err = recvall(buf, b); 
	            else luaL_argcheck(L, false, 2, "invalid receive pattern");
	            /* get a fixed number of bytes (minus what was already partially 
	                * received) */
	        } else err = recvraw(buf, Convert.ToInt32((int)((int)lua_tonumber(L, 2))-size), b);
	        /* check if there was an error */
	        if (err != pIO.IO_DONE) {
	            /* we can't push anyting in the stack before pushing the
	                * contents of the buffer. this is the reason for the complication */
	            luaL_pushresult(b);
	            lua_pushstring(L, new CharPtr(buf.io.error(buf.io.ctx, err))); 
	            lua_pushvalue(L, -2); 
	            lua_pushnil(L);
	            lua_replace(L, -4);
	        } else {
	            luaL_pushresult(b);
	            lua_pushnil(L);
	            lua_pushnil(L);
	        }
	        return lua_gettop(L) - top;
	    }

	    /*-------------------------------------------------------------------------*\
	    * Determines if there is any data in the read buffer
	    \*-------------------------------------------------------------------------*/
	    public static bool buffer_isempty(pBuffer buf) {
	        return buf.first >= buf.last;
	    }

	    /*=========================================================================*\
	    * Internal functions
	    \*=========================================================================*/
	    /*-------------------------------------------------------------------------*\
	    * Sends a block of data (unbuffered)
	    \*-------------------------------------------------------------------------*/
	    public static int STEPSIZE = 8192;
	    public static int sendraw(pBuffer buf, CharPtr data, int count, out uint sent) {
	        pIO io = buf.io;
	        pTimeout tm = buf.tm;
	        int total = 0;
	        int err = pIO.IO_DONE;
	        while (total < count && err == pIO.IO_DONE) {
	            int done = 0;
	            int step = (count-total <= STEPSIZE)? count-total: STEPSIZE;
	            err = io.send(io.ctx, (data + total), step, out done, tm);
	            total += done;
	        }
	        buf.sent += total;
            sent = (uint)total;
	        return err;
	    }

	    /*-------------------------------------------------------------------------*\
	    * Reads a fixed number of bytes (buffered)
	    \*-------------------------------------------------------------------------*/
	    public static int recvraw(pBuffer buf, int wanted, luaL_Buffer b) {
	        int err = pIO.IO_DONE;
	        int total = 0;
	        while (err == pIO.IO_DONE) {
	            int count = 0; CharPtr data = new CharPtr();
	            err = buffer_get(buf, out data, out count);
	            count = Math.Min(count, wanted - total);
	            luaL_addlstring(b, data, (uint)count);
	            buffer_skip(buf, count);
	            total += count;
	            if (total >= wanted) break;
	        }
	        return err;
	    }

	    /*-------------------------------------------------------------------------*\
	    * Reads everything until the connection is closed (buffered)
	    \*-------------------------------------------------------------------------*/
	    public static int recvall(pBuffer buf, luaL_Buffer b) {
	        int err = pIO.IO_DONE;
	        int total = 0;
	        while (err == pIO.IO_DONE) {
	            CharPtr data = new CharPtr(); int count = 0;
	            err = buffer_get(buf, out data, out count);
	            total += count;
	            luaL_addlstring(b, data, (uint)count);
	            buffer_skip(buf, count);
	        }
	        if (err == pIO.IO_CLOSED) {
	            if (total > 0) return pIO.IO_DONE;
	            else return pIO.IO_CLOSED;
	        } else return err;
	    }

	    /*-------------------------------------------------------------------------*\
	    * Reads a line terminated by a CR LF pair or just by a LF. The CR and LF 
	    * are not returned by the function and are discarded from the buffer
	    \*-------------------------------------------------------------------------*/
	    public static int recvline(pBuffer buf, luaL_Buffer b) {
	        int err = pIO.IO_DONE;
	        while (err == pIO.IO_DONE) {
	            int count = 0, pos; CharPtr data = new CharPtr();
	            err = buffer_get(buf, out data, out count);
	            pos = 0;
	            while (pos < count && data[pos] != '\n') {
	                /* we ignore all \r's */
	                if (data[pos] != '\r') luaL_putchar(b, data[pos]);
	                pos++;
	            }
	            if (pos < count) { /* found '\n' */
	                buffer_skip(buf, pos+1); /* skip '\n' too */
	                break; /* we are done */
	            } else /* reached the end of the buffer */
	                buffer_skip(buf, pos);
	        }
	        return err;
	    }

	    /*-------------------------------------------------------------------------*\
	    * Skips a given number of bytes from read buffer. No data is read from the
	    * transport layer
	    \*-------------------------------------------------------------------------*/
	    public static void buffer_skip(pBuffer buf, int count) {
	        buf.received += count;
	        buf.first += count;
	        if (buffer_isempty(buf)) 
	            buf.first = buf.last = 0;
	    }

	    /*-------------------------------------------------------------------------*\
	    * Return any data available in buffer, or get more data from transport layer
	    * if buffer is empty
	    \*-------------------------------------------------------------------------*/
	    public static int buffer_get(pBuffer buf, out CharPtr data, out int count) {
	        int err = pIO.IO_DONE;
	        pIO io = buf.io;
	        pTimeout tm = buf.tm;
	        if (buffer_isempty(buf)) {
	            int got = 0;
	            err = io.recv(io.ctx, buf.data, pBuffer.BUF_SIZE, out got, tm);
	            buf.first = 0;
	            buf.last = got;
	        }
	        count = buf.last - buf.first;
	        data = buf.data + buf.first;
	        return err;
	    }
	
	    /* except functions */
	    private static luaL_Reg[] except = {
		    new luaL_Reg("newtry", global_newtry),
		    new luaL_Reg("protect", global_protect),
		    new luaL_Reg(null, null) };

	    /*-------------------------------------------------------------------------*\
	    * Try factory
	    \*-------------------------------------------------------------------------*/
	    public static void wrap(lua_State L) {
	        lua_newtable(L);
	        lua_pushnumber(L, 1);
	        lua_pushvalue(L, -3);
	        lua_settable(L, -3);
	        lua_insert(L, -2);
	        lua_pop(L, 1);
	    }

	    public static int finalize(lua_State L) {
	        if (lua_toboolean(L, 1) == 0) {
	            lua_pushvalue(L, lua_upvalueindex(1));
	            lua_pcall(L, 0, 0, 0);
	            lua_settop(L, 2);
	            wrap(L);
	            lua_error(L);
	            return 0;
	        } else return lua_gettop(L);
	    }

	    public static int do_nothing(lua_State L) { 
	        return 0; 
	    }


        static lua_CFunction do_nothingD = new lua_CFunction(do_nothing);
	    static lua_CFunction finalizeD = new lua_CFunction(finalize);
	
	    public static int global_newtry(lua_State L) {
	        lua_settop(L, 1);
	        if (lua_isnil(L, 1)) lua_pushcfunction(L, do_nothingD);
	        lua_pushcclosure(L, finalizeD, 1);
	        return 1;
	    }

	    /*-------------------------------------------------------------------------*\
	    * Protect factory
	    \*-------------------------------------------------------------------------*/
	    public static int unwrap(lua_State L) {
	        if (lua_istable(L, -1)) {
	            lua_pushnumber(L, 1);
	            lua_gettable(L, -2);
	            lua_pushnil(L);
	            lua_insert(L, -2);
	            return 1;
	        } else return 0;
	    }

	    public static int protected_(lua_State L) {
	        lua_pushvalue(L, lua_upvalueindex(1));
	        lua_insert(L, 1);
	        if (lua_pcall(L, lua_gettop(L) - 1, LUA_MULTRET, 0) != 0) {
	            if (unwrap(L) > 0) return 2;
	            else lua_error(L);
	            return 0;
	        } else return lua_gettop(L);
	    }

        static lua_CFunction protected_D = new lua_CFunction(protected_);
	    public static int global_protect(lua_State L) {
	        lua_pushcclosure(L, protected_D, 1);
	        return 1;
	    }

	    /*-------------------------------------------------------------------------*\
	    * Init module
	    \*-------------------------------------------------------------------------*/
	    public static int except_open(lua_State L) {
		    luaI_openlib(L, null, except, 0);
	        return 0;
	    }

        public static void io_init(pIO io, pIO.sendD send, pIO.recvD recv, pIO.errorD error, Object ctx)
        {
	        io.send = send;
	        io.recv = recv;
	        io.error = error;
	        io.ctx = (pSocket)ctx;
	    }

	    /*-------------------------------------------------------------------------*\
	    * I/O error strings
	    \*-------------------------------------------------------------------------*/
	    public static CharPtr io_strerror(int err) {
	        switch (err) {
	            case pIO.IO_DONE: return null;
	            case pIO.IO_CLOSED: return new CharPtr("closed");
	            case pIO.IO_TIMEOUT: return new CharPtr("timeout");
	            default: return new CharPtr("unknown error"); 
	        }
	    }
	
	    private static String MIME_VERSION = "MIME 1.0.2";
	    private static String MIME_COPYRIGHT = "Copyright (C) 2004-2007 Diego Nehab";
	    private static String MIME_AUTHORS = "Diego Nehab";
	
	    public static CharPtr CRLF = new CharPtr("\r\n");
	    public static CharPtr EQCRLF = new CharPtr("=\r\n");
	
	    public static luaL_Reg[] mime = {
		    new luaL_Reg("dot", mime_global_dot),
		    new luaL_Reg("b64", mime_global_b64),
		    new luaL_Reg("eol", mime_global_eol),
		    new luaL_Reg("qp", mime_global_qp),
		    new luaL_Reg("qpwrp", mime_global_qpwrp),
		    new luaL_Reg("unb64", mime_global_unb64),
		    new luaL_Reg("unqp", mime_global_unqp),
		    new luaL_Reg("wrp", mime_global_wrp),
		    new luaL_Reg(null, null)
	    };
	
	    static CharPtr qpclass = new CharPtr(new char[256]);
	    static CharPtr qpbase = new CharPtr("0123456789ABCDEF");
	    static CharPtr qpunbase = new CharPtr(new char[256]);
	
	    private const char QP_PLAIN = (char)0;
        private const char QP_QUOTED = (char)1;
        private const char QP_CR = (char)2;
        private const char QP_IF_LAST = (char)3;
	
	    static CharPtr b64base = new CharPtr("ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789+/");
	    static CharPtr b64unbase = new CharPtr(new char[256]);
	
	    public static int luaopen_mime_core(lua_State L)
	    {
	        luaI_openlib(L, new CharPtr("mime"), mime, 0);
	        /* make version string available to scripts */
	        lua_pushstring(L, "_VERSION");
	        lua_pushstring(L, MIME_VERSION);
	        lua_rawset(L, -3);
	        /* initialize lookup tables */
	        qpsetup(qpclass, qpunbase);
	        b64setup(b64unbase);
	        return 1;
	    }
	
	    public static int mime_global_wrp(lua_State L)
	    {
	        uint size = 0;
	        int left = (int) luaL_checknumber(L, 1);
	        CharPtr input = luaL_optlstring(L, 2, (CharPtr)null, out size);
	        int length = (int) luaL_optnumber(L, 3, 76);
	        luaL_Buffer buffer = new luaL_Buffer();
	        /* end of input black-hole */
	        if (input == null) {
	            /* if last line has not been terminated, add a line break */
	            if (left < length) lua_pushstring(L, CRLF);
	            /* otherwise, we are done */
	            else lua_pushnil(L);
	            lua_pushnumber(L, length);
	            return 2;
	        } 
	        CharPtr last = input + size;
	        luaL_buffinit(L, buffer);
	        while (input < last) 
	        {
	            switch (input.chars[input.index]) {
	                case '\r':
	                    break;
	                case '\n':
	                    luaL_addstring(buffer, CRLF);
	                    left = length;
	                    break;
	                default:
	                    if (left <= 0) {
	                        left = length;
	                        luaL_addstring(buffer, CRLF);
	                    }
	                    luaL_putchar(buffer, input.chars[input.index]);
	                    left--;
	                    break;
	            }
	            input.inc();
	        }
	        luaL_pushresult(buffer);
	        lua_pushnumber(L, left);
	        return 2;
	    }
	
	    public static void b64setup(CharPtr b64unbase) 
	    {
	        int i;
	        for (i = 0; i <= 255; i++) b64unbase.setItem(i, (char)255);
	        for (i = 0; i < 64; i++) b64unbase.setItem(b64base[i], (char) i);
	        b64unbase.setItem((int)'=', (char)0);
	    }
	
	    public static long b64encode(char c, CharPtr input, int size, 
	            luaL_Buffer buffer)
	    {
	        input.setItem(size++, c);
	        if (size == 3) {
	            char[] code = new char[4];
	            long value = 0;
	            value += input[0]; value <<= 8;
	            value += input[1]; value <<= 8;
	            value += input[2]; 
	            code[3] = b64base[value & 0x3f]; value >>= 6;
	            code[2] = b64base[value & 0x3f]; value >>= 6;
	            code[1] = b64base[value & 0x3f]; value >>= 6;
	            code[0] = b64base[value];
	            luaL_addlstring(buffer, new CharPtr(code), 4);
	            size = 0;
	        }
	        return size;
	    }
	
	    public static long b64pad(CharPtr input, int size, 
	            luaL_Buffer buffer)
	    {
	        long value = 0;
	        char[] code = {'=', '=', '=', '='};
	        switch (size) {
	            case 1:
	                value = input[0] << 4;
	                code[1] = b64base[value & 0x3f]; value >>= 6;
	                code[0] = b64base[value];
	                luaL_addlstring(buffer, new CharPtr(code), 4);
	                break;
	            case 2:
	                value = input[0]; value <<= 8; 
	                value |= input[1]; value <<= 2;
	                code[2] = b64base[value & 0x3f]; value >>= 6;
	                code[1] = b64base[value & 0x3f]; value >>= 6;
	                code[0] = b64base[value];
	                luaL_addlstring(buffer, new CharPtr(code), 4);
	                break;
	            default:
	                break;
	        }
	        return 0;
	    }
	
	    public static long b64decode(char c, CharPtr input, int size, 
	            luaL_Buffer buffer)
	    {
	        /* ignore invalid characters */
	        if (b64unbase[c] > 64) return size;
	        input.setItem(size++, c);
	        /* decode atom */
	        if (size == 4) {
	            char[] decoded = new char[3];
	            int valid, value = 0;
	            value =  b64unbase[input[0]]; value <<= 6;
	            value |= b64unbase[input[1]]; value <<= 6;
	            value |= b64unbase[input[2]]; value <<= 6;
	            value |= b64unbase[input[3]];
	            decoded[2] = (char) (value & 0xff); value >>= 8;
	            decoded[1] = (char) (value & 0xff); value >>= 8;
	            decoded[0] = (char) value;
	            /* take care of paddding */
	            valid = (input[2] == '=') ? 1 : (input[3] == '=') ? 2 : 3; 
	            luaL_addlstring(buffer, new CharPtr(decoded), (uint)valid);
	            return 0;
	        /* need more data */
	        } else return size;
	    }
	
	    public static int mime_global_b64(lua_State L)
	    {
		    CharPtr atom = new CharPtr(new char[3]);
	        uint isize = 0, asize = 0;
	        CharPtr input = (CharPtr) luaL_optlstring(L, 1, (CharPtr)null, out isize);
	        /* end-of-input blackhole */
	        if (input == null || isize == 0) {
	            lua_pushnil(L);
	            lua_pushnil(L);
	            return 2;
	        }
	        //CharPtr last = CharPtr.OpAddition(input, isize);
	        luaL_Buffer buffer = new luaL_Buffer();
	        /* process first part of the input */
	        luaL_buffinit(L, buffer);
	        //byte[] encoded = Base64.encodeBytesToBytes(input.toByteArrayNoFinisher());
	        //byte[] encoded = com.dk.base64.Base64.encode(input.toByteArrayNoFinisher());
            CharPtr result = new CharPtr(Convert.ToBase64String(input.toByteArrayNoFinisher()));
            luaL_addlstring(buffer, result, (uint)result.chars.Length);
	        /*DK Comment-isizeP.argvalue = isize;
	        input = (CharPtr)luaL_optlstring(L, 2, (CharPtr)null, isizeP);
	        isize = isizeP.argvalue;*/
	        /* if second part is nil, we are done */
	        //DK Comment-if (input == null || isize == 0) {
	            asize = (uint) b64pad(atom, (int)asize, buffer);
	            luaL_pushresult(buffer);
	            if (lua_tostring(L, -1) == null) lua_pushnil(L);
	            lua_pushnil(L);
	            return 2;
	        //}
	        /* otherwise process the second part */
	        /*last = CharPtr.OpAddition(input, isize);
	        while (CharPtr.OpLessThan(input,last))
	        {
	    	    asize = (int) b64encode(input[), atom, (int)asize, buffer);
	    	    input.inc();
	        }*/
	        /*DK Comment - encoded = Base64.encodeBytesToBytes(input.toByteArray());
	        result = new CharPtr();
	        result.setByteArray(encoded);
	        luaL_addlstring(buffer, result, encoded.length);
	        luaL_pushresult(buffer);
	        lua_pushlstring(L, new CharPtr(atom), asize);
	        return 2;*/
	    }
	
	    public static int mime_global_unb64(lua_State L)
	    {
		    CharPtr atom = new CharPtr(new char[4]);
	        uint isize = 0, asize = 0;
	        CharPtr input = luaL_optlstring(L, 1, (CharPtr)null, out isize);
	        /* end-of-input blackhole */
	        if (input == null) {
	            lua_pushnil(L);
	            lua_pushnil(L);
	            return 2;
	        }
	        CharPtr last = input + isize;
	        luaL_Buffer buffer = new luaL_Buffer();
	        /* process first part of the input */
	        luaL_buffinit(L, buffer);
	    
	        byte[] decoded = new byte[1];
	        try
		    {
			        //decoded = Base64.decode(input.toByteArray());
                decoded = Convert.FromBase64CharArray(input.chars, 0, input.chars.Length);
		    }
		    catch(Exception e)
		    {
			    //e.printStackTrace();
		    }
		    CharPtr result = new CharPtr();
		    result.setByteArray(decoded);
		    luaL_addstring(buffer, result);
	        /*while (CharPtr.OpLessThan(input,last))
	        {
	            asize = (int) b64decode(input[), atom, (int)asize, buffer);
	            input.inc();
	        }*/
	        input = (CharPtr)luaL_optlstring(L, 2, (CharPtr)null, out isize);
	        /* if second is nil, we are done */
	        if (input == null) {
	            luaL_pushresult(buffer);
	            if (lua_tostring(L, -1) == null) lua_pushnil(L);
	            lua_pushnil(L);
	            return 2;
	        }
	    
	        decoded = new byte[1];
	        try
		    {
			        //decoded = Base64.decode(input.toByteArray());
                decoded = Convert.FromBase64CharArray(input.chars, 0, input.chars.Length);
		    }
		    catch(Exception e)
		    {
			    //e.printStackTrace();
		    }
	        result = new CharPtr();
		    result.setByteArray(decoded);
		    luaL_addstring(buffer, result);
	        /* otherwise, process the rest of the input */
	        /*last = CharPtr.OpAddition(input, isize);
	        while (CharPtr.OpLessThan(input,last))
	        {
	    	    asize = (int) b64decode(input[), atom, (int)asize, buffer);
	    	    input.inc();
	        }*/
	        luaL_pushresult(buffer);
	        lua_pushlstring(L, atom, asize);
	        return 2;
	    }
	
	    public static void qpsetup(CharPtr qpclass, CharPtr qpunbase)
	    {
	        int i;
	        for (i = 0; i < 256; i++) qpclass.setItem(i,(char) QP_QUOTED);
	        for (i = 33; i <= 60; i++) qpclass.setItem(i,(char) QP_PLAIN);
	        for (i = 62; i <= 126; i++) qpclass.setItem(i,(char) QP_PLAIN);
	        qpclass.setItem('\t', (char) QP_IF_LAST);; 
	        qpclass.setItem(' ', (char) QP_IF_LAST);;
	        qpclass.setItem('\r', (char) QP_CR);;
	        for (i = 0; i < 256; i++) qpunbase.setItem(i, (char) 255);
	        qpunbase.setItem('0', (char) 0); qpunbase.setItem('1', (char) 1); qpunbase.setItem('2', (char) 2);
	        qpunbase.setItem('3', (char) 3); qpunbase.setItem('4', (char) 4); qpunbase.setItem('5', (char) 5);
	        qpunbase.setItem('6', (char) 6); qpunbase.setItem('7', (char) 7); qpunbase.setItem('8',(char)  8);
	        qpunbase.setItem('9', (char) 9); qpunbase.setItem('A', (char) 10); qpunbase.setItem('a', (char) 10);
	        qpunbase.setItem('B', (char) 11); qpunbase.setItem('b', (char) 11); qpunbase.setItem('C', (char) 12);
	        qpunbase.setItem('c', (char) 12); qpunbase.setItem('D', (char) 13); qpunbase.setItem('d', (char) 13);
	        qpunbase.setItem('E', (char) 14); qpunbase.setItem('e', (char) 14); qpunbase.setItem('F', (char) 15);
	        qpunbase.setItem('f', (char) 15);
	    }
	
	    public static void qpquote(char c, luaL_Buffer buffer)
	    {
	        luaL_putchar(buffer, '=');
	        luaL_putchar(buffer, qpbase[c >> 4]);
	        luaL_putchar(buffer, qpbase[c & 0x0F]);
	    }
	
	    public static int qpencode(char c, CharPtr input, int size, 
	            CharPtr marker, luaL_Buffer buffer)
	    {
	        input.setItem(size++, c);
	        /* deal with all characters we can have */
	        while (size > 0) {
	            switch (qpclass[input[0]]) {
	                /* might be the CR of a CRLF sequence */
	                case QP_CR:
	                    if (size < 2) return size;
	                    if (input[1] == '\n') {
	                        luaL_addstring(buffer, marker);
	                        return 0;
	                    } else qpquote(input[0], buffer);
	                    break;
	                /* might be a space and that has to be quoted if last in line */
	                case QP_IF_LAST:
	                    if (size < 3) return size;
	                    /* if it is the last, quote it and we are done */
	                    if (input[1] == '\r' && input[2] == '\n') {
	                        qpquote(input[0], buffer);
	                        luaL_addstring(buffer, marker);
	                        return 0;
	                    } else luaL_putchar(buffer, input[0]);
	                    break;
	                    /* might have to be quoted always */
	                case QP_QUOTED:
	                    qpquote(input[0], buffer);
	                    break;
	                    /* might never have to be quoted */
	                default:
	                    luaL_putchar(buffer, input[0]);
	                    break;
	            }
	            input.setItem(0, input[1]); input.setItem(1, input[2]);
	            size--;
	        }
	        return 0;
	    }
	
	    public static int qppad(CharPtr input, int size, luaL_Buffer buffer)
	    {
	        int i;
	        for (i = 0; i < size; i++) {
	            if (qpclass[input[i]] == QP_PLAIN) luaL_putchar(buffer, input[i]);
	            else qpquote(input[i], buffer);
	        }
	        if (size > 0) luaL_addstring(buffer, EQCRLF);
	        return 0;
	    }
	
	    public static int mime_global_qp(lua_State L)
	    {

	        uint asize = 0, isize = 0;
	        CharPtr atom = new CharPtr(new char[3]);

	        CharPtr input = luaL_optlstring(L, 1, (CharPtr)null, out isize);
	        CharPtr marker = luaL_optstring(L, 3, CRLF);
	        luaL_Buffer buffer = new luaL_Buffer();
	        /* end-of-input blackhole */
	        if (input == null) {
	            lua_pushnil(L);
	            lua_pushnil(L);
	            return 2;
	        }
	        CharPtr last = CharPtr.OpAddition(input, isize);
	        /* process first part of input */
	        luaL_buffinit(L, buffer);
	        while (CharPtr.OpLessThan(input, last))
	        {
	            asize = (uint)qpencode(input[0], atom, (int)asize, marker, buffer);
	            input.inc();
	        }
	    
	        input = luaL_optlstring(L, 2, (CharPtr) null, out isize);
	        /* if second part is nil, we are done */
	        if (input == null) {
                asize = (uint)qppad(atom, (int)asize, buffer);
	            luaL_pushresult(buffer);
	            if (lua_tostring(L, -1) == null) lua_pushnil(L);
	            lua_pushnil(L);
	            return 2;
	        }
	        /* otherwise process rest of input */
	        last = CharPtr.OpAddition(input, isize);
	        while (CharPtr.OpLessThan(input, last))
	        {
                asize = (uint)qpencode(input[0], atom, (int)asize, marker, buffer);
		            input.inc();
	        }
	        luaL_pushresult(buffer);
	        lua_pushlstring(L, atom, asize);
	        return 2;
	    }
	
	    public static int qpdecode(char c, CharPtr input, int size, luaL_Buffer buffer) {
	        int d;
	        input.setItem(size++, c);
	        /* deal with all characters we can deal */
	        switch (input[0]) {
	            /* if we have an escape character */
	            case '=': 
	                if (size < 3) return size; 
	                /* eliminate soft line break */
	                if (input[1] == '\r' && input[2] == '\n') return 0;
	                /* decode quoted representation */
	                c = qpunbase[input[1]]; d = qpunbase[input[2]];
	                /* if it is an invalid, do not decode */
	                if (c > 15 || d > 15) luaL_addlstring(buffer, input, 3);
	                else luaL_putchar(buffer, (char) ((c << 4) + d));
	                return 0;
	            case '\r':
	                if (size < 2) return size; 
	                if (input[1] == '\n') luaL_addlstring(buffer, input, 2);
	                return 0;
	            default:
	                if (input[0] == '\t' || (input[0] > 31 && input[0] < 127))
	                    luaL_putchar(buffer, input[0]);
	                return 0;
	        }
	    }
	
	    public static int mime_global_unqp(lua_State L)
	    {
	        uint asize = 0, isize = 0;
	        CharPtr atom = new CharPtr(new char[3]);
	        CharPtr input = luaL_optlstring(L, 1, (CharPtr)null, out isize);
	        luaL_Buffer buffer = new luaL_Buffer();
	        /* end-of-input blackhole */
	        if (input == null) {
	            lua_pushnil(L);
	            lua_pushnil(L);
	            return 2;
	        }
	        CharPtr last = CharPtr.OpAddition(input, isize);
	        /* process first part of input */
	        luaL_buffinit(L, buffer);
	        while (CharPtr.OpLessThan(input, last))
	        {
	            asize = (uint)qpdecode(input[0], atom, (int)asize, buffer);
	            input.inc();
	        }
	        input = luaL_optlstring(L, 2, (CharPtr) null, out isize);
	        /* if second part is nil, we are done */
	        if (input == null) {
	            luaL_pushresult(buffer);
	            if (lua_tostring(L, -1) == null) lua_pushnil(L);
	            lua_pushnil(L);
	            return 2;
	        } 
	        /* otherwise process rest of input */
	        last = CharPtr.OpAddition(input, isize);
	        while (CharPtr.OpLessThan(input, last))
	        {
                asize = (uint)qpdecode(input[0], atom, (int)asize, buffer);
	            input.inc();
	        }
	        luaL_pushresult(buffer);
	        lua_pushlstring(L, atom, (uint)asize);
	        return 2;
	    }
	
	    public static int mime_global_qpwrp(lua_State L)
	    {
	        uint size = 0;
	        int left = (int) luaL_checknumber(L, 1);
	        CharPtr input = luaL_optlstring(L, 2, (CharPtr)null, out size);
	        int length = (int) luaL_optnumber(L, 3, 76);
	        luaL_Buffer buffer = new luaL_Buffer();
	        /* end-of-input blackhole */
	        if (input == null) {
	            if (left < length) lua_pushstring(L, EQCRLF);
	            else lua_pushnil(L);
	            lua_pushnumber(L, length);
	            return 2;
	        }
	        CharPtr last = CharPtr.OpAddition(input, size);
	        /* process all input */
	        luaL_buffinit(L, buffer);
	        while (CharPtr.OpLessThan(input, last)) {
	            switch (input[0]) {
	                case '\r':
	                    break;
	                case '\n':
	                    left = length;
	                    luaL_addstring(buffer, CRLF);
	                    break;
	                case '=':
	                    if (left <= 3) {
	                        left = length;
	                        luaL_addstring(buffer, EQCRLF);
	                    } 
	                    luaL_putchar(buffer, input[0]);
	                    left--;
	                    break;
	                default: 
	                    if (left <= 1) {
	                        left = length;
	                        luaL_addstring(buffer, EQCRLF);
	                    }
	                    luaL_putchar(buffer, input[0]);
	                    left--;
	                    break;
	            }
	            input.inc();
	        }
	        luaL_pushresult(buffer);
	        lua_pushnumber(L, left);
	        return 2;
	    }
	
	    public static bool eolcandidate(int c) { return (c == '\r' || c == '\n'); }
	    public static int eolprocess(int c, int last, CharPtr marker, 
	            luaL_Buffer buffer)
	    {
	        if (eolcandidate(c)) {
	            if (eolcandidate(last)) {
	                if (c == last) luaL_addstring(buffer, marker);
	                return 0;
	            } else {
	                luaL_addstring(buffer, marker);
	                return c;
	            }
	        } else {
	            luaL_putchar(buffer, (char) c);
	            return 0;
	        }
	    }
	
	    public static int mime_global_eol(lua_State L)
	    {
	        int ctx = luaL_checkint(L, 1);
	        uint isize = 0;
	        CharPtr input = luaL_optlstring(L, 2, (CharPtr)null, out isize);
	        CharPtr marker = luaL_optstring(L, 3, CRLF);
	        luaL_Buffer buffer = new luaL_Buffer();
	        luaL_buffinit(L, buffer);
	        /* end of input blackhole */
	        if (input == null) {
	            lua_pushnil(L);
	            lua_pushnumber(L, 0);
	            return 2;
	        }
	        CharPtr last = CharPtr.OpAddition(input, isize);
	        /* process all input */
	        while (CharPtr.OpLessThan(input, last))
	        {
	            ctx = eolprocess(input[0], ctx, marker, buffer);
	            input.inc();
	        }
	        luaL_pushresult(buffer);
	        lua_pushnumber(L, ctx);
	        return 2;
	    }
	
	    public static int dot(int c, int state, luaL_Buffer buffer)
	    {
	        luaL_putchar(buffer, (char) c);
	        switch (c) {
	            case '\r': 
	                return 1;
	            case '\n': 
	                return (state == 1)? 2: 0; 
	            case '.':  
	                if (state == 2) 
	                    luaL_putchar(buffer, '.');
                    return 0;
                    break;
	            default:
	                return 0;
	        }
	    }
	
	    public static int mime_global_dot(lua_State L)
	    {
	        uint isize = 0, state = (uint) luaL_checknumber(L, 1);
	        CharPtr input = luaL_optlstring(L, 2, (CharPtr)null, out isize);
	        luaL_Buffer buffer = new luaL_Buffer();
	        /* end-of-input blackhole */
	        if (input == null) {
	            lua_pushnil(L);
	            lua_pushnumber(L, 2);
	            return 2;
	        }
	        CharPtr last = CharPtr.OpAddition(input, isize);
	        /* process all input */
	        luaL_buffinit(L, buffer);
	        while (CharPtr.OpLessThan(input, last)) 
	        {
                state = (uint)dot(input[0], (int)state, buffer);
	            input.inc();
	        }
	        luaL_pushresult(buffer);
	        lua_pushnumber(L, state);
	        return 2;
	    }
	
	    public const int SO_REUSEADDR = 0;
	    public const int TCP_NODELAY = 1;
	    public const int SO_KEEPALIVE = 2;
	    public const int SO_DONTROUTE = 3;
	    public const int SO_BROADCAST = 4;
	    public const int IP_MULTICAST_LOOP = 5;
	    public const int SO_LINGER = 6;
	    public const int IP_ADD_MEMBERSHIP = 7;
	    public const int IP_DROP_MEMBERSHIP = 8;
	
	
	    public static int opt_meth_setoption(lua_State L, luaL_pOptReg[] opt2, pSocket ps)
	    {
	        CharPtr name = luaL_checkstring(L, 2);      /* obj, name, ... */
	        int i = 0;
	        luaL_pOptReg opt = opt2[i];
	        while (opt.name != null && strcmp(name, opt.name) != 0)
	        {
	            i++;
	            opt = opt2[i];
	        }
	        if (opt.func == null) {
	            luaL_argerror(L, 2, new CharPtr("unsupported option " + name));
	        }
	        return opt.func(L, ps);
	    }
	
	    /* enables reuse of local address */
	    public static int opt_reuseaddr(lua_State L, pSocket ps)
	    {
	        return opt_setboolean(L, ps, SO_REUSEADDR);
	    }
	
	    /* disables the Naggle algorithm */
	    public static int opt_tcp_nodelay(lua_State L, pSocket ps)
	    {
	        return opt_setboolean(L, ps, TCP_NODELAY); 
	    }

	    public static int opt_keepalive(lua_State L, pSocket ps)
	    {
	        return opt_setboolean(L, ps, SO_KEEPALIVE); 
	    }

	    public static int opt_dontroute(lua_State L, pSocket ps)
	    {
	        return opt_setboolean(L, ps, SO_DONTROUTE);
	    }

	    public static int opt_broadcast(lua_State L, pSocket ps)
	    {
	        return opt_setboolean(L, ps, SO_BROADCAST);
	    }

	    public static int opt_ip_multicast_loop(lua_State L, pSocket ps)
	    {
	        return opt_setboolean(L, ps, IP_MULTICAST_LOOP);
	    }
	
	    public class linger {
            public int l_onoff;                /* option on/off */
            public int l_linger;               /* linger time */
	    };
	
	    public static int opt_linger(lua_State L, pSocket ps)
	    {
	        linger li = new linger();                      /* obj, name, table */
	        if (!lua_istable(L, 3)) luaL_typerror(L, 3, lua_typename(L, LUA_TTABLE));
	        lua_pushstring(L, "on");
	        lua_gettable(L, 3);
	        if (!lua_isboolean(L, -1)) 
	            luaL_argerror(L, 3, "boolean 'on' field expected");
	        li.l_onoff = (int) lua_toboolean(L, -1);
	        lua_pushstring(L, "timeout");
	        lua_gettable(L, 3);
	        if (lua_isnumber(L, -1) == 0) 
	            luaL_argerror(L, 3, "number 'timeout' field expected");
	        li.l_linger = (int) lua_tonumber(L, -1);
	        return opt_set(L, ps, SO_LINGER, li);
	    }
	
	    public static int opt_ip_multicast_ttl(lua_State L, pSocket ps)
	    {
	        int val = (int) luaL_checknumber(L, 3);    /* obj, name, int */
	        return opt_set(L, ps, SO_LINGER, new CharPtr(val.ToString()));
	    }

	    public static int opt_ip_add_membership(lua_State L, pSocket ps)
	    {
	        return opt_setmembership(L, ps, IP_ADD_MEMBERSHIP);
	    }

	    public static int opt_ip_drop_membersip(lua_State L, pSocket ps)
	    {
	        return opt_setmembership(L, ps, IP_DROP_MEMBERSHIP);
	    }
	
	    public class ip_mreq {
            public 
#if !NETFX_CORE
                DnsEndPoint
#else
                HostName
#endif
                imr_multiaddr;  /* IP multicast address of group */
            public 
#if !NETFX_CORE
                DnsEndPoint
#else
                HostName
#endif 
                imr_interface;  /* local IP address of interface */
	    };
	    public static int opt_setmembership(lua_State L, pSocket ps, int name)
	    {
	        ip_mreq val = new ip_mreq();                   /* obj, name, table */
	        if (!lua_istable(L, 3)) luaL_typerror(L, 3, lua_typename(L, LUA_TTABLE));
	        lua_pushstring(L, "multiaddr");
	        lua_gettable(L, 3);
	        if (lua_isstring(L, -1) == 0) 
	            luaL_argerror(L, 3, "string 'multiaddr' field expected");
	        val.imr_multiaddr = null;
	        try
		    {
#if WINDOWS_PHONE
			    val.imr_multiaddr = new DnsEndPoint(lua_tostring(L, -1).toString(), 0);
#elif NETFX_CORE
                val.imr_multiaddr = new HostName(lua_tostring(L, -1).toString());
#endif
		    }
		    catch(Exception e)
		    {
			    //e.printStackTrace();
		    }
	        if (val.imr_multiaddr == null) 
	            luaL_argerror(L, 3, "invalid 'multiaddr' ip address");
	        lua_pushstring(L, "interface");
	        lua_gettable(L, 3);
	        if (lua_isstring(L, -1) == 0) 
	            luaL_argerror(L, 3, "string 'interface' field expected");
	        //val.imr_interface.s_addr = htonl(INADDR_ANY);
	        val.imr_interface = null;
	        try
		    {
#if WINDOWS_PHONE
			    val.imr_interface = new DnsEndPoint("0.0.0.0", 0);
#elif NETFX_CORE
                val.imr_interface = new HostName("0.0.0.0");
#endif
		    }
		    catch(Exception e1)
		    {
			    //e1.printStackTrace();
		    }
	        try
		    {
#if WINDOWS_PHONE
                val.imr_interface = new DnsEndPoint(lua_tostring(L, -1).toString(), 0);
#elif NETFX_CORE
                val.imr_interface = new HostName(lua_tostring(L, -1).toString());
#endif
		    }
		    catch(Exception e)
		    {
			    //e.printStackTrace();
		    }
	        if (strcmp(lua_tostring(L, -1), "*") != 0 &&
	    		    val.imr_interface == null) 
	            luaL_argerror(L, 3, "invalid 'interface' ip address");
	        return opt_set(L, ps, name, val);
	    }
	
	    public static 
	    int opt_set(lua_State L, pSocket ps, int name, Object val)
	    {
		    switch(name)
		    {
			    case SO_REUSEADDR:
				    /*if(ps.GetTcpSocket() != null)
				    {
					    try
					    {
						    ps.GetTcpSocket().setReuseAddress(Boolean.valueOf(val.toString()));
					    }
					    catch(SocketException e)
					    {
						    e.printStackTrace();
					    }
				    }*/
				    break;
			    case TCP_NODELAY:
				    /*if(ps.GetTcpSocket() != null)
				    {
					    try
					    {
						    ps.GetTcpSocket().setTcpNoDelay(Boolean.valueOf(val.toString()));
					    }
					    catch(SocketException e)
					    {
						    e.printStackTrace();
					    }
				    }*/
				    break;
			    case SO_KEEPALIVE:
				    /*if(ps.GetTcpSocket() != null)
				    {
					    try
					    {
						    ps.GetTcpSocket().setKeepAlive(Boolean.valueOf(val.toString()));
					    }
					    catch(SocketException e)
					    {
						    e.printStackTrace();
					    }
				    }*/
				    break;
			    case SO_DONTROUTE:
				
				    break;
			    case SO_BROADCAST:
				    /*if(ps.GetUdpSocket() != null)
				    {
					    try
					    {
						    ps.GetUdpSocket().setBroadcast(Boolean.valueOf(val.toString()));
					    }
					    catch(SocketException e)
					    {
						    e.printStackTrace();
					    }
				    }*/
				    break;
			    case IP_MULTICAST_LOOP:
				    /*if(ps.GetUdpSocket() != null)
				    {
					    if(MulticastSocket.class.isInstance(ps.GetUdpSocket()))
						    ((MulticastSocket)ps.GetUdpSocket()).(Boolean.valueOf(val.toString()));
				    }*/
				    break;
			    case SO_LINGER:
				    /*if(ps.GetTcpSocket() != null)
				    {
					    linger l = (linger)val;
					    try
					    {
						    ps.GetTcpSocket().setSoLinger(Boolean.valueOf(String.valueOf(l.l_onoff)), l.l_linger);
					    }
					    catch(SocketException e)
					    {
						    e.printStackTrace();
					    }
				    }*/
				    break;
			    case IP_ADD_MEMBERSHIP:
				    /*if(ps.GetUdpSocket() != null)
				    {
					    if(MulticastSocket.class.isInstance(ps.GetUdpSocket()))
					    {
					    }
				    }*/
				    break;
			    case IP_DROP_MEMBERSHIP:
				    break;
		    }
	        /*if (setsockopt(*ps, level, name, (char *) val, len) < 0) {
	            lua_pushnil(L);
	            lua_pushstring(L, "setsockopt failed");
	            return 2;
	        }*/
	        lua_pushnumber(L, 1);
	        return 1;
	    }

	    public static int opt_setboolean(lua_State L, pSocket ps, int name)
	    {
	        int val = auxiliar_checkboolean(L, 3);             /* obj, name, bool */
	        return opt_set(L, ps, name, val);
	    }
	
	    public static luaL_Reg[] select = {
		    new luaL_Reg("select", global_select),
		    new luaL_Reg(null, null)
	    };
	
	    public static int select_open(lua_State L) {
	        luaI_openlib(L, null, select, 0);
	        return 0;
	    }
	
	    public static int global_select(lua_State L) {
	        /*int rtab, wtab, itab, ret, ndirty;
	        pSocket max_fd;
	        fd_set rset, wset;
	        t_timeout tm;
	        double t = luaL_optnumber(L, 3, -1);
	        FD_ZERO(&rset); FD_ZERO(&wset);
	        lua_settop(L, 3);
	        lua_newtable(L); itab = lua_gettop(L);
	        lua_newtable(L); rtab = lua_gettop(L);
	        lua_newtable(L); wtab = lua_gettop(L);
	        max_fd = collect_fd(L, 1, SOCKET_INVALID, itab, &rset);
	        ndirty = check_dirty(L, 1, rtab, &rset);
	        t = ndirty > 0? 0.0: t;
	        timeout_init(&tm, t, -1);
	        timeout_markstart(&tm);
	        max_fd = collect_fd(L, 2, max_fd, itab, &wset);
	        ret = socket_select(max_fd+1, &rset, &wset, NULL, &tm);
	        if (ret > 0 || ndirty > 0) {
	            return_fd(L, &rset, max_fd+1, itab, rtab, ndirty);
	            return_fd(L, &wset, max_fd+1, itab, wtab, 0);
	            make_assoc(L, rtab);
	            make_assoc(L, wtab);
	            return 2;
	        } else if (ret == 0) {
	            lua_pushstring(L, "timeout");
	            return 3;
	        } else {*/
	            lua_pushstring(L, "error");
	            return 3;
	        //}
	    }
	
	    public static luaL_Reg[] tcpCreateFunc = {
		    new luaL_Reg("select", global_select),
	        new luaL_Reg("__gc",        meth_close),
	        new luaL_Reg("__tostring",  auxiliar_tostring),
	        new luaL_Reg("accept",      meth_accept),
	        new luaL_Reg("bind",        meth_bind),
	        new luaL_Reg("close",       meth_close),
	        new luaL_Reg("connect",     meth_connect),
	        new luaL_Reg("dirty",       meth_dirty),
	        new luaL_Reg("getfd",       meth_getfd),
	        new luaL_Reg("getpeername", meth_getpeername),
	        new luaL_Reg("getsockname", meth_getsockname),
	        new luaL_Reg("getstats",    meth_getstats),
	        new luaL_Reg("setstats",    meth_setstats),
	        new luaL_Reg("listen",      meth_listen),
	        new luaL_Reg("receive",     meth_receive),
	        new luaL_Reg("send",        meth_send),
	        new luaL_Reg("setfd",       meth_setfd),
	        new luaL_Reg("setoption",   meth_setoption),
	        new luaL_Reg("setpeername", meth_connect),
	        new luaL_Reg("setsockname", meth_bind),
	        new luaL_Reg("settimeout",  meth_settimeout),
	        new luaL_Reg("shutdown",    meth_shutdown),
		    new luaL_Reg(null, null)
	    };
	
        public delegate int pOpt_function(lua_State L, pSocket p);
	
	    public class luaL_pOptReg {

		    public CharPtr name;
		    public pOpt_function func;

		    public luaL_pOptReg(String strName, pOpt_function function) {
			    name = strName;
                func = function;
		    }

		    public pOpt_function GetJavaFunction() {
			    return this.func;
		    }
	    }
	    public static luaL_pOptReg[] opt = {
		    new luaL_pOptReg("keepalive",   opt_keepalive),
		    new luaL_pOptReg("reuseaddr",   opt_reuseaddr),
		    new luaL_pOptReg("tcp-nodelay", opt_tcp_nodelay),
		    new luaL_pOptReg("linger",      opt_linger),
		    new luaL_pOptReg(null, null)
	    };
	
	    public static luaL_Reg[] tcpCreate = {
		    new luaL_Reg("tcp", global_create),
		    new luaL_Reg(null, null)
	    };
	
	    public static int tcp_open(lua_State L)
	    {
	        /* create classes */
	        auxiliar_newclass(L, new CharPtr("tcp{master}"), tcpCreateFunc);
	        auxiliar_newclass(L, new CharPtr("tcp{client}"), tcpCreateFunc);
	        auxiliar_newclass(L, new CharPtr("tcp{server}"), tcpCreateFunc);
	        /* create class groups */
	        auxiliar_add2group(L, new CharPtr("tcp{master}"), new CharPtr("tcp{any}"));
	        auxiliar_add2group(L, new CharPtr("tcp{client}"), new CharPtr("tcp{any}"));
	        auxiliar_add2group(L, new CharPtr("tcp{server}"), new CharPtr("tcp{any}"));
	        /* define library functions */
	        luaI_openlib(L, null, tcpCreate, 0); 
	        return 0;
	    }
	
	    public static int meth_send(lua_State L) {
	        pTcp tcp = (pTcp) auxiliar_checkclass(L, new CharPtr("tcp{client}"), 1);
	        return buffer_meth_send(L, tcp.buf);
	    }

	    public static int meth_receive(lua_State L) {
		    pTcp tcp = (pTcp) auxiliar_checkclass(L, new CharPtr("tcp{client}"), 1);
	        return buffer_meth_receive(L, tcp.buf);
	    }

	    public static int meth_getstats(lua_State L) {
		    pTcp tcp = (pTcp) auxiliar_checkclass(L, new CharPtr("tcp{client}"), 1);
	        return buffer_meth_getstats(L, tcp.buf);
	    }

	    public static int meth_setstats(lua_State L) {
		    pTcp tcp = (pTcp) auxiliar_checkclass(L, new CharPtr("tcp{client}"), 1);
	        return buffer_meth_setstats(L, tcp.buf);
	    }
	
	    public static int meth_setoption(lua_State L)
	    {
		    pTcp tcp = (pTcp) auxiliar_checkgroup(L, new CharPtr("tcp{any}"), 1);
	        return opt_meth_setoption(L, opt, tcp.sock);
	    }
	
	    public static int meth_getfd(lua_State L)
	    {
		    pTcp tcp = (pTcp) auxiliar_checkgroup(L, new CharPtr("tcp{any}"), 1);
		
	        Lunar<pSocket>.push(L, tcp.sock, false);
	        return 1;
	    }

	    /* this is very dangerous, but can be handy for those that are brave enough */
	    public static int meth_setfd(lua_State L)
	    {
		    pTcp tcp = (pTcp) auxiliar_checkgroup(L, new CharPtr("tcp{any}"), 1);
	        tcp.sock = (pSocket) Lua.lua_touserdata(L, 2);
	        return 0;
	    }

	    public static int meth_dirty(lua_State L)
	    {
		    pTcp tcp = (pTcp) auxiliar_checkgroup(L, new CharPtr("tcp{any}"), 1);
	        lua_pushboolean(L, !buffer_isempty(tcp.buf));
	        return 1;
	    }
	
	    public static int meth_accept(lua_State L)
	    {
		    pTcp server = (pTcp) auxiliar_checkclass(L,  new CharPtr("tcp{server}"), 1);
	        pTimeout tm = timeout_markstart(server.tm);
	        pSocket sock = new pSocket(pSocket.SOCKET_TYPE_LISTENER);
	        int err = server.sock.Accept(out sock, null, null, tm);
	        /* if successful, push client socket */
	        if (err == pIO.IO_DONE) {
	            pTcp clnt = (pTcp) lua_newuserdata(L, typeof(pTcp));
	            auxiliar_setclass(L, new CharPtr("tcp{client}"), -1);
	            /* initialize structure fields */
	        
	            sock.SetNonBlocking(false);
	            clnt.sock = sock;
	            io_init(clnt.io, CSSocket.socket_send, CSSocket.socket_recv, 
	        		    CSSocket.socket_ioerror, clnt.sock);
	            timeout_init(clnt.tm, -1, -1);
	            buffer_init(clnt.buf, clnt.io, clnt.tm);
	            return 1;
	        } else {
	            lua_pushnil(L); 
	            lua_pushstring(L, "error socket accept tcp");
	            return 2;
	        }
	    }
	
	    public static int meth_bind(lua_State L)
	    {
	        pTcp tcp = (pTcp) auxiliar_checkclass(L, new CharPtr("tcp{master}"), 1);
	        CharPtr address =  luaL_checkstring(L, 2);
	        int port = (int) luaL_checknumber(L, 3);
	        CharPtr err = tcp.sock.Bind(address, port);
	        if (err != null) {
	            lua_pushnil(L);
	            lua_pushstring(L, err);
	            return 2;
	        }
	        lua_pushnumber(L, 1);
	        return 1;
	    }
	
	    public static int meth_connect(lua_State L)
	    {
	        pTcp tcp = (pTcp) auxiliar_checkgroup(L, new CharPtr("tcp{any}"), 1);
	        CharPtr address =  luaL_checkstring(L, 2);
	        int port = (int) luaL_checknumber(L, 3);
	        pTimeout tm = timeout_markstart(tcp.tm);
	        CharPtr err = tcp.sock.Connect(address, port, tm);
	        /* have to set the class even if it failed due to non-blocking connects */
	        auxiliar_setclass(L, new CharPtr("tcp{client}"), 1);
	        if (err != null) {
	            lua_pushnil(L);
	            lua_pushstring(L, err);
	            return 2;
	        }
	        /* turn master object into a client object */
	        lua_pushnumber(L, 1);
	        return 1;
	    }
	
	    public static int meth_close(lua_State L)
	    {
		    pTcp tcp = (pTcp) auxiliar_checkgroup(L, new CharPtr("tcp{any}"), 1);
	        tcp.sock.Destroy();
	        lua_pushnumber(L, 1);
	        return 1;
	    }
	
	    public static int meth_listen(lua_State L)
	    {
		    pTcp tcp = (pTcp) auxiliar_checkclass(L, new CharPtr("tcp{master}"), 1);
	        int backlog = (int) luaL_optnumber(L, 2, 32);
	        int err = tcp.sock.Listen(backlog);
	        if (err != pIO.IO_DONE) {
	            lua_pushnil(L);
	            lua_pushstring(L, new CharPtr("Cannot listen"));
	            return 2;
	        }
	        /* turn master object into a server object */
	        auxiliar_setclass(L, new CharPtr("tcp{server}"), 1);
	        lua_pushnumber(L, 1);
	        return 1;
	    }
	
	    public static int meth_shutdown(lua_State L)
	    {
	        pTcp tcp = (pTcp) auxiliar_checkclass(L, new CharPtr("tcp{client}"), 1);
	        CharPtr how = luaL_optstring(L, 2, new CharPtr("both"));
	        switch (how[0]) {
	            case 'b':
	                if (strcmp(how, new CharPtr("both")) != 0)
	                {
	            	    luaL_argerror(L, 2, "invalid shutdown method");
	        	        return 0;
	                }
	                tcp.sock.Shutdown(2);
	                break;
	            case 's':
	                if (strcmp(how, new CharPtr("send")) != 0)
	                {
	            	    luaL_argerror(L, 2, "invalid shutdown method");
	        	        return 0;
	                }
	                tcp.sock.Shutdown(1);
	                break;
	            case 'r':
	                if (strcmp(how, new CharPtr("receive")) != 0)
	                {
	            	    luaL_argerror(L, 2, "invalid shutdown method");
	        	        return 0;
	                }
	                tcp.sock.Shutdown(0);
	                break;
	        }
	        lua_pushnumber(L, 1);
	        return 1;	    
	    }
	
	    public static int meth_getpeername(lua_State L)
	    {
		    pTcp tcp = (pTcp) auxiliar_checkgroup(L, new CharPtr("tcp{any}"), 1);
            int port = 0;
	        CharPtr peerName = tcp.sock.GetPeerName(ref port);
	        if (peerName == null) {
	            lua_pushnil(L);
	            lua_pushstring(L, new CharPtr("getpeername failed"));
	        } else {
	            lua_pushstring(L, peerName);
	            lua_pushnumber(L, port);
	        }
	        return 2;
	    }

	    public static int meth_getsockname(lua_State L)
	    {
		    pTcp tcp = (pTcp) auxiliar_checkgroup(L, new CharPtr("tcp{any}"), 1);
            int port = 0;
	        CharPtr sockName = tcp.sock.GetSockName(ref port);
	        if (sockName == null) {
	            lua_pushnil(L);
	            lua_pushstring(L, new CharPtr("getsockname failed"));
	        } else {
	            lua_pushstring(L, sockName);
	            lua_pushnumber(L, port);
	        }
	        return 2;
	    }
	
	    public static int meth_settimeout(lua_State L)
	    {
		    pTcp tcp = (pTcp) auxiliar_checkgroup(L, new CharPtr("tcp{any}"), 1);
	        return timeout_meth_settimeout(L, tcp);
	    }
	
	    public static int global_create(lua_State L)
	    {
		    pSocket sock;
		    sock = new pSocket(pSocket.SOCKET_TYPE_TCP);
	        /* try to allocate a system socket */
	        if (sock != null) { 
	            /* allocate tcp object */
	            pTcp tcp = (pTcp) lua_newuserdata(L, typeof(pTcp));
	            /* set its type as master object */
	            auxiliar_setclass(L, new CharPtr("tcp{master}"), -1);
	            /* initialize remaining structure fields */
	            sock.SetNonBlocking(false);
	            tcp.sock = sock;
	            io_init(tcp.io, CSSocket.socket_send, CSSocket.socket_recv, 
	                    CSSocket.socket_ioerror, tcp.sock);
	            timeout_init(tcp.tm, -1, -1);
	            buffer_init(tcp.buf, tcp.io, tcp.tm);
	            return 1;
	        } else {
	            lua_pushnil(L);
	            lua_pushstring(L, "Cannot create socket");
	            return 2;
	        }
	    }
	
	    public static luaL_Reg[] timeout = {
		    new luaL_Reg("gettime",   timeout_lua_gettime),
		    new luaL_Reg("sleep",   timeout_lua_sleep),
		    new luaL_Reg(null, null)
	    };
	
	    public static void timeout_init(pTimeout tm, double block, double total) {
	        tm.block = block;
	        tm.total = total;
	    }
	
	    public static double timeout_get(pTimeout tm) {
	        if (tm.block < 0.0 && tm.total < 0.0) {
	            return -1;
	        } else if (tm.block < 0.0) {
	            double t = tm.total - timeout_gettime() + tm.start;
	            return Math.Max(t, 0.0);
	        } else if (tm.total < 0.0) {
	            return tm.block;
	        } else {
	            double t = tm.total - timeout_gettime() + tm.start;
	            return Math.Min(tm.block, Math.Max(t, 0.0));
	        }
	    }
	
	    public static double timeout_getstart(pTimeout tm) {
	        return tm.start;
	    }
	
	    public static double timeout_getretry(pTimeout tm) {
	        if (tm.block < 0.0 && tm.total < 0.0) {
	            return -1;
	        } else if (tm.block < 0.0) {
	            double t = tm.total - timeout_gettime() + tm.start;
	            return Math.Max(t, 0.0);
	        } else if (tm.total < 0.0) {
	            double t = tm.block - timeout_gettime() + tm.start;
	            return Math.Max(t, 0.0);
	        } else {
	            double t = tm.total - timeout_gettime() + tm.start;
	            return Math.Min(tm.block, Math.Max(t, 0.0));
	        }
	    }
	
	    public static pTimeout timeout_markstart(pTimeout tm) {
	        tm.start = timeout_gettime();
	        return tm;
	    }
	
	    public static double timeout_gettime() {
            return DateTime.Now.Ticks;
	    }
	
	    public static int timeout_open(lua_State L) {
	        luaI_openlib(L, null, timeout, 0);
	        return 0;
	    }
	
	    public static int timeout_meth_settimeout(lua_State L, pTcp tcp) {
	        double t = luaL_optnumber(L, 2, -1);
	        CharPtr mode = luaL_optstring(L, 3, "b");
	        pTimeout tm = tcp.tm;
	        if(tcp.sock != null)
	    	    tcp.sock.SetTimeout((int)t);
	        switch (mode[0]) {
	            case 'b':
	                tm.block = t; 
	                break;
	            case 'r': case 't':
	                tm.total = t;
	                break;
	            default:
	                luaL_argcheck(L, false, 3, "invalid timeout mode");
	                break;
	        }
	        lua_pushnumber(L, 1);
	        return 1;
	    }
	
	    public static int timeout_lua_gettime(lua_State L)
	    {
	        lua_pushnumber(L, timeout_gettime());
	        return 1;
	    }
	
	    public static int timeout_lua_sleep(lua_State L)
	    {
	        double n = luaL_checknumber(L, 1);
	        try
		    {
#if !NETFX_CORE
			    Thread.Sleep(new TimeSpan((long) n));
#else
                new System.Threading.ManualResetEvent(false).WaitOne((int)n);
#endif
		    }
		    catch(Exception e)
		    {
			    //e.printStackTrace();
		    }
		    return 0; 
	    }
	
	    public static luaL_Reg[] inet = {
		    new luaL_Reg("toip", inet_global_toip),
		    new luaL_Reg("tohostname", inet_global_tohostname),
		    new luaL_Reg("gethostname", inet_global_gethostname),
		    new luaL_Reg(null, null)
	    };
	
	    public static int inet_open(lua_State L)
	    {
	        lua_pushstring(L, "dns");
	        lua_newtable(L);
	        luaI_openlib(L, null, inet, 0);
	        lua_settable(L, -3);
	        return 0;
	    }
	
	    public static int inet_global_tohostname(lua_State L) {
	        CharPtr address = luaL_checkstring(L, 1);
#if WINDOWS_PHONE
	        DnsEndPoint addr = null;
		    try
		    {
                addr = new DnsEndPoint(address.toString(), 0);
		    }
#elif NETFX_CORE
            HostName addr = null;
            try
            {
                addr = new HostName(address.toString());
            }
#endif
		    catch(Exception e)
		    {
			    //e.printStackTrace();
		    }
	        if (addr == null) {
	            lua_pushnil(L);
	            lua_pushstring(L, "cannot get hostname");
	            return 2;
	        }
	        lua_pushstring(L, addr.
#if !NETFX_CORE
                Host
#else
                DisplayName
#endif
                );
	        inet_pushresolved(L, addr);
	        return 2;
	    }
	
	    public static int inet_global_toip(lua_State L)
	    {
	        CharPtr address = luaL_checkstring(L, 1);
#if WINDOWS_PHONE
            DnsEndPoint addr = null;
		    try
		    {
                addr = new DnsEndPoint(address.toString(), 0);
		    }
#elif NETFX_CORE
            HostName addr = null;
            try
            {
                addr = new HostName(address.toString());
            }
#endif
		    catch(Exception e)
		    {
			    //e.printStackTrace();
		    }
	        if (addr == null) {
	            lua_pushnil(L);
	            lua_pushstring(L, "cannot get ip");
	            return 2;
	        }
	        lua_pushstring(L, addr
#if NETFX_CORE
                .DisplayName
#else
                .Host
#endif
                );
	        inet_pushresolved(L, addr);
	        return 2;
	    }
	
	    public static int inet_global_gethostname(lua_State L)
	    {
		    String name = null;
#if WINDOWS_PHONE
		    try
		    {
			    name = new DnsEndPoint("127.0.0.1", 0).Host;
		    }
#elif NETFX_CORE
            try
            {
                name = new HostName("127.0.0.1").DisplayName;
            }
#endif
		    catch(Exception e)
		    {
			    //e.printStackTrace();
		    }
	        if (name == null) {
	            lua_pushnil(L);
	            lua_pushstring(L, "gethostname failed");
	            return 2;
	        } else {
	            lua_pushstring(L, name);
	            return 1;
	        }
	    }
	
	    public static void inet_pushresolved(lua_State L,
#if !NETFX_CORE
            DnsEndPoint
#else
            HostName
#endif
            addr)
	    {
	        int i, resolved;
	        lua_newtable(L); resolved = lua_gettop(L);
	        lua_pushstring(L, "name");
#if !NETFX_CORE
	        lua_pushstring(L, addr.Host);
#else
            lua_pushstring(L, addr.DisplayName);
#endif
	        lua_settable(L, resolved);
	        lua_pushstring(L, "ip");
	        lua_pushstring(L, "alias");
	        i = 1;
	        /*DnsEndPoint[] alias = null;
		    try
		    {
			    alias =  InetAddress.getAllByName(addr.getHostName());
		    }
		    catch(UnknownHostException e)
		    {
			    e.printStackTrace();
		    }*/
	        lua_newtable(L);
	        /*if (alias != null) 
	        {
	            for(DnsEndPoint aliasaddr in alias)
	            {
	                lua_pushnumber(L, i);
	                lua_pushstring(L, aliasaddr.getHostName());
	                lua_settable(L, -3);
	            }
	        }*/
	        lua_settable(L, resolved);
	        i = 1;
	        lua_newtable(L);
	        /*if (alias != null) 
	        {
	    	    for(InetAddress aliasaddr : alias)
	            {
	                lua_pushnumber(L, i);
	                lua_pushstring(L, aliasaddr.getHostAddress());
	                lua_settable(L, -3);
	            }
	        }*/
	        lua_settable(L, resolved);
	    }
	
	    public static luaL_Reg[] socketmod = {
		    new luaL_Reg("auxiliar",   auxiliar_open),
		    new luaL_Reg("except",   except_open),
		    new luaL_Reg("timeout",   timeout_open),
		    new luaL_Reg("buffer",   buffer_open),
		    new luaL_Reg("inet",   inet_open),
		    new luaL_Reg("tcp",   tcp_open),
		    //new luaL_Reg("udp",  timeout_lua_sleep),
		    new luaL_Reg("select",   select_open),
		    new luaL_Reg(null, null)
	    };

        public static luaL_Reg[] socket = {
		    new luaL_Reg("skip",      global_skip),
		    new luaL_Reg("__unload",  global_unload),
		    new luaL_Reg(null, null)
	    };

	    /*-------------------------------------------------------------------------*\
	    * Skip a few arguments
	    \*-------------------------------------------------------------------------*/
	    public static int global_skip(lua_State L) {
	        int amount = luaL_checkint(L, 1);
	        int ret = lua_gettop(L) - amount - 1;
	        return ret >= 0 ? ret : 0;
	    }

	    /*-------------------------------------------------------------------------*\
	    * Unloads the library
	    \*-------------------------------------------------------------------------*/
	    public static int global_unload(lua_State L) {
	        //socket_close();
	        return 0;
	    }

	    /*-------------------------------------------------------------------------*\
	    * Setup basic stuff.
	    \*-------------------------------------------------------------------------*/
	    public static int socket_base_open(lua_State L) {
	        //if (socket_open()) {
	            /* export functions (and leave namespace table on top of stack) */
	            luaI_openlib(L, new CharPtr("socket"), socket, 0);
	            /* make version string available to scripts */
	            lua_pushstring(L, "_VERSION");
	            lua_pushstring(L, "Java Socket 1.0");
	            lua_rawset(L, -3);
	            return 1;
	        /*} else {
	            lua_pushstring(L, "unable to initialize library");
	            lua_error(L);
	            return 0;
	        }*/
	    }

	    /*-------------------------------------------------------------------------*\
	    * Initializes all library modules.
	    \*-------------------------------------------------------------------------*/
	    public static int luaopen_socket_core(lua_State L) {
	        int i;
	        socket_base_open(L);
	        for (i = 0; socketmod[i].name != null; i++)
	        {
	    	    socketmod[i].func(L);
	        }
	        return 1;
	    }
    }
}
