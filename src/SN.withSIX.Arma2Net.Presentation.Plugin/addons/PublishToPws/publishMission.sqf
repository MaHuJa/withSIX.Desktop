private ["_missionName","_worldName"];

_missionName = _this select 0;
_worldName = _this select 1;

diag_log["Publishing Mission to PwS: ", _missionName, _worldName];
"Arma2Net.Unmanaged" callExtension format["SixArma2Net [PublishMission, '%1', '%2']", _missionName, _worldName];
