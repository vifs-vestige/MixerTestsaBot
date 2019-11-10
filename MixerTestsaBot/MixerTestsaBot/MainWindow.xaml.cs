using Mixer.Base;
using Mixer.Base.Clients;
using Mixer.Base.Model.Channel;
using Mixer.Base.Model.Chat;
using Mixer.Base.Model.OAuth;
using Mixer.Base.Model.User;
using Mixer.Base.Web;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Configuration;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace MixerTestsaBot {
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window {
        private MixerConnection Connection;
        private ExpandedChannelModel Channel;
        private PrivatePopulatedUserModel User;
        private ChatClient ChatClient;

        private ObservableCollection<ChatMessage> ChatMessages = new ObservableCollection<ChatMessage>();
        private ObservableCollection<ChatUser> ChatUsers = new ObservableCollection<ChatUser>();

        private ChatBot Bot;

        public MainWindow() {
            InitializeComponent();

            Bot = new ChatBot();
        }

        private async void Login_Click(object sender, RoutedEventArgs e) {
            Connection = await MixerConnection.ConnectViaLocalhostOAuthBrowser(Settings.Default.ClientId, GetScope());

            if (Connection != null) {
                User = await Connection.Users.GetCurrentUser();
                Channel = await Connection.Channels.GetChannel(Settings.Default.Channel);
                ChatClient = await ChatClient.CreateFromChannel(Connection, Channel);

                ChatClient.OnMessageOccurred += ChatClient_MessageOccurred;
                ChatClient.OnDisconnectOccurred += ChatClient_OnDisconnectOccurred;

                if (await ChatClient.Connect() && await ChatClient.Authenticate()) {
                    Log("connected");
                }
            }
        }

        private void ChatClient_MessageOccurred(object sender, ChatMessageEventModel e) {
            var message = new ChatMessage(e);
            if (message.UserName != Settings.Default.BotName) {
                Log(message.UserName + ": " + message.Message);
                var output = Bot.parseInput(message.Message, message.UserName);
                if (output != null) {
                    ChatClient.SendMessage(output);
                }
            }
        }

        private async void ChatClient_OnDisconnectOccurred(object sender, System.Net.WebSockets.WebSocketCloseStatus e) {
            Log("Disconnection Occurred, attempting reconnection...");

            do {
                await Task.Delay(2500);
            }
            while (!await ChatClient.Connect() && !await ChatClient.Authenticate());

            Log("Reconnection successful");
        }

        public void Log(string stuff) {
            ChatOutput.Text += stuff + "\n";
        }

        private List<OAuthClientScopeEnum> GetScope() {
            return new List<OAuthClientScopeEnum>()
{
                OAuthClientScopeEnum.chat__chat,
                OAuthClientScopeEnum.chat__connect,

                OAuthClientScopeEnum.channel__details__self,
                OAuthClientScopeEnum.channel__update__self,

                OAuthClientScopeEnum.user__details__self,
                OAuthClientScopeEnum.user__log__self,
                OAuthClientScopeEnum.user__notification__self,
                OAuthClientScopeEnum.user__update__self
            };
        }

        private void Button_Click(object sender, RoutedEventArgs e) {
            ChatClient.SendMessage("test from window");
        }
    }
}
