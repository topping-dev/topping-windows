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
	--local jsonString = "{method: \"DoLoginDevice\", id:1, params:[\"test\", \"test\"]}";
	local jsonString = GetJayrockRPCJson("DoLoginDevice", "test", "test");
	serverIp = "http://192.168.1.115:8080/";
	httpClient:StartAsyncLoad(serverIp .. "Login.ashx/DoLoginDevice?m=1", jsonString, "loginAsync");
end

function LoginButton_Create(pGUI, luacontext)
	pGUI:RegisterEventFunction("Click", LuaTranslator.Register(pGUI, "LoginButton_Click"));
end

RegisterGuiEvent("loginLoginButton", GUIEVENT_CREATE, "LoginButton_Create");
--Login Button End

function Login_NFC(pGUI, context, data)
	print(table.show(data));
	--local dataArr = split(data, "/");
	--LuaDialog.MessageBox(context, "Title", table.show(data));
end

RegisterGuiEvent("Main", GUIEVENT_NFC, "Login_NFC");
