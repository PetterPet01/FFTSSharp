using System;
using System.Runtime.InteropServices;
using System.IO;
namespace PetterPet.FFTSSharp
{
	using static FFTSManager;
	public class FFTS : IDisposable
	{
		#region Dispose Implementation
		bool disposed;

		protected virtual void Dispose(bool disposing)
		{
			if (!disposed)
			{
				if (disposing)
				{
					//dispose managed resources
				}
			}
			//dispose unmanaged resources
			Free();
			disposed = true;
		}

		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}
		#endregion
		/**
		 * C pointer
		 */
		private IntPtr p;
		/**
		 * Minimum size of input
		 */
		public long inSize { get; private set; }
		long inByteSize;
		/**
		 * Minimum size of output
		 */
		public long outSize { get; private set; }
		long outByteSize;
		/*
		 * Input pointer
		 */
		IntPtr inPtr;
		/*
		 * Output pointer
		 */
		IntPtr outPtr;

		static void FatalIfNotLoaded()
		{
			if (!FFTSManager.loaded)
				throw CreateException("Native FFTS library not yet loaded." +
					" Please call FFTSManager.LoadAppropriateDll() before any activities.");
		}

		private FFTS(IntPtr p, long inSize)
		{
			this.p = p;
			this.inSize = inSize;
			this.inByteSize = AlignedSize(inSize) * sizeof(float);
			this.outSize = inSize;
			this.outByteSize = AlignedSize(inSize) * sizeof(float);
			inPtr = FFTSCaller.alMlc(inSize * sizeof(float), 32);
			outPtr = FFTSCaller.alMlc(inSize * sizeof(float), 32);
		}

		private FFTS(IntPtr p, long inSize, long outSize)
		{
			this.p = p;
			this.inSize = inSize;
			this.inByteSize = AlignedSize(inSize) * sizeof(float);
			this.outSize = outSize;
			this.outByteSize = AlignedSize(outSize) * sizeof(float);
			inPtr = FFTSCaller.alMlc(inSize * sizeof(float), 32);
			outPtr = FFTSCaller.alMlc(outSize * sizeof(float), 32);
		}
		/**
		 * The sign to use for a forward transform.
		 */
		public static readonly int Forward = -1;
		/**
		 * The sign to use for a backward transform.
		 */
		public static readonly int Backward = 1;

		/**
		 * Create a FFT plan for a 1-dimensional complex transform.
		 *
		 * The src and dst parameters to execute() use complex data.
		 *
		 * @param sign The direction of the transform.
		 * @param N The size of the transform.
		 * @return
		 */
		public static FFTS Complex(int sign, int N)
		{
			FatalIfNotLoaded();
			IntPtr plan = FFTSCaller.ffts_init_1d(N, sign);
			if (plan == IntPtr.Zero)
				throw CreateException("Invalid plan initilized.");
			return new FFTS(plan, N * 2);
		}

		/**
		 * Create a FFT plan for a 2-dimensional complex transform.
		 * @param sign The direction of the transform.
		 * @param N1 The size of the transform.
		 * @param N2 The size of the transform.
		 * @return
		 */
		public static FFTS Complex(int sign, int N1, int N2)
		{
			FatalIfNotLoaded();
			IntPtr plan = FFTSCaller.ffts_init_2d(N1, N2, sign);
			if (plan == IntPtr.Zero)
				throw CreateException("Invalid plan initilized.");
			return new FFTS(plan, N1 * N2 * 2);
		}

		public static FFTS Complex(int sign, params int[] Ns)
		{
			FatalIfNotLoaded();
			IntPtr plan = FFTSCaller.ffts_init_nd(Ns.Length, Ns, sign);
			if (plan == IntPtr.Zero)
				throw CreateException("Invalid plan initilized.");
			return new FFTS(plan, Size(Ns) * 2);
		}

		public static FFTS Real(int sign, int N)
		{
			FatalIfNotLoaded();
			IntPtr plan = FFTSCaller.ffts_init_1d_real(N, sign);
			if (plan == IntPtr.Zero)
				throw CreateException("Invalid plan initilized.");
			int outSize = FFTSManager.OutSize(N);
			return new FFTS(plan, sign == Forward ? N : outSize, sign == Forward ? outSize : N);
		}

		public static FFTS Real(int sign, int N1, int N2)
		{
			FatalIfNotLoaded();
			IntPtr plan = FFTSCaller.ffts_init_2d_real(N1, N2, sign);
			if (plan == IntPtr.Zero)
				throw CreateException("Invalid plan initilized.");
			int outSize = FFTSManager.OutSize(N1, N2);
			return new FFTS(plan, sign == Forward ? N1 * N2 : outSize, sign == Forward ? outSize : N1 * N2);
		}

