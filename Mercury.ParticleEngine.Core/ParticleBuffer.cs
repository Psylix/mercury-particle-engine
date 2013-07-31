﻿namespace Mercury.ParticleEngine
{
    using System;
    using System.Runtime.InteropServices;

    internal unsafe class ParticleBuffer : IDisposable
    {
        private readonly Particle* _buffer;
        private int _head;

        public readonly IntPtr NativePointer;
        public readonly int Size;

        public ParticleBuffer(int size)
        {
            Size = size;

            NativePointer = Marshal.AllocHGlobal(Particle.SizeInBytes * Size);
            _buffer = (Particle*)NativePointer.ToPointer();

            GC.AddMemoryPressure(Particle.SizeInBytes * size);
        }

        public int Available
        {
            get { return Size - Count; }
        }

        public int Count { get; private set; }

        public ParticleIterator Release(int releaseQuantity)
        {
            var numToRelease = Math.Min(releaseQuantity, Available);

            var tail = (_head + Count) % Size;
            Count += numToRelease;

            return new ParticleIterator(_buffer, Size, tail, numToRelease);
        }

        public void Reclaim(int number)
        {
            _head = (_head + number) % Size;
            Count -= number;
        }

        public ParticleIterator GetIterator()
        {
            return new ParticleIterator(_buffer, Size, _head, Count);
        }

        private bool _disposed;

        public void Dispose()
        {
            if (!_disposed)
            {
                Marshal.FreeHGlobal(NativePointer);
                _disposed = true;

                GC.RemoveMemoryPressure(Particle.SizeInBytes * Size);
            }
            
            GC.SuppressFinalize(this);
        }

        ~ParticleBuffer()
        {
            Dispose();
        }
    }
}