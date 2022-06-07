﻿using ChatManagerUtility.Configs;
using ChatManagerUtility.Events;
using Exiled.API.Features;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using static ChatManagerUtility.Config;

namespace ChatManagerUtility
{
    public class ChatManagerUpdater
    {

        public bool isRunning;

        public Thread currentRunningThread;

        public CancellationTokenSource cancellationToken;

        public Queue<MessageTypeHandler> AllIncomingMessages;

        public Queue<MessageTypeHandler> MessagesToDisplay;

        private int SleepTime { get; set; } = 3500;

        private int CharacterLimit { get; set; }

        private Player player { get; set; }

        private StringBuilder MsgToSend { get; set; }

        private int DisplayableLimit { get; set; }

        private int DisplayTimeLimit { get; set; }

        private LocationEnum TextPlacement { get; set; }

        readonly ChatColors chatColors;

        public ChatManagerUpdater(Player currentPlayer)
        {
            //CancellationTokenSource cts = new CancellationTokenSource();
            //ThreadPool.QueueUserWorkItem(new WaitCallback(ChatManagerParser), cts.Token);
            Log.Debug($"ChatManagerUpdater loaded in", ChatManagerUtilityMain.Instance.Config.IsDebugEnabled);
            cancellationToken = new CancellationTokenSource();
            isRunning = true;
            GlobalMessaging.IncomingGlobalMessage += this.ConsumeGlobalMessage;
            LocalMessaging.IncomingLocalMessage += this.ConsumeLocalMessage;
            PrivateMessaging.IncomingPrivateMessage += this.ConsumePrivateMessage;
            TeamMessaging.IncomingTeamMessage += this.ConsumeTeamMessage;

            AllIncomingMessages = new Queue<MessageTypeHandler>();
            MessagesToDisplay = new Queue<MessageTypeHandler>();
            SleepTime = (int)(ChatManagerUtilityMain.Instance.Config.SleepTime * 1000);
            CharacterLimit = ChatManagerUtilityMain.Instance.Config.CharacterLimit;
            player = currentPlayer;
            MsgToSend = new StringBuilder();
            DisplayableLimit = ChatManagerUtilityMain.Instance.Config.DisplayLimit;
            DisplayTimeLimit = (int)(ChatManagerUtilityMain.Instance.Config.DisplayTimeLimit * 1000);
            TextPlacement = ChatManagerUtilityMain.Instance.Config.TextPlacement;

            switch (TextPlacement) {
                case LocationEnum.Center:
                    MsgToSend.Append("<align=\"center\">");
                    break;
                case LocationEnum.Right:
                    MsgToSend.Append("<align=\"right\">");
                    break;
                case LocationEnum.Left:
                default:
                    MsgToSend.Append("<align=\"left\">");
                    break;
            }

            chatColors = ChatManagerUtilityMain.Instance.Config.AssociatedChatColors;
            Log.Debug($"ChatManagerUpdater Finished loading in", ChatManagerUtilityMain.Instance.Config.IsDebugEnabled);

            currentRunningThread = new Thread(() => ChatManager(cancellationToken.Token, this));
            currentRunningThread.Start();

        }

        private void ConsumeGlobalMessage(GlobalMsgEventArgs ev)
        {
            Log.Debug($"ConsumeGlobalMessage loaded in", ChatManagerUtilityMain.Instance.Config.IsDebugEnabled);
            AllIncomingMessages.Enqueue(new MessageTypeHandler(ev.GetMessage(), Time.time, CharacterLimit, ev.GetMsgType(), chatColors.ParseColor(ev.GetMsgType())));
            Log.Debug($"ConsumeGlobalMessage Finished loading in", ChatManagerUtilityMain.Instance.Config.IsDebugEnabled);
        }
        private void ConsumeLocalMessage(LocalMsgEventArgs ev)
        {
            Log.Debug($"ConsumeLocalMessage loaded in", ChatManagerUtilityMain.Instance.Config.IsDebugEnabled);
            AllIncomingMessages.Enqueue(new MessageTypeHandler(ev.GetMessage(), Time.time, CharacterLimit, ev.GetMsgType(), chatColors.ParseColor(ev.GetMsgType())));
            Log.Debug($"ConsumeLocalMessage Finished loading in", ChatManagerUtilityMain.Instance.Config.IsDebugEnabled);
        }

