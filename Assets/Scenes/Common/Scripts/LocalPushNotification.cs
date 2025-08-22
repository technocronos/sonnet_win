#if UNITY_ANDROID
using Unity.Notifications.Android;
#endif
#if UNITY_IOS
using Unity.Notifications.iOS;
#endif
using System;


// ローカルプッシュ通知送信クラス
public static class LocalPushNotification
{

    // Androidで使用するプッシュ通知用のチャンネルを登録する。    
    public static void RegisterChannel(string cannelId, string title, string description)
    {
#if UNITY_ANDROID
        // チャンネルの登録
        var channel = new AndroidNotificationChannel()
        {
            Id = cannelId,
            Name = title,
            Importance = Importance.High,//ドキュメント　重要度を設定するを参照　https://developer.android.com/training/notify-user/channels?hl=ja
            Description = description,
        };
        AndroidNotificationCenter.RegisterNotificationChannel(channel);
#endif
    }

    /// 通知をすべてクリアーします。   
    public static void AllClear()
    {
#if UNITY_ANDROID
        // Androidの通知をすべて削除します。
        AndroidNotificationCenter.CancelAllScheduledNotifications();
        AndroidNotificationCenter.CancelAllNotifications();
#endif
#if UNITY_IOS
        // iOSの通知をすべて削除します。
        iOSNotificationCenter.RemoveAllScheduledNotifications();
        iOSNotificationCenter.RemoveAllDeliveredNotifications();
        // バッジを消します。
        iOSNotificationCenter.ApplicationBadge = 0;
#endif
    }

    /// 通知をキャンセルします。   
    public static void Cancel(int identifier)
    {
#if UNITY_ANDROID
        AndroidNotificationCenter.CancelNotification(identifier);
#endif
#if UNITY_IOS
        iOSNotificationCenter.RemoveScheduledNotification($"_notification_{identifier}");
#endif
    }

    // プッシュ通知を登録します。    
    public static int AddSchedule(string title, string message, int badgeCount, int elapsedTime, string cannelId, int identifier = 0)
    {
#if UNITY_ANDROID
        return SetAndroidNotification(title, message, badgeCount, elapsedTime, cannelId, identifier);
#endif
#if UNITY_IOS
        return SetIOSNotification(title, message, badgeCount, elapsedTime, identifier);
#else
        return 0;
#endif
    }



#if UNITY_IOS
    // 通知を登録(iOS)
    static private int SetIOSNotification(string title, string message, int badgeCount, int elapsedTime, int identifier)
    {
        // 通知を作成
        iOSNotificationCenter.ScheduleNotification(new iOSNotification()
        {
            //プッシュ通知を個別に取り消しなどをする場合はこのIdentifierを使用します。(未検証)
            Identifier = $"_notification_{identifier}",
            Title = title,
            Body = message,
            ShowInForeground = false,
            Badge = badgeCount,
            Trigger = new iOSNotificationTimeIntervalTrigger()
            {
                TimeInterval = new TimeSpan(0, 0, elapsedTime),
                Repeats = false
            }
        });

        return identifier;
    }
#endif



#if UNITY_ANDROID

    // 通知を登録(Android)   
    static private int SetAndroidNotification(string title, string message, int badgeCount, int elapsedTime, string cannelId, int identifier)
    {
        // 通知を作成します。
        var notification = new AndroidNotification
        {
            Title = title,
            Text = message,
            Number = badgeCount,

            //Androidのアイコンを設定
            SmallIcon = "ic_stat_notify_small",//どの画像を使用するかアイコンのIdentifierを指定　指定したIdentifierが見つからない場合アプリアイコンになる。
            LargeIcon = "ic_stat_notify_large",//どの画像を使用するかアイコンのIdentifierを指定　指定したIdentifierが見つからない場合アプリアイコンになる。
            FireTime = DateTime.Now.AddSeconds(elapsedTime)
        };

        // 通知を送信します。
        //AndroidNotificationCenter.SendNotification(notification, cannelId);

        if (AndroidNotificationCenter.CheckScheduledNotificationStatus(identifier) == NotificationStatus.Scheduled)
        {
            // Replace the currently scheduled notification with a new notification.
            AndroidNotificationCenter.UpdateScheduledNotification(identifier, notification, cannelId);
        }
        else if (AndroidNotificationCenter.CheckScheduledNotificationStatus(identifier) == NotificationStatus.Delivered)
        {
            //Remove the notification from the status bar
            AndroidNotificationCenter.CancelNotification(identifier);
        }
        else if (AndroidNotificationCenter.CheckScheduledNotificationStatus(identifier) == NotificationStatus.Unknown)
        {
            return AndroidNotificationCenter.SendNotification(notification, cannelId);
        }

        return identifier;
    }
#endif
}
