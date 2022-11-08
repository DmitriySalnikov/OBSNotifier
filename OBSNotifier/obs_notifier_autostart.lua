--[[
	The original code was written by xcasxcursex
	https://github.com/DmitriySalnikov/OBSNotifier/issues/5
--]]

obs = obslua

function script_description()
	return [[Starts OBSNotifier when OBS starts]]
end

function script_defaults(settings)
	obs.obs_data_set_default_bool(settings, "lowprio", true)
end

function script_properties()
	local properties = obs.obs_properties_create()
	obs.obs_properties_add_bool(properties, "lowprio", "Run with lower priority")
	obs.obs_properties_apply_settings(properties, settings)
	return properties
end

function script_update(settings)
	local lowprio = obs.obs_data_get_bool(settings, "lowprio")
	start_OBSNotifier(path, lowprio)
end

function start_OBSNotifier(path, lowprio)
	local priostring = lowprio and "/BELOWNORMAL" or "/NORMAL"
	local StartCommand = "start /B /MIN " .. priostring .. " \"OBSNotifier\" \"&OBS_NOTIFIER_PATH&\" --force_close"
	obs.script_log(obs.LOG_INFO, "Executing command: " .. StartCommand)
	
	local output = os.execute(StartCommand)
	obs.script_log(obs.LOG_INFO, "Output: " .. output)
end