        private void ConsumePrivateMessage(PrivateMsgEventArgs ev)
        {
            Log.Debug($"ConsumePrivateMessage loaded in", ChatManagerUtilityMain.Instance.Config.IsDebugEnabled);
            AllIncomingMessages.Enqueue(new MessageTypeHandler(ev.GetMessage(), Time.time, CharacterLimit, ev.GetMsgType(), chatColors.ParseColor(ev.GetMsgType())));
            Log.Debug($"ConsumePrivateMessage Finished loading in", ChatManagerUtilityMain.Instance.Config.IsDebugEnabled);
        }
        private void ConsumeTeamMessage(TeamMsgEventArgs ev)
        {
            Log.Debug($"ConsumeTeamMessage loaded in", ChatManagerUtilityMain.Instance.Config.IsDebugEnabled);
            AllIncomingMessages.Enqueue(new MessageTypeHandler(ev.GetMessage(), Time.time, CharacterLimit, ev.GetMsgType(), chatColors.ParseColor(ev.GetMsgType())));
            Log.Debug($"ConsumeTeamMessage Finished loading in", ChatManagerUtilityMain.Instance.Config.IsDebugEnabled);
        }


        private static void ChatManager(object obj, ChatManagerUpdater operatingClass)
        {
            Log.Debug($"ChatManager loaded in", ChatManagerUtilityMain.Instance.Config.IsDebugEnabled);
            CancellationToken token = (CancellationToken)obj;

            while (ChatManagerCore.isRunning)
            {
                Log.Debug($"ChatManager Finished loading in, in loop", ChatManagerUtilityMain.Instance.Config.IsDebugEnabled);
                if (token.IsCancellationRequested)
                {
                    Log.Debug($"ChatManager, Cancellation received.", ChatManagerUtilityMain.Instance.Config.IsDebugEnabled);
                    return;
                }

                operatingClass.ProcessQueue();

                Thread.Sleep(operatingClass.SleepTime);
                Log.Debug($"ChatManager, After sleep.", ChatManagerUtilityMain.Instance.Config.IsDebugEnabled);
            }
        }

        public void SendMessage()
        {
            //this.player.hint.add(msg);
            this.player = Player.Get(this.player.Id);
            Log.Debug($"SendMessage entered, msg to send {MsgToSend} and Player {player is null}, Nickname {player?.Nickname}", ChatManagerUtilityMain.Instance.Config.IsDebugEnabled);
            //this.player.ShowHint("<line-height=75%><voffset=30em><align=center><color=#247BA0> hi </color> asdasdasd </align> </voffset>", 10);
            this.player.ShowHint(MsgToSend.ToString());
            MsgToSend.Clear();
            Log.Debug($"SendMessage After send and clear.", ChatManagerUtilityMain.Instance.Config.IsDebugEnabled);
        }

        public void AppendSendableHint(string msg){
            Log.Debug($"AppendSendableHint entered, msg: {msg}", ChatManagerUtilityMain.Instance.Config.IsDebugEnabled);
            MsgToSend.Append(msg);
            Log.Debug($"AppendSendableHint finished.", ChatManagerUtilityMain.Instance.Config.IsDebugEnabled);
        }

        private void ProcessQueue(){

            Log.Debug($"ProcessQueue entered, starting MessageCount loop AllIncomingMessages.Count {AllIncomingMessages.Count} vs MessagesToDisplay.Count {MessagesToDisplay.Count}", ChatManagerUtilityMain.Instance.Config.IsDebugEnabled);
            while (MessagesToDisplay.Count < DisplayableLimit)
            {
                if(AllIncomingMessages.IsEmpty()){
                    break;
                }
                MessagesToDisplay.Enqueue(AllIncomingMessages.Dequeue());
            }
            Log.Debug($"ProcessQueue finished first check if there were enough messages, how many msg's: {MessagesToDisplay.Count}.", ChatManagerUtilityMain.Instance.Config.IsDebugEnabled);

            Queue<MessageTypeHandler> messagesToRetain = new Queue<MessageTypeHandler>();
            while (!MessagesToDisplay.IsEmpty())
            {
                MessageTypeHandler currentMessageHandler = MessagesToDisplay.Dequeue();
                AppendSendableHint(currentMessageHandler.Msg);
                if (Time.time - currentMessageHandler.MsgTime < DisplayTimeLimit){
                    messagesToRetain.Append(currentMessageHandler);
                }
            }
            Log.Debug($"ProcessQueue, MessagesToDisplay iterated, and finished.", ChatManagerUtilityMain.Instance.Config.IsDebugEnabled);
            MessagesToDisplay = messagesToRetain;
            SendMessage();
            Log.Debug($"ProcessQueue finished.", ChatManagerUtilityMain.Instance.Config.IsDebugEnabled);
        }

        internal void shutdown()
        {
            Log.Debug($"shutdown for ChatManagerUpdater called.", ChatManagerUtilityMain.Instance.Config.IsDebugEnabled);
            this.cancellationToken.Cancel();
        }
    }
}