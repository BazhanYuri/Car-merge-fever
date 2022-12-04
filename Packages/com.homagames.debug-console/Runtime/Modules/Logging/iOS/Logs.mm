extern "C" void _Homa_DebugConsole_CopyText( const char* text ) 
{
	[UIPasteboard generalPasteboard].string = [NSString stringWithUTF8String:text];
}