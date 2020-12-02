pParent = nil;
pViewInflater = nil;

function GetViewInflater(luacontext)
	local a = 1;
	if a == 1 then
		pViewInflater = LuaViewInflater.Create(luacontext);
	end
	
	return pViewInflater;
end

-- Local variables
pSaveButton = nil;
pTitle = nil;
pDescription = nil;
pReporter = nil;
pAffectedUser = nil;
pPriority = nil;
pSeverity = nil;
pImpact = nil;

-- Save Button Start

function SaveButton_Click(a,b)
	
end

function SaveButton_Constructor(pGUI, luacontext)
	pSaveButton = pGUI;
	pGUI:RegisterEventFunction("Click", LuaTranslator.Register(pGUI, "SaveButton_Click"));
end

RegisterGuiEvent("ButSave", GUIEVENT_CREATE, "SaveButton_Constructor");
--Save Button End

-- Title Start
function Title_Constructor(pGUI, luacontext)
	pTitle = pGUI;
end

RegisterGuiEvent("ETTitle", GUIEVENT_CREATE, "Title_Constructor");
-- Title End

-- Description Start
function Description_Constructor(pGUI, luacontext)
	pDescription = pGUI;
end

RegisterGuiEvent("ETDescription", GUIEVENT_CREATE, "Description_Constructor");
-- Description End

-- Reporter Start
function Reporter_Constructor(pGUI, luacontext)
	pReporter = pGUI;
	pGUI:AddComboItem("User", "User");
	pGUI:ShowDelete(0);
	pGUI:SetEditable(0);
end

RegisterGuiEvent("CBReporter", GUIEVENT_CREATE, "Reporter_Constructor");
-- Reporter End

-- AffectedUser Start
function AffectedUser_Constructor(pGUI, luacontext)
	pAffectedUser = pGUI;
	pGUI:AddComboItem("User", "User");
	pGUI:ShowDelete(0);
	pGUI:SetEditable(0);
end

RegisterGuiEvent("CBAffectedUser", GUIEVENT_CREATE, "AffectedUser_Constructor");
-- AffectedUser End

-- Priority Start
function Priority_Constructor(pGUI, luacontext)
	pPriority = pGUI;
	pGUI:AddComboItem("High", "High");
	pGUI:AddComboItem("Medium", "Medium");
	pGUI:AddComboItem("Low", "Low");
	pGUI:ShowDelete(0);
	pGUI:SetEditable(0);
end

RegisterGuiEvent("CBPriority", GUIEVENT_CREATE, "Priority_Constructor");
-- Priority End

-- Severity Start
function Severity_Constructor(pGUI, luacontext)
	pSeverity = pGUI;
	pGUI:AddComboItem("High", "High");
	pGUI:AddComboItem("Medium", "Medium");
	pGUI:AddComboItem("Low", "Low");
	pGUI:ShowDelete(0);
	pGUI:SetEditable(0);
end

RegisterGuiEvent("CBSeverity", GUIEVENT_CREATE, "Severity_Constructor");
-- Severity End

-- Impact Start
function Impact_Constructor(pGUI, luacontext)
	pImpact = pGUI;
	pGUI:AddComboItem("High", "High");
	pGUI:AddComboItem("Medium", "Medium");
	pGUI:AddComboItem("Low", "Low");
	pGUI:ShowDelete(0);
	pGUI:SetEditable(0);
end

RegisterGuiEvent("CBImpact", GUIEVENT_CREATE, "Impact_Constructor");
-- Impact End

print(GUIEVENT_ZERO .. " val");
print(GUIEVENT_CREATE .. " val");
print(GUIEVENT_UPDATE .. " val");













































































function Button_Test_Click(a, b)
	pParent:DeleteClick();
end

function GUI_Event(pGUI, luacontext)
	local tabHost = LuaTabForm:Create(luacontext, "tabs");
	local formA = LuaForm:CreateForTab(luacontext, "forma");
	local formB = LuaForm:CreateForTab(luacontext, "formb");
	tabHost:AddTabSrc(formA, "Tab1", "", "bigmarkergreen.png", "main.xml");
	tabHost:AddTabSrc(formB, "Tab2", "", "bigmarkergreen.png", "main.xml");
	tabHost:Setup(pGUI);
	local point = { x = 0, y = { a = 0, b = 1} };
	print(point.x)
	-- point = { x = 0, y = 0 };
	-- abc = pGUI:LuaStructTest(point);
	-- abc.x = 10;
	-- adapterview = LGAdapterView:Create(luacontext);
	-- adapterview:AddValue("sad");
	-- view = LGView:Create(luacontext);
	-- pGUI:LuaStructTest(point);
	-- pParent = pGUI;
	-- pButton = pGUI:AddButton("buttonmenu", "test", "Sil", 222, 296, 119, 37, 0);
	-- pButton:RegisterEventFunction("Click", LuaTranslator:Register(pButton, "Button_Test_Click"));
end

RegisterGuiEvent("Main", GUIEVENT_CREATE, "GUI_Event");

function ListViewTest_AdapterView(pGui, position, data, oldView, luacontext)
	local viewToRet;
	if viewToRet == nil then
		viewToRet = LGTextView:Create(luacontext, "TextViewTag");
	else
		viewToRet = oldView;
	end
	
	viewToRet:SetText(data);
	return viewToRet;
end

function ListViewTest_Constructor(pGUI, luacontext)
	local pAdapter = LGAdapterView:Create(luacontext, "ListAdapterTest");
	pAdapter:AddValue(0, "test 1");
	pAdapter:AddValue(1, "test 2");
	RegisterGuiEvent("ListAdapterTest", GUIEVENT_ADAPTERVIEW, "ListViewTest_AdapterView");
	pGUI:SetAdapter(pAdapter);
end

RegisterGuiEvent("ListViewTest", GUIEVENT_CREATE, "ListViewTest_Constructor");

function Info_TestButton_Click(a, b)

end

function Info_TestButton_Constructor(pGUI)
	pGUI:RegisterEventFunction("Click", LuaTranslator:Register(pGUI, "Info_GUI_Click"));
	database = LuaDatabase:Create(pGUI);
end

RegisterGuiEvent("TestButton", 1, "Info_TestButton_Constructor"); 