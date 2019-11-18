using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.CompilerServices;
using System.ComponentModel;


namespace WellPressChart
{
    class Well : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        public void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private double density;
        private double height;
        public int ID { get; set; }
        public double Density
        {
            get
            {
                return density;
            }
            set
            {
                density = value;
                OnPropertyChanged(nameof(Density));
            }
        }
        public double Height
        {
            get
            {
                return height;
            }
            set
            {
                height = value;
                OnPropertyChanged(nameof(Height));
            }
        }
        private const double G = 9.81;
        public SortedList<double, double> PressForHeightMark = new SortedList<double, double>();
        //internal List<double> PressForHeightMark = new List<double>();
        public void PressureCalculation(int iterations)
        {
            PressForHeightMark.Clear();
            for (int i = 0; i < iterations; i++) PressForHeightMark.Add(((Height * i)/iterations), (Density * ((Height * i) / iterations) * G));
        }
        public Well(int ID, double Density, double Height) { this.ID = ID; this.Density = Density; this.Height = Height; }
    }
}
