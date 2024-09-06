using System.Runtime.InteropServices;
using System.Security;

[assembly: ComVisible(false)]
[assembly: Guid("dc2bb779-4f63-4d14-8db7-7865586c694a")]

// Need to allow partially trusted callers to get SSRS to call our builder
//	from the header/footer...
[assembly: AllowPartiallyTrustedCallers]
