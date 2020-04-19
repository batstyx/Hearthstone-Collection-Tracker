using Hearthstone_Collection_Tracker.Controls;
using Hearthstone_Collection_Tracker.Internal;
using Hearthstone_Collection_Tracker.Internal.Importing;
using Hearthstone_Deck_Tracker;
using MahApps.Metro.Controls;
using MahApps.Metro.Controls.Dialogs;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using HearthMirror.Enums;
using Hearthstone_Deck_Tracker.Utility.Extensions;
using Hearthstone_Collection_Tracker.ViewModels;

namespace Hearthstone_Collection_Tracker
{
    /// <summary>
    /// Interaction logic for SettingsWindow.xaml
    /// </summary>
    public partial class SettingsWindow : MetroWindow
    {
        public Thickness TitleBarMargin
        {
            get { return new Thickness(0, TitlebarHeight, 0, 0); }
        }

        public Window PluginWindow { get; set; }

        public PluginSettings Settings { get; set; }

        public SettingsWindow(PluginSettings settings)
        {
            this.Settings = settings;
            InitializeComponent();

            UpdateAccountsComboBox();

            this.DataContext = this;
            var setsOption = SetCardsManager.CollectableSets.Select(s => new KeyValuePair<string, string>(s, s)).ToList();
            setsOption.Insert(0, new KeyValuePair<string, string>("All", null));
            CheckboxUseDecksForDesiredCards.IsEnabled = CheckboxEnableDesiredCardsFeature.IsChecked.Value;
        }

        private void UpdateAccountsComboBox()
        {
            ComboboxCurrentAccount.Items.Clear();
            foreach (var acc in Settings.Accounts)
            {
                ComboboxCurrentAccount.Items.Add(acc);
            }
            ComboboxCurrentAccount.Items.Refresh();
            ComboboxCurrentAccount.SelectedValue = Settings.ActiveAccount;

            ButtonDeleteAccount.IsEnabled = ComboboxCurrentAccount.Items.Count > 1;
        }

        private void ButtonAddAccount_Click(object sender, RoutedEventArgs e)
        {
            EditAccountWindow window = new EditAccountWindow();
            window.Owner = this;
            window.ExistingAccounts = Settings.Accounts.Select(acc => acc.AccountName).ToList();
            if (window.ShowDialog() == true)
            {
                Settings.AddAccount(window.AccountName);
                Settings.SetActiveAccount(window.AccountName);

                UpdateAccountsComboBox();
            }
        }

        private void ComboboxCurrentAccount_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.AddedItems.Count == 0 || e.RemovedItems.Count == 0)
            {
                return;
            }
            // close plugin window
            if (PluginWindow != null && PluginWindow.IsVisible)
            {
                PluginWindow.Close();
            }
            else
            {
                Settings.SaveCurrentAccount(Settings.ActiveAccountSetsInfo.ToList());
            }

