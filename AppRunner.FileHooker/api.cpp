#include "api.h"

std::wstring GetModuleFileNameString(HMODULE hModule) {

    auto requiredSize = GetModuleFileNameW(hModule, nullptr, 0);
    wchar_t *buffer = new wchar_t[requiredSize];
    GetModuleFileNameW(hModule, buffer, requiredSize);

    std::wstring result = std::wstring(buffer);
    delete[] buffer;

    return result;
}