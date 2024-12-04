#include <Windows.h>
#include <string>
#include "string_utils.h"

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

void MakeStringLower(std::wstring &str) {
    for (auto &c : str) {
        c = std::tolower(c);
    }
}

void MakeStandardPath(std::wstring &str) {
    if (str.starts_with(LR"(\\?\)")) {
        str = str.substr(4);
    } else if (str.starts_with(LR"(\??\)")) {
        str = str.substr(4);
    }
}