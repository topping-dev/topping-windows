function ListViewHelpDesk_AdapterView_ItemSelected(pGui, listView, detailView, position, data)
	local form = LuaForm.GetActiveForm();
	if detailView ~= nil then
		if detailView:IsInitialized() == false then
			--Intialize the lgview structure of detail fragment
			--frag = LuaFragment.CreateWithUI(form:GetContext(), "historyList", "history.xml");
			detailView:SetViewId("helpdeskinfo");
			detailView:SetViewXML("helpdeskinfo.xml");
		end
		--Set values of detail fragment
				
	else
		selectedHelpDeskId = position + 1;
		selectedHelpDeskInfo = helpDeskDataArr[position + 1];
		LuaForm.CreateWithUI(form:GetContext(), "helpdeskinfo", "helpdeskinfo.xml");
	end
end

function ListViewHelpDesk_AdapterView(pGui, listView, position, data, oldView, luacontext)
	local viewToRet = nil;
	if oldView == nil then
		local inflator = LuaViewInflator.Create(luacontext);
		viewToRet = inflator:ParseFile("helpdeskAdapter.xml", listView);
		inflator:Free();
		
	else
		viewToRet = oldView;
	end
	
	local dataTo = helpDeskDataArr[position + 1];
	
	local tvTitle = viewToRet:GetViewById("helpdeskAdapterTVTitle");
	tvTitle:SetText(dataTo["title"]);
	local tvDateCreated = viewToRet:GetViewById("helpdeskAdapterTVDateCreated");
	tvDateCreated:SetText(dataTo["dateCreated"]);
	
	local status = dataTo["status"];
	local statusStr = "";
	if status == 1 then
		statusStr = "Acik";
	elseif status == 2 then
		statusStr = "Islemde";
	elseif status == 3 then
		statusStr = "Cozumlenmis";
	elseif status == 4 then
		statusStr = "Esi var"
	elseif status == 5 then
		statusStr = "Cozumlenmis";
	elseif status == 6 then
		statusStr = "Alakasiz";
	elseif status == 7 then
		statusStr = "Baslanmamis";
	elseif status == 8 then
		statusStr = "Bitmis";
	elseif status == 9 then
		statusStr = "Geri bakis";		
	end
	
	local tvState = viewToRet:GetViewById("helpdeskAdapterTVState");
	tvState:SetText(statusStr);
	
	local priority = dataTo["priority"];
	local priorityStr = "";
	if priority == 1 then
		priorityStr = "Cok kritik";
	elseif priority == 2 then
		priorityStr = "Kritik";
	elseif priority == 3 then
		priorityStr = "Orta";
	elseif priority == 4 then
		priorityStr = "Dusuk";
	elseif priority == 5 then
		priorityStr = "Cok Dusuk";
	end
	
	local tvPriority = viewToRet:GetViewById("helpdeskAdapterTVPriority");
	tvPriority:SetText(priorityStr);
	
	return viewToRet;
end



function HelpDeskHttpClient_OnFinished(pVariable, incidentsdata)
	local jso = LuaJSONObject.CreateJSOFromString(incidentsdata);
	local form = LuaForm.GetActiveForm();
	local pAdapter = LGAdapterView.Create(form:GetContext(), "ListAdapterHelpDesk");
	pAdapter:RegisterEventFunction("ItemSelected", LuaTranslator.Register(pAdapter, "ListViewHelpDesk_AdapterView_ItemSelected"));
	RegisterGuiEvent("ListAdapterHelpDesk", GUIEVENT_ADAPTERVIEW, "ListViewHelpDesk_AdapterView");
	local records = jso:GetJSONObject("Records");
	lists = records:GetJSONArray("list");
	local i = 0;
	helpDeskDataArr = { };
	local max = lists:Count(); 
	for i = 0, (max - 1), 1 do
		local jsoIncident = lists:GetJSONObject(i);
		local dataForAdapter = { id=jsoIncident:GetInt("id"), title=jsoIncident:GetString("title"), dateCreated=jsoIncident:GetString("datecreated"), priority=jsoIncident:GetInt("priority"), status=jsoIncident:GetInt("status"), description=jsoIncident:GetString("about") }
		table.insert(helpDeskDataArr, dataForAdapter);
		pAdapter:AddValue(i, "");
	end
	local list = form:GetViewById("helpdeskList")
	list:SetAdapter(pAdapter);
