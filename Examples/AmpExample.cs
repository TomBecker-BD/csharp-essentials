using System;
using System.ComponentModel;

namespace Essentials.Examples
{
    public class AmpViewModel : INotifyPropertyChanged
    {
        int _level;

        public int Level
        {
            get { return _level; }
            set
            {
                if (_level != value)
                {
                    _level = value;
                    OnPropertyChanged(nameof(Level));
                }
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    public class AmpData
    {
        public int Level;
    }

    public class AmpReadOnlyData
    {
        public readonly int Level;

        public AmpReadOnlyData(int level)
        {
            Level = level;
        }
    }

    public class AmpData2
    {
        public int Level { get; set; }
    }

    public class AmpState
    {
        public int Level { get; private set; }

        public AmpState(int level)
        {
            Level = level;
        }
    }

    public interface IAmp : INotifyPropertyChanged
    {
        int Level { get; set; }
    }

    public class Guitar : IDisposable
    {
        IAmp _amp;

        IAmp Amp
        {
            get { return _amp; }
            set
            {
                if (_amp != value)
                {
                    if (_amp != null)
                    {
                        _amp.PropertyChanged -= Amp_PropertyChanged;
                    }
                    _amp = value;
                    if (_amp != null)
                    {
                        _amp.PropertyChanged += Amp_PropertyChanged;
                    }
                }
            }
        }

        void Amp_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
        }

        static void SetLevel(Guitar guitar)
        {
            // Possible race
            const int MaxLevel = 11;
            if (guitar.Amp.Level < MaxLevel)
            {
                guitar.Amp.Level = MaxLevel;
            }
        }

        static void SetLevel2(Guitar guitar)
        {
            const int MaxLevel = 11;
            // Better
            var amp = guitar.Amp;
            if (amp.Level < MaxLevel)
            {
                amp.Level = MaxLevel;
            }
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                Amp = null;
            }
        }

        public void Dispose()
        {
            Dispose(true);
        }
    }
}
