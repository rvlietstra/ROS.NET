#ifndef __Callers_h__
#define __Callers_h__

#include "XmlRpcServerMethodWrapper.h"
#include "XmlRpc.h"		// needed for XMLRPC_API
using namespace XmlRpc;
#ifdef __cplusplus
extern "C" {
#endif
	//bullshit sanity check
	typedef void (*FuncPtr)(int i);	
	extern XMLRPC_API int IntegerEcho(int val);
	extern XMLRPC_API void IntegerEchoFunctionPtr(FuncPtr fptr);	
	extern XMLRPC_API bool IntegerEchoRepeat(int val);
	
	extern XMLRPC_API void StringPassingTest(const char *str);
	//general	
	typedef void (*EricRulz)(const char *c);
	extern XMLRPC_API void SetStringOutFunc(EricRulz fptr);	
	//XmlRpcClient
	extern XMLRPC_API XmlRpcClient* XmlRpcClient_Create(const char *host, int port, const char *uri);
	extern XMLRPC_API void XmlRpcClient_Close(XmlRpcClient* instance);
	extern XMLRPC_API bool XmlRpcClient_Execute(XmlRpcClient* instance, const char* method, XmlRpcValue const& parameterss, XmlRpcValue& result);
	extern XMLRPC_API bool XmlRpcClient_ExecuteNonBlock(XmlRpcClient* instance, const char* method, XmlRpcValue const& parameterss);
	extern XMLRPC_API bool XmlRpcClient_ExecuteCheckDone(XmlRpcClient* instance, XmlRpcValue& result);
	extern XMLRPC_API unsigned XmlRpcClient_HandleEvent(XmlRpcClient* instance, unsigned eventType);
	extern XMLRPC_API bool XmlRpcClient_IsFault(XmlRpcClient* instance);
	extern XMLRPC_API const char* XmlRpcClient_GetHost(XmlRpcClient* instance);
	extern XMLRPC_API const char* XmlRpcClient_GetUri(XmlRpcClient* instance);
	extern XMLRPC_API int XmlRpcClient_GetPort(XmlRpcClient* instance);
	extern XMLRPC_API const char* XmlRpcClient_GetRequest(XmlRpcClient* instance);
	extern XMLRPC_API const char* XmlRpcClient_GetHeader(XmlRpcClient* instance);
	extern XMLRPC_API const char* XmlRpcClient_GetResponse(XmlRpcClient* instance);
	extern XMLRPC_API int XmlRpcClient_GetSendAttempts(XmlRpcClient* instance);
	extern XMLRPC_API int XmlRpcClient_GetBytesWritten(XmlRpcClient* instance);
	extern XMLRPC_API bool XmlRpcClient_GetExecuting(XmlRpcClient* instance);
	extern XMLRPC_API bool XmlRpcClient_GetEOF(XmlRpcClient* instance, XmlRpcValue& result);
	extern XMLRPC_API int XmlRpcClient_GetContentLength(XmlRpcClient* instance);
	extern XMLRPC_API XmlRpcDispatch* XmlRpcClient_GetXmlRpcDispatch(XmlRpcClient* instance);

	//XmlRpcValue	
	extern XMLRPC_API XmlRpcValue *XmlRpcValue_Create1();
	extern XMLRPC_API XmlRpcValue *XmlRpcValue_Create2(bool value);
	extern XMLRPC_API XmlRpcValue *XmlRpcValue_Create3(int value);
	extern XMLRPC_API XmlRpcValue *XmlRpcValue_Create4(double value);
	extern XMLRPC_API XmlRpcValue *XmlRpcValue_Create5(const char* value);
	extern XMLRPC_API XmlRpcValue *XmlRpcValue_Create6(void* value, int nBytes);
	extern XMLRPC_API XmlRpcValue *XmlRpcValue_Create7(const char* xml, int offset);
	extern XMLRPC_API XmlRpcValue *XmlRpcValue_Create8(XmlRpcValue *rhs);
	extern XMLRPC_API void XmlRpcValue_Clear(XmlRpcValue* instance);
	extern XMLRPC_API bool XmlRpcValue_Valid(XmlRpcValue* instance);
	extern XMLRPC_API int XmlRpcValue_Type(XmlRpcValue* instance);
	extern XMLRPC_API int XmlRpcValue_Size(XmlRpcValue* instance);	
	extern XMLRPC_API void XmlRpcValue_SetSize(XmlRpcValue* instance, int size);
	extern XMLRPC_API bool XmlRpcValue_HasMember(XmlRpcValue* instance, const char* name);
	extern XMLRPC_API XmlRpcValue* XmlRpcValue_Get1(XmlRpcValue* instance, int key);
	extern XMLRPC_API XmlRpcValue* XmlRpcValue_Get2(XmlRpcValue* instance, const char* key);
	extern XMLRPC_API void XmlRpcValue_Set1(XmlRpcValue* instance, int key, const char *value);
	extern XMLRPC_API void XmlRpcValue_Set2(XmlRpcValue* instance, const char* key, const char *value);
	extern XMLRPC_API void XmlRpcValue_Set3(XmlRpcValue* instance, int key, XmlRpcValue *value);
	extern XMLRPC_API void XmlRpcValue_Set4(XmlRpcValue* instance, const char* key, XmlRpcValue *value);
	extern XMLRPC_API void XmlRpcValue_Set5(XmlRpcValue* instance, int key, int *value);
	extern XMLRPC_API void XmlRpcValue_Set6(XmlRpcValue* instance, const char* key, int *value);
	extern XMLRPC_API void XmlRpcValue_Set7(XmlRpcValue* instance, int key, bool *value);
	extern XMLRPC_API void XmlRpcValue_Set8(XmlRpcValue* instance, const char* key, bool *value);
	extern XMLRPC_API void XmlRpcValue_Set9(XmlRpcValue* instance, int key, double *value);
	extern XMLRPC_API void XmlRpcValue_Set10(XmlRpcValue* instance, const char* key, double *value);
	
	//XmlRpcDispatch
	extern XMLRPC_API XmlRpcDispatch *XmlRpcDispatch_Create();
	extern XMLRPC_API void XmlRpcDispatch_Close(XmlRpcDispatch *instance);
	extern XMLRPC_API void XmlRPcDispatch_AddSource(XmlRpcDispatch* instance, XmlRpcSource *source, unsigned eventMask);
	extern XMLRPC_API void XmlRpcDispatch_RemoveSource(XmlRpcDispatch* instance, XmlRpcSource *source);
	extern XMLRPC_API void XmlRpcDispatch_SetSourceEvents(XmlRpcDispatch* instance, XmlRpcSource *source, unsigned eventMask);
	extern XMLRPC_API void XmlRpcDispatch_Work(XmlRpcDispatch *instance, double msTime);
	extern XMLRPC_API void XmlRpcDispatch_Exit(XmlRpcDispatch *instance);
	extern XMLRPC_API void XmlRPcDispatch_Clear(XmlRpcDispatch *instance);

	//XmlRpcSource
	extern XMLRPC_API XmlRpcSource *XmlRpcSource_Create(int fd, bool deleteOnClose);
	extern XMLRPC_API void XmlRpcSource_Close(XmlRpcSource *instance);
	extern XMLRPC_API int XmlRpcSource_GetFd(XmlRpcSource *instance);
	extern XMLRPC_API void XmlRpcSource_SetFd(XmlRpcSource *instance, int fd);
	extern XMLRPC_API bool XmlRpcSource_GetKeepOpen(XmlRpcSource *instance);
	extern XMLRPC_API void XmlRpcSource_SetKeepOpen(XmlRpcSource *instance, bool b);
	extern XMLRPC_API unsigned handleEvent(unsigned eventType);

	//XmlRpcServerMethod
	extern XMLRPC_API XmlRpcServerMethodWrapper *XmlRpcServerMethod_Create(char *name, XmlRpcServer *server);
	extern XMLRPC_API void XmlRpcServerMethod_SetFunc(XmlRpcServerMethodWrapper *instance, XmlRpcServerFUNC func);
	extern XMLRPC_API void XmlRpcServerMethod_Execute(XmlRpcServerMethodWrapper *instance, XmlRpcValue *parms, XmlRpcValue *res);
	
	//XmlRpcServer	
	extern XMLRPC_API XmlRpcServer *XmlRpcServer_Create();
	extern XMLRPC_API void XmlRpcServer_AddMethod(XmlRpcServer *instance, XmlRpcServerMethod *method);
	extern XMLRPC_API void XmlRpcServer_RemoveMethod(XmlRpcServer *instance, XmlRpcServerMethod *method);
	extern XMLRPC_API void XmlRpcServer_RemoveMethodByName(XmlRpcServer *instance, char *name);
	extern XMLRPC_API XmlRpcServerMethod *XmlRpcServer_FindMethod(XmlRpcServer *instance, char *name);
	extern XMLRPC_API bool XmlRpcServer_BindAndListen(XmlRpcServer *instance, int port, int backlog);
	extern XMLRPC_API void XmlRpcServer_Work(XmlRpcServer *instance,double msTime);
	extern XMLRPC_API void XmlRpcServer_Exit(XmlRpcServer *instance);
	extern XMLRPC_API void XmlRpcServer_Shutdown(XmlRpcServer *instance);
	extern XMLRPC_API int XmlRpcServer_GetPort(XmlRpcServer *instance);
	extern XMLRPC_API XmlRpcDispatch *XmlRpcServer_GetDispatch(XmlRpcServer *instance);
#ifdef __cplusplus
}
#endif

#endif