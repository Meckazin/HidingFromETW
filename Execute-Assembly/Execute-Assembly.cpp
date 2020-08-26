#include <metahost.h>
#include <stdio.h>
#pragma comment(lib, "mscoree.lib")

int main()
{
	ICLRMetaHost* metaHost = NULL;
	IEnumUnknown* runtime = NULL;
	ICLRRuntimeInfo* runtimeInfo = NULL;
	ICLRRuntimeHost* runtimeHost = NULL;
	IUnknown* enumRuntime = NULL;
	LPWSTR frameworkName = NULL;
	DWORD bytes = 2048, result = 0;
	HRESULT hr;

	printf("CLR via native code.... @_xpn_\n\n");

	if (CLRCreateInstance(CLSID_CLRMetaHost, IID_ICLRMetaHost, (LPVOID*)&metaHost) != S_OK) {
		printf("[x] Error: CLRCreateInstance(..)\n");
		return 2;
	}

	if (metaHost->EnumerateInstalledRuntimes(&runtime) != S_OK) {
		printf("[x] Error: EnumerateInstalledRuntimes(..)\n");
		return 2;
	}

	frameworkName = (LPWSTR)LocalAlloc(LPTR, 2048);
	if (frameworkName == NULL) {
		printf("[x] Error: malloc could not allocate\n");
		return 2;
	}

	// Enumerate through runtimes and show supported frameworks
	while (runtime->Next(1, &enumRuntime, 0) == S_OK) {
		if (enumRuntime->QueryInterface<ICLRRuntimeInfo>(&runtimeInfo) == S_OK) {
			if (runtimeInfo != NULL) {
				runtimeInfo->GetVersionString(frameworkName, &bytes);
				wprintf(L"[*] Supported Framework: %s\n", frameworkName);
			}
		}
	}

	// For demo, we just use the last supported runtime
	if (runtimeInfo->GetInterface(CLSID_CLRRuntimeHost, IID_ICLRRuntimeHost, (LPVOID*)&runtimeHost) != S_OK) {
		printf("[x] ..GetInterface(CLSID_CLRRuntimeHost...) failed\n");
		return 2;
	}

	if (runtimeHost == NULL || bytes == 0) {
		wprintf(L"[*] Using runtime: %s\n", frameworkName);
	}

	// Start runtime, and load our assembly
	runtimeHost->Start();

	printf("[*] ======= Calling .NET Code =======\n\n");
	if (runtimeHost->ExecuteInDefaultAppDomain(
		L"Rubeus.dll",
		L"Rubeus.Program",
		L"Main",
		L"klist",
		&result
	) != S_OK) {
		printf("[x] Error: ExecuteInDefaultAppDomain(..) failed\n");
		return 2;
	}
	printf("[*] ======= Done =======\n");


	return 0;
}

