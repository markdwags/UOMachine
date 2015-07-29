// Loader.cpp : Defines the exported functions for the DLL application.
//

#include "stdafx.h"

enum LOADER_ERROR {
	SUCCESS = 0,
	NO_OPEN_EXE,
	NO_MAP_EXE,
	NO_READ_EXE_DATA,
	NO_RUN_EXE,
	NO_ALLOC_MEN,
	NO_WRITE,
	NO_VPROTECT,
	NO_READ,
	UNKNOWN_ERROR = 99
};

LOADER_ERROR __declspec(dllexport) __cdecl Load(char *client, char *clientPath, char *dll, char *dllPath, char *dllFunction, char* params, int *pid) {
	PVOID dataAlloc;
	CHAR codeBuffer[256];
	DWORD codePosition = 0;
	STARTUPINFOA si;
	PROCESS_INFORMATION pi;

	// Set enviroment variable for QT to find qwindows.dll or will fail to launch.
	CHAR qtPlatformPath[128];
	memset(qtPlatformPath, 0, 128);
	strcat_s(qtPlatformPath, dllPath);
	strcat_s(qtPlatformPath, "\\Platforms\\");
	qtPlatformPath[strlen(qtPlatformPath)] = 0;
	SetEnvironmentVariableA("QT_QPA_PLATFORM_PLUGIN_PATH", qtPlatformPath);

	// Add UOS directory to PATH
	memset(qtPlatformPath, 0, 128);
	strcat_s(qtPlatformPath, dllPath);
	SetEnvironmentVariableA("PATH", qtPlatformPath);

	memset(codeBuffer, 0, 256);
	memset(&si, 0, sizeof(STARTUPINFOA));
	memset(&pi, 0, sizeof(PROCESS_INFORMATION));

	if (!CreateProcessA(client, params, 0, 0, 0, CREATE_SUSPENDED, 0, clientPath, &si, &pi)) {
		int error = GetLastError();
		return NO_RUN_EXE;
	}

	CONTEXT context;
	memset(&context, 0, sizeof(CONTEXT));
	context.ContextFlags = 0x10000 | 0x01 | 0x02 | 0x04;
	GetThreadContext(pi.hThread, &context);

	DWORD eip = context.Eip;

	*pid = pi.dwProcessId;

	dataAlloc = VirtualAllocEx(pi.hProcess, 0, 256, MEM_COMMIT | MEM_RESERVE, PAGE_EXECUTE_READWRITE);
	if (dataAlloc == 0) {
		return NO_ALLOC_MEN;
	}

	// write DLL name to buffer
	PCHAR dllPos = (char*)dataAlloc + codePosition;
	memcpy(codeBuffer, dll, strlen(dll));
	codePosition += (strlen(dll) + 1);

	// write DLL function name to buffer
	PCHAR dllFuncPos = (char*)dataAlloc + codePosition;
	memcpy(codeBuffer + codePosition, dllFunction, strlen(dllFunction));
	codePosition += (strlen(dllFunction) + 1);

	PCHAR ptr = codeBuffer + codePosition;
	memset(ptr, 0x90, sizeof(codeBuffer)-codePosition);

	DWORD codeStart = codePosition + 5;

	HMODULE hModule = LoadLibraryA("kernel32.dll");
	if (hModule == NULL)
		return NO_WRITE;
	LPVOID loadLibrary = GetProcAddress(hModule, "LoadLibraryA");
	LPVOID getProcAddress = GetProcAddress(hModule, "GetProcAddress");

	/* Inject code into client, LoadLibrary's UOS.dll and then calls Install function then JMP's to original EIP */
	unsigned char code[] = {
		0x9C						 /* 00:00 PUSHFD */,
		0x60						 /* 01:01 PUSHAD */,
		0x68, 0x00, 0x00, 0x00, 0x00 /* 2:6 PUSH dllName */,
		0xB8, 0x00, 0x00, 0x00, 0x00 /* 7:11 MOV eax, LoadLibraryA */,
		0xFF, 0xD0					 /* 12:13 CALL eax */,
		0xBB, 0x00, 0x00, 0x00, 0x00 /* 14:18 MOV ebx, dllFunc */,
		0x53						 /* 19:19 PUSH ebx */,
		0x50						 /* 20:20 PUSH eax */,
		0xB9, 0x00, 0x00, 0x00, 0x00 /* 21:25 MOV ecx, GetProcAddress */,
		0xFF, 0xD1					 /* 26:27 CALL ecx */,
		0xFF, 0xD0					 /* 28:29 CALL eax */,
		0x61						 /* 30:30 POPAD */,
		0x9D						 /* 31:31 POPFD */,
		0xE9, 0x00, 0x00, 0x00, 0x00 /* 32:36 JMP origEip */ };

	code[3] = (CHAR)(dllPos);
	code[4] = (CHAR)((DWORD)dllPos >> 8);
	code[5] = (CHAR)((DWORD)dllPos >> 16);
	code[6] = (CHAR)((DWORD)dllPos >> 24);

	code[8] = (CHAR)(loadLibrary);
	code[9] = (CHAR)((DWORD)loadLibrary >> 8);
	code[10] = (CHAR)((DWORD)loadLibrary >> 16);
	code[11] = (CHAR)((DWORD)loadLibrary >> 24);

	code[15] = (CHAR)(dllFuncPos);
	code[16] = (CHAR)((DWORD)dllFuncPos >> 8);
	code[17] = (CHAR)((DWORD)dllFuncPos >> 16);
	code[18] = (CHAR)((DWORD)dllFuncPos >> 24);

	code[22] = (CHAR)(getProcAddress);
	code[23] = (CHAR)((DWORD)getProcAddress >> 8);
	code[24] = (CHAR)((DWORD)getProcAddress >> 16);
	code[25] = (CHAR)((DWORD)getProcAddress >> 24);

	char *eipPtr = (char*)((char*)eip - ((char*)dataAlloc + codeStart + sizeof(code)));

	code[33] = (CHAR)(eipPtr);
	code[34] = (CHAR)((DWORD)eipPtr >> 8);
	code[35] = (CHAR)((DWORD)eipPtr >> 16);
	code[36] = (CHAR)((DWORD)eipPtr >> 24);

	memcpy(codeBuffer + codeStart, code, sizeof(code));
	SIZE_T out = 0;
	WriteProcessMemory(pi.hProcess, dataAlloc, codeBuffer, 256, &out);

	context.Eip = ((DWORD)dataAlloc + codeStart);
	SetThreadContext(pi.hThread, &context);

	ResumeThread(pi.hThread);

	CloseHandle(pi.hProcess);
	CloseHandle(pi.hThread);

	return SUCCESS;
}