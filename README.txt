FindSimilar Audio Search Utility utilising MFCC methods README
=================================================================
Per Ivar Nerseth, 2014
perivar@nerseth.com

FindSimilar. Version 1.0.11.
Copyright (C) 2012-2014 Per Ivar Nerseth.

Usage: FindSimilar.exe <Arguments>

Arguments:
        -scandir=<scan directory path and create audio fingerprints - ignore existing files>
        -match=<path to the wave file to find matches for>
        -matchid=<database id to the wave file to find matches for>

Optional Arguments:
        -gui    <open up the Find Similar Client GUI>
        -resetdb        <clean database, used together with scandir>
        -num=<number of matches to return when querying>
        -percentage=0.x <percentage above and below duration when querying>
        -type=<distance method to use when querying. Choose between:>
                kl      =Kullback Leibler Divergence/ Distance (default)
                dtw     =Dynamic Time Warping - Euclidean
                dtwe    =Dynamic Time Warping - Euclidean
                dtwe2   =Dynamic Time Warping - Squared Euclidean
                dtwman  =Dynamic Time Warping - Manhattan
                dtwmax  =Dynamic Time Warping - Maximum
                ucrdtw  =Dynamic Time Warping - UCR Suite (fast)
                Or use the distance method directly:
        -kl     <Use Kullback Leibler Divergence/ Distance (default)>
        -dtw    <Use Dynamic Time Warping - Euclidean>
        -dtwe   <Use Dynamic Time Warping - Euclidean>
        -dtwe2  <Use Dynamic Time Warping - Squared Euclidean>
        -dtwman <Use Dynamic Time Warping - Manhattan>
        -dtwmax <Use Dynamic Time Warping - Maximum>
        -ucrdtw <Use Dynamic Time Warping - UCR Suite (fast)>

        -? or -help=show this usage help>

Normal Steps are:
1. Scan Directory
FindSimilar.exe -scandir="path/to/audiosamples/dir" -resetdb

2. Optional - Scan more directories
FindSimilar.exe -scandir="path/to/another_audiosamples/dir"

3. Use either command prompt utility
FindSimilar.exe -match="path/to/audiosample.wav|mp3|flac|wma|etc"
or
FindSimilar.exe -matchid=4

4. Or use GUI client
FindSimilar.exe -gui
