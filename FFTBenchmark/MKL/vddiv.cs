/*******************************************************************************
!   Copyright (C) 2009 Intel Corporation. All Rights Reserved.
!
!   The information and  material ("Material") provided below is  owned by Intel
!   Corporation  or its  suppliers  or  licensors, and  title  to such  Material
!   remains with Intel Corporation or  its suppliers or licensors.  The Material
!   contains proprietary  information of Intel  or its suppliers  and licensors.
!   The Material is protected by worldwide copyright laws and treaty provisions.
!   No  part  of  the  Material  may  be  used,  copied,  reproduced,  modified,
!   published, uploaded,  posted, transmitted,  distributed or disclosed  in any
!   way without Intel's prior express  written permission.  No license under any
!   patent, copyright or  other intellectual property rights in  the Material is
!   granted  to  or  conferred  upon  you,  either  expressly,  by  implication,
!   inducement,  estoppel or  otherwise.   Any license  under such  intellectual
!   property rights must be express and approved by Intel in writing.
!******************************************************************************/

using System;
using System.Security;
using System.Runtime.InteropServices;
using mkl;

/**
 * Example showing how to call the Intel MKL VML vddiv()
 * function and its callback mechanism.
 */

public class test_vddiv 
{
    private test_vddiv() {}
    public static void MainTest(string[] args) 
    {
		/* Data initialization */
		int N=3;
        double[] A = new double[] {1,2,3};
        double[] B = new double[] {2,0,2};
        double[] Y = new double[3];
        VML.vmlDelegateErrorCallBack oldcallback;
        /* Print initial data */
        printVector("Vector A",A,N);
        printVector("Vector B",B,N);
        /* Set callback function */
        VML.vmlDelegateErrorCallBack myCallBack =
            new VML.vmlDelegateErrorCallBack(resolve);
        oldcallback = VML.vmlSetErrorCallBack(myCallBack);
		/* Computation */
		VML.vddiv(N,A,B,Y);
        VML.vmlClearErrorCallBack();
		/* Print the result */
		printVector("Vector Y",Y,N);
        Console.WriteLine("TEST PASSED");
		Console.WriteLine();
	}

    /** Managed callback function */
    public static int resolve(ref VML.VmlErrorContext err_struct)
    {
        int code    = err_struct.iCode;
        int index   = err_struct.iIndex;
        double arg1 = err_struct.dbA1;
        double arg2 = err_struct.dbA2;
        double res1 = err_struct.dbR1;
        double res2 = err_struct.dbR2;
        err_struct.dbR1 = 777;
        Console.WriteLine("CallBack, in function: " + err_struct.cFuncName +
            " errcode=" + code + " index=" + index +
            " arg1=" + arg1 + " arg2=" + arg2 + " res=" + res1);
        Console.WriteLine("CallBack, result correction: " + err_struct.dbR1);
        return 0;
    }

    /** Print vector X */
    private static void printVector(string prompt, double[] X, int N) 
    {
        Console.WriteLine(prompt);
        for (int n=0; n<N; n++)
            Console.Write("\t" + X[n]);
        Console.WriteLine();
    }
}

namespace mkl 
{
	/** VML wrappers */
	public sealed class VML
	{
		private VML() {}

		/** VML vdDiv wrapper */
		public static void vddiv(int N, double[] A, double[] B, double[] Y)
		{
			VMLNative.vdDiv(N, A, B, Y);
		}
		[StructLayout(LayoutKind.Sequential,CharSet=CharSet.Ansi)]
		public struct VmlErrorContext     
		{
			public int iCode;   /* Error status value */
			public int iIndex;  /* Index for bad array element,
                                 * or bad array dimension,
                                 *  or bad array pointer */
			public double dbA1; /* Error argument 1 */
			public double dbA2; /* Error argument 2 */
			public double dbR1; /* Error result 1 */
			public double dbR2; /* Error result 2 */
			[MarshalAs(UnmanagedType.ByValTStr,SizeConst=64)]
			public String cFuncName; /* Function name */
			int iFuncNameLen;   /* Length of functionname*/
		}
		/** VML callback */
		/** Attribute UnmanagedFunctionPointer requires .NET Framework >=2.0 */
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		public delegate int
		vmlDelegateErrorCallBack(ref VmlErrorContext handle);
		/** VML vmlSetErrorCallBack wrapper */
		public static vmlDelegateErrorCallBack
		vmlSetErrorCallBack(vmlDelegateErrorCallBack handle)
		{
			return VMLNative.vmlSetErrorCallBack(handle);
		}
		/** VML vmlClearErrorCallBack wrapper */
		public static vmlDelegateErrorCallBack vmlClearErrorCallBack()
		{
			return VMLNative.vmlClearErrorCallBack();
		}
	}

	/** VML native declarations */
	[SuppressUnmanagedCodeSecurity]
	internal sealed class VMLNative
	{
		private VMLNative() {}

		/** VML vdDiv native declaration */
		[DllImport("mkl_rt.dll", CallingConvention=CallingConvention.Cdecl,
			 ExactSpelling=true, SetLastError=false)]
		internal static extern int vdDiv(int N,
			[In] double[] A, [In] double[] B, [Out] double[] Y);

		/** VML Callback */
		/** VML native vmlSetErrorCallBack declaration */
		[DllImport("mkl_rt.dll", CallingConvention=CallingConvention.Cdecl,
			 ExactSpelling=true, SetLastError=false)]
		internal static extern VML.vmlDelegateErrorCallBack
			vmlSetErrorCallBack(VML.vmlDelegateErrorCallBack handle);
		/** VML native vmlClearErrorCallBack declaration */
		[DllImport("mkl_rt.dll", CallingConvention=CallingConvention.Cdecl,
			 ExactSpelling=true, SetLastError=false)]
		internal static extern VML.vmlDelegateErrorCallBack
			vmlClearErrorCallBack();
	}
}
