#define _CRT_SECURE_NO_WARNINGS 0

#include "pch.h"
#include <string>
#include <map>
#include <cstdlib>
#include <iostream>
#include <sstream>
#include <Windows.h>
#include <MinHook.h>
#include "api.h"

typedef HANDLE(*FuncCreateFileA)(LPCSTR lpFileName, DWORD dwDesiredAccess, DWORD dwShareMode, LPSECURITY_ATTRIBUTES lpSecurityAttributes, DWORD dwCreationDisposition, DWORD dwFlagsAndAttributes, HANDLE hTemplateFile);
typedef HANDLE(*FuncCreateFileW)(LPCWSTR lpFileName, DWORD dwDesiredAccess, DWORD dwShareMode, LPSECURITY_ATTRIBUTES lpSecurityAttributes, DWORD dwCreationDisposition, DWORD dwFlagsAndAttributes, HANDLE hTemplateFile);
typedef HFILE(*FuncOpenFile)(LPCSTR lpFileName, LPOFSTRUCT lpReOpenBuff, UINT uStyle);

std::map<std::wstring, std::wstring> appRunnerFileMaps;
FuncCreateFileA funcCreateFileA;
FuncCreateFileW funcCreateFileW;
FuncOpenFile funcOpenFile;

std::wstring AnsiToUnicode(const std::string &str) {
    int len = MultiByteToWideChar(CP_ACP, 0, str.c_str(), -1, NULL, 0);
    wchar_t *strUnicode = new wchar_t[len];
    wmemset(strUnicode, 0, len);
    MultiByteToWideChar(CP_ACP, 0, str.c_str(), -1, strUnicode, len);

    std::wstring strTemp(strUnicode);
    delete[]strUnicode;
    strUnicode = NULL;
    return strTemp;
}

std::string UnicodeToAnsi(const std::wstring &str) {

    int len = WideCharToMultiByte(CP_ACP, 0, str.c_str(), -1, NULL, 0, NULL, NULL);
    char *strAnsi = new char[len];
    memset(strAnsi, 0, len);
    WideCharToMultiByte(CP_ACP, 0, str.c_str(), -1, strAnsi, len, NULL, NULL);

    std::string strTemp(strAnsi);
    delete[]strAnsi;
    strAnsi = NULL;
    return strTemp;
}

void MakeFileFullPath(std::wstring &str) {
    wchar_t *buffer = (wchar_t *)malloc(600);

    auto size = GetFullPathNameW(str.c_str(), 300, buffer, nullptr);
    if (size > 300) {
        free(buffer);
        buffer = (wchar_t *)malloc(size * 2);
        GetFullPathNameW(str.c_str(), size, buffer, nullptr);
    }

    str = std::wstring(buffer);
    free(buffer);
}

void MakeStringLower(std::string &str) {
    for (size_t i = 0; i < str.length(); i++) {
        str[i] = std::tolower(str[i]);
    }
}

void MakeStringLower(std::wstring &str) {
    for (size_t i = 0; i < str.length(); i++) {
        str[i] = std::tolower(str[i]);
    }
}

bool GetFileMapTarget(const std::wstring &from, std::wstring &target) {
    auto longPathPrefix = std::wstring(LR"(\\?\)");
    auto key = from;
    if (key.rfind(longPathPrefix, 0) == 0) {
        key = key.substr(4, key.length() - 4);
    }

    auto iterator = appRunnerFileMaps.find(key);
    if (iterator != appRunnerFileMaps.end()) {
        target = iterator->second;
        return true;
    }

    return false;
}

HANDLE HookCreateFileA(LPCSTR lpFileName, DWORD dwDesiredAccess, DWORD dwShareMode, LPSECURITY_ATTRIBUTES lpSecurityAttributes, DWORD dwCreationDisposition, DWORD dwFlagsAndAttributes, HANDLE hTemplateFile) {
    auto fileName = AnsiToUnicode(std::string(lpFileName));
    MakeFileFullPath(fileName);
    MakeStringLower(fileName);

    std::wstring mapTarget;
    if (GetFileMapTarget(fileName, mapTarget)) {
        fileName = mapTarget;
    }

    return funcCreateFileA(UnicodeToAnsi(fileName).c_str(), dwDesiredAccess, dwShareMode, lpSecurityAttributes, dwCreationDisposition, dwFlagsAndAttributes, hTemplateFile);
}

HANDLE HookCreateFileW(LPCWSTR lpFileName, DWORD dwDesiredAccess, DWORD dwShareMode, LPSECURITY_ATTRIBUTES lpSecurityAttributes, DWORD dwCreationDisposition, DWORD dwFlagsAndAttributes, HANDLE hTemplateFile) {
    auto fileName = std::wstring(lpFileName);
    MakeFileFullPath(fileName);
    MakeStringLower(fileName);

    std::wstring mapTarget;
    if (GetFileMapTarget(fileName, mapTarget)) {
        fileName = mapTarget;
    }

    return funcCreateFileW(fileName.c_str(), dwDesiredAccess, dwShareMode, lpSecurityAttributes, dwCreationDisposition, dwFlagsAndAttributes, hTemplateFile);
}

