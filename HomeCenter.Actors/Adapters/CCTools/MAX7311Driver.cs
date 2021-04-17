using System;
using System.Linq;
using HomeCenter.Extensions;

namespace HomeCenter.Adapters.Common
{
    /// <summary>
    /// Driver for controlling MAX7311
    /// Value of first byte informs about message type
    /// Register 0-1=Input
    /// Register 2-3=Output
    /// Register 4-5=Inversion
    /// Register 6-7=Configuration
    /// Register 8=Timeout.
    /// </summary>
    public class MAX7311Driver
    {
        private readonly byte[] _committedState = new byte[StateSize];
        private readonly byte[] _state = new byte[StateSize];
        private const int StateSize = 2;

        /// <summary>
        /// Prepare configuration for MAX7311 device.
        /// </summary>
        /// <param name="firstPortWriteMode">When set to True first port will be used in write mode, when False it will be used for reading.</param>
        /// <param name="secondPortWriteMode">When set to True second port will be used in write mode, when False it will be used for reading.</param>
        /// <returns>Configuration bytes.</returns>
        public byte[] Configure(bool firstPortWriteMode, bool secondPortWriteMode)
        {
            return new byte[]
            {
                0x06, // Configuration mode
                (byte)(firstPortWriteMode ? 0x00 : 0xFF),
                (byte)(secondPortWriteMode ? 0x00 : 0xFF),
            };
        }

        private byte[] GetWriteTable(byte[] state)
        {
            if (state == null)
            {
                throw new ArgumentNullException(nameof(state));
            }

            if (state.Length != StateSize)
            {
                throw new ArgumentException("Length is invalid.", nameof(state));
            }

            return new byte[]
            {
                0x02,      // Output register
                state[0],  // The state of ports 0-7
                state[1],   // The state of ports 8-15
            };
        }

        public int BufferSize => _committedState.Length;

        public byte[] GetReadTable()
        {
            return new byte[]
            {
                0x00,      // Input register
            };
        }

        public bool GetState(int id)
        {
            return _committedState.GetBit(id);
        }

        public byte[] GetState()
        {
            return _committedState.ToArray();
        }

        public byte[] GenerateNewState(int pinNumber, bool state)
        {
            _state.SetBit(pinNumber, state);

            return GetWriteTable(_state);
        }

        public void AcceptNewState()
        {
            Buffer.BlockCopy(_state, 0, _committedState, 0, _state.Length);
        }

        public void RevertNewState()
        {
            Buffer.BlockCopy(_committedState, 0, _state, 0, _state.Length);
        }

        public bool TrySaveState(byte[] newState, out byte[] oldState)
        {
            if (newState.SequenceEqual(_committedState) || newState.Length == 0)
            {
                oldState = Array.Empty<byte>();
                return false;
            }

            oldState = _committedState.ToArray();

            Buffer.BlockCopy(newState, 0, _state, 0, newState.Length);
            Buffer.BlockCopy(newState, 0, _committedState, 0, newState.Length);

            return true;
        }
    }
}