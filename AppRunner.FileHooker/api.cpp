#include "api.h"

std::wstring GetModuleFileNameString(HMODULE hModule) {
    wchar_t buffer[260];
    auto requiredSize = GetModuleFileNameW(hModule, buffer, 260);
    if (requiredSize < 260) {
        return std::wstring(buffer);
    }

    wchar_t *bufferOnHeap = new wchar_t[requiredSize];
    GetModuleFileNameW(hModule, bufferOnHeap, requiredSize);

    std::wstring result = std::wstring(bufferOnHeap);
    delete[] bufferOnHeap;

    return result;
}