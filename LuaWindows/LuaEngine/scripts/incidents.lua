function ListViewIncidents_AdapterView_ItemSelected(pGui, listView, position, data)
	local form = LuaForm.GetActiveForm();
	LuaForm.CreateWithUI(form:GetContext(), "historyList", "history.xml");
end

local incidentListArr = { }

function ListViewIncidents_AdapterView(pGui, listView, position, data, oldView, luacontext)
	if oldView == nil then
		local inflator = LuaViewInflator.Create(luacontext);
		local viewToRet = inflator:ParseFile("incidentsAdapter.xml", listView);
		inflator:Free();
		print(incidentListArr);
		local dataForAdapter = incidentListArr[position + 1];
		for k,v in pairs(incidentListArr) do print(k,v) end
		local tvTitle = viewToRet:GetViewById("incidentsAdapterTVTitle");
		--tvTitle:SetText(dataForAdapter["title"]);
		tvTitle:SetText("aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa");
		local tvDate = viewToRet:GetViewById("incidentsAdapterTVDate");
		tvDate:SetText("b");--LuaDefines:GetHumanReadableDate(dataForAdapter["datecreated"]));
		local tvStatus = viewToRet:GetViewById("incidentsAdapterTVStatus");
		--tvStatus:SetText(dataForAdapter["priority"]);
		tvStatus:SetText(tostring(position));
		return viewToRet;
		--viewToRet = LGTextView.Create(luacontext, "TextViewTag");
	else
		local viewToRet = oldView;
		local dataForAdapter = incidentListArr[position + 1];
		local tvTitle = viewToRet:GetViewById("incidentsAdapterTVTitle");
		--tvTitle:SetText(dataForAdapter["title"]);
		tvTitle:SetText("aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa");
		local tvDate = viewToRet:GetViewById("incidentsAdapterTVDate");
		--tvDate:SetText(LuaDefines:GetHumanReadableDate(dataForAdapter["datecreated"]));
		tvDate:SetText("b");
		local tvStatus = viewToRet:GetViewById("incidentsAdapterTVStatus");
		--tvStatus:SetText(dataForAdapter["priority"]);
		tvStatus:SetText(tostring(position));
		return viewToRet;
	end
end

function ButIncidentsCreate_Click(pGUI)
	local form = LuaForm.GetActiveForm();
	LuaForm.CreateWithUI(form:GetContext(), "newIncident", "newIncident.xml");
end

function IncidentsHttpClient_OnFinished(pVariable, incidentsdata)
	local jso = LuaJSONObject.CreateJSOFromString(incidentsdata);
	local form = LuaForm.GetActiveForm();
	local pAdapter = LGAdapterView.Create(form:GetContext(), "ListAdapterTest");
	pAdapter:RegisterEventFunction("ItemSelected", LuaTranslator.Register(pAdapter, "ListViewIncidents_AdapterView_ItemSelected"));
	local records = jso:GetJSONObject("Records");
	local lists = records:GetJSONArray("list");
	local i = 0;
	local max = lists:Count(); 
	for i = 0, (max - 1), 1 do
		local jsoIncident = lists:GetJSONObject(i);
		--local dataForAdapter = { title=jsoIncident:GetString("title"), datecreated=jsoIncident:GetInt("datecreated"), priority=jsoIncident:GetInt("priority_id") }
		local dataForAdapter = { }
		table.insert(incidentListArr, dataForAdapter);
		pAdapter:AddValue(i, "");
	end
	RegisterGuiEvent("ListAdapterTest", GUIEVENT_ADAPTERVIEW, "ListViewIncidents_AdapterView");
	local list = form:GetViewById("incidentsList")
	list:SetAdapter(pAdapter);
end

function ListViewIncidents_Create(pGUI, luacontext)
	local form = LuaForm.GetActiveForm();
	--Create httpClient
	local httpClient = LuaHttpClient.Create("login");
	httpClient:SetContentType("application/json");
	httpClient:RegisterEventFunction("OnFinish", LuaTranslator.Register(httpClient, "IncidentsHttpClient_OnFinished"));
	--httpClient:StartAsyncLoad("http://merkut.alangoya.com/proxy/store/CommonHandler.ashx?op=read&store=event_Event", "", "merkut");
	httpClient:StartAsyncLoad("http://eservis.diyanet.gov.tr/proxy/store/commonhandler.ashx?store=Service&op=read", "", "merkut");
end

RegisterGuiEvent("incidentsList", GUIEVENT_CREATE, "ListViewIncidents_Create");

function ButIncidentsCreate_Create(pGUI, luacontext)
	pGUI:RegisterEventFunction("Click", LuaTranslator.Register(pGUI, "ButIncidentsCreate_Click"));
end

RegisterGuiEvent("incidentsButCreate", GUIEVENT_CREATE, "ButIncidentsCreate_Create");