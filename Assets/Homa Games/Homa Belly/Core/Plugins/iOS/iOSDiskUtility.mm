extern "C" {
    int getAvailableBytes();
}

int getAvailableBytes() {
    NSArray *paths = NSSearchPathForDirectoriesInDomains(NSLibraryDirectory, NSUserDomainMask, YES);
    NSDictionary *dict = [[NSFileManager defaultManager] attributesOfFileSystemForPath:[paths lastObject] error:nil];

    if (dict) {
        return (int)[[dict objectForKey: NSFileSystemFreeSize] floatValue];
    }

    return 0;
}