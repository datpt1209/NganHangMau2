using System.ComponentModel;
using System.Windows;
using ToastNotifications;
using ToastNotifications.Lifetime;
using ToastNotifications.Lifetime.Clear;
using ToastNotifications.Messages;
using ToastNotifications.Position;

namespace NganHangMau2
{
    public class ToastNotificationService
    {
        private static Notifier _notifier;

        static ToastNotificationService()
        {
            InitializeNotifier();
        }

        private static void InitializeNotifier()
        {
            var parentWindow = Application.Current.Windows.OfType<MainWindow>().FirstOrDefault();  // Ensure this is not null
            if (parentWindow == null)
            {
                throw new InvalidOperationException("Parent window is not set.");
            }

            _notifier = new Notifier(cfg =>
            {
                cfg.PositionProvider = new WindowPositionProvider(
                    parentWindow,
                    Corner.TopRight,
                    10, 10);

                cfg.LifetimeSupervisor = new TimeAndCountBasedLifetimeSupervisor(
                    TimeSpan.FromSeconds(3), MaximumNotificationCount.FromCount(5));

                cfg.Dispatcher = Application.Current.Dispatcher;
            });
        }
        //static ToastNotificationService()
        //{
        //    _notifier = new Notifier(cfg =>
        //    {
        //        cfg.PositionProvider = new WindowPositionProvider(
        //            parentWindow: Application.Current.MainWindow,
        //            corner: Corner.TopRight,
        //            offsetX: 10,
        //            offsetY: 10);

        //        cfg.LifetimeSupervisor = new TimeAndCountBasedLifetimeSupervisor(
        //            notificationLifetime: TimeSpan.FromSeconds(5),
        //            maximumNotificationCount: MaximumNotificationCount.FromCount(5));

        //        cfg.Dispatcher = Application.Current.Dispatcher;
        //    });
        //    _notifier.ClearMessages(new ClearAll());
        //}

        public static void ShowSuccess(string message)
        {
            _notifier.ShowSuccess(message);
        }

        public void OnUnloaded()
        {
            _notifier.Dispose();
        }

        public static void ShowInformation(string message)
        {
            _notifier.ShowInformation(message);
        }

        public static void ShowWarning(string message)
        {
            _notifier.ShowWarning(message);
        }

        public static void ShowError(string message)
        {
            _notifier.ShowError(message);
        }

        public static void ClearMessages()
        {
            _notifier.ClearMessages(new ClearAll());
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName = null)
        {
            var handler = PropertyChanged;
            if (handler != null)
                handler.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public void ClearAll()
        {
            _notifier.ClearMessages(new ClearAll());
        }
    }
}