		public static FFTS Real(int sign, params int[] Ns)
		{
			FatalIfNotLoaded();
			IntPtr plan = FFTSCaller.ffts_init_nd_real(Ns.Length, Ns, sign);
			if (plan == IntPtr.Zero)
				throw CreateException("Invalid plan initilized.");
			int outSize = FFTSManager.OutSize(Ns);
			return new FFTS(plan, sign == Forward ? Size(Ns) : outSize, sign == Forward ? outSize : Size(Ns));
		}

		/**
		 * Execute this plan with the given array data.
		 *
		 * @param src
		 * @param dst
		 */
		public void Execute(float[] src, float[] dst)
		{
			int srcLen = src.Length;
			int dstLen = dst.Length;
			if (srcLen < inSize || dstLen < outSize)
				throw CreateException("Input or output size of the params' array is invalid.");
			if (p == IntPtr.Zero)
				throw CreateException("Plan uninitialized.");
			//unsafe
			//{
			//fixed (float* srcVoid = src)
			//	Buffer.MemoryCopy(srcVoid, (void*)inPtr, inByteSize, srcLen * sizeof(float));
			Marshal.Copy(src, 0, inPtr, srcLen);
			FFTSCaller.ffts_execute(p, inPtr, outPtr);
			//fixed (float* dstVoid = dst)
			//Buffer.MemoryCopy((void*)outPtr, dstVoid, outByteSize, dstLen * sizeof(float));
			Marshal.Copy(outPtr, dst, 0, dstLen);
			//}
		}

		/**
		 * Free the plan.
		 */
		private void Free()
		{
			if (p == IntPtr.Zero)
				throw CreateException("Plan uninitialized.");
			FFTSCaller.ffts_free(p);
			FFTSCaller.alFree(inPtr);
			FFTSCaller.alFree(outPtr);
		}

