#include <Windows.h>
#include <string>
#include "fs_utils.h"

void MakeFileFullPath(std::wstring &str) {
    wchar_t buffer[300];

    auto size = GetFullPathNameW(str.c_str(), 300, buffer, nullptr);
    if (size <= 300) {
        str = std::wstring(buffer);
        return;
    }

    auto bufferOnHeap = new wchar_t[size];
    GetFullPathNameW(str.c_str(), size, bufferOnHeap, nullptr);
    str = std::wstring(bufferOnHeap);
    delete[] bufferOnHeap;
}

std::wstring GetFileNtPath(HANDLE fileHandle) { // ·µ»Ø\\?\XXX
    WCHAR buffer[300];
    DWORD len = GetFinalPathNameByHandleW(fileHandle, buffer, 300, FILE_NAME_NORMALIZED);
    if (len == 0) {
        return L"";
    } else if (len <= 300) {
        return std::wstring(buffer, len);
    } else {
        auto path = std::wstring(len, '\0');
        len = GetFinalPathNameByHandleW(fileHandle, (LPWSTR)path.c_str(), len, FILE_NAME_NORMALIZED);
        return path;
    }
}