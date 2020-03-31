using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;
using UnityEditor;
using UnityEngine;

using Unity.Notifications.iOS;

[assembly: InternalsVisibleTo("Unity.Notifications.Tests")]
namespace Unity.Notifications
{
    [HelpURL("Packages/com.unity.mobile.notifications/documentation.html")]
    internal class NotificationSettingsManager : ScriptableObject
    {
        internal static readonly string k_SettingsPath = "ProjectSettings/MobileNotificationsSettings.asset";

        public int ToolbarIndex = 0;

        public List<NotificationSetting> iOSNotificationSettings;
        public List<NotificationSetting> AndroidNotificationSettings;

        [SerializeField]
        private NotificationSettingsCollection m_iOSNotificationSettingsValues;

        [SerializeField]
        private NotificationSettingsCollection m_AndroidNotificationSettingsValues;

        public List<DrawableResourceData> DrawableResources = new List<DrawableResourceData>();

        public List<NotificationSetting> iOSNotificationSettingsFlat
        {
            get
            {
                var target = new List<NotificationSetting>();
                FlattenList(iOSNotificationSettings, target);
                return target;
            }
        }

        public List<NotificationSetting> AndroidNotificationSettingsFlat
        {
            get
            {
                var target = new List<NotificationSetting>();
                FlattenList(AndroidNotificationSettings, target);
                return target;
            }
        }

        private void FlattenList(List<NotificationSetting> source, List<NotificationSetting> target)
        {
            foreach (var setting in source)
            {
                target.Add(setting);

                if (setting.Dependencies != null)
                {
                    FlattenList(setting.Dependencies, target);
                }
            }
        }

