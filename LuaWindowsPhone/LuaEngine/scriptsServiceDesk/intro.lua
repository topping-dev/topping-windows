function ButProjects_Click(pGUI)
	local form = LuaForm.GetActiveForm();
	LuaForm.CreateWithUI(form:GetContext(), "incidents", "incidents.xml");
end

function ButProjects_Create(pGUI, luacontext)
	pGUI:RegisterEventFunction("Click", LuaTranslator.Register(pGUI, "ButProjects_Click"));
end

function ButHelpDesk_Click(pGUI)
	local form = LuaForm.GetActiveForm();
	selectedIncident = -1;
	LuaForm.CreateWithUI(form:GetContext(), "helpdesk", "helpdesk.xml");
end

function ButHelpDesk_Create(pGUI, luacontext)
	pGUI:RegisterEventFunction("Click", LuaTranslator.Register(pGUI, "ButHelpDesk_Click"));
end

RegisterGuiEvent("introButMyProjects", GUIEVENT_CREATE, "ButProjects_Create");
RegisterGuiEvent("introButHelpDesk", GUIEVENT_CREATE, "ButHelpDesk_Create");