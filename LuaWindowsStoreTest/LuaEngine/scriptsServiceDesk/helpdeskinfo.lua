function ButHelpDeskInfoSave_Click(pGUI)
	local form = LuaForm.GetActiveForm();
end

function ButHelpDeskInfoTasks_Click(pGUI)
	local form = LuaForm.GetActiveForm();
	LuaForm.CreateWithUI(form:GetContext(), "tasksList", "tasks.xml");
end

function ButHelpDeskInfoActivities_Click(pGUI)
	local form = LuaForm.GetActiveForm();
	LuaForm.CreateWithUI(form:GetContext(), "helpdeskMessageList", "helpdeskmessage.xml");
end

function HelpDeskInfoLLMain_Create(pGUI, luacontext)
	local form = LuaForm.GetActiveForm();
end

function HelpDeskInfoTVTitle_Create(pGUI, luacontext)
	pGUI:SetText(selectedHelpDeskInfo["title"]);
end

function HelpDeskInfoTVDescription_Create(pGUI, luacontext)
	pGUI:SetText(selectedHelpDeskInfo["description"]);
end

function HelpDeskInfoCBPriority_Create(pGUI, luacontext)
	local form = LuaForm.GetActiveForm();
	pGUI:SetEditable(0);
	pGUI:ShowDelete(0);

	pGUI:AddComboItem("Cok kritik", "1");
	pGUI:AddComboItem("Kritik", "2");
	pGUI:AddComboItem("Orta", "3");
	pGUI:AddComboItem("Dusuk", "4");
	pGUI:AddComboItem("Cok Dusuk", "5");
	
	pGUI:SetSelected(tonumber(selectedHelpDeskInfo["priority"]));
end

function HelpDeskInfoCBStatus_Create(pGUI, luacontext)
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
	
	pGUI:SetSelected(tonumber(selectedHelpDeskInfo["status"]));
end

function HelpDeskInfoButSave_Create(pGUI, luacontext)
	pGUI:RegisterEventFunction("Click", LuaTranslator.Register(pGUI, "ButHelpDeskInfoSave_Click"));
end

function HelpDeskInfoButTasks_Create(pGUI, luacontext)
	pGUI:RegisterEventFunction("Click", LuaTranslator.Register(pGUI, "ButHelpDeskInfoTasks_Click"));
end

function HelpDeskInfoButActivities_Create(pGUI, luacontext)
	pGUI:RegisterEventFunction("Click", LuaTranslator.Register(pGUI, "ButHelpDeskInfoActivities_Click"));
end

RegisterGuiEvent("helpdeskinfoLLMain", GUIEVENT_CREATE, "HelpDeskInfoLLMain_Create");
RegisterGuiEvent("helpdeskinfoTVTitle", GUIEVENT_CREATE, "HelpDeskInfoTVTitle_Create");
RegisterGuiEvent("helpdeskinfoTVDescription", GUIEVENT_CREATE, "HelpDeskInfoTVDescription_Create");
RegisterGuiEvent("helpdeskinfoCBPriority", GUIEVENT_CREATE, "HelpDeskInfoCBPriority_Create");
RegisterGuiEvent("helpdeskinfoCBStatus", GUIEVENT_CREATE, "HelpDeskInfoCBStatus_Create");
RegisterGuiEvent("helpdeskinfoButSave", GUIEVENT_CREATE, "HelpDeskInfoButSave_Create");
RegisterGuiEvent("helpdeskinfoButTasks", GUIEVENT_CREATE, "HelpDeskInfoButTasks_Create");
RegisterGuiEvent("helpdeskinfoButActivities", GUIEVENT_CREATE, "HelpDeskInfoButActivities_Create");