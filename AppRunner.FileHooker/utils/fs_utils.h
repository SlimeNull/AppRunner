#pragma once
#include <Windows.h>
#include <string>

void MakeFileFullPath(std::wstring &str);
std::wstring GetFileNtPath(HANDLE fileHandle);