using System;
using System.Security;
using System.Runtime.InteropServices;
using System.ComponentModel;
using System.Reflection;
namespace FFTSSharp
{
	using static FFTSManager;
	public unsafe class FFTS
	{
		/**
		 * C pointer
		 */
		private IntPtr p;
		/**
		 * Minimum size of input
		 */
		public long inSize;
		long inByteSize;
		/**
		 * Minimum size of output
		 */
		public long outSize;
		long outByteSize;
		/*
		 * Input pointer
		 */
		IntPtr inPtr;
		/*
		 * Output pointer
		 */
		IntPtr outPtr;

		private FFTS(IntPtr p, long inSize)
		{
			this.p = p;
			this.inSize = inSize;
			this.inByteSize = alignedSize(inSize) * sizeof(float);
			this.outSize = inSize;
			this.outByteSize = alignedSize(inSize) * sizeof(float);
			inPtr = FFTSCaller.alMlc(inSize * sizeof(float), 32);
			outPtr = FFTSCaller.alMlc(inSize * sizeof(float), 32);
		}

		private FFTS(IntPtr p, long inSize, long outSize)
		{
			this.p = p;
			this.inSize = inSize;
			this.inByteSize = alignedSize(inSize) * sizeof(float);
			this.outSize = outSize;
			this.outByteSize = alignedSize(outSize) * sizeof(float);
			inPtr = FFTSCaller.alMlc(inSize * sizeof(float), 32);
			outPtr = FFTSCaller.alMlc(outSize * sizeof(float), 32);
		}
		/**
		 * The sign to use for a forward transform.
		 */
		public static int FORWARD = -1;
		/**
		 * The sign to use for a backward transform.
		 */
		public static int BACKWARD = 1;

		/**
		 * Create a FFT plan for a 1-dimensional complex transform.
		 *
		 * The src and dst parameters to execute() use complex data.
		 *
		 * @param sign The direction of the transform.
		 * @param N The size of the transform.
		 * @return
		 */
		public static FFTS complex(int sign, int N)
		{
			IntPtr plan = FFTSCaller.ffts_init_1d(N, sign);
			if (plan == IntPtr.Zero)
				throw new OutOfMemoryException();
			return new FFTS(plan, N * 2);
		}

		/**
		 * Create a FFT plan for a 2-dimensional complex transform.
		 * @param sign The direction of the transform.
		 * @param N1 The size of the transform.
		 * @param N2 The size of the transform.
		 * @return
		 */
		public static FFTS complex(int sign, int N1, int N2)
		{
			IntPtr plan = FFTSCaller.ffts_init_2d(N1, N2, sign);
			if (plan == IntPtr.Zero)
				throw new OutOfMemoryException();
			return new FFTS(plan, N1 * N2 * 2);
		}

		public static FFTS complex(int sign, params int[] Ns)
		{
			IntPtr plan = FFTSCaller.ffts_init_nd(Ns.Length, Ns, sign);
			if (plan == IntPtr.Zero)
				throw new OutOfMemoryException();
			return new FFTS(plan, size(Ns) * 2);
		}

		public static FFTS real(int sign, int N)
		{
			IntPtr plan = FFTSCaller.ffts_init_1d_real(N, sign);
			if (plan == IntPtr.Zero)
				throw new OutOfMemoryException();
			int outSize = FFTSManager.outSize(N);
			return new FFTS(plan, sign == FORWARD ? N : outSize, sign == FORWARD ? outSize : N);
        }

		public static FFTS real(int sign, int N1, int N2)
		{
			IntPtr plan = FFTSCaller.ffts_init_2d_real(N1, N2, sign);
			if (plan == IntPtr.Zero)
				throw new OutOfMemoryException();
			int outSize = FFTSManager.outSize(N1, N2);
			return new FFTS(plan, sign == FORWARD ? N1 * N2 : outSize, sign == FORWARD ? outSize : N1 * N2);
		}

		public static FFTS real(int sign, params int[] Ns)
		{
			IntPtr plan = FFTSCaller.ffts_init_nd_real(Ns.Length, Ns, sign);
			if (plan == IntPtr.Zero)
				throw new OutOfMemoryException();
			int outSize = FFTSManager.outSize(Ns);
			return new FFTS(plan, sign == FORWARD ? size(Ns) : outSize, sign == FORWARD ? outSize : size(Ns));
		}

