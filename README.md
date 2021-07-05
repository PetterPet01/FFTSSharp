![FFTSSharp Logo](https://github.com/PetterPet01/FFTSSharp/blob/main/logo.png?raw=true "FFTSSharp - Wrapper for FFTS")

A basic C# wrapper for [FFTS](https://github.com/anthonix/ffts) (The Fastest Fourier Transform in the South).

FFTSSharp is [MIT](https://opensource.org/licenses/MIT) Licensed, so personal and commmercial use alike is allowed. For further details, see the [LICENSE](LICENSE) file.

The dynamically linked libraries (DLLs) used for the wrapper are custom built with CMake and should work normally for Windows users. The DLLs are included in the package and the folder "ffts-dlls" is to be put at the application's startup path.

## Features
* Full support over all 3 types of SSE (SSE1, SSE2, SSE3) and even without SSE (nonSSE). A built-in SSE availability detector takes care of this.
* Full managed function calls to FFTS unmanaged functions
* Easy-to-use wrapper around FFTS with high performance
* Test programs for basic usage and benchmark code

## Benchmark
This is a benchmark comparing FFTSSharp against Intel's MKL C# implementation (https://software.intel.com/content/www/us/en/develop/articles/using-intel-mkl-in-your-c-program.html).

Tests on sizes of power of 2 with complex-to-complex transforms. Transforms for each sizes are run 100000 times and the results are averaged out.

The benchmark is carried out on i7-2640M CPU; SSE3 for FFTSSharp.

| Length |   FFTS  |    MKL   |
|:------:|:-------:|:--------:|
|   256  |  356ns  |   376ns  |
|   512  |  1551ns |  1555ns  |
|  1024  |  1551ns |  1555ns  |
|  2048  |  3949ns |  12520ns |
|  4096  |  8342ns |  22965ns |
|  8192  | 20423ns |  45957ns |
|  16384 | 46817ns |  89313ns |
|  32768 | 90406ns | 188335ns |

## Target
.NET Framework 4.5 (or .NET Standard 2.0) and above\
For target of .NET Framework 2.0 - 3.5 please refer to [FFTSSharpLegacy](FFTSSharpLegacy)

## Get FFTSSharp
For ease of installation, you can add the lastest release of FFTSSharp into your project using [Nuget]()

## Usage
* Include the library
```cs
using FFTSSharp;
```
* Initial loading of FFTS DLLs
```cs
FFTSManager.LoadAppropriateDll(FFTSManager.InstructionType.Auto);
```
There consists of 4 enums in FFTSManager.InstructionType: nonSSE, SSE, SSE2, SSE3, Auto\
(Choose "Auto" for generic loading)
* Create an instance of FFTS (think of it like creating a plan)
```cs
var ffts = FFTS.Real(FFTS.FORWARD, 16);
```
* Declare the input and output array (length of the input and output must be known beforehand)
```cs
float[] input = new float[16];
float[] output = new float[18];
```
* Compute the result
```cs
ffts.Execute(input, output);
```
* Free the plan
```cs
ffts.Free();
```

## Current problems
* For now, only 1D real and complex, forward and inverse tranforms are supported. FFTS support for 2D and above is insufficiently documented and is incomplete.

## Credits
Much thanks to the author, [anthonix](https://github.com/anthonix) for creating [FFTS](https://github.com/anthonix/ffts) with a very permissive license.\
Thanks to [DllImportX](https://github.com/rodrigo-speller/DllImportX) made available by [rodrigo-speller](https://github.com/rodrigo-speller).

## Thoughts
Contributions and issue reports are welcomed.


