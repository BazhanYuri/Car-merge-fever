import android.content.Context;
import android.util.Log;
import com.google.android.gms.appset.AppSet;
import com.google.android.gms.appset.AppSetIdClient;
import com.google.android.gms.appset.AppSetIdInfo;
import com.google.android.gms.tasks.OnCompleteListener;
import com.google.android.gms.tasks.Task;
import com.unity3d.player.UnityPlayer;

public class AndroidAppSetIdUtility
{
    public interface AppSetIdCallback
    {
        public void OnAppSetIdRetrieved(boolean success,String appSetId,int scope);
    }

    public static void GetAppSetId(AppSetIdCallback callback)
    {
        Context context = UnityPlayer.currentActivity.getApplicationContext();
        AppSetIdClient client = AppSet.getClient(context);
        Task<AppSetIdInfo> task = client.getAppSetIdInfo();

        task.addOnCompleteListener(new OnCompleteListener<AppSetIdInfo>() {
            @Override
            public void onComplete(Task<AppSetIdInfo> task) {
                if(task.isSuccessful())
                {
                    AppSetIdInfo info = task.getResult();
                    // Determine current scope of app set ID.
                    int scope = info.getScope();

                    // Read app set ID value, which uses version 4 of the
                    // universally unique identifier (UUID) format.
                    String id = info.getId();
                    Log.d("AndroidAppSetIdUtility", id);
                    callback.OnAppSetIdRetrieved(true,id,scope);
                }
                else
                {
                    callback.OnAppSetIdRetrieved(false,"unknown",-1);
                }
            }
        });
    }
}