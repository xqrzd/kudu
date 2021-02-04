#if NETCOREAPP3_1 || NET5_0
using System;
using System.Runtime.CompilerServices;
using System.Runtime.Intrinsics;
using System.Runtime.Intrinsics.X86;

namespace Knet.Kudu.Client.Tablet
{
    public static partial class KeyEncoder
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int EncodeBinary(
            ReadOnlySpan<byte> source, Span<byte> destination)
        {
            if (Sse41.IsSupported)
            {
                return EncodeBinarySse(source, destination);
            }

            return EncodeBinarySlow(source, destination);
        }

        private static unsafe int EncodeBinarySse(
            ReadOnlySpan<byte> source, Span<byte> destination)
        {
            var length = (uint)source.Length;

            if ((uint)destination.Length < length * 2)
                ThrowException();

            fixed (byte* src = source)
            fixed (byte* dest = destination)
            {
                var srcCurrent = src;
                var destCurrent = dest;

                var remainder = length % 16;
                var lastBlockIndex = length - remainder;
                var blockEnd = src + lastBlockIndex;
                var end = src + length;

                while (srcCurrent < blockEnd)
                {
                    var data = Sse2.LoadVector128(srcCurrent);
                    var zeros = Vector128<byte>.Zero;

                    var zeroBytes = Sse2.CompareEqual(data, zeros);
                    bool allZeros = Sse41.TestZ(zeroBytes, zeroBytes);

                    if (allZeros)
                        Sse2.Store(destCurrent, data);
                    else
                        break;

                    srcCurrent += 16;
                    destCurrent += 16;
                }

                while (srcCurrent < end)
                {
                    byte value = *srcCurrent++;
                    if (value == 0)
                    {
                        *destCurrent++ = 0;
                        *destCurrent++ = 1;
                    }
                    else
                    {
                        *destCurrent++ = value;
                    }
                }

                var written = destCurrent - dest;
                return (int)written;
            }
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        private static void ThrowException() =>
            throw new Exception("Destination must be at least double source");
    }
}
#endif
