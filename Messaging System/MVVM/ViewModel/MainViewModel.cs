using Messaging_Client.MVVM.Core;
using Messaging_Client.MVVM.Model;
using Messaging_Client.MVVM.View;
using Messaging_Client.Net;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace Messaging_Client.MVVM.ViewModel
{
    internal class MainViewModel : INotifyPropertyChanged
    {
        public ObservableCollection<UserModel> Users { get; set; }
        public ObservableCollection<string> Messages { get; set; }

        public RelayCommand MinimizeWindowCommand { get; set; }
        public RelayCommand CloseWindowCommand { get; set; }
        public RelayCommand ConnectToServerCommand { get; set; }
        public RelayCommand SendMessageCommand { get; set; }

        #region INotifyPropertyChanged Members

        public event PropertyChangedEventHandler? PropertyChanged;

        string _userName;
        public string Username
        {
            get { return _userName; }
            set { _userName = value; OnPropertyChanged(nameof(Username)); }
        }

        string _ipAddress;
        public string IpAddress
        {
            get { return _ipAddress; }
            set { _ipAddress = value; OnPropertyChanged(nameof(IpAddress)); }
        }

        int _port;
        public int Port
        {
            get { return _port; }
            set { _port = value; OnPropertyChanged(nameof(Port)); }
        }
        string _message;
        public string Message
        {
            get { return _message; }
            set { _message = value; OnPropertyChanged(nameof(Message)); }
        }

        bool _userOnline;
        public bool UserOnline
        {
            get { return _userOnline; }
            set { _userOnline = value; OnPropertyChanged(nameof(UserOnline)); }
        }
        bool _userOffline = true;
        public bool UserOffline
        {
            get { return _userOffline; }
            set { _userOffline = value; OnPropertyChanged(nameof(UserOffline)); }
        }
        #endregion
        private UserModel _user;
        private Server _server;

        public MainViewModel()
        {
            
            MinimizeWindowCommand = new RelayCommand(o => MinimizeWindow());
            CloseWindowCommand = new RelayCommand(o => CloseWindow());

            Users = new ObservableCollection<UserModel>();
            Messages = new ObservableCollection<string>();

            _server = new Server();
            _server.ConnectedEvent += UserConnected;
            _server.DisconnectedEvent += UserDisconnected;
            _server.NewMessageEvent += NewMessage;


            IpAddress = _server.IpAddress;
            Port = _server.Port;

            ConnectToServerCommand = new RelayCommand(o => _server.ConnectToServer(Username),
                o => !string.IsNullOrWhiteSpace(Username));

            SendMessageCommand = new RelayCommand(o => _server.SendMessageToServer(Message),
                o => !string.IsNullOrWhiteSpace(Message));
        }
        private void UserConnected()
        {
            _user = new()
            {
                Username = _server.PacketReader.ReadMessage(),
                UID = _server.PacketReader.ReadMessage()
            };

            if (!Users.Any(x => x.UID == _user.UID))
            {
                Application.Current.Dispatcher.Invoke(() => Users.Add(_user));
                UserOnline = true;
                UserOffline = false;
            }
        }

        private void NewMessage()
        {
            var msg = _server.PacketReader.ReadMessage();
            Application.Current.Dispatcher.Invoke(() => Messages.Add(msg));
            Message = string.Empty;
        }

        private void UserDisconnected()
        {
            var uid = _server.PacketReader.ReadMessage();
            var user = Users.FirstOrDefault(x => x.UID == uid);
            Application.Current.Dispatcher.Invoke(() => Users.Remove(user));


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

