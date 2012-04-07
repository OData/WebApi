// Copyright (c) Microsoft Corporation. All rights reserved. See License.txt in the project root for license information.

using System.Security;

//// REVIEW: RonCain -- This version is used by the WebStackRuntime assemblies that use types
//// from System.ComponentModelDataAnnotations which is not [SecurityTransparent]
//// in .Net 4.0.   Attempting to make the WebStackRuntime assemblies be
//// [SecurityTransparent] results in security exceptions on any type reference
//// to DataAnnotations.
//// Search for [SecuritySafeCritical] in WebStackRuntime
//// assemblies to find the places that rely on this use of [Aptca]

[assembly: AllowPartiallyTrustedCallers]
