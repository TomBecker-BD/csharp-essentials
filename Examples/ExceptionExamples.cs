using System;
using System.IO;

namespace Essentials.Examples.Except
{
    public class Amp : IDisposable
    {
        public int Level
        {
            get;
            set;
        }

        public void Dispose()
        {
            throw new NotImplementedException();
        }
    }

    public class Effect : IDisposable
    {
        string _name;

        public Effect(string name)
        {
            _name = name;
        }

        public void Dispose()
        {
            throw new NotImplementedException();
        }
    }
}

namespace Essentials.Examples.Except.BAD
{
    public class Guitar : IDisposable
    {
        Amp _amp;
        Effect _effect;

        public Guitar()
        {
        }

        public Guitar(string effectName, int level) // BAD
        {
            _effect = new Effect(effectName);
            _amp = new Amp() { Level = level };
        }

        public void Config(string path) // BAD
        {
            var reader = File.OpenText(path);
            _effect = new Effect(reader.ReadLine());
            _amp = new Amp()
            {
                Level = int.Parse(reader.ReadLine())
            };
            reader.Close();
        }

        public void ConnectAmp()
        {
            _amp?.Dispose();
            _amp = new Amp();
            _amp.Level = 11;
        }

        public void Dispose()
        {
            _amp?.Dispose();
            _amp = null;
            _effect?.Dispose();
            _effect = null;
        }
    }
}

namespace Essentials.Examples.Except.BASIC
{
    public class Guitar : IDisposable
    {
        Amp _amp;
        Effect _effect;

        public Guitar()
        {
        }

        public Guitar(int level, string effectName) // BASIC
        {
            try
            {
                _amp = new Amp();
                _amp.Level = level;
                _effect = new Effect(effectName);
            }
            catch
            {
                _amp?.Dispose();
                _effect?.Dispose();
                throw;
            }
        }

        public void Config(string path) // BASIC
        {
            _amp?.Dispose();
            _effect?.Dispose();
            using (var reader = File.OpenText(path))
            {
                _amp = new Amp();
                _amp.Level = int.Parse(reader.ReadLine());
                _effect = new Effect(reader.ReadLine());
            }
        }

        public void ConnectAmp() // BASIC
        {
            _amp?.Dispose();
            _amp = new Amp();
            _amp.Level = 11;
        }

        public void Dispose()
        {
            _amp?.Dispose();
            _amp = null;
            _effect?.Dispose();
            _effect = null;
        }
    }
}

namespace Essentials.Examples.Except.STRONG
{
    public class Guitar : IDisposable
    {
        Amp _amp;
        Effect _effect;

        public Guitar()
        {
        }

        public Guitar(int level, string effectName) // STRONG
        {
            try
            {
                _amp = new Amp();
                _amp.Level = level;
                _effect = new Effect(effectName);
            }
            catch
            {
                _amp?.Dispose();
                _effect?.Dispose();
                throw;
            }
        }

        public void Config(string path) // STRONG
        {
            Amp tempAmp = null;
            Effect tempEffect = null;
            try
            {
                using (var reader = File.OpenText(path))
                {
                    tempAmp = new Amp();
                    tempAmp.Level = int.Parse(reader.ReadLine());
                    tempEffect = new Effect(reader.ReadLine());
                }
            }
            catch
            {
                tempAmp?.Dispose();
                tempEffect?.Dispose();
                throw;
            }
            _amp?.Dispose();
            _effect?.Dispose();
            _amp = tempAmp;
            _effect = tempEffect;
        }

        public void ConnectAmp() // STRONG
        {
            Amp tempAmp = new Amp();
            try
            {
                tempAmp.Level = 11;
            }
            catch
            {
                tempAmp.Dispose();
                throw;
            }
            _amp?.Dispose();
            _amp = tempAmp;
        }

        public void Dispose()
        {
            _amp?.Dispose();
            _amp = null;
            _effect?.Dispose();
            _effect = null;
        }
    }
}

namespace Essentials.Examples.Except.NEUTRAL
{
    public class Guitar : IDisposable
    {
        Amp _amp;
        Effect _effect;

        public Guitar()
        {
        }

        public Guitar(int level, string effectName) // STRONG
        {
            try
            {
                _amp = new Amp();
                _amp.Level = level;
                _effect = new Effect(effectName);
            }
            catch
            {
                _amp?.Dispose();
                _effect?.Dispose();
                throw;
            }
        }

        public void Config(string path) // STRONG
        {
            Amp tempAmp = null;
            Effect tempEffect = null;
            try
            {
                using (var reader = File.OpenText(path))
                {
                    tempAmp = new Amp();
                    tempAmp.Level = int.Parse(reader.ReadLine());
                    tempEffect = new Effect(reader.ReadLine());
                }

                Util.Swap(ref _amp, ref tempAmp);
                Util.Swap(ref _effect, ref tempEffect);
            }
            finally
            {
                tempAmp?.Dispose();
                tempEffect?.Dispose();
            }
        }

        public void ConnectAmp() // STRONG
        {
            Amp tempAmp = new Amp();
            try
            {
                tempAmp.Level = 11;
                Util.Swap(ref _amp, ref tempAmp);
            }
            finally
            {
                tempAmp.Dispose();
            }
        }

        public void Dispose()
        {
            _amp?.Dispose();
            _amp = null;
            _effect?.Dispose();
            _effect = null;
        }

        public static void Swap(Guitar a, Guitar b) // NOTHROW
        {
            Util.Swap(ref a._amp, ref b._amp);
            Util.Swap(ref a._effect, ref b._effect);
        }

        public void Template()
        {
            // 1. Create new state objects. 
            try
            {
                // 2. Modify new state objects (may throw). 
                // 3. Swap new and old state objects (no-throw).
            }
            finally
            {
                // 4. Delete unused state objects.
            }
        }
    }
}
