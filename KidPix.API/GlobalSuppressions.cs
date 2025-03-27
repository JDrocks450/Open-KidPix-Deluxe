// This file is used by Code Analysis to maintain SuppressMessage
// attributes that are applied to this project.
// Project-level suppressions either have no target or are given
// a specific target and scoped to a namespace, type, member, etc.

using System.Diagnostics.CodeAnalysis;

//This application is currently only available on Windows 10 or 11.
//(BMP operations are Windows only as of now)
[assembly: SuppressMessage("Interoperability", "CA1416:Validate platform compatibility", Justification = "<Pending>", Scope = "member", Target = "~M:KidPix.API.Importer.Graphics.Brushes.BMPBrush.Plaster(KidPix.API.Importer.Graphics.BMPHeader,System.Byte[],System.Boolean)~System.Drawing.Bitmap")]
