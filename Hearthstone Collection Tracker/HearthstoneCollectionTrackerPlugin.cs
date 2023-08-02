﻿using HearthMirror.Enums;
using Hearthstone_Collection_Tracker.Internal;
using Hearthstone_Collection_Tracker.Internal.DataUpdaters;
using Hearthstone_Deck_Tracker.Enums.Hearthstone;
using Hearthstone_Deck_Tracker.Hearthstone;
using Hearthstone_Deck_Tracker.Utility.Logging;
using MahApps.Metro.Controls.Dialogs;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Windows.Controls;
using System.Windows.Threading;

namespace Hearthstone_Collection_Tracker
{
    public class HearthstoneCollectionTrackerPlugin : Hearthstone_Deck_Tracker.Plugins.IPlugin
    {
        public void OnLoad()
        {
            DefaultDataUpdater.PerformUpdates();

            if (!Directory.Exists(PluginDataDir))
                Directory.CreateDirectory(PluginDataDir);

            Settings = PluginSettings.LoadSettings(PluginDataDir);

            MainMenuItem = new MenuItem { Header = "Collections" };
            foreach (var account in Settings.Accounts)
            {
                var menuItem = new MenuItem { Header = account.AccountName};
                menuItem.Click += (sender, args) =>
                {
                    var selectedAccount = menuItem.Header as string;
                    if (Settings.ActiveAccount != selectedAccount) Settings.SetActiveAccount(selectedAccount);
                    if (MainWindow == null)
                    {
                        InitializeMainWindow();
                        Debug.Assert(MainWindow != null, "_mainWindow != null");
                        MainWindow.Show();
                    }
                    else
                    {
                        MainWindow.Refresh();
                        MainWindow.Activate();
                    }
                };
                MainMenuItem.Items.Add(menuItem);
            }
            MainMenuItem.Items.Add(new Separator());
            var settingsMenuItem = new MenuItem { Header = "Settings" };
            settingsMenuItem.Click += (sender, args) => OnButtonPress();
            MainMenuItem.Items.Add(settingsMenuItem);

            Hearthstone_Deck_Tracker.API.DeckManagerEvents.OnDeckCreated.Add(HandleHearthstoneDeckUpdated);
            Hearthstone_Deck_Tracker.API.DeckManagerEvents.OnDeckUpdated.Add(HandleHearthstoneDeckUpdated);
            Hearthstone_Deck_Tracker.API.DeckManagerEvents.OnDeckDeleted.Add(HandleHearthstoneDeckDeleted);

            DispatcherTimer importTimer = new DispatcherTimer {Interval = new TimeSpan(0, 0, 0, 5)};
            importTimer.Tick += ImportTimerOnTick; 
			importTimer.IsEnabled = true;
			importTimer.Start();
        }

	    private void ImportTimerOnTick(object sender, EventArgs eventArgs)
	    {
			if (Hearthstone_Deck_Tracker.Core.Game.CurrentMode != Mode.COLLECTIONMANAGER || !Settings.EnableAutoImport || HearthMirror.Status.GetStatus().MirrorStatus != MirrorStatus.Ok)
				return;
			try
			{
				//SettingsWindow.ImportHearthMirror(false); 
				var importWithHearthmirror = SettingsWindow.ImportWithHearthmirror(Settings);
				if (importWithHearthmirror.Result == false)
				{
					throw new NullReferenceException("error");
				}
			}
			catch(Exception e)
			{
				Log.WriteLine("Error when auto-importing in Hearthstone Collection Tracker", LogType.Warning);
                Log.Error(e);
			}

	    }

        private void HandleHearthstoneDeckDeleted(IEnumerable<Deck> decks)
        {
            MainWindow.Refresh();
        }

