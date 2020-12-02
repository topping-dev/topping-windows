function ListViewTasks_AdapterView(pGui, listView, position, data, oldView, luacontext)
	if oldView == nil then
		local inflator = LuaViewInflator.Create(luacontext);
		local viewToRet = inflator:ParseFile("tasksAdapter.xml", listView);
		inflator:Free();
		local tvDetail = viewToRet:GetViewById("tasksAdapterTVDetail");
		tvDetail:SetText(data);
		return viewToRet;
	else
		local viewToRet = oldView;
		local tvDetail = viewToRet:GetViewById("tasksAdapterTVDetail");
		tvDetail:SetText(data);
		return viewToRet;
	end
end

function ListViewTasks_Create(pGUI, luacontext)
	local pAdapter = LGAdapterView.Create(luacontext, "ListAdapterTasks");
	RegisterGuiEvent("ListAdapterTasks", GUIEVENT_ADAPTERVIEW, "ListViewTasks_AdapterView");
	pAdapter:AddValue(0, "1. common.cs icindeki hatalar duzeltilecek");
	pAdapter:AddValue(1, "2. Genel kod hata duzeltmeleri yapilacak");
	pAdapter:AddValue(2, "3. Dokumentasyon yazilacak.");
	pAdapter:AddValue(3, "4. Aramaya filtre eklenecek.");
	pGUI:SetAdapter(pAdapter);
end

RegisterGuiEvent("tasksList", GUIEVENT_CREATE, "ListViewTasks_Create");