HFILE HookOpenFile(LPCSTR lpFileName, LPOFSTRUCT lpReOpenBuff, UINT uStyle) {
    MessageBoxW(nullptr, L"Opening file", nullptr, 0);

    return 0;
}

bool InjectSelf(HMODULE hModule, HANDLE hProcess) {
    auto selfName = GetModuleFileNameString(hModule);

    auto remoteAddress = VirtualAllocEx(hProcess, nullptr, selfName.size(), MEM_COMMIT, PAGE_READWRITE);
    if (remoteAddress == nullptr) {
        return false;
    }

    SIZE_T bytesWriten;
    auto writeProcessMemoryResult = WriteProcessMemory(hProcess, remoteAddress, selfName.c_str(), selfName.size(), &bytesWriten);
    if (!writeProcessMemoryResult ||
        bytesWriten == 0) {
        return false;
    }

    auto procAddress = (LPTHREAD_START_ROUTINE)&LoadLibraryW;
    DWORD threadId;
    auto remoteThread = CreateRemoteThread(
        hProcess,
        0,
        0,
        procAddress,
        remoteAddress,
        0,
        &threadId);

    if (remoteThread == nullptr) {
        VirtualFreeEx(hProcess, remoteAddress, selfName.size(), MEM_RELEASE);
        return false;
    }

    WaitForSingleObject(remoteThread, 100);
    CloseHandle(remoteThread);
    VirtualFreeEx(hProcess, remoteAddress, selfName.size(), MEM_RELEASE);

    return true;
}

void InitializeHooks(HMODULE hModule) {
#if _DEBUG
    MessageBoxW(nullptr, L"Initailizing FileHooker", nullptr, 0);
#endif
    auto appRunnerFileMapsPtr = getenv("SlimeNull.AppRunner.FileMaps");

#if _DEBUG
    if (appRunnerFileMapsPtr == nullptr) {
        appRunnerFileMapsPtr = R"(c:\users\slimenull\desktop\abc.txt;E:\Workspace\Temp\abaaba.txt)";
    }
#endif


    if (appRunnerFileMapsPtr == nullptr) {
        return;
    }

    auto appRunnerFileMapsString = std::string(appRunnerFileMapsPtr);

    auto stringStream = std::istringstream(appRunnerFileMapsString);
    auto token = std::string();
    auto key = std::string();

    while (std::getline(stringStream, token, ';')) {
        if (key.length() == 0) {
            MakeStringLower(token);
            char fullFileNameBuffer[600];
            auto getFullPathNameResult = GetFullPathNameA(token.c_str(), 600, fullFileNameBuffer, nullptr);

            if (getFullPathNameResult != 0) {
                token = std::string(fullFileNameBuffer);
            }

            key = token;
        } else {
            auto wKey = AnsiToUnicode(key);
            auto wValue = AnsiToUnicode(token);
            appRunnerFileMaps[wKey] = wValue;
            key = std::string();
        }
    }

    auto moduleKernel32 = GetModuleHandleA("Kernel32");

    if (moduleKernel32 == nullptr) {
        return;
    }

    auto procCreateFileA = GetProcAddress(moduleKernel32, "CreateFileA");
    auto procCreateFileW = GetProcAddress(moduleKernel32, "CreateFileW");
    auto procOpenFile = GetProcAddress(moduleKernel32, "OpenFile");

    MH_Initialize();

    if (procCreateFileA != nullptr) {
        MH_CreateHook(procCreateFileA, &HookCreateFileA, (void **)&funcCreateFileA);
        MH_EnableHook(procCreateFileA);
    }

    if (procCreateFileW != nullptr) {
        MH_CreateHook(procCreateFileW, &HookCreateFileW, (void **)&funcCreateFileW);
        MH_EnableHook(procCreateFileW);
    }

    if (procOpenFile != nullptr) {
        MH_CreateHook(procOpenFile, &HookOpenFile, (void **)&funcOpenFile);
        MH_EnableHook(procOpenFile);
    }
}

BOOL APIENTRY DllMain(
    HMODULE hModule,
    DWORD  ul_reason_for_call,
    LPVOID lpReserved
) {
    switch (ul_reason_for_call) {
        case DLL_PROCESS_ATTACH:
            InitializeHooks();
            break;
        case DLL_THREAD_ATTACH:
            break;
        case DLL_THREAD_DETACH:
            break;
        case DLL_PROCESS_DETACH:
            break;
    }
    return TRUE;
}

