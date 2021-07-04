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
 * Example showing how to call the Intel MKL FFT dfti() function.
 */

public class test_dfti 
{
    private test_dfti() {}
    public static void MainTest(string[] args) 
    {
        IntPtr desc = new IntPtr();
        int precision = DFTI.DOUBLE;
        int forward_domain = DFTI.REAL;
        int dimension=1, length=6;

        /* The data to be transformed */
        double[] x_normal      = new double [ length ];
        double[] x_transformed = new double [ length ];

        /* Create new DFTI descriptor */
        int ret = DFTI.DftiCreateDescriptor(ref desc,
            precision,forward_domain,dimension,length);

        /* Setup the scale factor */
        long transform_size = length;
        double scale_factor = 1.0 / transform_size;
        DFTI.DftiSetValue(desc,DFTI.BACKWARD_SCALE,scale_factor);

        /* Try floating-point and GetValue function */
        double backward_scale = 0;
        DFTI.DftiGetValue(desc, DFTI.BACKWARD_SCALE, ref backward_scale);
        Console.WriteLine("Backward transform scale: " + backward_scale);

        /* Setup the transform parameters */
        DFTI.DftiSetValue(desc,DFTI.PLACEMENT,DFTI.NOT_INPLACE);
        DFTI.DftiSetValue(desc,DFTI.PACKED_FORMAT,DFTI.PACK_FORMAT);

        /* Commit the descriptor */
        DFTI.DftiCommitDescriptor(desc);

        /* Initialize the data array */
        Console.WriteLine("Initial data:");
        for (int i=0; i<length; i++) {
            x_normal[i] = i;
            Console.Write("\t" + i);
        }
        Console.WriteLine();

        /* Forward, then backward transform */
        DFTI.DftiComputeForward(desc,x_normal,x_transformed);
        DFTI.DftiComputeBackward(desc,x_transformed,x_normal);

        DFTI.DftiFreeDescriptor(ref desc);

        /* Check the data array */
        Console.WriteLine("Resulting data:");
        for (int i=0; i<length; i++) {
            Console.Write("\t" + x_normal[i]);
        }
		Console.WriteLine();
		Console.WriteLine("TEST PASSED");
		Console.WriteLine();
	}
}

namespace mkl 
{
	public sealed class DFTI
	{
		private DFTI() {}

		/** Constants for DFTI, file "mkl_dfti.h" */
		/** DFTI configuration parameters */
		public static int PRECISION = 3;
		public static int FORWARD_DOMAIN = 0;
		public static int DIMENSION = 1;
		public static int LENGTHS = 2;
		public static int NUMBER_OF_TRANSFORMS = 7;
		public static int FORWARD_SCALE = 4;
		public static int BACKWARD_SCALE = 5;
		public static int PLACEMENT = 11;
		public static int COMPLEX_STORAGE = 8;
		public static int REAL_STORAGE = 9;
		public static int CONJUGATE_EVEN_STORAGE = 10;
		public static int DESCRIPTOR_NAME = 20;
		public static int PACKED_FORMAT = 21;
		public static int NUMBER_OF_USER_THREADS = 26;
		public static int INPUT_DISTANCE = 14;
		public static int OUTPUT_DISTANCE = 15;
		public static int INPUT_STRIDES = 12;
		public static int OUTPUT_STRIDES = 13;
		public static int ORDERING = 18;
		public static int TRANSPOSE = 19;
		public static int COMMIT_STATUS = 22;
		public static int VERSION = 23;
		/** DFTI configuration values */
		public static int SINGLE = 35;
		public static int DOUBLE = 36;
		public static int COMPLEX = 32;
		public static int REAL = 33;
		public static int INPLACE = 43;
		public static int NOT_INPLACE = 44;
		public static int COMPLEX_COMPLEX = 39;
		public static int REAL_REAL = 42;
		public static int COMPLEX_REAL = 40;
		public static int REAL_COMPLEX = 41;
		public static int COMMITTED = 30;
		public static int UNCOMMITTED = 31;
		public static int ORDERED = 48;
		public static int BACKWARD_SCRAMBLED = 49;
		public static int NONE = 53;
		public static int CCS_FORMAT = 54;
		public static int PACK_FORMAT = 55;
		public static int PERM_FORMAT = 56;
		public static int CCE_FORMAT = 57;
		public static int VERSION_LENGTH = 198;
		public static int MAX_NAME_LENGTH = 10;
		public static int MAX_MESSAGE_LENGTH = 40;
		/** DFTI predefined error classes */
		public static int NO_ERROR = 0;
		public static int MEMORY_ERROR = 1;
		public static int INVALID_CONFIGURATION = 2;
		public static int INCONSISTENT_CONFIGURATION = 3;
		public static int NUMBER_OF_THREADS_ERROR = 8;
		public static int MULTITHREADED_ERROR = 4;
		public static int BAD_DESCRIPTOR = 5;
		public static int UNIMPLEMENTED = 6;
		public static int MKL_INTERNAL_ERROR = 7;
		public static int LENGTH_EXCEEDS_INT32 = 9;

