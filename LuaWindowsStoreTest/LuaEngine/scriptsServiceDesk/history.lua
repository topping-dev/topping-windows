function ListViewHistory_AdapterView(pGui, listView, position, data, oldView, luacontext)
	if oldView == nil then
		local inflator = LuaViewInflator.Create(luacontext);
		local viewToRet = inflator:ParseFile("historyAdapter.xml", listView);
		inflator:Free();
		local tvMessage = viewToRet:GetViewById("historyAdapterTVMessage");
		tvMessage:SetText(data);
		if position == 0 then
			tvMessage:SetTextColor("#FF0000");
		else
			tvMessage:SetTextColor("#0000FF");
		end
		return viewToRet;
		--viewToRet = LGTextView:Create(luacontext, "TextViewTag");
	else
		local viewToRet = oldView;
		local tvMessage = viewToRet:GetViewById("historyAdapterTVMessage");
		tvMessage:SetText(data);
		if position == 0 then
			tvMessage:SetTextColor("#FF0000");
		else
			tvMessage:SetTextColor("#0000FF");
		end
		return viewToRet;
	end
end

function ListViewHistory_Create(pGUI, luacontext)
	local form = LuaForm.GetActiveForm();
	local pAdapter = LGAdapterView.Create(luacontext, "ListAdapterHistory");
	RegisterGuiEvent("ListAdapterHistory", GUIEVENT_ADAPTERVIEW, "ListViewHistory_AdapterView");
	pAdapter:AddValue(0, "print() fonksiyonunda sorun var");
	pAdapter:AddValue(1, "^-->1.0 surumunden sonra duzeltildi");
	pGUI:SetAdapter(pAdapter);
end

RegisterGuiEvent("historyList", GUIEVENT_CREATE, "ListViewHistory_Create");