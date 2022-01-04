MathNet.Numeric must be version 3.20.2 due to a conflict with (CUDA? Json? don't remember) in later versions!

Oscillators.
Each oscillator has its own filter, pitch envelope and ADSR envelope holding the settings
for those. Since the oscillators are in Patch, all those settings will be saved in files.
The GUI's, however, are generated at start, or when user changes layout, and are populated
with settings from each oscillator, or, if only one GUI is present, like some of them are
in some layouts, the oscillator that is selected, automatically or via the view button.

Sound generating:

Since the synthesizer is polyphonic, each pressed key needs its own set of oscillators.
Oscillators are stored in Patch.Oscillators[][] where the first dimension is representing
the key pressed. It is normally coalled the 'poly'. The second dimension is the actual
oscillator represented by the oscillator GUI. 

FrameServer is collecting wave data from PolyServer and adds each sample from each poly
together to make one set of wave data to deliver as an AudioFrame, rather than delivering
one set of wave data for each pressed key, each poly.

PolyServer. There is one PolyServer for each 'poly'. The 'poly' is stored in Patch.Polyphony.
When the frame server calls each one of the PolyServers, those are responsible for collecting
wave data from all oscillator used. 'Used' oscillators either has volume set higher than zero
thus outputting sound, or are used as modulators.

Wave data is either created in the project's internal C# code, or in CUDA C code in a dll
that uses an NVidia graphics card to create the wave data.

A flag in MainPage, usingGraphicsCard, is used to select whether to use the NVidia graphics
card or not.

Filtering:

There is only one filter GUI, because to the user there is only one filter. However, since
it is possible to control the filter from the envelope generaters (ADSR and PitchEnvelope)
each poly must have its own filter. Therefore the filter for each poly is handled in the
PolyServer.

Modulation:

There are four modulation types:
AM modulates the amplitude of any waveform.
FM modulates the frequency of any waveform.
PM modulates the phase of a waveform and is intended to be used on squarewave only, but all
waveforms that uses phase are affected, whinch might be useful, but also resembles FM for
all except squarewave.
FM sine is a special FM modulation used only between two sine waves that both are set to
keyboard (not LFO) in order to emulate Yamaha DX style FM synthesis. 

(SampleCount is assumed to be 480 here below, but can have other value depending on sound
card settings.)
Wave data is generated in arrays of 480 double at a rate of 100 times per second. Most modulation
could then be used with a snapshot of the modulation data over all 480 samples, but not all.
Therefore, modulation data is generated together with the wave data.

Correction: No frequencies fits exactly into 480 samples. FFT must be done over a number off full
cycles of the waveform. Adapting frequencies to be able to somehow fit into the sampling windows
changes the frequency too much, rendering us a very poorly tuned instrument.

Use a secondary area of suitable length to hold a number of full waves. Note when phase passes
two PI, or a multiple of tso PI, and mark the point. Use the part for TTF filtering, and copy
back to WaveData. If too short, restart from the beginning. Besides from issues with low frequencies
mentioned below this works fine for the first frame requested. However following frames has to
be handled differently.

Rather than restarting when next frame is requested, the fftData should be extended. Since it can
not be extended, a large array should be created from start. Then a part of fftData containing
at least one cycle is filtered and copied back to WaveData. Use my DynamicArray.DynamicComplexArray
changing start and end of part to be filtered. Do not filter same part more than once! When the
entire fftData has been used, move index to beginning.

When frequency is low, the amount of sample gets very high, and filtering takes too long. Comb
samples to use only a suitable fraction of them in fftData, and re-create the skipped ones when
copying back to WaveData by copying the same sample multiple times. Use variable CombFactor for
that. CombFactor should be set at KeyOn().

When filtering, just use DynamicComplexArray in sted of WaveData and generate 480 samples and
continue until reaching 2 PI. Filter on that, then copy the first 480 bytes to WaveData. For
next 480 bytes, back up the pointers from the previously last created point, at 2 PI, as long
as it can be done without start pont goes lower than 0.

Using nVidia graphics card:

At startup, the application tries to find an nVidia graphics card, and if found, to examin its
capabilities, using CUDA. The sample code I started with is however not quite correct in the
structure receiving the response, so I cannot right now find data about kernels, memory size etc.

If a graphics card i found, MainPage.UsingGraphicsCard is set to true, else false. It is possible
to turn off this using a switch in the GUI.

Current version of CudaSupport.cs, that implements calls to the dll containing the CUDA code,
is called each time an array of samples needs to be generated, following the same scheme as
the C# code. However, this means a lot of calls to the dll, which takes too long time, so
the use of the graphics card is actually slightly less efficient than the C# code. Therefore
the use of a graphics card needs to be totally rewritten in order to allow for more to be done
in each call to the dll. Since the calls to the dll is currently using the MainPage.UsingGraphicsCard
flag at various places in the C# code, it needs to be extracted to be used in some other way.

E.g. currently wave data and modulation data is generated in separate calls to the dll, but should
be generated in one call. More such efficiency improvements could probably be found.

For now, CudaSupport is modified by adding line 46 to fail detection of the graphics card:
result = -1;
When starting work on a better version of the CUDA support, remove line 46 first!

New modus modified operandi:

Key on requests a poly number from the dispatcher, initiates all oscillators that has a volume > 0, 
and all modulators connected to those oscillators (and all modulators connected to those modulators)
and starts envelopes (pitch and adsr).
All active oscillators, including all modulators, are reset to start from zero.
All active oscillators and modulators are initiated with one cycle of data, and if the filter
on a modulator is active, the data is filtered.

The frame server gets requests for AudioFrame data frames and calls the poly server for data arrays
for each active poly. It adds all poly data arrays together, sample by sample, to produce one audio
frame, which it delivers to the AudioGraph system.

The poly server calls all active oscillators with the requested poly number for an array of samples.
It adds all server data arrays samples together and delivers the array to the frame server.

The oscillator generates an array of audio data by either doing DX style FM synthesis or by using the
pre-created cycle of data to create an array of data of the correct frequency.
Then the oscillator checks all modulators and modulates accordingly, using the modulators pre-created
cycle of data to generate the modulator wave form with the set frequency, after key number or LFO.

In case of DX style synthesis it might be useful to create a vibrato to the active oscillator, but the
FM input is already busy with the DX style synthesis, so the PM input is used instead, since it only
is used on square waves, and now it is a sine wave so it is free.

The reason for using pre-created and filtered cycle samples is that the filtering usds FFT, which is
a very demanding operation, thus not suitable to do on each sample. It is therefore applied on the
pre-created cycle. Between each delivered audio frame the one cycle data arrays is re-created should
any settings have been changed.

=====================================================================================================

New ideas about filtering.

Problem: Fourier transformations takes too much time to be done on all frame creations. FFT in CUDA
is not (yet) done for two reasons: My card is deprecated and it is not possible to use sample code
from CUDA 11.4 using CUDA 10.2 (whitch supports my card). I had no success in applying CooleyTurkeyC
algorithms since they need to be used in loops that changes values for next loop, thus can not be
transformed into parallel executed code. Two, I am waiting for a new computer but I am not sure
I will be able to implement code from the example simpleCUFFT to make a faster CUDA filter.

Even if a CUDA sulotion gets done, there are a lot of people that does not have a CUDA 11.4 compa-
tible computer, thus needs to run filter on the CPU, so, filtering on CPU has to be done some other
way.

Idea:

Whenever changing waveform a sample of a number of periods are created, and whenever anything is
changed in the filter, this sample is filtered. This number of samples is then used by the
oscillator to generate a waveform of correct pitch.

The number of periods and the number of samples per period has to be tried out. Too much data
takes too long to create (remember that the user might have a key down listening to the result
of the setting changes) thus creating too long drop-outs during changes. Also take in account
the use of the modulation wheel! Too little data and too few periods makes the filter create
a poor filtering experience. 

There might be a back-draw to this: Playing different pitches will transpose not only the pitch,
but also the filtered result. What if key follow is zero?

Responsibility:

If a DX style synthesis is made, the oscillator is creating it in the CPU code.
If a non-DX style synthesis is made and the filter is not active, the oscillator is creating it
in the CPU code.
If a non-DX style synthesis is made and the filter is active but CUDA is not available, the
filter is creating is in CPU code.
If a non-DX style synthesis is made and the filter is active and CUDA is available, the oscillator
is creating the waveform in CPU code, and filters it in CUDA code.

The Keyboard:
	Att KeyOn:
		Calls Filter to create a waveform.

GUI:	
If the waveform is changed in the oscillator GUI or the filter is changed in the filter GUI
the filter is called again to create a waveform etc.

FrameServer calls PolyServer for each active poly.
	The PolyServer:
		For all active oscillators (those with volume > 0):
			If CUDA is available:
				Calls Oscillator to create wave data.
					Oscillator:
						Create wave data from Filter waveform for current key.
						If filter is needed:
							Call CUDA to filter data
						If modulation is needed:
							Modulate data
			If CUDA is not available:
				If filter is active:
					Calls Oscillator to create wave data.
						Oscillator:
							Create wave data from Filter waveform for current key.
							If modulation is needed:
								Modulate data
				If filter is not active:
					Calls Oscillator to create wave data.
						Oscillator:
							Create wave data from Filter waveform for current key.


=====================================================================================================

Minimalistic form to create wave data.

Rather than using the MakeWave calculations in Oscillator, and then filter it, we take a much more
efficient approach: The WaveShape class holds a pre-created and, if needed, pre-filtered one cycle
of wave data. Oscillator then uses this to create wave data with correct frequency.

However, this is not a solution that works in all situations since we have modulation. When a wave
is modulated the wave shape changes between frames, and must be recreated, which is ok unless it
also is filtered. In that situation we are back to filtering the data for each frame, which is not
possible for the CPU do do due to the massive calculations done during the two FFTs performed.

So, during modulation, the filtering must not be done for every frame. Doing it every sixth frame
is probably possible, since playing one note with filtering during each frame did work in previous
versions of the application, but not playing all six notes. Conclusion would be that it could be
ok to filter every sixth time modulation updates the wave shape. This is open for some experimen-
tation. Too few filterings might cause audiable steps in the modulation, too many might cause
missing frames.

There is also cause for a different solution when using XM modulations.
	AM and FM modulations is handled by the Oscillator since it just changes the amplitude during
	AM modulation, and either uses changing StepSize or sets the frequency for each frame, which-
	ever turns out to work best.
	Pulsewidth modulation changes the wave shape with each new frame. This could be handled either
	in WaveShape, in witch case Oscillator can use it as with all other wave data. Or let the
	Oscillator handle it by itself.
	XM modulation is either phase shift in a square wave or DX style modulation between two, three
	or four sine waves all following the keyboard (are not LFOs), so they can never be used at
	the same time.
	DX style modulation can not be pre-generated once by the WaveShape class since the oscillators
	involved can have different frequencies, thus the frames changes every time. But the procedure
	to generate the wave data can be done for each frame without using too much CPU time.
This ends up showing that we can follow the same scheme in all cases. The WaveShape always creates
the wave shape, once for most wave forms, once for each frame for XM modulated wave forms, and 
then filters the wave form every n:th frame. The Oscillator will only handle AM and FM modulations.

Procedure:

Wave form settings or output volume changes ->
	create a new wave shape.