        public static NotificationSettingsManager Initialize()
        {
            var settingsManager = LoadNotificationSettingsManager();

            bool dirty = false;
            if (settingsManager.m_iOSNotificationSettingsValues == null)
            {
                settingsManager.m_iOSNotificationSettingsValues = new NotificationSettingsCollection();
                dirty = true;
            }

            if (settingsManager.m_AndroidNotificationSettingsValues == null)
            {
                settingsManager.m_AndroidNotificationSettingsValues = new NotificationSettingsCollection();
                dirty = true;
            }

            // Create the default settings for iOS.
            var iOSSettings = new List<NotificationSetting>()
            {
                new NotificationSetting(
                    "UnityNotificationRequestAuthorizationOnAppLaunch",
                    "Request Authorization on App Launch",
                    "It's recommended to make the authorization request during the app's launch cycle. If this is enabled the authorization pop-up will show up immediately during launching. Otherwise you need to manually create an AuthorizationRequest before sending or receiving notifications.",
                    settingsManager.GetNotificationSettingValue("UnityNotificationRequestAuthorizationOnAppLaunch", true, false),
                    dependencies: new List<NotificationSetting>()
                    {
                        new NotificationSetting(
                            "UnityNotificationDefaultAuthorizationOptions",
                            "Default Notification Authorization Options",
                            "Configure the notification interaction types which will be included in the authorization request if \"Request Authorization on App Launch\" is enabled.",
                            settingsManager.GetNotificationSettingValue("UnityNotificationDefaultAuthorizationOptions",
                                AuthorizationOption.Alert | AuthorizationOption.Badge | AuthorizationOption.Sound, false)),
                    }),
                new NotificationSetting(
                    "UnityAddRemoteNotificationCapability",
                    "Enable Push Notifications",
                    "Enable this to add the push notification capability to the Xcode project.",
                    settingsManager.GetNotificationSettingValue("UnityAddRemoteNotificationCapability", false, false),
                    false,
                    new List<NotificationSetting>()
                    {
                        new NotificationSetting(
                            "UnityNotificationRequestAuthorizationForRemoteNotificationsOnAppLaunch",
                            "Register for Push Notifications on App Launch",
                            "Enable this to automatically register your app with APNs after launching to receive remote notifications. You need to manually create an AuthorizationRequest to get the device token.",
                            settingsManager.GetNotificationSettingValue("UnityNotificationRequestAuthorizationForRemoteNotificationsOnAppLaunch", false, false)),
                        new NotificationSetting(
                            "UnityRemoteNotificationForegroundPresentationOptions",
                            "Remote Notification Foreground Presentation Options",
                            "Configure the default presentation options for received remote notifications. In order to use the specified presentation options, your app must have received the authorization (the user might change it at any time).",
                            settingsManager.GetNotificationSettingValue("UnityRemoteNotificationForegroundPresentationOptions", (PresentationOption)iOSPresentationOption.All, false)),
                        new NotificationSetting("UnityUseAPSReleaseEnvironment",
                            "Enable Release Environment for APS",
                            "Enable this when signing the app with a production certificate.",
                            settingsManager.GetNotificationSettingValue("UnityUseAPSReleaseEnvironment", false, false),
                            false),
                    }),
                new NotificationSetting("UnityUseLocationNotificationTrigger",
                    "Include CoreLocation Framework",
                    "Include the CoreLocation framework to use the iOSNotificationLocationTrigger in your project.",
                    settingsManager.GetNotificationSettingValue("UnityUseLocationNotificationTrigger", false, false),
                    false)
            };

            if (settingsManager.iOSNotificationSettings == null || settingsManager.iOSNotificationSettings.Count != iOSSettings.Count)
            {
                settingsManager.iOSNotificationSettings = iOSSettings;
                dirty = true;
            }

            // Create the default settings for Android.
            var androidSettings = new List<NotificationSetting>()
            {
                new NotificationSetting(
                    "UnityNotificationAndroidRescheduleOnDeviceRestart",
                    "Reschedule on Device Restart",
                    "Enable this to automatically reschedule all non-expired notifications after device restart. By default AndroidSettings removes all scheduled notifications after restarting.",
                    settingsManager.GetNotificationSettingValue("UnityNotificationAndroidRescheduleOnDeviceRestart", false, true)),
                new NotificationSetting(
                    "UnityNotificationAndroidUseCustomActivity",
                    "Use Custom Activity",
                    "Enable this to override the activity which will be opened when the user taps the notification.",
                    settingsManager.GetNotificationSettingValue("UnityNotificationAndroidUseCustomActivity", false, true),
                    dependencies: new List<NotificationSetting>()
                    {
                        new NotificationSetting(
                            "UnityNotificationAndroidCustomActivityString",
                            "Custom Activity Name",
                            "The full class name of the activity which will be assigned to the notification.",
                            settingsManager.GetNotificationSettingValue("UnityNotificationAndroidCustomActivityString", "com.unity3d.player.UnityPlayerActivity", true))
                    })
            };

            if (settingsManager.AndroidNotificationSettings == null || settingsManager.AndroidNotificationSettings.Count != androidSettings.Count)
            {
                settingsManager.AndroidNotificationSettings = androidSettings;
                dirty = true;
            }

            settingsManager.SaveSettings(dirty);

            return settingsManager;
        }

        private static NotificationSettingsManager LoadNotificationSettingsManager()
        {
            // TODO: Migrate legacy settings.
            //const string k_LegacyAssetPath = "Editor/com.unity.mobile.notifications/NotificationSettings.asset";
            //var assetRelPath = Path.Combine("Assets", k_LegacyAssetPath);
            //var settingsManager = AssetDatabase.LoadAssetAtPath<NotificationSettingsManager>(assetRelPath);

            var settingsManager = CreateInstance<NotificationSettingsManager>();
            if (File.Exists(k_SettingsPath))
            {
                var settingsJson = File.ReadAllText(k_SettingsPath);
                EditorJsonUtility.FromJsonOverwrite(settingsJson, settingsManager);
            }

            return settingsManager;
        }