            string selectedAccountName = (e.AddedItems[0] as AccountSummary).AccountName;
            Settings.SetActiveAccount(selectedAccountName);
        }

        private async void ButtonDeleteAccount_Click(object sender, RoutedEventArgs e)
        {
            string currentAccountName = Settings.ActiveAccount;
            var messageWindowSettings = new MetroDialogSettings()
            {
                AffirmativeButtonText = "Yes",
                NegativeButtonText = "No"
            };
            var result = await this.ShowMessageAsync("Caution",
                string.Format("Do you want to delete account {0}?", currentAccountName),
                MessageDialogStyle.AffirmativeAndNegative,
                messageWindowSettings);

            if (result != MessageDialogResult.Affirmative)
            {
                return;
            }
            
            // close plugin window
            if (PluginWindow != null && PluginWindow.IsVisible)
            {
                PluginWindow.Close();
            }

            Settings.DeleteAccount(currentAccountName);
            UpdateAccountsComboBox();
        }

        private void ButtonImport_Click(object sender, RoutedEventArgs e)
        {
            FlyoutImport.IsOpen = true;
        }

        private async void ButtonImportFromGame_Click(object sender, RoutedEventArgs e)
        {
            ImportHearthMirror(true);
        }

	    public async void ImportHearthMirror(bool showBoxes = false)
	    {
		    if(HearthMirror.Status.GetStatus().MirrorStatus != MirrorStatus.Ok)
		    {
			    if(showBoxes)
					this.ShowMessageAsync("Not Ready", "Make sure HS is open and you are in the Collection.").Forget();
				return;
			}
			var result = await ImportWithHearthmirror(Settings);
		    var resultText = result == true ? "Successfully imported!" : "Unable to import :(";
		    this.ShowMessageAsync("Import result", resultText).Forget();
	    }
	    public static async Task<bool> ImportWithHearthmirror(PluginSettings theSettings)
	    {
			var importObject = new HearthstoneImporter();
			try
			{
				var selectedSetToImport = new KeyValuePair<string, string>("All", "").Value;
				var collection = importObject.Import(selectedSetToImport);
                var setChangesList = new List<BasicSetCollectionInfo>();
                foreach (var set in collection)
				{
					var existingSet = theSettings.ActiveAccountSetsInfo.FirstOrDefault(s => s.SetName == set.SetName);
					if(existingSet == null)
					{
						theSettings.ActiveAccountSetsInfo.Add(set);
					}
					else
					{
                        var setChanges = setChangesList.FirstOrDefault(s => s.SetName == set.SetName) ?? new BasicSetCollectionInfo { SetName = set.SetName, Cards = new List<CardInCollection>(), };
                        // keep desired amount
                        foreach (var card in set.Cards)
						{
							var existingCardInfo = existingSet.Cards.FirstOrDefault(c => c.CardId == card.CardId);
							if(existingCardInfo != null)
							{
								card.DesiredAmount = existingCardInfo.DesiredAmount;
                                if (card.AmountNonGolden != existingCardInfo.AmountNonGolden || card.AmountGolden != existingCardInfo.AmountGolden)
                                    setChanges?.Cards.Add(new CardInCollection(card.Card, card.AmountNonGolden - existingCardInfo.AmountNonGolden, card.AmountGolden - existingCardInfo.AmountGolden));
							}
						}
						existingSet.Cards = set.Cards;
                        if (setChanges.Cards.Count > 0)
                            setChangesList.Add(setChanges);
					}
				}
                if (theSettings.EnableImportHistory)
                    ImportHistory.SaveChange(theSettings.Accounts.FirstOrDefault(a => a.AccountName == theSettings.ActiveAccount), setChangesList);

            }
			catch(ImportingException)
			{
				return false;
			}


			// save imported collection			
            HearthstoneCollectionTrackerPlugin.Settings.SaveCurrentAccount();
		    return true;
	    }

        private void ButtonEditAccount_Click(object sender, RoutedEventArgs e)
        {
            EditAccountWindow window = new EditAccountWindow();
            window.Owner = this;
            window.ExistingAccounts = Settings.Accounts
                                              .Where(ac => ac.AccountName != Settings.ActiveAccount)
                                              .Select(acc => acc.AccountName)
                                              .ToList();
            window.AccountName = Settings.ActiveAccount;
            if (window.ShowDialog() == true)
            {
                Settings.RenameCurrentAccount(window.AccountName);

                UpdateAccountsComboBox();
            }
        }

        // Only enable Use Decks option if Desired Cards are enabled
        private void CheckboxEnableDesiredCardsFeature_Checked(object sender, RoutedEventArgs e)
        {
            CheckboxUseDecksForDesiredCards.IsEnabled = true;
        }

        // If we uncheck Desired Cards we need to also clear the Use Decks option
        private void CheckboxEnableDesiredCardsFeature_Unchecked(object sender, RoutedEventArgs e)
        {
            CheckboxUseDecksForDesiredCards.IsEnabled = false;
            CheckboxUseDecksForDesiredCards.IsChecked = false;
        }

        private void UpdateMainWindow(object sender, RoutedEventArgs e)
        {
            MainWindow.Refresh();
        }
    }
}