end

function ListViewHelpDesk_Create(pGUI, luacontext)
	local form = LuaForm.GetActiveForm();
	local httpClient = LuaHttpClient.Create("login");
	httpClient:SetContentType("application/json");
	httpClient:RegisterEventFunction("OnFinish", LuaTranslator.Register(httpClient, "HelpDeskHttpClient_OnFinished"));
	local selType = "record_agile_Agile";
	if selectedIncidentType == 1 then
		selType = "record_Traditional";
	end
	--httpClient:StartAsyncLoad(serverIp .. "workspace/Merkut/proxy/store/CommonHandler.ashx?op=read&store=" .. selType .. "&filter=%5B%7B%22field%22:%22project_id%22,%22value%22:" .. selectedIncident .. ",%22type%22:%22number%22%7D%5D", "", "helpDeskAsync");
	httpClient:StartAsyncLoad("http://www.sombrenuit.org/test.json?op=read&store=" .. selType .. "&filter=%5B%7B%22field%22:%22project_id%22,%22value%22:" .. selectedIncident .. ",%22type%22:%22number%22%7D%5D", "", "helpDeskAsync");
	print(selectedIncident);
	print(selType);
	print(serverIp .. "workspace/Merkut/proxy/store/CommonHandler.ashx?op=read&store=" .. selType .. "&filter=%5B%7B%22field%22:%22project_id%22,%22value%22:" .. selectedIncident .. ",%22type%22:%22number%22%7D%5D")
end

function ListViewHelpDeskCBStatus_Changed(pGUI, name, tag)
	local form = LuaForm.GetActiveForm();
	local list = form:GetViewById("helpdeskList");
	local pAdapter = list:GetAdapter();
	pAdapter:RemoveAllValues();
	
	local numberId = tonumber(tag);
	
	local i = 0;
	helpDeskDataArr = { };
	local max = lists:Count();
	
	for i = 0, (max - 1), 1 do
		local jsoIncident = lists:GetJSONObject(i);
		local statusId = jsoIncident:GetInt("status");
		if numberId == statusId then 
			local dataForAdapter = { id=jsoIncident:GetInt("id"), title=jsoIncident:GetString("title"), dateCreated=jsoIncident:GetString("datecreated"), priority=jsoIncident:GetInt("priority"), status=statusId, description=jsoIncident:GetString("about") }
			table.insert(helpDeskDataArr, dataForAdapter);
			pAdapter:AddValue(i, "");
		end
	end
end

function ListViewHelpDeskCBStatus_Create(pGUI, luacontext)
	local form = LuaForm.GetActiveForm();
	pGUI:SetEditable(0);
	pGUI:ShowDelete(0);
	pGUI:AddComboItem("Acik", "1");
	pGUI:AddComboItem("Islemde", "2");
	pGUI:AddComboItem("Cozumlenmis", "3");
	pGUI:AddComboItem("Esi var", "4")
	pGUI:AddComboItem("Cozumlenmis", "5");
	pGUI:AddComboItem("Alakasiz", "6");
	pGUI:AddComboItem("Baslanmamis", "7");
	pGUI:AddComboItem("Bitmis", "8");
	pGUI:AddComboItem("Geri bakis", "9");
	pGUI:RegisterEventFunction("Changed", LuaTranslator.Register(pGUI, "ListViewHelpDeskCBStatus_Changed"))
end

RegisterGuiEvent("helpdeskList", GUIEVENT_CREATE, "ListViewHelpDesk_Create");
RegisterGuiEvent("helpdeskCBStatus", GUIEVENT_CREATE, "ListViewHelpDeskCBStatus_Create");