        private T GetNotificationSettingValue<T>(string key, T defaultValue, bool isAndroid)
        {
            var collection = isAndroid ? m_AndroidNotificationSettingsValues : m_iOSNotificationSettingsValues;

            try
            {
                var value = collection[key];
                if (value != null)
                    return (T)value;
            }
            catch (InvalidCastException)
            {
                Debug.LogWarning("Failed loading : " + key + " for type:" + defaultValue.GetType() + "Expected : " + collection[key].GetType());
                //Just return default value if it's a new setting that was not yet serialized.
            }

            collection[key] = defaultValue;
            return defaultValue;
        }

        public void SaveSetting(NotificationSetting setting, BuildTargetGroup target)
        {
            var collection = (target == BuildTargetGroup.Android) ? m_AndroidNotificationSettingsValues : m_iOSNotificationSettingsValues;

            if (!collection.Contains(setting.Key) || collection[setting.Key].ToString() != setting.Value.ToString())
            {
                collection[setting.Key] = setting.Value;
                SaveSettings();
            }
        }

        public void SaveSettings(bool forceSave = true)
        {
            if (!forceSave && File.Exists(k_SettingsPath))
                return;

            File.WriteAllText(k_SettingsPath, EditorJsonUtility.ToJson(this, true));
        }

        public void AddDrawableResource(string id, Texture2D image, NotificationIconType type)
        {
            var drawableResource = new DrawableResourceData();
            drawableResource.Id = id;
            drawableResource.Type = type;
            drawableResource.Asset = image;

            DrawableResources.Add(drawableResource);
            SaveSettings();
        }

        public void RemoveDrawableResource(int index)
        {
            DrawableResources.RemoveAt(index);
            SaveSettings();
        }

        public Dictionary<string, byte[]> GenerateDrawableResourcesForExport()
        {
            var icons = new Dictionary<string, byte[]>();
            foreach (var drawableResource in DrawableResources)
            {
                if (!drawableResource.Verify())
                {
                    Debug.LogWarning(string.Format("Failed exporting: '{0}' AndroidSettings notification icon because:\n {1} ", drawableResource.Id,
                        DrawableResourceData.GenerateErrorString(drawableResource.Errors)));
                    continue;
                }

                var texture = TextureAssetUtils.ProcessTextureForType(drawableResource.Asset, drawableResource.Type);

                var scale = drawableResource.Type == NotificationIconType.Small ? 0.375f : 1;

                var textXhdpi = TextureAssetUtils.ScaleTexture(texture, (int)(128 * scale), (int)(128 * scale));
                var textHdpi  = TextureAssetUtils.ScaleTexture(texture, (int)(96 * scale), (int)(96 * scale));
                var textMdpi  = TextureAssetUtils.ScaleTexture(texture, (int)(64 * scale), (int)(64 * scale));
                var textLdpi  = TextureAssetUtils.ScaleTexture(texture, (int)(48 * scale), (int)(48 * scale));

                icons[string.Format("drawable-xhdpi-v11/{0}.png", drawableResource.Id)] = textXhdpi.EncodeToPNG();
                icons[string.Format("drawable-hdpi-v11/{0}.png", drawableResource.Id)] = textHdpi.EncodeToPNG();
                icons[string.Format("drawable-mdpi-v11/{0}.png", drawableResource.Id)] = textMdpi.EncodeToPNG();
                icons[string.Format("drawable-ldpi-v11/{0}.png", drawableResource.Id)] = textLdpi.EncodeToPNG();

                if (drawableResource.Type == NotificationIconType.Large)
                {
                    var textXxhdpi = TextureAssetUtils.ScaleTexture(texture, (int)(192 * scale), (int)(192 * scale));
                    icons[string.Format("drawable-xxhdpi-v11/{0}.png", drawableResource.Id)] = textXxhdpi.EncodeToPNG();
                }
            }

            return icons;
        }
    }
}
