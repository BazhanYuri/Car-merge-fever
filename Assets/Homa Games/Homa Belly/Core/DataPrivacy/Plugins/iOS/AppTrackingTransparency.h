#ifdef __cplusplus
extern "C" {
#endif

typedef void (*AppTrackingTransparencyCallback)(int result);
void requestTrackingAuthorization(AppTrackingTransparencyCallback callback);
void openPrivacySettings(AppTrackingTransparencyCallback callback);
char* identifierForAdvertising();
int trackingAuthorizationStatus();
#ifdef __cplusplus
}
#endif
