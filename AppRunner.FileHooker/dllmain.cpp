#define _CRT_SECURE_NO_WARNINGS 0

#include "pch.h"
#include <string>
#include <map>
#include <cstdlib>
#include <iostream>
#include <sstream>
#include <Windows.h>
#include <MinHook.h>


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

HANDLE HookCreateFileA(LPCSTR lpFileName, DWORD dwDesiredAccess, DWORD dwShareMode, LPSECURITY_ATTRIBUTES lpSecurityAttributes, DWORD dwCreationDisposition, DWORD dwFlagsAndAttributes, HANDLE hTemplateFile) {
    char buffer[600]{ 0 };
    auto getFullPathNameResult = GetFullPathNameA(lpFileName, 600, buffer, nullptr);
    if (getFullPathNameResult != 0) {
        lpFileName = buffer;
    }

    auto fileName = AnsiToUnicode(std::string(lpFileName));
    MakeStringLower(fileName);

    auto valueIterator = appRunnerFileMaps.find(fileName);
    if (valueIterator != appRunnerFileMaps.end()) {
#if _DEBUG
        auto msg = std::wstring(L"Creating file;");
        msg += fileName;
        msg += L";";
        msg += valueIterator->second;
        MessageBoxW(nullptr, msg.c_str(), nullptr, 0);
#endif
        fileName = valueIterator->second;
    }

    return funcCreateFileA(UnicodeToAnsi(fileName).c_str(), dwDesiredAccess, dwShareMode, lpSecurityAttributes, dwCreationDisposition, dwFlagsAndAttributes, hTemplateFile);
}

HANDLE HookCreateFileW(LPCWSTR lpFileName, DWORD dwDesiredAccess, DWORD dwShareMode, LPSECURITY_ATTRIBUTES lpSecurityAttributes, DWORD dwCreationDisposition, DWORD dwFlagsAndAttributes, HANDLE hTemplateFile) {
    wchar_t buffer[300]{ 0 };
    auto getFullPathNameResult = GetFullPathNameW(lpFileName, 300, buffer, nullptr);
    if (getFullPathNameResult != 0) {
        lpFileName = buffer;
    }

    auto fileName = std::wstring(lpFileName);
    MakeStringLower(fileName);

    auto valueIterator = appRunnerFileMaps.find(fileName);
    if (valueIterator != appRunnerFileMaps.end()) {
#if _DEBUG
        auto msg = std::wstring(L"Creating file;");
        msg += fileName;
        msg += L";";
        msg += valueIterator->second;
        MessageBoxW(nullptr, msg.c_str(), nullptr, 0);
#endif
        fileName = valueIterator->second;
    }

    return funcCreateFileW(fileName.c_str(), dwDesiredAccess, dwShareMode, lpSecurityAttributes, dwCreationDisposition, dwFlagsAndAttributes, hTemplateFile);
}

HFILE HookOpenFile(LPCSTR lpFileName, LPOFSTRUCT lpReOpenBuff, UINT uStyle) {
    MessageBoxW(nullptr, L"Opening file", nullptr, 0);

    return 0;
}

void InitializeHooks() {
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
        auto ret = MH_CreateHook(procCreateFileA, &HookCreateFileA, (void **)&funcCreateFileA);
        if (ret != 0) {
            MessageBoxW(nullptr, L"CreateFileA Hook Not OK", nullptr, 0);
            return;
        }

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

