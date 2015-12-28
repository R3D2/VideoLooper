using MahApps.Metro.Controls;
using MahApps.Metro.Controls.Dialogs;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace VideoLooper.Views
{
    /// <summary>
    /// Home View of the Application
    /// </summary>
    public partial class Home : UserControl
    {
        #region VARS
        private bool _blError = false; //Error Feedback for the user
        #endregion

        #region GET/SET
        public bool BlError
        {
            get { return _blError; }
            set { _blError = value; }
        }
        #endregion

        public Home()
        {
            InitializeComponent();
        }

        /// <summary>
        /// View displaying error message if set to true
        /// </summary>
        /// <param name="blError"></param>
        public Home(bool blError)
        {
            BlError = blError;
            InitializeComponent();
            ShowError();
        }

        /// <summary>
        /// Async method showing the message
        /// </summary>
        async private void ShowError()
        {
            if(BlError)
            {
                await ShowMessage();
            }
        }

        /// <summary>
        /// ShowMessage Method to display the message
        /// </summary>
        /// <returns></returns>
        async private Task<MessageDialogResult> ShowMessage()
        {
            var metroWindow = (Application.Current.MainWindow as MetroWindow);
            return await metroWindow.ShowMessageAsync("Error", "The device has been disconnected !");
        }
    }
}