		/*
		 * Calculate the number of elements required to store one
		 * set of n-dimensional data.
		 */
		protected static long Size(int[] Ns)
		{
			long s = Ns[0];
			for (int i = 1; i < Ns.Length; i++)
				s *= Ns[i];
			return s;
		}
	}
	public static class FFTSManager
	{
		static string dllFileName = "";
		internal static FFTSNative FFTSCaller;
		internal static bool loaded = false;

		/*
		 * 0 --- nonsse 
		 * 1 --- sse
		 * 2 --- sse2
		 * 3 --- sse3
		 * 4 --- auto
		*/
		static readonly string[] tagsEquivalent = new string[] { "nonsse", "sse", "sse2", "sse3" };
		public enum InstructionType
		{
			nonSSE = 0,
			SSE = 1,
			SSE2 = 2,
			SSE3 = 3,
			Auto = 4
		}
		static byte LastByte(int number)
		{
			return (byte)(number >> (8 * 4)); //Get the 4th byte of a 32-bit int
		}
		static bool GetBit(byte b, int index)
		{
			return (b & (1 << index)) != 0;
		}
		static FFTSNative LoadLibrary(string dllName)
		{
			return DllImportXFactory.Build<FFTSNative>(entry =>
			{
				entry.DllName = dllName;
			});
		}
		public static void LoadAppropriateDll(InstructionType type = InstructionType.Auto)
		{
			if (FFTSCaller != null) return;
			//Console.WriteLine("loading");
			string architect = IntPtr.Size == 8 ? "x64" : "x86";
			string bareMinimumPath = string.Format("ffts-dlls\\ffts{0}nonsse.dll", architect);

			if (dllFileName == "")
				if (File.Exists(bareMinimumPath))
					FFTSCaller = LoadLibrary(bareMinimumPath);
				else
					throw CreateException("DLLs not found");
			byte instructionsState = LastByte(FFTSCaller.SIMDSupport());
			bool[] sseSupport = new bool[3] { GetBit(instructionsState, 0),
				GetBit(instructionsState, 1), GetBit(instructionsState, 2) };
			int sse = 0;
			foreach (bool b in sseSupport)
				sse += b ? 1 : 0;
			string inType = "";
			int typeIndex = (int)type;
			if (type != InstructionType.Auto && sse >= typeIndex)
				inType = tagsEquivalent[typeIndex];
			else if (type != InstructionType.Auto && sse < typeIndex)
				throw CreateException("Instruction type is unsupported.");
			else
				inType = tagsEquivalent[sse];
			//Console.WriteLine("Selected Instruction Type: " + inType);
			dllFileName = string.Format("ffts-dlls\\ffts{0}{1}.dll", architect, inType);
			FFTSCaller = LoadLibrary(dllFileName);
			loaded = true;
		}
		/*
		* MUST be a multiple of sizeof(float) or 4
		*/
		static int byteAlignment = 32;

		static int floatAlignment = byteAlignment / sizeof(float);
		internal static long AlignedSize(long initialSize)
		{
			long modulo = initialSize % floatAlignment;
			long c1 = initialSize - modulo;
			long c2 = initialSize + floatAlignment - modulo;
			return c1 == initialSize ? c1 : c2;
		}
		internal static float[] CorrectAlignment(float[] fA)
		{
			int size = fA.Length;
			float[] returnfA = new float[AlignedSize(size)];
			Buffer.BlockCopy(fA, 0, returnfA, 0, size);
			return returnfA;
		}
		internal static int OutSize(params int[] inSize)
		{
			int inLen = inSize.Length;
			int finLen = inSize[0];
			for (int i = 1; i < inLen - 1; i++)
				finLen *= inSize[i];
			finLen *= (inSize[inLen - 1] / 2 + 1) * 2;
			return finLen;
		}
		internal static int OutSize(int inSize)
		{
			return (inSize / 2 + 1) * 2;
		}
		//Throwhelper
		internal static FFTSException CreateException(string message)
		{
			return new FFTSException(message);
		}
	}
	internal class FFTSException : Exception
	{
		public FFTSException()
		{
		}

		public FFTSException(string name)
			: base(string.Format("An exception ocurred on FFTSSharp: {0}", name))
		{
		}
	}

	public interface FFTSNative
	{
		[DllImportX("ffts", CallingConvention = CallingConvention.Cdecl,
			 ExactSpelling = true, SetLastError = false)]
		IntPtr ffts_init_1d(int N, int sign);

		[DllImportX("ffts", CallingConvention = CallingConvention.Cdecl,
			 ExactSpelling = true, SetLastError = false)]
		IntPtr ffts_init_2d(int N1, int N2, int sign);

		[DllImportX("ffts", CallingConvention = CallingConvention.Cdecl,
			 ExactSpelling = true, SetLastError = false)]
		IntPtr ffts_init_nd(int n, int[] Ns, int sign);

		[DllImportX("ffts", CallingConvention = CallingConvention.Cdecl,
			 ExactSpelling = true, SetLastError = false)]
		IntPtr ffts_init_1d_real(int N, int sign);

		[DllImportX("ffts", CallingConvention = CallingConvention.Cdecl,
			 ExactSpelling = true, SetLastError = false)]
		IntPtr ffts_init_2d_real(int N1, int N2, int sign);

		[DllImportX("ffts", CallingConvention = CallingConvention.Cdecl,
			 ExactSpelling = true, SetLastError = false)]
		IntPtr ffts_init_nd_real(int n, int[] Ns, int sign);

		[DllImportX("ffts", CallingConvention = CallingConvention.Cdecl,
			 ExactSpelling = true, SetLastError = false)]
		void ffts_execute([In] IntPtr p, [In] IntPtr src, [Out] IntPtr dst);

		[DllImportX("ffts", CallingConvention = CallingConvention.Cdecl,
			 ExactSpelling = true, SetLastError = false)]
		void ffts_free(IntPtr p);

		[DllImportX("ffts", CallingConvention = CallingConvention.Cdecl,
			 ExactSpelling = true, SetLastError = false)]
		/*Added Support For C# Work*/
		IntPtr alMlc(long size, int alignment);

		[DllImportX("ffts", CallingConvention = CallingConvention.Cdecl,
			 ExactSpelling = true, SetLastError = false)]
		IntPtr alFree(IntPtr obj);

		[DllImportX("ffts", CallingConvention = CallingConvention.Cdecl,
			 ExactSpelling = true, SetLastError = false)]
		//0, sseSupportted)
		//1, sse2Supportted)
		//2, sse3Supportted)
		//3, ssse3Supportted)
		//4, sse4_1Supportted)
		//5, sse4_2Supportted)
		//6, sse4aSupportted)
		//7, avxSupportted)
		int SIMDSupport();

		[DllImportX("ffts", CallingConvention = CallingConvention.Cdecl,
			 ExactSpelling = true, SetLastError = false)]
		void sse_cmul(IntPtr in1, IntPtr in2, IntPtr output);

		[DllImportX("ffts", CallingConvention = CallingConvention.Cdecl,
			 ExactSpelling = true, SetLastError = false)]
		void sse_bulk_cmul(IntPtr in1, IntPtr in2, IntPtr output, int start, int end);
	}
}
