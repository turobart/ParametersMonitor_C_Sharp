using System;
using System.Collections.Generic;
using System.Text;
using System.ComponentModel;
using System.Threading;
using System.IO.Ports;


namespace ParametersMonitor.Model
{
    public class TurboPump : INotifyPropertyChanged
    {
        private string header;
        private string pumpName;
        private string pumpEnabled = "Collapsed";
        public int PumpRow { get; set; }
        public string PumpName
        {
            get { return pumpName; }
            set
            {
                pumpName = value;
                OnPropertyChanged();
            }
        }
        public string Header 
        {
            get { return header; }
            set
            {
                header = value;
                OnPropertyChanged();
            }
        }

        public SerialPort COMportSerial { get; set; }
        public CancellationTokenSource StopCOMcomm { get; set; }
        
        public string PumpEnabled
        {
            get { return pumpEnabled; }
            set
            {
                pumpEnabled = value;
                OnPropertyChanged();
            }
        }
        //region INotifyPropertyChanged Members 
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
        //endregion
    }
}
