/*
** $Id: linit.c,v 1.14.1.1 2007/12/27 13:02:25 roberto Exp $
** Initialization of libraries for lua.c
** See Copyright Notice in lua.h
*/

using System;
using System.Collections.Generic;
using System.Text;

namespace LuaCSharp
{
	public partial class Lua
	{
		private readonly static luaL_Reg[] lualibs = {
		  new luaL_Reg("", luaopen_base),
		  new luaL_Reg(LUA_LOADLIBNAME, luaopen_package),
		  new luaL_Reg(LUA_TABLIBNAME, luaopen_table),
		  new luaL_Reg(LUA_IOLIBNAME, luaopen_io),
		  new luaL_Reg(LUA_OSLIBNAME, luaopen_os),
		  new luaL_Reg(LUA_STRLIBNAME, luaopen_string),
		  new luaL_Reg(LUA_MATHLIBNAME, luaopen_math),
		  new luaL_Reg(LUA_DBLIBNAME, luaopen_debug),
          new luaL_Reg("socket", luaopen_socket_core),
          new luaL_Reg("mime", luaopen_mime_core),
		  new luaL_Reg(null, null)
		};


		public static void luaL_openlibs (lua_State L) {
		  for (int i=0; i<lualibs.Length-1; i++)
		  {
			luaL_Reg lib = lualibs[i];
			lua_pushcfunction(L, lib.func);
			lua_pushstring(L, lib.name);
			lua_call(L, 1, 0);
		  }
          luaL_loadfile(L, "mime.lua");
          lua_pcall(L, 0, 0, 0);
          luaL_loadfile(L, "socket.lua");
          lua_pcall(L, 0, 0, 0);
          /*luaL_loadfile(L, "socket/ftp.lua");
            lua_pcall(L, 0, 0, 0);
            luaL_loadfile(L, "socket/http.lua");
            lua_pcall(L, 0, 0, 0);
            luaL_loadfile(L, "socket/smtp.lua");
            lua_pcall(L, 0, 0, 0);
            luaL_loadfile(L, "socket/tp.lua");
            lua_pcall(L, 0, 0, 0);*/
          luaL_loadfile(L, "socket/url.lua");
          lua_pcall(L, 0, 0, 0);
          luaL_loadfile(L, "ltn12.lua");
          lua_pcall(L, 0, 0, 0);
		}

	}
}