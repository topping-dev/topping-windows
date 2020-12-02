-- Login Button Start
function LoginHttpClient_OnFinished(pVariable, data)
	print(pVariable);
	print(data);
	local form = LuaForm.GetActiveForm();
	pVariable:Free();
	LuaForm.CreateWithUI(form:GetContext(), "incidentsList", "incidents.xml");
end

function LoginButton_Click(pGUI, context)
	local form = LuaForm.GetActiveForm();
	local username = form:GetViewById("loginUsername");
	local password = form:GetViewById("loginPassword");
	local uText = username:GetText();
	local pText = password:GetText();
	local httpClient = LuaHttpClient.Create("login");
	httpClient:SetContentType("application/json");
	httpClient:RegisterEventFunction("OnFinish", LuaTranslator.Register(httpClient, "LoginHttpClient_OnFinished"));
	--local jsonString = "[{username: \"syd\", password: \"654654\" }]";
	--httpClient:StartAsyncLoad("http://merkut.alangoya.com/proxy/model/UserHandler.ashx?op=login", jsonString, "merkut");
	local jsonString = "[{\"id\":null,\"personnelnumber\":\"1000\",\"fullname\":null,\"password\":\"654654\",\"tcnumber\":null,\"usertitle_id\":null,\"userstatus_id\":null,\"phone\":null,\"service_id\":null}]";
	httpClient:StartAsyncLoad("http://eservis.diyanet.gov.tr/proxy/model/UserHandler.ashx?op=login", jsonString, "merkut");
end

function LoginButton_Create(pGUI, luacontext)
	pGUI:RegisterEventFunction("Click", LuaTranslator.Register(pGUI, "LoginButton_Click"));
end

RegisterGuiEvent("loginLoginButton", GUIEVENT_CREATE, "LoginButton_Create");
--Login Button End
