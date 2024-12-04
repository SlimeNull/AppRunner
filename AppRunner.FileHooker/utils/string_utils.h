#pragma once
#include <string>

std::wstring AnsiToUnicode(const std::string &str);
std::string UnicodeToAnsi(const std::wstring &str);

void MakeStringLower(std::wstring &str);
void MakeStandardPath(std::wstring &str);