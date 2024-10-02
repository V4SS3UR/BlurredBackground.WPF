using TestApplication.Core;

namespace TestApplication.MVVM.ViewModel
{
    internal class MainInterface_ViewModel : ObservableObject
    {
        public static MainInterface_ViewModel Instance;


        private bool _isBlurEnabled; public bool IsBlurEnabled
        {
            get { return _isBlurEnabled; }
            set { _isBlurEnabled = value; OnPropertyChanged(); }
        }

        private double _blurRadius; public double BlurRadius
        {
            get { return _blurRadius; }
            set { _blurRadius = value; OnPropertyChanged(); }
        }

        private double _merging; public double Merging
        {
            get { return _merging; }
            set { _merging = value; OnPropertyChanged(); }
        }

        private int _dpi; public int DPI
        {
            get { return _dpi; }
            set { _dpi = value; OnPropertyChanged(); }
        }


        private object _currentView; public object CurrentView
        {
            get { return _currentView; }
            set { _currentView = value; OnPropertyChanged(); }
        }




        public MainInterface_ViewModel()
        {
            Instance = this;

            // Set default values
            IsBlurEnabled = true;
            BlurRadius = 20.0;
            Merging = 0.9;
            DPI = 30;
        }

    }
}
