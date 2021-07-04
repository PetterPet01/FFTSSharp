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
 * Example showing how to call the Intel MKL BLAS dgemm()
 * function using the CBLAS interface.
 */

public class test_dgemm 
{
    private test_dgemm() {}
    public static void MainTest(string[] args) {
        /* Data initialization */
        int Order = CBLAS.ORDER.RowMajor;
        int TransA = CBLAS.TRANSPOSE.NoTrans;
        int TransB = CBLAS.TRANSPOSE.NoTrans;
        int M=2, N=4, K=3;
        int lda=K, ldb=N, ldc=N;
        double[] A = new double[] {1,2,3, 4,5,6};
        double[] B = new double[] {0,1,0,1, 1,0,0,1, 1,0,1,0};
        double[] C = new double[] {5,1,3,3, 11,4,6,9};
        double alpha=1, beta=-1;
		Console.WriteLine("MKL cblas_dgemm example");
		Console.WriteLine();
		/* Print initial data */
		Console.WriteLine("alpha=" + alpha);
        Console.WriteLine("beta=" + beta);
        printMatrix("Matrix A",A,M,K);
        printMatrix("Matrix B",B,K,N);
        printMatrix("Initial C",C,M,N);
        /* Computation */
        CBLAS.dgemm(Order, TransA, TransB, M, N, K,
            alpha, A, lda, B, ldb, beta, C, ldc);
        /* Print the result */
        printMatrix("Resulting C",C,M,N);
        Console.WriteLine("TEST PASSED");
		Console.WriteLine();
	}

    /** Print the matrix X assuming row-major order of elements. */
    private static void printMatrix(String prompt, double[] X, int I, int J) {
        Console.WriteLine(prompt);
        for (int i=0; i<I; i++) {
            for (int j=0; j<J; j++)
                Console.Write("\t" + X[i*J+j]);
            Console.WriteLine();
        }
    }
}

namespace mkl 
{
	/** CBLAS wrappers */
	public sealed class CBLAS
	{
		private CBLAS() {}

		/** Constants for CBLAS_ORDER enum, file "mkl_cblas.h" */
		public sealed class ORDER 
		{
			private ORDER() {}
			public static int RowMajor=101;  /* row-major arrays */
			public static int ColMajor=102;  /* column-major arrays */
		}

		/** Constants for CBLAS_TRANSPOSE enum, file "mkl_cblas.h" */
		public sealed class TRANSPOSE 
		{
			private TRANSPOSE() {}
			public static int NoTrans  =111; /* trans='N' */
			public static int Trans    =112; /* trans='T' */
			public static int ConjTrans=113; /* trans='C' */
		}

		/** CBLAS cblas_dgemm wrapper */
		public static void dgemm(int Order, int TransA, int TransB,
			int M, int N, int K, double alpha, double[] A, int lda,
			double[] B, int ldb, double beta, double[] C, int ldc)
		{
			CBLASNative.cblas_dgemm(Order, TransA, TransB, M, N, K,
				alpha, A, lda, B, ldb, beta, C, ldc);
		}
	}

	/** CBLAS native declarations */
	[SuppressUnmanagedCodeSecurity]
	internal sealed class CBLASNative
	{
		private CBLASNative() {}

		/** CBLAS native cblas_dgemm declaration */
		[DllImport("mkl_rt.dll", CallingConvention=CallingConvention.Cdecl,
			 ExactSpelling=true, SetLastError=false)]
		internal static extern void cblas_dgemm(
			int Order, int TransA, int TransB, int M, int N, int K,
			double alpha, [In] double[] A, int lda, [In] double[] B, int ldb,
			double beta, [In, Out] double[] C, int ldc);
	}
}
