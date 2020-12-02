-- Login Button Start
function LoginHttpClient_OnFinished(pVariable, data)
	print(pVariable);
	print(data);
	local form = LuaForm.GetActiveForm();
	pVariable:Free();
	LuaForm.CreateWithUI(form:GetContext(), "intro", "intro.xml");
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
	local jsonString = "[{username: \"alangoya\", password: \"654654\" }]";
	--serverIp = "http://192.168.1.27/"
	serverIp = "http://192.168.1.27/";
	httpClient:StartAsyncLoad(serverIp .. "workspace/Merkut/proxy/model/UserHandler.ashx?op=login", jsonString, "loginAsync");
end

function LoginButton_Create(pGUI, luacontext)
	pGUI:RegisterEventFunction("Click", LuaTranslator.Register(pGUI, "LoginButton_Click"));
end

RegisterGuiEvent("loginLoginButton", GUIEVENT_CREATE, "LoginButton_Create");
--Login Button End
