function IntroHttpClient_OnFinished(pVariable, data)
	print(data);
	--local form = LuaForm.GetActiveForm();
	--LuaMessageBox.Show(form:GetContext(), "Title", "Added to basket");
end

function Intro_NFC(pGUI, context, data)
	print(table.show(data));
	for id,record in pairs(data) do
		if record["type"] == "uri" then
			local uri = record["uri"];
			local uriArr = string.split(uri, "/");
			
			local httpClient = LuaHttpClient.Create("login");
			--httpClient:SetContentType("text/plain");
			httpClient:RegisterEventFunction("OnFinish", LuaTranslator.Register(httpClient, "IntroHttpClient_OnFinished"));
			local postStr = "id=" .. uriArr[4];
			httpClient:StartAsyncLoad(serverIp .. "Basket.ashx?m=1", postStr, "loginAsync");
		end
	end
	
	--LuaMessageBox.Show(context, "Title", table.show(data));
end

function ButHelpDesk_Click(pGUI)
	local t = {}
	local gt = {}
	gt["uri"] = "alng://1/01";
	gt["type"] = "uri";
	t["0"] = gt;
	Intro_NFC(pGUI, luacontextG, t); 
end

function ButHelpDesk_Create(pGUI, luacontext)
	pGUI:RegisterEventFunction("Click", LuaTranslator.Register(pGUI, "ButHelpDesk_Click"));
end

RegisterGuiEvent("introTVStatus", GUIEVENT_CREATE, "ButHelpDesk_Create");
RegisterGuiEvent("intro", GUIEVENT_NFC, "Intro_NFC");