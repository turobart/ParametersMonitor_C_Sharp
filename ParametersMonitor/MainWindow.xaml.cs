using System;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Input;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.ComponentModel;
using System.Reflection;
using System.IO.Ports;
using System.Diagnostics;
using ParametersMonitor.Model;

namespace ParametersMonitor
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window, INotifyPropertyChanged
    {
        public TurboPump XXPump { get; set; }
        public ObservableCollection<MenuItemViewModel> XXMenuItems { get; set; }
        public ObservableCollection<MenuItemViewModel> YYMenuItems { get; set; }
        public ObservableCollection<MenuItemViewModel> Cryo1MenuItems { get; set; }
        public ObservableCollection<MenuItemViewModel> Cryo2MenuItems { get; set; }

        public MainWindow()
        {
            InitializeComponent();

            XXPump = new TurboPump { Header = "XX Pump: None" , PumpRow = 2, PumpName = "XX Pump"};

            XXMenuItems = new ObservableCollection<MenuItemViewModel>
                {
                    new MenuItemViewModel { Header = "None" , State = true , GrpNm = "XX_Turbo"}
                };
            YYMenuItems = new ObservableCollection<MenuItemViewModel>
                {
                    new MenuItemViewModel { Header = "None", State = true , GrpNm = "YY_Turbo" }
                };
            Cryo1MenuItems = new ObservableCollection<MenuItemViewModel>
                {
                    new MenuItemViewModel { Header = "None" , State = true , GrpNm = "C1"}
                };
            Cryo2MenuItems = new ObservableCollection<MenuItemViewModel>
                {
                    new MenuItemViewModel { Header = "None", State = true , GrpNm = "C2" }
                };

            Get_COMs();
            DataContext = this;
        }
        private void EnablePump_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = true;
        }
        private void EnablePump_Executed(object sender, ExecutedRoutedEventArgs e)
        { 
            if(XXPump.PumpEnabled == "Collapsed") { XXPump.PumpEnabled = "Visible"; }
            else{ XXPump.PumpEnabled = "Collapsed"; }
            Debug.WriteLine("dziala");
        }
        private void ConnectCOMPortsCommand_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            MenuItem pickedCOM = e.OriginalSource as MenuItem;
            if (!(pickedCOM.Header is string stringHeader) || stringHeader == "None")
            {
                RadioButton rb = pickedCOM.Icon as RadioButton;
                if (rb != null)
                {
                    e.CanExecute = !(bool)rb.IsChecked;
                }
            }
            else
            {
                e.CanExecute = true;
                pickedCOM.IsEnabled = !(XXMenuItems.Single(i => i.Header == stringHeader).State) &&
                                      !(YYMenuItems.Single(i => i.Header == stringHeader).State) &&
                                      !(Cryo1MenuItems.Single(i => i.Header == stringHeader).State) &&
                                      !(Cryo2MenuItems.Single(i => i.Header == stringHeader).State);
            }
        }

        private void ConnectCOMPortsCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            MenuItem pickedPump = sender as MenuItem;
            string pumpName = pickedPump.Header as string;
            //string stringHeader = pickedPump.Header as string;
            /*MenuItem pickedCOM = e.OriginalSource as MenuItem;
            string stringHeader = pickedCOM.Header as string;*/
            MenuItem mi = e.OriginalSource as MenuItem;
            string ComNumber = mi.Header as string;
            if (mi != null)
            {
                if (mi.Icon is RadioButton rb)
                {
                    Debug.WriteLine(mi.Header);
                    rb.IsChecked = true;
                    XXPump.Header = String.Concat(XXPump.Header.Substring(0, XXPump.Header.IndexOf(":") + 2), ComNumber);
                    //pickedPump.Header = String.Concat(pumpName.Substring(0, pumpName.IndexOf(":") + 2), ComNumber);

                    if (ComNumber == "None")
                    {
                        XXPump.StopCOMcomm.Cancel();
                        XXPump.StopCOMcomm.Dispose();
                        XXPump.COMportSerial.Close();
                        XXPump.COMportSerial.Dispose();
                        Debug.WriteLine("closed");
                    }
                    else
                    {

                        //pickedPump.Header = String.Concat(pickedPump.Name, ": ", pickedCom);
                        XXPump.COMportSerial = new SerialPort
                        {
                            PortName = ComNumber,
                            BaudRate = 9600,
                            Parity = Parity.None,
                            DataBits = 8,
                            StopBits = StopBits.One,
                            ReadTimeout = 200,
                            WriteTimeout = 50
                        };
                        XXPump.StopCOMcomm = new CancellationTokenSource();
                        try
                        {
                            if (XXPump.COMportSerial.IsOpen)
                            {
                                XXPump.COMportSerial.Close();
                            }

                            XXPump.COMportSerial.Open();
                        }
                        catch (Exception ex) { if (XXPump.COMportSerial.IsOpen) { XXPump.COMportSerial.Close(); } }
                        var dueTime = TimeSpan.FromSeconds(0);
                        var interval = TimeSpan.FromSeconds(1);

                        RunPeriodicAsync(OnTick, dueTime, interval, XXPump.StopCOMcomm.Token);
                        //XXPump.COMportSerial.Write("Q");
                        //var buffer = new byte[XXPump.COMportSerial.BytesToRead];
                        //byte[] buffer = new byte[serial.BytesToRead];
                        //var recieved_data = XXPump.COMportSerial.Read(buffer, 0, buffer.Length);
                        //string recieved_data = serial.ReadExisting();
                        //Debug.WriteLine(recieved_data);
                    }

                }
            }

            
        }
        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string propertyName = null)
        {
            PropertyChangedEventHandler handler = this.PropertyChanged;
            if (handler != null)
            {
                var e = new PropertyChangedEventArgs(propertyName);
                handler(this, e);
            }
        }
        private void OnTick()
        {
            XXPump.COMportSerial.Write("Q");
            var buffer = new byte[XXPump.COMportSerial.BytesToRead];
            //byte[] buffer = new byte[serial.BytesToRead];
            var recieved_data = XXPump.COMportSerial.Read(buffer, 0, buffer.Length);
            //string recieved_data = serial.ReadExisting();
            Debug.WriteLine(recieved_data);
        }
        private void Get_COMs()
        {
            string[] ports = SerialPort.GetPortNames();
            foreach (string port in ports)
            {
                XXMenuItems.Add(new MenuItemViewModel { Header = port, State = false , GrpNm = "XX_Turbo"});
                YYMenuItems.Add(new MenuItemViewModel { Header = port, State = false, GrpNm = "YY_Turbo" });
                Cryo1MenuItems.Add(new MenuItemViewModel { Header = port, State = false, GrpNm = "C1" });
                Cryo2MenuItems.Add(new MenuItemViewModel { Header = port, State = false, GrpNm = "C2" });
            }
        }

        private void EditName_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            TextBox tb = sender as TextBox;
            //if (tb.Name == )
            tb.Background = new SolidColorBrush(Color.FromArgb(0xFF, 0xD4, 0xD4, 0xD4));
            tb.AllowDrop = false;
            tb.Focus();
            tb.IsReadOnly = false;
        }

        private void EditName_KeyDown(object sender, KeyEventArgs e)
        {
            TextBox tb = sender as TextBox;
            if (e.Key == Key.Enter)
            {
                string currentCOM = "None";
                tb.AllowDrop = false;
                tb.IsReadOnly = true;
                tb.Background = new SolidColorBrush(Color.FromArgb(0x00, 0x00, 0x00, 0x00));
                if (XXPump.COMportSerial != null) { currentCOM = XXPump.COMportSerial.PortName; }
                XXPump.PumpName = tb.Text;
                XXPump.Header = String.Concat(tb.Text, ": ", currentCOM);
                Keyboard.ClearFocus();
            }
            //User presses ESCAPE key: cancel edit and return don't change label text
            else if (e.Key == Key.Escape)
            {
                Keyboard.ClearFocus();
            }
            else
            {
                //Key other than Enter, Tab, or Escape was pressed
            }
        }

        private void EditName_MouseUp(object sender, MouseButtonEventArgs e)
        {
            TextBox tb = sender as TextBox;
            tb.IsReadOnly = true;
        }

        // The `onTick` method will be called periodically unless cancelled.
        private static async Task RunPeriodicAsync(Action onTick,
                                                   TimeSpan dueTime,
                                                   TimeSpan interval,
                                                   CancellationToken token)
        {
            // Initial wait time before we begin the periodic loop.
            if (dueTime > TimeSpan.Zero)
                await Task.Delay(dueTime, token);

            // Repeat this loop until cancelled.
            while (!token.IsCancellationRequested)
            {
                // Call our onTick function.
                onTick?.Invoke();

                // Wait to repeat again.
                if (interval > TimeSpan.Zero)
                    await Task.Delay(interval, token);
            }
        }
    }

    public class MenuItemViewModel
    {
        private readonly ICommand _command;

        public MenuItemViewModel()
        {
            _command = new CommandViewModel(Execute);
        }
        
        public string Header { get; set; }
        public string GrpNm { get; set; }
        public bool State { get; set; }

        public bool IsCheckable { get; set; }
        public bool IsEnabled { get; set; }

        public ObservableCollection<MenuItemViewModel> MenuItems { get; set; }
        public ObservableCollection<MenuItemViewModel> XXMenuItems { get; set; }
        public ObservableCollection<MenuItemViewModel> YYMenuItems { get; set; }
        public ObservableCollection<MenuItemViewModel> Cryo1MenuItems { get; set; }
        public ObservableCollection<MenuItemViewModel> Cro2MenuItems { get; set; }

        public ICommand Command
        {
            get
            {
                return _command;
            }
        }

        private void Execute()
        {
            // (NOTE: In a view model, you normally should not use MessageBox.Show()).
            MessageBox.Show("Clicked at " + Header);
        }
    }

    public class CommandViewModel : ICommand
    {
        private readonly Action _action;

        public CommandViewModel(Action action)
        {
            _action = action;
        }

        public void Execute(object o)
        {
            _action();
        }

        public bool CanExecute(object o)
        {
            return true;
        }

        public event EventHandler CanExecuteChanged
        {
            add { }
            remove { }
        }
    }
    
    //public class COMMenuItemViewModel
   /* {
        public COMMenuItemViewModel()
        {
       
        }

        public string Header { get; set; }
        public SerialPort COMportSerial { get; set; }
        public CancellationTokenSource stopCOMcomm { get; set; }

    }*/

    public static class CustomCommands
    {
        public static readonly RoutedUICommand ConnectCOM = new RoutedUICommand("ConnectCOM", "ConnectCOM", typeof(CustomCommands));
        public static readonly RoutedUICommand EnablePump = new RoutedUICommand("EnablePump", "EnablePump", typeof(CustomCommands));

    }

}
