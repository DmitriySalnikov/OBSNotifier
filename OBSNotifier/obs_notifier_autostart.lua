﻿--[[
	The original code was written by xcasxcursex
	https://github.com/DmitriySalnikov/OBSNotifier/issues/5
--]]

obs = obslua

-- WinAPI
-- https://stackoverflow.com/a/61269226/8980874
local ffi = require("ffi")
local bit32 = require("bit")
local BELOW_NORMAL_PRIORITY_CLASS = 0x00004000
local NORMAL_PRIORITY_CLASS = 0x00000020
local CREATE_NEW_PROCESS_GROUP = 0x00000200

ffi.cdef [[ typedef struct _STARTUPINFOW { uint32_t cb; void * lpReserved; void * lpDesktop; void * lpTitle; uint32_t dwX; uint32_t dwY; uint32_t dwXSize; uint32_t dwYSize; uint32_t dwXCountChars; uint32_t dwYCountChars; uint32_t dwFillAttribute; uint32_t dwFlags; uint16_t wShowWindow; uint16_t cbReserved2; void * lpReserved2; void ** hStdInput; void ** hStdOutput; void ** hStdError; } STARTUPINFOW, *LPSTARTUPINFOW; typedef struct _PROCESS_INFORMATION { void ** hProcess; void ** hThread; uint32_t dwProcessId; uint32_t dwThreadId; } PROCESS_INFORMATION, *LPPROCESS_INFORMATION; uint32_t CreateProcessW( void *, const wchar_t * commandLine, void *, void *, uint32_t, uint32_t, void *, const wchar_t *, LPSTARTUPINFOW, LPPROCESS_INFORMATION ); uint32_t CloseHandle(void **);]]
ffi.cdef [[ int MultiByteToWideChar(unsigned int CodePage, unsigned int dwFlags, const char* lpMultiByteStr, int cbMultiByte, wchar_t* lpWideCharStr, int cchWideChar); ]]

local function execute(commandLine, priority)
	local si = ffi.new("STARTUPINFOW")
	si.cb = ffi.sizeof(si)
	local pi = ffi.new("PROCESS_INFORMATION")

	local widestr = ffi.new("wchar_t[?]", 1024);
	ffi.C.MultiByteToWideChar(65001, 0, commandLine, #commandLine, widestr, 1024)

	local ok = ffi.C.CreateProcessW(nil, widestr, nil, nil, 0, bit32.bor(priority, CREATE_NEW_PROCESS_GROUP), nil, nil, si, pi) ~= 0
	if ok then
		ffi.C.CloseHandle(pi.hProcess)
		ffi.C.CloseHandle(pi.hThread)
	end
	return ok -- true/false
end
-- ~WinAPI

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
	start_OBSNotifier(lowprio)
end

function start_OBSNotifier(lowprio)
	local prio = lowprio and BELOW_NORMAL_PRIORITY_CLASS or NORMAL_PRIORITY_CLASS
	local StartCommand = "\"&OBS_NOTIFIER_PATH&\" --force_close"
	obs.script_log(obs.LOG_INFO,
		"Executing command: " .. StartCommand .. ". With " .. (lowprio and "\"Below Normal\"" or "\"Normal\"") .. " priority.")

	local output = execute(StartCommand, prio)
	obs.script_log(obs.LOG_INFO, "Output: " .. tostring(output))
end
