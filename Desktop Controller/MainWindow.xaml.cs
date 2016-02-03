using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Threading;

using MessageBox = System.Windows.MessageBox;
using TouchlessControllerConfiguration = PXCMTouchlessController.ProfileInfo.Configuration;
using FF_TouchlessControllerViewer.cs;

namespace Desktop_Controller
{ 
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        #region Consts and class members
        
        private Boolean MouseClick = false;
        private const double VerticalScrollSensitivity = 15f;
        private const double HorizontalScrollSensitivity = 1000f;
        private const double HorizontalScrollStep = 10f;
        private const double VerticalScrollStep = 0.15f;
        private readonly RealSenseEngine m_rsEngine;

        #endregion

        public MainWindow()
        {
            InitializeComponent();

            try
            {
                m_rsEngine = new RealSenseEngine();
                m_rsEngine.UXEventFired += OnFiredUxEventDelegate;
                m_rsEngine.AlertFired += OnFiredAlertDelegate;
                m_rsEngine.SetConfiguration(GetCurrentConfiguration());
                m_rsEngine.Start();
            }
            catch (Exception e)
            {
                log.Text += "Error loading engine: " + e.Message + "\n";
                MessageBox.Show(
                    "Error loading engine\n" +
                    "Reason: " + e.Message +
                    "\nSample will run without Touchless Controller", "Engine Error",
                    MessageBoxButton.OK, MessageBoxImage.Exclamation);
            }
            // smoother cursor upgrading
            CompositionTarget.Rendering += CompositionTarget_Rendering;
        }

        #region App's TouchlessController Configuration Methods

        private TouchlessControllerConfiguration GetCurrentConfiguration()
        {
            var config = TouchlessControllerConfiguration.Configuration_Allow_Zoom;
            config |= TouchlessControllerConfiguration.Configuration_Scroll_Horizontally;
            config |= TouchlessControllerConfiguration.Configuration_Scroll_Vertically;
            config |= TouchlessControllerConfiguration.Configuration_Edge_Scroll_Horizontally;
            config |= TouchlessControllerConfiguration.Configuration_Edge_Scroll_Vertically;
            //config |= TouchlessControllerConfiguration.Configuration_Allow_Back;
            config |= TouchlessControllerConfiguration.Configuration_Allow_Selection;
            return config;
        }

        #endregion

        #region Window Related Methods

        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {

        }

        private void MainWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (m_rsEngine != null)
            {
                m_rsEngine.Shutdown();
            }
        }

        private void CompositionTarget_Rendering(object sender, EventArgs e)
        {

        }

        #endregion

        #region TouchlessController Event Handlers

        private void OnFiredAlertDelegate(PXCMTouchlessController.AlertData data)
        {
            Dispatcher.BeginInvoke((Action)(() =>
            {
                switch (data.type)
                {
                    case PXCMTouchlessController.AlertData.AlertType.Alert_TooClose:
                        log.Text += "Your hand is too close.\n";
                        break;
                    case PXCMTouchlessController.AlertData.AlertType.Alert_TooFar:
                        log.Text += "Your hand is too far away.\n";
                        break;
                    case PXCMTouchlessController.AlertData.AlertType.Alert_NoAlerts:
                        break;
                }
            }));
        }

        private void OnFiredUxEventDelegate(PXCMTouchlessController.UXEventData data)
        {
            var posX = data.position.x;
            var posY = data.position.y;
            var horizontal = CursorMapping.HorizontalMappingConstant;
            var vertical = CursorMapping.VeriticalMappingContant;
            var Y = (int)(posY * vertical);
            var X = (int)(posX * horizontal);

            Dispatcher.BeginInvoke((Action)(() =>
            {
                switch (data.type)
                {
                    case PXCMTouchlessController.UXEventData.UXEventType.UXEvent_CursorVisible:
                        log.Text += "Recognized hand input. Tracking...\n";
                        break;
                    case PXCMTouchlessController.UXEventData.UXEventType.UXEvent_CursorNotVisible:
                        log.Text += "Lost tracked hand...\n";
                        break;
                    case PXCMTouchlessController.UXEventData.UXEventType.UXEvent_CursorMove:
                        labelX.Dispatcher.Invoke(
                            new Action(() => labelX.Content = "x " + data.position.x.ToString()));
                        labelY.Dispatcher.Invoke(
                            new Action(() => labelY.Content = "y " + data.position.y.ToString()));
                        labelZ.Dispatcher.Invoke(
                            new Action(() => labelZ.Content = "z " + data.position.z.ToString()));
                        if (gameMode.IsChecked == true)
                        {
                            int sens = (int)sensitivity.Value;

                            // generating user input rectangle 
                            if (posX > 0.5 && posX < 0.7)
                            {
                                SetCursorPos(horizontal / 2 - sens / 2 + (int)(posX / 2 * sens), vertical / 2);
                            } else if (posX > 0.8)
                            {
                                SetCursorPos(horizontal / 2 - sens / 2 + (int)(posX  * sens), vertical / 2);
                            } else if (posY > 0.6)
                            {
                                SetCursorPos(horizontal / 2, vertical / 2 - sens / 2 + (int)(posY * sens));
                            } else if (posY < 0.4)
                            {
                                SetCursorPos(horizontal / 2, vertical / 2 - sens / 2 + (int)(posY * sens));
                            }
                            //SetCursorPos((horizontal / 2 - sens / 2) + (int)(posX * sens), (vertical / 2 - sens / 2) + (int)(posY * sens));
                        }
                        else
                        {
                            SetCursorPos(X, Y);
                        }
                        break;
                    case PXCMTouchlessController.UXEventData.UXEventType.UXEvent_Select:
                        log.Text += "Found gesture 'Select'\n";
                        break;
                    case PXCMTouchlessController.UXEventData.UXEventType.UXEvent_Scroll:
                        Thread.Sleep(10);
                        log.Text += "Found gesture 'Scroll'\n";
                        if (MouseClick == true)
                        {
                            LeftUp(X, Y);
                            MouseClick = false;
                        } else if (MouseClick == false)
                        {
                            LeftDown(X, Y);
                            MouseClick = true;
                        }
                        break;
                    case PXCMTouchlessController.UXEventData.UXEventType.UXEvent_EndScroll:
                        log.Text += "Found gesture 'EndScroll'\n";
                        break;
                    default:
                        break;

                }
            }));
        }

        #endregion
       
        [System.Runtime.InteropServices.DllImport("user32.dll", SetLastError = true)]
        public static extern long SetCursorPos(int x, int y);
        [System.Runtime.InteropServices.DllImport("user32.dll", SetLastError = true)]
        public static extern void mouse_event(int dwFlags, int dx, int dy, int cButtons, int dwExtraInfo);

        private const int MOUSEEVENTF_LEFTDOWN = 0x02;
        private const int MOUSEEVENTF_LEFTUP = 0x04;
        private const int MOUSEEVENTF_RIGHTDOWN = 0x08;
        private const int MOUSEEVENTF_RIGHTUP = 0x10;

        public void LeftDown(int X, int Y)
        {
            mouse_event(MOUSEEVENTF_LEFTDOWN, X, Y, 0, 0);
            MouseClick = true;
        }
        public void LeftUp(int X, int Y)
        {
            mouse_event(MOUSEEVENTF_LEFTUP, X, Y, 0, 0);
            MouseClick = false;
        }

        public class CursorMapping
        {
            public const int VeriticalMappingContant = 1080;
            public const int HorizontalMappingConstant = 1920;
        }

        private void textBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            log.ScrollToEnd();
        }
    }
}
