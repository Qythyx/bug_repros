/*
 * As per https://stackoverflow.com/questions/64749385/predefined-type-system-runtime-compilerservices-isexternalinit-is-not-defined
 * and https://developercommunity.visualstudio.com/content/problem/1244809/error-cs0518-predefined-type-systemruntimecompiler.html.
 * Having this file fixes this error:
 * Error CS0518: Predefined type 'System.Runtime.CompilerServices.IsExternalInit' is not defined or imported
 */
namespace System.Runtime.CompilerServices
{
	internal static class IsExternalInit { }
}