		/**
		 * Execute this plan with the given array data.
		 *
		 * @param src
		 * @param dst
		 */
		public unsafe void execute(float[] src, float[] dst)
		{
			int srcLen = src.Length;
			int dstLen = dst.Length;
			if (srcLen < inSize || dstLen < outSize)
				throw new IndexOutOfRangeException();
			if (p == IntPtr.Zero)
				throw new NullReferenceException();
			//fixed (float* srcVoid = src)
			//	Buffer.MemoryCopy(srcVoid, inPtr, inByteSize, srcLen * sizeof(float));
            Marshal.Copy(src, 0, inPtr, srcLen);
            FFTSCaller.ffts_execute(p, inPtr, outPtr);
			//fixed (float* dstVoid = dst)
			//	Buffer.MemoryCopy(outPtr, dstVoid, outByteSize, dstLen * sizeof(float));
            Marshal.Copy(outPtr, dst, 0, dstLen);
        }

		/**
		 * Free the plan.
		 */
		public void free()
		{
			if (p == IntPtr.Zero)
				throw new NullReferenceException();
			FFTSCaller.ffts_free(p);
			FFTSCaller.alFree((IntPtr)inPtr);
			FFTSCaller.alFree((IntPtr)outPtr);
		}

		/*
		 * Calculate the number of elements required to store one
		 * set of n-dimensional data.
		 */
		protected static long size(int[] Ns)
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
		public enum InstructionType
		{
			[Description("nonsse")]
			nonSSE,
			[Description("sse")]
			SSE,
			[Description("sse2")]
			SSE2,
			[Description("sse3")]
			SSE3,			
			Auto
		}
		static string GetEnumDescription(Enum value)
		{
			// Get the Description attribute value for the enum value
			FieldInfo fi = value.GetType().GetField(value.ToString());
			DescriptionAttribute[] attributes = (DescriptionAttribute[])fi.GetCustomAttributes(typeof(DescriptionAttribute), false);

			if (attributes.Length > 0)
				return attributes[0].Description;
			else
				return value.ToString();
		}
		static byte lastByte(this int number)
        {
			return (byte)(number >> (8 * 4)); //Get the 4th byte of a 32-bit int
		}
		static bool getBit(this byte b, int index)
        {
			return (b & (1 << index)) != 0;
		}
		static FFTSNative loadLibrary(string dllName)
		{
			return DllImportXFactory.Build<FFTSNative>(entry =>
			{
				entry.DllName = dllName;
			});
		}
		public static void loadAppropriateDll(InstructionType type = InstructionType.Auto)
		{
			if (FFTSCaller != null) return;
			Console.WriteLine("loading");
			string architect = Environment.Is64BitProcess ? "x64" : "x86";

			string applicationPath = AppContext.BaseDirectory;
			string bareMinimumPath = string.Format("{0}ffts-dlls\\ffts{1}nonsse.dll", applicationPath, architect);

			if (dllFileName == "")
				FFTSCaller = loadLibrary(bareMinimumPath);
			byte instructionsState = FFTSCaller.SIMDSupport().lastByte();
            bool[] sseSupport = new bool[3] {instructionsState.getBit(0),
                instructionsState.getBit(1), instructionsState.getBit(2) };
			int sse = 0;
			foreach (bool b in sseSupport)
				sse += b ? 1 : 0;
			string inType = "";
			if (type != InstructionType.Auto && sse >= (int)type)
				inType = GetEnumDescription(type);
			else if (type != InstructionType.Auto && sse < (int)type)
				throw new InvalidEnumArgumentException("Instruction type is unsupported");
			else
				inType = GetEnumDescription((InstructionType)sse);
			Console.WriteLine("Selected Instruction Type: " + inType);
			dllFileName = string.Format("{0}ffts-dlls\\ffts{1}{2}.dll", applicationPath, architect, inType);
			FFTSCaller = loadLibrary(dllFileName);
		}
		/*
		* MUST be a multiple of sizeof(float) or 4
		*/
		static int byteAlignment = 32;

		static int floatAlignment = byteAlignment / sizeof(float);
		internal static long alignedSize(long initialSize)
        {
			long modulo = initialSize % floatAlignment;
			long c1 = initialSize - modulo;
			long c2 = initialSize + floatAlignment - modulo;
			return c1 == initialSize ? c1 : c2;
        }
		internal static float[] correctAlignment(float[] fA)
        {
			int size = fA.Length;
			float[] returnfA = new float[alignedSize(size)];
			Buffer.BlockCopy(fA, 0, returnfA, 0, size);
			return returnfA;
        }
		internal static int outSize(params int[] inSize)
        {
			int inLen = inSize.Length;
			int finLen = inSize[0];
			for (int i = 1; i < inLen - 1; i++)
				finLen *= inSize[i];
			finLen *= (inSize[inLen - 1] / 2 + 1) * 2;
			return finLen;
        }
		internal static int outSize(int inSize)
        {
			return (inSize / 2 + 1) * 2;
		}
	}
	[SuppressUnmanagedCodeSecurity]
	public unsafe interface FFTSNative
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
