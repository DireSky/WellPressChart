using LiveCharts;
using LiveCharts.Configurations;
using LiveCharts.Wpf;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Threading.Tasks;
using System.Windows.Input;

namespace WellPressChart.ViewModel
{
    internal class WindowVM : INotifyPropertyChanged
    {

        public double Density { get; set; }
        public double Height { get; set; }
        public int Iterations { get; set; }
        public double ProgressValue { get; set; }
        private string launchButtonText = "Запуск";
        public string LaunchButtonText
        {
            get => launchButtonText;
            set
            {
                launchButtonText = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(LaunchButtonText)));
            }
        }
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
        public RelayCommand AddCommand => addCommand ??
                (addCommand = new RelayCommand(obj =>
                {
                    Well well = new Well(Wells.Count + 1, Density, Height);
                    Wells.Insert(0, well);
                    SelectedWell = well;
                }));
        private RelayCommand launch;
        public RelayCommand Launch => launch ??
                (launch = new RelayCommand(obj =>
                {
                    WorkCondition = !WorkCondition;
                    if (WorkCondition)
                    {
                        StartCalculationsAsync();
                    }
                    LaunchButtonText = WorkCondition ? "Остановить" : "Запуск";
                    OnPropertyChanged(nameof(LaunchButtonText));
                }));

        private async void StartCalculationsAsync()
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
                    ProgressValue = (100 / (Wells.Count * 1.0)) * i;
                    System.Threading.Thread.Sleep(1000);
                    OnPropertyChanged(nameof(ProgressValue));
                }
            };
            LaunchButtonText = "Запуск";
        }
        public Well SelectedWell
        {
            get => selectedWell;
            set
            {
                if (selectedWell == value)
                {
                    return;
                }
                selectedWell = value;
                ChartPressure();
                OnPropertyChanged(nameof(SelectedWell));
            }
        }
        public void ChartPressure()
        {
            CartesianMapper<KeyValuePair<double, double>> mapper = Mappers.Xy<KeyValuePair<double, double>>().X(value => value.Key).Y(value => value.Value);
            SeriesViews = new SeriesCollection(mapper)
            {
                new LineSeries
                {
                    Values = new ChartValues<KeyValuePair<double, double>>(SelectedWell.PressForHeightMark)
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
        private readonly Action<object> execute;
        private readonly Func<object, bool> canExecute;

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
            return canExecute == null || canExecute(parameter);
        }
        public void Execute(object parameter)
        {
            execute(parameter);
        }
    }
}
