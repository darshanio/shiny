﻿using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.Reactive.Threading.Tasks;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using Foundation;
using UIKit;
using UserNotifications;
using Shiny.Settings;


namespace Shiny.Push
{
    public class PushManager : AbstractPushManager
    {
        readonly Subject<IDictionary<string, string>> pushSubject;


        // launch options?  UIApplication.LaunchOptionsRemoteNotificationKey
        public PushManager(ISettings settings) : base(settings)
        {
            this.pushSubject = new Subject<IDictionary<string, string>>();
        }


        public override IObservable<IDictionary<string, string>> WhenReceived() => this.pushSubject;


        public override async Task<PushAccessState> RequestAccess(CancellationToken cancelToken = default)
        {
            var deviceToken = await this.RequestDeviceToken(cancelToken);
            this.CurrentRegistrationToken = ToTokenString(deviceToken);
            this.CurrentRegistrationTokenDate = DateTime.UtcNow;
            return new PushAccessState(AccessState.Available, this.CurrentRegistrationToken);
        }


        public override Task UnRegister()
            => Dispatcher.InvokeOnMainThreadAsync(UIApplication.SharedApplication.UnregisterForRemoteNotifications);            


        protected virtual async Task<NSData> RequestDeviceToken(CancellationToken cancelToken = default)
        {
            var result = await UNUserNotificationCenter.Current.RequestAuthorizationAsync(
                UNAuthorizationOptions.Alert |
                UNAuthorizationOptions.Badge |
                UNAuthorizationOptions.Sound
            );
            if (!result.Item1)
                throw new Exception(result.Item2.LocalizedDescription);

            var remoteTask = iOSShinyHost.WhenRegisteredForRemoteNotifications().Take(1).ToTask(cancelToken);
            await Dispatcher.InvokeOnMainThreadAsync(UIApplication.SharedApplication.RegisterForRemoteNotifications);

            var data = await remoteTask;
            return data;
        }


        protected static string? ToTokenString(NSData deviceToken)
        {
            string? token = null;
            if (deviceToken.Length > 0)
            {
                if (UIDevice.CurrentDevice.CheckSystemVersion(13, 0))
                {
                    var data = deviceToken.ToArray();
                    token = BitConverter
                        .ToString(data)
                        .Replace("-", "")
                        .Replace("\"", "");
                }
                else if (!deviceToken.Description.IsEmpty())
                {
                    token = deviceToken.Description.Trim('<', '>');
                }
            }
            return token;
        }
    }
}