        private void HandleHearthstoneDeckUpdated(Deck deck)
        {
            if (deck == null || !Settings.NotifyNewDeckMissingCards)
                return;

            if (deck.IsArenaDeck)
                return;

            List<Tuple<Card, int>> missingCards = new List<Tuple<Card, int>>();

            foreach (var deckCard in deck.Cards)
            {
                var cardSet = Settings.ActiveAccountSetsInfo.FirstOrDefault(set => set.CardSet == deckCard.CardSet);
                var collectionCard = cardSet?.Cards.FirstOrDefault(c => c.CardId == deckCard.Id);
                if (collectionCard == null)
                {
                    continue;
                }

                int missingAmount = Math.Max(0, deckCard.Count - (collectionCard.AmountGolden + collectionCard.AmountNonGolden));
                if (missingAmount > 0)
                {
                    missingCards.Add(new Tuple<Card, int>((Card) deckCard.Clone(), missingAmount));
                }
            }

            if (missingCards.Any())
            {
                MainWindow.Refresh();
                StringBuilder alertSb = new StringBuilder();
                foreach (var gr in missingCards.GroupBy(c => c.Item1.Set))
                {
                    alertSb.AppendFormat("{0} set:", gr.Key);
                    alertSb.AppendLine();
                    foreach(var card in gr)
                    {
                        alertSb.AppendFormat("  • {0} ({1});", card.Item1.LocalizedName, card.Item2);
                        alertSb.AppendLine();
                    }
                }
                alertSb.Append("You can disable this alert in Collection Tracker plugin settings.");
                Hearthstone_Deck_Tracker.Core.MainWindow.ShowMessageAsync("Missing cards in collection", alertSb.ToString());
            }
            deck.MissingCards = missingCards.Select(mc =>
            {
                mc.Item1.Count = mc.Item2;
                return mc.Item1;
            }).ToList();
        }

        public void OnUnload()
        {
            if (MainWindow != null)
            {
                if (MainWindow.IsVisible)
                {
                    MainWindow.Close();
                }
                MainWindow = null;
            }
            if (SettingsWindow != null)
            {
                if (SettingsWindow.IsVisible)
                {
                    SettingsWindow.Close();
                }
                SettingsWindow = null;
            }
            Settings.SaveSettings(PluginDataDir);
        }

        public void OnButtonPress()
        {
            if (SettingsWindow == null)
            {
                SettingsWindow = new SettingsWindow(Settings) {PluginWindow = MainWindow};
                SettingsWindow.Closed += (sender, args) =>
                {
                    SettingsWindow = null;
                };
                SettingsWindow.Show();
            }
            else
            {
                SettingsWindow.Activate();
            }
        }

        public void OnUpdate()
        {
            //CheckForUpdates();
        }

        public string Name => "Collection Tracker";

        public string Description => @"Helps user to keep track on packs progess, suggesting the packs that will most probably contain missing cards. 
No longer supported by https://github.com/HearthSim/Hearthstone-Collection-Tracker
This version built from https://github.com/batstyx/Hearthstone-Collection-Tracker";

        public string ButtonText => "Settings & Import";

        public string Author => "Vasilev Konstantin & the Community";

        public static readonly Version AssemblyVersion = Assembly.GetExecutingAssembly().GetName().Version;

        public static readonly Version PluginVersion = new Version(AssemblyVersion.Major, AssemblyVersion.Minor, AssemblyVersion.Build);

        public Version Version => PluginVersion;

        protected MenuItem MainMenuItem { get; set; }

        protected static MainWindow MainWindow;

        protected SettingsWindow SettingsWindow;

        protected void InitializeMainWindow()
        {
            if (MainWindow == null)
            {
                MainWindow = new MainWindow
                {
                    Left = Settings.CollectionWindowLeft,
                    Top = Settings.CollectionWindowTop,
                    Width = Settings.CollectionWindowWidth,
                    Height = Settings.CollectionWindowHeight,
                    Filter = {OnlyMissing = !Settings.DefaultShowAllCards}
                };
                MainWindow.Closed += (sender, args) =>
                {
                    Settings.CollectionWindowLeft = MainWindow.Left;
                    Settings.CollectionWindowTop = MainWindow.Top;
                    Settings.CollectionWindowWidth = MainWindow.Width;
                    Settings.CollectionWindowHeight = MainWindow.Height;
                    if (MainWindow.Filter != null)
                    {
                        Settings.DefaultShowAllCards = !MainWindow.Filter.OnlyMissing;
                    }
                    MainWindow = null;
                };
            }
        }

        public MenuItem MenuItem => MainMenuItem;

        internal static string PluginDataDir => System.IO.Path.Combine(Hearthstone_Deck_Tracker.Config.Instance.DataDir, "CollectionTracker");

        public static PluginSettings Settings { get; set; }
    }
}
