-- Category Start
function Category_Create(pGUI, luacontext)
	pPriority = pGUI;
	pGUI:AddComboItem("Ana Kart", "11");
	pGUI:AddComboItem("Arayuz", "5");
	pGUI:AddComboItem("Donanim", "2");
	pGUI:AddComboItem("Ekran Karti", "9");
	pGUI:AddComboItem("Isletim Sistemi", "6");
	pGUI:AddComboItem("Monitor", "8");
	pGUI:AddComboItem("RAM", "10");
	pGUI:AddComboItem("Ses Karti", "12");
	pGUI:AddComboItem("Tarayici", "14");
	pGUI:AddComboItem("Uygulama", "7");
	pGUI:AddComboItem("Veritabani", "4");
	pGUI:AddComboItem("Web Sunucusu", "3");
	pGUI:AddComboItem("Yazici", "13");
	pGUI:AddComboItem("Yazilim", "1");
	pGUI:ShowDelete(0);
	pGUI:SetEditable(0);
end

RegisterGuiEvent("newIncidentCBCategory", GUIEVENT_CREATE, "Category_Create");
-- Category End

-- Priority Start
function Priority_Create(pGUI, luacontext)
	pPriority = pGUI;
	pGUI:AddComboItem("Cok kritik", "1");
	pGUI:AddComboItem("Kritik", "2");
	pGUI:AddComboItem("Orta", "3");
	pGUI:AddComboItem("Dusuk", "4");
	pGUI:AddComboItem("Cok Dusuk", "5");	
	pGUI:ShowDelete(0);
	pGUI:SetEditable(0);
end

RegisterGuiEvent("newIncidentCBPriority", GUIEVENT_CREATE, "Priority_Create");
-- Priority End

-- Create Button Start
function IncidentCreateHttpClient_OnFinished(pVariable, incidentsdata)
print(incidentsdata);
local form = LuaForm.GetActiveForm();
form:Close();
end

function CreateButton_Click(a,b)
	local form = LuaForm.GetActiveForm();
	local lgtitle = form:GetViewById("newIncidentETTitle");
	local lgdescription = form:GetViewById("newIncidentETDescription");
	local cbcategory = form:GetViewById("newIncidentCBCategory");
	local cbpriority = form:GetViewById("newIncidentCBPriority");
	local title = lgtitle:GetText();
	local desc = lgdescription:GetText();
	local category = cbcategory:GetSelectedTag();
	local priority = cbpriority:GetSelectedTag();
	local httpClient = LuaHttpClient.Create("login");
	httpClient:SetContentType("application/json;charset=utf-8");
	httpClient:RegisterEventFunction("OnFinish", LuaTranslator:Register(httpClient, "IncidentCreateHttpClient_OnFinished"));
	local jsonString = "[{ \"title\":\"" .. title .. "\", \"category_id\":\"" .. category .. "\", \"description\":\"" .. desc .. "\", \"priority_id\":\"" .. priority .. "\"}]";
	print(jsonString);
	httpClient:StartAsyncLoad("http://merkut.alangoya.com/proxy/model/CommonHandler.ashx?model=Event&op=create", jsonString, "incidentsAsync");
end

function CreateButton_Create(pGUI, luacontext)
	pGUI:RegisterEventFunction("Click", LuaTranslator.Register(pGUI, "CreateButton_Click"));
end

RegisterGuiEvent("newIncidentButCreate", GUIEVENT_CREATE, "CreateButton_Create");
--Login Button End