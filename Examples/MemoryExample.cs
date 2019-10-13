using System;
using System.Collections.Generic;
using System.ComponentModel;
using Microsoft.Win32.SafeHandles;

namespace Essentials.Examples
{
    public class PlainOldData
    {
        byte[] _data;
        Dictionary<string, int> _index;

        public PlainOldData(byte[] data, Dictionary<string, int> index)
        {
            _data = data;
            _index = index;
        }

        public byte GetData(string key)
        {
            return _data[_index[key]];
        }
    }

    public class ZombieObject
    {
        IAmp _amp;

        public ZombieObject(IAmp amp)
        {
            _amp = amp;
            _amp.PropertyChanged += Amp_PropertyChanged;
        }

        void Amp_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
        }
    }

    public class LazyCleanup
    {
        SafeMemoryMappedFileHandle _file;

        ~LazyCleanup()
        {
            if (_file != null)
            {
                _file.Dispose();
                _file = null;
            }
        }
    }

    public class ZombieFree : IDisposable
    {
        IAmp _amp;

        public ZombieFree(IAmp amp)
        {
            _amp = amp;
            _amp.PropertyChanged += Amp_PropertyChanged;
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (_amp != null)
                {
                    _amp.PropertyChanged -= Amp_PropertyChanged;
                    _amp = null;
                }
            }
        }

        public void Dispose()
        {
            Dispose(true);
        }

        void Amp_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
        }
    }

    public class BetterCleanup : IDisposable
    {
        IAmp _amp;
        SafeMemoryMappedFileHandle _file;

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (_amp != null)
                {
                    _amp.PropertyChanged -= Amp_PropertyChanged;
                    _amp = null;
                }
                if (_file != null)
                {
                    _file.Dispose();
                    _file = null;
                }
            }
        }

        public void Dispose()
        {
            Dispose(true);
        }

        void Amp_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
        }
    }

    public interface IAmp : INotifyPropertyChanged
    {
        int Level { get; set; }
    }

}
