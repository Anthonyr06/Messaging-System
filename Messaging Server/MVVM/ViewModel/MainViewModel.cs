using Messaging_Server.MVVM.Core;
using Messaging_Server.Net;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace Messaging_Server.MVVM.ViewModel
{
    internal class MainViewModel : INotifyPropertyChanged
    {
        public RelayCommand MinimizeWindowCommand { get; set; }
        public RelayCommand CloseWindowCommand { get; set; }
        public RelayCommand StartServerCommand { get; set; }
        public RelayCommand StopServerCommand { get; set; }

        #region INotifyPropertyChanged Members

        public event PropertyChangedEventHandler? PropertyChanged;

        bool _serverIsOnline;
        public bool ServerIsOnline
        {
            get { return _serverIsOnline; }
            set { _serverIsOnline = value; OnPropertyChanged(nameof(ServerIsOnline)); }
        }
        bool _serverIsOffline;
        public bool ServerIsOffline
        {
            get { return _serverIsOffline; }
            set { _serverIsOffline = value; OnPropertyChanged(nameof(ServerIsOffline)); }
        }

        int _onlineUsers;
        public int OnlineUsers
        {
            get { return _onlineUsers; }
            set { _onlineUsers = value; OnPropertyChanged(nameof(OnlineUsers)); }
        }

        int _messagesQty;
        public int MessagesQty
        {
            get { return _messagesQty; }
            set { _messagesQty = value; OnPropertyChanged(nameof(MessagesQty)); }
        }

        string _serverBadgeText;
        public string ServerBadgeText 
        {
            get { return _serverBadgeText; }
            set { _serverBadgeText = value; OnPropertyChanged(nameof(ServerBadgeText)); }
        }

        string _serverBadgeImgSource;
        public string ServerBadgeImgSource
        {
            get { return _serverBadgeImgSource; }   
            set { _serverBadgeImgSource = value; OnPropertyChanged(nameof(ServerBadgeImgSource)); }
        }

        string _serverBadgeColor;
        public string ServerBadgeColor
        {
            get { return _serverBadgeColor; }
            set { _serverBadgeColor = value; OnPropertyChanged(nameof(ServerBadgeColor)); }
        }

        string _usersBadgeColor;
        public string UsersBadgeColor
        {
            get { return _usersBadgeColor; }
            set { _usersBadgeColor = value; OnPropertyChanged(nameof(UsersBadgeColor)); }
        }

        string _totalMessagesBadgeColor;
        public string TotalMessagesBadgeColor
        {
            get { return _totalMessagesBadgeColor; }
            set { _totalMessagesBadgeColor = value; OnPropertyChanged(nameof(TotalMessagesBadgeColor)); }
        }

        string _ipBadgeColor;
        public string IpBadgeColor
        {
            get { return _ipBadgeColor; }
            set { _ipBadgeColor = value; OnPropertyChanged(nameof(IpBadgeColor)); }
        }

        string _ipAddress;
        public string IpAddress
        {
            get { return _ipAddress; }
            set { _ipAddress = value; OnPropertyChanged(nameof(IpAddress)); }
        }

        string _port;
        public string Port
        {
            get { return _port; }
            set { _port = value; OnPropertyChanged(nameof(Port)); }
        }

        #endregion
        public MainViewModel()
        {
            MinimizeWindowCommand = new RelayCommand(o => MinimizeWindow());
            CloseWindowCommand = new RelayCommand(o => CloseWindow());

            ServerManagement.ConnectedEvent += ServerOn;
            ServerManagement.DisconnectedEvent += ServerOff;
            ServerManagement.ClientJoinEvent += ClientJoin;
            ServerManagement.ClientLeftEvent += ClientLeft;
            ServerManagement.NewMessageEvent += NewMessage;

            ServerOff();

            StartServerCommand = new RelayCommand(o => ServerManagement.StartServer(), o => ServerIsOffline);
            StopServerCommand = new RelayCommand(o => ServerManagement.StopServer(), o => ServerIsOnline);
        }

        private void NewMessage()
        {
            if (MessagesQty < 999)
                Application.Current.Dispatcher.Invoke(() => MessagesQty += 1 );
        }

        private void ClientLeft()
        {
            Application.Current.Dispatcher.Invoke(() => OnlineUsers -= 1);
        }

        private void ClientJoin()
        {
            if (OnlineUsers < 999)
                Application.Current.Dispatcher.Invoke(() => OnlineUsers+=1);
        }

        private void ServerOff()
        {
            Application.Current.Dispatcher.Invoke(() => ServerIsOffline = true);
            Application.Current.Dispatcher.Invoke(() => ServerIsOnline = false);
            Application.Current.Dispatcher.Invoke(() => ServerBadgeText = "Server Off");
            Application.Current.Dispatcher.Invoke(() => ServerBadgeImgSource = "/Images/server-off.png");
            Application.Current.Dispatcher.Invoke(() => ServerBadgeColor = "Gray");
            Application.Current.Dispatcher.Invoke(() => IpBadgeColor = "Gray");
            Application.Current.Dispatcher.Invoke(() => UsersBadgeColor = "Gray");
            Application.Current.Dispatcher.Invoke(() => TotalMessagesBadgeColor = "Gray");
            Application.Current.Dispatcher.Invoke(() => IpAddress = "XXX.XXX.XXX.XXX");
            Application.Current.Dispatcher.Invoke(() => Port = "XXXX");
            Application.Current.Dispatcher.Invoke(() => OnlineUsers = 0);
            Application.Current.Dispatcher.Invoke(() => MessagesQty = 0);
        }

        private void ServerOn()
        {
            Application.Current.Dispatcher.Invoke(() => ServerIsOffline = false);
            Application.Current.Dispatcher.Invoke(() => ServerIsOnline = true);
            Application.Current.Dispatcher.Invoke(() => ServerBadgeText = "Server On");
            Application.Current.Dispatcher.Invoke(() => ServerBadgeImgSource = "/Images/Server-on.png");
            Application.Current.Dispatcher.Invoke(() => ServerBadgeColor = "Green");
            Application.Current.Dispatcher.Invoke(() => IpBadgeColor = "Orange");
            Application.Current.Dispatcher.Invoke(() => UsersBadgeColor = "BlueViolet");
            Application.Current.Dispatcher.Invoke(() => TotalMessagesBadgeColor = "Red");
            Application.Current.Dispatcher.Invoke(() => IpAddress = ServerManagement.IpAddress);
            Application.Current.Dispatcher.Invoke(() => Port = ServerManagement.Port.ToString());
        }

        private static void CloseWindow()
        {
            Application.Current.Shutdown();
        }

        private static void MinimizeWindow()
        {
            Application.Current.MainWindow.WindowState = WindowState.Minimized;
        }
        protected void OnPropertyChanged(string name)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
}
