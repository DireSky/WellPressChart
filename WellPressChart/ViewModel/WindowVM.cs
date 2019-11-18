using System;
using System.Collections;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.CompilerServices;
using System.ComponentModel;
using System.Windows.Input;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using LiveCharts;
using LiveCharts.Wpf;
using LiveCharts.Configurations;
using LiveCharts.Defaults;

namespace WellPressChart.ViewModel
{
    class WindowVM : INotifyPropertyChanged
    {

        public double Density { get; set; }
        public double Height { get; set; }
        public int Iterations { get; set; }
        public double ProgressValue { get; set; }
        public string LaunchButtonText { get; set; } = "Запуск";
        public bool WorkCondition { get; set; } = false;
        private Well selectedWell = new Well(1, 1.0, 1.0);
        public ObservableCollection<Well> Wells { get; set; }
        public SeriesCollection SeriesViews { get; set; }
        public WindowVM()
        {
            ChartPressure();
            Wells = new ObservableCollection<Well>
            {

            };
        }   
        private RelayCommand addCommand;
        public RelayCommand AddCommand
        {
            get
            {
                return addCommand ??
                (addCommand = new RelayCommand(obj =>
                {
                    Well well = new Well(Wells.Count + 1, Density, Height);
                    Wells.Insert(0, well);
                    SelectedWell = well;
                }));
            }
        }
        private RelayCommand launch;
        public RelayCommand Launch
        {
            get
            {
                return launch ??
                (launch = new RelayCommand(obj =>
                {
                    StartCalculationsAsync();
                    WorkCondition = !WorkCondition;
                    LaunchButtonText = WorkCondition ? "Запуск" : "Остановить";
                    OnPropertyChanged(nameof(LaunchButtonText));
                }));
            }
        }
        async void StartCalculationsAsync()
        {
            await Task.Run(() => Counting());
        }
        public void Counting()
        {
            ProgressValue = 0;
            int i = 0;
            foreach (Well well in Wells)
            {
                if (WorkCondition)
                {
                    well.PressureCalculation(Iterations);
                    ++i;
                    ProgressValue = (100 / Wells.Count) * i;
                    System.Threading.Thread.Sleep(1000);
                    OnPropertyChanged(nameof(ProgressValue));
                }
                else return;
            }
        }
        public Well SelectedWell
        {
            get { return selectedWell; }
            set
            {
                if (selectedWell == value) return;
                selectedWell = value;
                ChartPressure();
                OnPropertyChanged(nameof(SelectedWell));
            }
        }
        public void ChartPressure()
        {
            var mapper = Mappers.Xy<double>()
                .X((value, index) => value) //use the value as X
                .Y((value, index) => index); //use the index as Y
            SeriesViews = new SeriesCollection
            {
                new LineSeries
                { 
                    //Values = new ChartValues<double>{1, 2, 5, 4}
                    Values = new ChartValues<double>(SelectedWell.PressForHeightMark.Values)
                }
            };
            OnPropertyChanged(nameof(SeriesViews)); 
        }
        public event PropertyChangedEventHandler PropertyChanged;
        public void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
    public class RelayCommand : ICommand
    {
        private Action<object> execute;
        private Func<object, bool> canExecute;

        public event EventHandler CanExecuteChanged
        {
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }

        public RelayCommand(Action<object> execute, Func<object, bool> canExecute = null)
        {
            this.execute = execute;
            this.canExecute = canExecute;
        }

        public bool CanExecute(object parameter)
        {
            return this.canExecute == null || this.canExecute(parameter);
        }
        public void Execute(object parameter)
        {
            this.execute(parameter);
        }
    }
}
