using System;
using System.Collections.Generic;
using System.Threading;

namespace Signal_Generator
{
    /// <summary>
    /// delegate for new signal data generated
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="data"></param>
    public delegate void OnDataGenerated(object sender, Queue<double> data);

    /// <summary>
    /// signal class which holds signal properties
    /// </summary>
    public class SignalGenerator
    {
        /// <summary>
        /// constructor to set signal properties
        /// </summary>
        /// <param name="phase"></param>
        /// <param name="amplitude"></param>
        /// <param name="frequency"></param>
        /// <param name="sampleFrequency"></param>
        /// <param name="time"></param>
        public SignalGenerator(double phase, double amplitude, double frequency, double sampleFrequency, double time)
        {
            _time = time;
            _phase = phase;
            _amplitude = amplitude;
            _frequency = frequency;
            _sampleFrequency = sampleFrequency;

            _interval = 1.0 / SampleFrequency;
            _timeStamp = 0.0;

            UpdateSignalDataQueue();

            _busy.Set();
        }

        /// <summary>
        /// copy constructor
        /// </summary>
        /// <param name="source"></param>
        public SignalGenerator(SignalGenerator source)
        {
            if (source == null) return;
            if (this == source) return;

            Time = source.Time;
            Phase = source.Phase;
            Amplitude = source.Amplitude;
            Frequency = source.Frequency;
            SampleFrequency = source.SampleFrequency;
        }

        /// <summary>
        /// start to generate signal data
        /// </summary>
        public void Start()
        {
            if (_signalData == null)
                return;
           
            _signalData.Clear();

            while (_busy.WaitOne())
            {
                _dataAvilablEvent.Reset();

                if (_timeStamp > Time) _timeStamp = 0;

                var data = Amplitude * Math.Cos(2 * Math.PI * Frequency * _timeStamp + Phase);

                if (_signalData.Count == _signalLength)
                    _signalData.Dequeue();

                _signalData.Enqueue(data);

                OnDataGenerated?.Invoke(this, _signalData);

                _timeStamp += _interval;

                Thread.Sleep(10);

                _dataAvilablEvent.Set();
            }
        }

        /// <summary>
        /// stop timer (do not generate signal data)
        /// </summary>
        public void Stop()
        {
            _dataAvilablEvent.WaitOne();
            _busy.Reset();
        }

        /// <summary>
        /// restart to generate signal data 
        /// </summary>
        private void Restart()
        {
            _signalData.Clear();

            _interval = 1.0 / SampleFrequency;
            _timeStamp = 0.0;

            _busy.Set();
        }

        /// <summary>
        /// modify size of a signal data queue
        /// </summary>
        private void UpdateSignalDataQueue()
        {
            _signalLength = (int)Math.Floor(Time * SampleFrequency);
            _signalData = new Queue<double>(_signalLength);
        }

        /// <summary>
        /// length of a signal
        /// </summary>
        private int _signalLength;

        /// <summary>
        /// signal data
        /// </summary>
        private Queue<double> _signalData;

        /// <summary>
        /// time to hold signal data
        /// </summary>
        private double _time;

        /// <summary>
        /// time property
        /// </summary>
        public double Time
        {
            get {return _time;}
            set
            {
                Stop();
                _time = value;
                UpdateSignalDataQueue();
                Restart();
            }
        }

        /// <summary>
        /// sample frequency
        /// </summary>
        private double _sampleFrequency;

        /// <summary>
        /// sample frequency property
        /// </summary>
        public double SampleFrequency
        {
            get { return _sampleFrequency; }
            set
            {
                Stop();
                _sampleFrequency = value;
                UpdateSignalDataQueue();
                Restart();
            }
        }

        /// <summary>
        /// phase
        /// </summary>
        private double _phase;

        /// <summary>
        /// phase property
        /// </summary>
        public double Phase
        {
            get {return _phase;}
            set
            {
                Stop();
                _phase = value;
                Restart();
            }
        }

        /// <summary>
        /// amplitude
        /// </summary>
        private double _amplitude;

        /// <summary>
        /// amplitude property
        /// </summary>
        public double Amplitude
        {
            get { return _amplitude; }
            set
            {
                Stop();
                _amplitude = value;
                Restart();
            }
        }

        /// <summary>
        /// frequency
        /// </summary>
        private double _frequency;

        /// <summary>
        /// frequency property
        /// </summary>
        public double Frequency
        {
            get { return _frequency; }
            set
            {
                Stop();
                _frequency = value;
                Restart();
            }
        }

        /// <summary>
        /// time interval between two samples of a signal
        /// </summary>
        private double _interval;

        /// <summary>
        /// amount of time in seconds to record signal data
        /// </summary>
        private double _timeStamp;

        /// <summary>
        /// signal data generated event
        /// </summary>
        public event OnDataGenerated OnDataGenerated;

        /// <summary>
        /// to read serial port data one by one
        /// </summary>
        private readonly ManualResetEvent _busy = new ManualResetEvent(false);

        /// <summary>
        /// auto reset evetnt for thread syncronization
        /// </summary>
        private readonly AutoResetEvent _dataAvilablEvent = new AutoResetEvent(false);
    }
}
