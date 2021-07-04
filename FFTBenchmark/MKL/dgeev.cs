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
 * Example showing how to call the Intel MKL LAPCK dgeev() function.
 */

public class dgeev 
{
    private dgeev() {}
    public static void MainTest(String[] args) {
        int n = 5;
        int info;
        double[] A = new double[] { -2, -5,  1, -1, -3,
                                   5,  0, -4, -1,  9,
                                   8, -1,  1,  6,  0,
                                   2,  0,  3,  1, -2,
                                  -4, -3,  4, -4, -2, };
        double[] Wr = new double[5];
        double[] Wi = new double[5];
		/* Print the parameters */
		Console.WriteLine("MKL LAPACK dgeev example");
		Console.WriteLine();
        printMatrix("Matrix A:",A,n,n);
        /* Compute */
        info = LAPACK.dgeev(LAPACK.JOB.JobN,LAPACK.JOB.JobN,n,A,n,Wr,Wi,null,1,null,1);
        /* Print the result */
        printMatrix("Matrix A on exit:",A,n,n);
        printVector("Wr on exit:",Wr,n);
        printVector("Wi on exit:",Wi,n);
        Console.WriteLine("info on exit: " + info);
        Console.WriteLine("TEST PASSED");
		Console.WriteLine();
	}

    /** Print the matrix X assuming row-major order of elements. */
    private static void printMatrix(String prompt, double[] X, int I, int J) 
    {
        Console.WriteLine(prompt);
        for (int i=0; i<I; i++) 
        {
            for (int j=0; j<J; j++)
                Console.Write("\t" + X[i*J+j]);
            Console.WriteLine();
        }
    }
    /** Print the vector X */
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
	public sealed class LAPACK
	{
		private LAPACK() {}

		/* Constants for LAPACK_JOB */
		public sealed class JOB
		{
			private JOB() {}
			public static int JobN=201;      /* job ='N' */
			public static int JobV=202;      /* job ='V' */
			public static int JobA=203;      /* job ='A' */
			public static int JobS=204;      /* job ='S' */
			public static int JobO=205;      /* job ='O' */
		};

		/* LAPACK dgeev wrapper */
		public static int dgeev(int jobvl, int jobvr, int n,
			double[] a, int lda, double[] wr, double[] wi,
			double[] vl, int ldvl, double[] vr, int ldvr)
		{
			int lwork = -1, info = 0;
			double[] work1 = new double[1];
			double[] work;
			char c_jobvl, c_jobvr;
			if(jobvl == LAPACK.JOB.JobN) 
			{
				c_jobvl = 'n';
			} 
			else if(jobvl == LAPACK.JOB.JobV) 
			{
				c_jobvl = 'v';
			} 
			else 
			{
				return -1;
			}
			if(jobvr == LAPACK.JOB.JobN) 
			{
				c_jobvr = 'n';
			} 
			else if(jobvr == LAPACK.JOB.JobV) 
			{
				c_jobvr = 'v';
			} 
			else 
			{
				return -2;
			}
			LAPACKNative.dgeev(ref c_jobvl, ref c_jobvr, ref n,
				a, ref lda, wr, wi, vl, ref ldvl, vr, ref ldvr,
				work1, ref lwork, ref info);
			if(info != 0) 
			{
				return info;
			}
			lwork = (int)work1[0];
			work = new double[lwork];
			LAPACKNative.dgeev(ref c_jobvl, ref c_jobvr, ref n,
				a, ref lda, wr, wi, vl, ref ldvl, vr, ref ldvr,
				work, ref lwork, ref info);

			return info;
		}
	}

	/** LAPACK native declarations */
	[SuppressUnmanagedCodeSecurity]
	internal sealed class LAPACKNative
	{
		private LAPACKNative() {}

		[DllImport("mkl_rt.dll", CallingConvention=CallingConvention.Cdecl,
			 ExactSpelling=true, SetLastError=false)]
		internal static extern void dgeev(ref char jobvl, ref char jobvr,
			ref int n, [In, Out] double[] a, ref int lda,
			[Out] double[] wr, [Out] double[] wi,
			[Out] double[] vl, ref int ldvl, [Out] double[] vr, ref int ldvr,
			[In, Out] double[] work, ref int lwork, ref int info);
	}
}
