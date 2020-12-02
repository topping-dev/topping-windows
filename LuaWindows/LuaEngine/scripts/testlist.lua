function ListViewTest_AdapterView(pGui, parent, position, data, oldView, luacontext)
	local viewToRet;
	if oldView == nil then
		local inflator = LuaViewInflator.Create(luacontext);
		viewToRet = inflator:ParseFile("testlistAdapter.xml", listView);
		inflator:Free();
	else
		viewToRet = oldView;
	end
	return viewToRet;
end

function ListViewTest_Constructor(pGUI, luacontext)
	local pAdapter = LGAdapterView:Create(luacontext, "ListAdapterTest");
	pAdapter:AddValue(0, "test 1");
	pAdapter:AddValue(1, "test 2");
	RegisterGuiEvent("ListAdapterTest", GUIEVENT_ADAPTERVIEW, "ListViewTest_AdapterView");
	pGUI:SetAdapter(pAdapter);
end

RegisterGuiEvent("ListViewTest", GUIEVENT_CREATE, "ListViewTest_Constructor");