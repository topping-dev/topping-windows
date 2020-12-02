local incidentListArr = { }

function ListViewIncidents_AdapterView_ItemSelected(pGui, listView, detailView, position, data)
	local form = LuaForm.GetActiveForm();
	if detailView ~= nil then
		if detailView:IsInitialized() == false then
			--Intialize the lgview structure of detail fragment
			--frag = LuaFragment.CreateWithUI(form:GetContext(), "helpdeskList", "helpdesk.xml");
			detailView:SetViewId("helpdeskList");
			detailView:SetViewXML("helpdesk.xml");
		end
		--Set values of detail fragment
				
	else
		local dataForAdapter = incidentListArr[position + 1];
		--Buna bir cozum
		selectedIncident = dataForAdapter["id"];
		selectedIncidentType = dataForAdapter["projectType"];
		LuaForm.CreateWithUI(form:GetContext(), "helpdeskList", "helpdesk.xml");
	end
end

function ListViewIncidents_AdapterView(pGui, listView, position, data, oldView, luacontext)
	if oldView == nil then
	print("if");
		local inflator = LuaViewInflator.Create(luacontext);
		local viewToRet = inflator:ParseFile("incidentsAdapter.xml", listView);
		inflator:Free();
		print(incidentListArr);
		local dataForAdapter = incidentListArr[position + 1];
		for k,v in pairs(incidentListArr) do print(k,v) end
		local tvTitle = viewToRet:GetViewById("incidentsAdapterTVTitle");
		tvTitle:SetText(dataForAdapter["title"]);
		local butType = viewToRet:GetViewById("incidentsAdapterButType");
		local pType = dataForAdapter["projectType"];
		--[[if pType == 1 then
			butType:SetText("G");
			butType:SetBackground("@drawable/blueback");
		else
			butType:SetText("C");
			butType:SetBackground("@drawable/redback");
		end]]
		return viewToRet;
		--viewToRet = LGTextView.Create(luacontext, "TextViewTag");
	else
	print("else");
		local viewToRet = oldView;
		local dataForAdapter = incidentListArr[position + 1];
		local tvTitle = viewToRet:GetViewById("incidentsAdapterTVTitle");
		tvTitle:SetText(dataForAdapter["title"]);
		local butType = viewToRet:GetViewById("incidentsAdapterButType");
		--[[local pType = dataForAdapter["projectType"];
		if pType == 1 then
			butType:SetText("G");
			butType:SetBackground("@drawable/blueback");
		else
			butType:SetText("C");
			butType:SetBackground("@drawable/redback");
		end]]
		return viewToRet;
	end
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
		local dataForAdapter = { id=jsoIncident:GetInt("id"), title=jsoIncident:GetString("name"), projectType=jsoIncident:GetInt("projecttype_id") }
		table.insert(incidentListArr, dataForAdapter);
		pAdapter:AddValue(i, "a");
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
	--local jsonString = "[{username: \"syd\", password: \"654654\" }]";
	httpClient:StartAsyncLoad(serverIp .. "workspace/Merkut/proxy/store/CommonHandler.ashx?store=Project&op=read&page=1&start=0&limit=100", "", "incidentsAsync");
end

RegisterGuiEvent("incidentsList", GUIEVENT_CREATE, "ListViewIncidents_Create");