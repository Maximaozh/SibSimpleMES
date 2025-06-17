using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SibOrder
{
    internal class RowDataViewModel
    {
        private uint[] _values;

        public RowDataViewModel(uint[] values)
        {
            _values = values ?? throw new ArgumentNullException(nameof(values));
        }

        public uint this[int index]
        {
            get => _values[index];
            set
            {
                if (_values[index] != value)
                {
                    _values[index] = value;
                    OnPropertyChanged($"Column{index}");
                }
            }
        }

        public uint[] Values => _values;

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
