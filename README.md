# FFTSSharp
C# wrapper for FFTS (South)
A fast C# wrapper for FFTS (Fastest Fourier Transform in the South). The dynamically linked libraries (DLLs) used for the wrapper are custom built with CMake and should work normally for Windows users. The DLLs are included in the package.

# Features
* Full support over all 3 types of SSE (SSE1, SSE2, SSE3) and even without SSE (nonSSE). A built-in SSE availability detector takes care of this.
* Full managed function calls to FFTS unmanaged functions
* Easy-to-use wrapper around FFTS with high performance
* (Coming soon) Test programs for basic usage and benchmark code

# Benchmark
This is a benchmark comparing FFTSSharp against Intel's MKL C# implementation (https://software.intel.com/content/www/us/en/develop/articles/using-intel-mkl-in-your-c-program.html). Tests on sizes of power of 2. Transforms are ran 100000 times and the results are averaged out. The benchmark is carried out on i7-2640M CPU (benchmark code will be included soon).


|Length | FFTSSharp     | MKL           |
|:-----:|:-------------:|:-------------:|
| 256   | 356ns         | 376ns         |
| 512   | 748ns         | 748ns         |
| 1024  | 1551ns        | 1555ns        |
| 2048  | 3949ns        | 12520ns       |
| 4096  | 8342ns        | 22965ns       |
| 8192  | 20423ns       | 45957ns       |
| 16384 | 46817ns       | 89313ns       |
| 32767 | 90406ns       | 188335ns      |

# Current problems
* For now, only 1D real and complex, forward and inverse tranforms are supported. FFTS support for 2D and above is insufficiently documented and is incomplete.

# Thoughts
Contributions and issue reports are welcomed.
