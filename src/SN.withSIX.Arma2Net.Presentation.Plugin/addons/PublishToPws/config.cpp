class CfgPatches
{
	class SIX_UI_PublishToPwS
	{
		units[]={};
		weapons[]={};
		requiredVersion=0.1;
		requiredAddons[]={"CAUI"};
	};
};

class CA_Title;
class CA_Title_Back;
class RscShortcutButton;
class RscPicture;
class RscText;

class RscMap
{
	class controls;
};
class RscDisplayArcadeMap: RscMap
{
	class controls: controls
	{
		class Load;
		class PublishToPwSButton: Load
		{
			idc = -1;
			text = $STR_SIX_PUBLISHTOPWS_BUTTON;
//			x = "(SafeZoneW + SafeZoneX) - (1 - 0.795)";
			y = 0.68;
//			w = 0.202;
			shortcuts[] = {};
			toolTip = "";
//			default = 0;
//			class TextPos
//			{
//				left = 0.052;
//				top = 0.034;
//				right = 0.005;
//				bottom = 0.005;
//			};
			onButtonClick = "(ctrlParent (_this select 0)) createDisplay ""RscDisplayTemplatePublishToPwS"";";
		};
	};
};

class RscDisplayTemplatePublishToPwS
{
	access = 0;
//	idd = 29;
	idd = 4712;
	movingEnable = 1;
	onLoad = "[] call compile preprocessFileLineNumbers ""six\ui\publishtopwsbutton\initRscDisplayTemplatePublishToPwS.sqf"";";
	class controls
	{
		class CA_Title: CA_Title
		{
			idc = -1;
			x = 0.275734;
			y = 0.313156;
			w = 0.433827;
			h = 0.039216;
			colorText[] = {0.95,0.95,0.95,1};
			sizeEx = 0.03;
			text = $STR_SIX_PUBLISHTOPWS_TITLE;
		};
		class WarningTextLine1: RscText
		{
			idc = -1;
			style = 2;//center
			x = 0.283087;
			y = 0.381784;
			w = 0.44;
			h = 0.029412;
			sizeEx = 0.02;
			text = $STR_SIX_PUBLISHTOPWS_WARNINGTEXTLINE1;
		};
		class WarningTextLine2: WarningTextLine1
		{
			y = 0.425902;
			text = $STR_SIX_PUBLISHTOPWS_WARNINGTEXTLINE2;
		};
		class CA_ButtonOK: RscShortcutButton
		{
			idc = 1;
			x = 0.34191;
			y = 0.489628;
			text = $STR_DISP_OK;
			default = 1;
			class TextPos
			{
				left = 0.05;
				top = 0.034;
				right = 0.005;
				bottom = 0.005;
			};
			onButtonClick = "[missionName,worldName] call SIX_fnc_publishMission";
		};
		class CA_ButtonCancel: RscShortcutButton
		{
			idc = 2;
			x = 0.536765;
			y = 0.489628;
			text = $STR_DISP_CANCEL;
			default = 0;
			class TextPos
			{
				left = 0.05;
				top = 0.034;
				right = 0.005;
				bottom = 0.005;
			};
		};
	};
	class controlsBackground
	{
		class CA_Background: RscPicture
		{
			x = 0.268381;
			y = 0.303352;
			w = 0.463239;
			h = 0.274512;
			text = "#(argb,8,8,3)color(1,1,1,1)";
			colortext[] = {0.1961,0.1451,0.0941,0.75};
		};
		class Blackback: RscPicture
		{
			x = "SafeZoneXAbs-SafeZoneWAbs*2";
			y = "SafeZoneY-SafeZoneH";
			w = "SafeZoneWAbs*6";
			h = "SafeZoneH*3";
			text = "#(argb,8,8,3)color(0,0,0,1)";
			colortext[] = {1,1,1,0.4};
		};
		class CA_Title_Background: CA_Title_Back
		{
			x = 0.275734;
			y = 0.313156;
			w = 0.448533;
			h = 0.039216;
			text = "#(argb,8,8,3)color(1,1,1,1)";
			colorText[] = {0,0,0,0.6};
			moving = 1;
		};
	};
};
