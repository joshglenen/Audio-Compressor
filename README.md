# Audio-Compressor
A user friendly audio dynamic range compressor. Currently acts as a low pass peak volume filter to remove loud noises that could otherwise cause a distraction or ear strain. Fully controllable and open source. This project has only been tested on Windows OS 10.

# GUI
![image](https://github.com/joshglenen/Audio-Compressor/blob/master/Cavfe4.PNG)

# Mode: Default
This mode uses CSCore to sample the peak audio levels of any running applications. This application is intended for one audio stream only. The peak values are compared to the maximum system volume defined by the user as the preamp volume. If the peak or average of peaks passes a threshold (% of preamp volume), the system volume will be lowered. If the peak volume falls below the lower threshold, the system volume will raise. This application is meant to be automatic and unobtrusive. It will not change the application volume and requires no setup. 

This mode has a major issue were the sound will drop substantially over the thresholds. If the levels are constantly passing the thresholds, the sound will be choppy and distracting. This mode is fine for its intended purpose: to prevent film audio encoded with a high dynamic range where the audio stream is unaccesable and unmodifiable. This will make action movies watchable without having the quiet parts inaudible and the loud parts deafining.

# Mode: True Gain
This mode applies a gain defined by the ratio to the system volume. Each time the audio is sampled, it is tested on two threasholds. If the peak volume is above the upper threshold, (defaultVol * (1 - ((val - UpThresh) * (1 - attenuation))) is applied to the system volume to lower the volume. A similar formula is used to increase the volume when the peak falls below the lower threshold. A graph of the effect the ratio has on the output is shown below.

![image](https://github.com/joshglenen/Audio-Compressor/blob/master/graph.PNG)

Note that the output of the above formula is not the output of the ideal compressor as a compressor is intended to change the actual peak volume whereas this application changes the system volume. The magnitude of the change depends on the threshold as well as the ratio. 

This mode is not working as there are several issues that were encountered. 
1) CSCore is reporting frequent errors. This may be an issue with .net, Windows 10, or CSCore. To fix, I would need to rewrite the application without CSCore which is too much work for what was a day project initially. 
2) Reading the volume causes CSCore to not work at all, and setting the volume causes CSCore to throw occasional errors. Both are caused by the application attempting to read the peak volume. This again could be a limitation or bug of .net, Windows 10, or CSCore. The solution is to use the preamp setting and only set the volume occasionally. These needs are met by the other modes.
3) The audio, when working, is choppy. This is a limitation of WPF which does not opperate fast enough for the changes to be inaudible. The solution is to use a physical amplifier which will be able to sample much faster than 1kHz.

# Conclusion
Further development would best be to work from the ground up with a more efficient language like c++. A physical amplifier is more practical as it will sample faster and be capable ofm applying a true compression of the input signal instead of using the system volume controls. This would also be universal but inconvenient unless used for a stationary purpose.
