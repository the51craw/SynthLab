using MathNet.Numerics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace SynthLab
{
    public class DynamicComplexArray
    {
        public int Start { set { SetStart(value); _length = _end - _start; } }
        public int End { set { SetEnd(value); _length = _end - _start; } }

        private Complex32[] _array;
        private int _length;
        private int _start;
        private int _end;

        public DynamicComplexArray(int size)
        {
            _array = new Complex32[size];
            _length = size;
            _start = 0;
            _end = _length - 1;
        }

        private void SetStart(int value)
        {
            if (value < 0 || value > Length - 1)
            {
                throw new ArgumentException("Start must be within 0 to array length - 1");
            }
            else
            {
                _start = value;
            }
        }

        private void SetEnd(int value)
        {
            if (value < 0 || value > Length - 1)
            {
                throw new ArgumentException("End must be within 0 to array length - 1");
            }
            else
            {
                _end = value;
            }
        }

        public Complex32 this[int index]
        {
            get
            {
                CheckIndex(index, _length);
                return _array[index + _start];
            }
            set
            {
                CheckIndex(index, _length);
                _array[index + _start] = value;
            }
        }

        public int Length
        {
            get { return _length; }
            set
            {
                CheckIndex(value);
                _length = value;
            }
        }

        private void CheckIndex(int index)
        {
            this.CheckIndex(index + _start, _end - _start - 1);
        }

        private void CheckIndex(int index, int maxValue)
        {
            if (index + _start < _start || index + _start > _end)
            {
                throw new ArgumentException("New array length must be positive and lower or equal to original size");
            }
        }
    }
}