		/** DFTI DftiCreateDescriptor wrapper */
		public static int DftiCreateDescriptor(ref IntPtr desc,
			int precision, int domain, int dimention, int length)
		{
			return DFTINative.DftiCreateDescriptor(ref desc,
				precision, domain, dimention, length);
		}
		/** DFTI DftiFreeDescriptor wrapper */
		public static int DftiFreeDescriptor(ref IntPtr desc)
		{
			return DFTINative.DftiFreeDescriptor(ref desc);
		}
		/** DFTI DftiSetValue wrapper */
		public static int DftiSetValue(IntPtr desc,
			int config_param, int config_val)
		{
			return DFTINative.DftiSetValue(desc,
				config_param, config_val);
		}
		/** DFTI DftiSetValue wrapper */
		public static int DftiSetValue(IntPtr desc,
			int config_param, double config_val)
		{
			return DFTINative.DftiSetValue(desc,
				config_param, config_val);
		}
		/** DFTI DftiGetValue wrapper */
		public static int DftiGetValue(IntPtr desc,
			int config_param, ref double config_val)
		{
			return DFTINative.DftiGetValue(desc,
				config_param, ref config_val);
		}
		/** DFTI DftiCommitDescriptor wrapper */
		public static int DftiCommitDescriptor(IntPtr desc)
		{
			return DFTINative.DftiCommitDescriptor(desc);
		}
		/** DFTI DftiComputeForward wrapper */
		public static int DftiComputeForward(IntPtr desc,
			[In] double[] x_in, [Out] double[] x_out)
		{
			return DFTINative.DftiComputeForward(desc, x_in, x_out);
		}
		/** DFTI DftiComputeBackward wrapper */
		public static int DftiComputeBackward(IntPtr desc,
			[In] double[] x_in, [Out] double[] x_out)
		{
			return DFTINative.DftiComputeBackward(desc, x_in, x_out);
		}
	}

	/** DFTI native declarations */
	[SuppressUnmanagedCodeSecurity]
	internal sealed class DFTINative 
	{
		/** DFTI native DftiCreateDescriptor declaration */
		[DllImport("mkl_rt.dll", CallingConvention=CallingConvention.Cdecl,
			 ExactSpelling=true, SetLastError=false)]
		internal static extern int DftiCreateDescriptor(ref IntPtr desc,
			int precision, int domain, int dimention, int length);
		/** DFTI native DftiCommitDescriptor declaration */
		[DllImport("mkl_rt.dll", CallingConvention=CallingConvention.Cdecl,
			 ExactSpelling=true, SetLastError=false)]
		internal static extern int DftiCommitDescriptor(IntPtr desc);
		/** DFTI native DftiFreeDescriptor declaration */
		[DllImport("mkl_rt.dll", CallingConvention=CallingConvention.Cdecl,
			 ExactSpelling=true, SetLastError=false)]
		internal static extern int DftiFreeDescriptor(ref IntPtr desc);
		/** DFTI native DftiSetValue declaration */
		[DllImport("mkl_rt.dll", CallingConvention=CallingConvention.Cdecl,
			 ExactSpelling=true, SetLastError=false)]
		internal static extern int DftiSetValue(IntPtr desc,
			int config_param, int config_val);
		/** DFTI native DftiSetValue declaration */
		[DllImport("mkl_rt.dll", CallingConvention=CallingConvention.Cdecl,
			 ExactSpelling=true, SetLastError=false)]
		internal static extern int DftiSetValue(IntPtr desc,
			int config_param, double config_val);
		/** DFTI native DftiGetValue declaration */
		[DllImport("mkl_rt.dll", CallingConvention=CallingConvention.Cdecl,
			 ExactSpelling=true, SetLastError=false)]
		internal static extern int DftiGetValue(IntPtr desc,
			int config_param, ref double config_val);
		/** DFTI native DftiComputeForward declaration */
		[DllImport("mkl_rt.dll", CallingConvention=CallingConvention.Cdecl,
			 ExactSpelling=true, SetLastError=false)]
		internal static extern int DftiComputeForward(IntPtr desc,
			[In] double[] x_in, [Out] double[] x_out);
		/** DFTI native DftiComputeBackward declaration */
		[DllImport("mkl_rt.dll", CallingConvention=CallingConvention.Cdecl,
			 ExactSpelling=true, SetLastError=false)]
		internal static extern int DftiComputeBackward(IntPtr desc,
			[In] double[] x_in, [Out] double[] x_out);
	}
}
