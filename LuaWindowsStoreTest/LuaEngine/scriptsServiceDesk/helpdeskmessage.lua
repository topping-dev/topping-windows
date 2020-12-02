function ListViewHelpDeskMessage_AdapterView(pGui, listView, position, data, oldView, luacontext)
	if oldView == nil then
		local inflator = LuaViewInflator.Create(luacontext);
		local viewToRet = inflator:ParseFile("helpdeskmessageAdapter.xml", listView);
		inflator:Free();
		local tvSender = viewToRet:GetViewById("helpdeskMessageAdapterTVSender");
		tvSender:SetText(data["0"]);
		local tvDetail = viewToRet:GetViewById("helpdeskMessageAdapterTVDetail");
		tvDetail:SetText(data["1"]);
		return viewToRet;
	else
		local viewToRet = oldView;
		local tvSender = viewToRet:GetViewById("helpdeskMessageAdapterTVSender");
		tvSender:SetText(data["0"]);
		local tvDetail = viewToRet:GetViewById("helpdeskMessageAdapterTVDetail");
		tvDetail:SetText(data["1"]);
		return viewToRet;
	end
end

function ListViewHelpDeskMessage_Create(pGUI, luacontext)
	local pAdapter = LGAdapterView.Create(luacontext, "ListAdapterHelpDeskMessage");
	RegisterGuiEvent("ListAdapterHelpDeskMessage", GUIEVENT_ADAPTERVIEW, "ListViewHelpDeskMessage_AdapterView");
	local arr1 = { "Osman", "Hatalar duzeltildi" };
	pAdapter:AddValue(0, arr1);
	local arr2 = { "Hilmi", "Hatalar duzeltilmemis" };
	pAdapter:AddValue(1, arr2);
	local arr3 = { "Hayri", "Hatalar duzeltildi" }
	pAdapter:AddValue(2, arr3);
	pGUI:SetAdapter(pAdapter);
end

function HelpDeskMessageETText_Create(pGUI, luacontext)
	local form = LuaForm.GetActiveForm();
end

local count = 3;

function HelpDeskMessageButSend_Click(a,b)
	local form = LuaForm.GetActiveForm();
	local list = form:GetViewById("helpdeskMessageList");
	local ETText = form:GetViewById("helpdeskMessageETText");
	local pAdapter = list:GetAdapter();
	local message = ETText:GetText();
	pAdapter:AddValue(count, { "alangoya", message });
	count = count + 1;
end

function HelpDeskMessageButSend_Create(pGUI, luacontext)
	pGUI:RegisterEventFunction("Click", LuaTranslator.Register(pGUI, "HelpDeskMessageButSend_Click"));
end

RegisterGuiEvent("helpdeskMessageList", GUIEVENT_CREATE, "ListViewHelpDeskMessage_Create");
RegisterGuiEvent("helpdeskMessageETText", GUIEVENT_CREATE, "HelpDeskMessageETText_Create");
RegisterGuiEvent("helpdeskMessageButSend", GUIEVENT_CREATE, "HelpDeskMessageButSend_Create");