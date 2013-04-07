BASSFLAC 2.4
Copyright (c) 2004-2009 Un4seen Developments Ltd. All rights reserved.

Files that you should have found in the BASSFLAC package
========================================================
Win32 version
-------------
BASSFLAC.TXT    This file
BASSFLAC.DLL    The BASSFLAC module
BASSFLAC.CHM    BASSFLAC documentation
C\              C/C++ API...
  BASSFLAC.H      BASSFLAC C/C++ header file
  BASSFLAC.LIB    BASSFLAC import library
VB\             Visual Basic API...
  BASSFLAC.BAS    BASSFLAC Visual Basic module
DELPHI\         Delphi API...
  BASSFLAC.PAS    BASSFLAC Delphi unit

MacOSX version
--------------
BASSFLAC.TXT    This file
LIBBASSFLAC.DYLIB  The BASSFLAC module
BASSFLAC.CHM    BASSFLAC documentation
BASSFLAC.H      BASSFLAC C/C++ header file
MAKEFILE

NOTE: To view the documentation, you will need a CHM viewer, such as CHMOX
      which is included in the BASS package.


What's the point?
=================
BASSFLAC is an extension to the BASS audio library, enabling the playing of
FLAC (Free Lossless Audio Codec) encoded files.


Requirements
============
BASS 2.4 is required.


Using BASSFLAC
==============
The plugin system (see BASS_PluginLoad) can be used to add FLAC support to
the standard BASS stream (and sample) creation functions. Dedicated FLAC
stream creation functions are also provided by BASSFLAC.

Win32 version
-------------
To use BASSFLAC with Borland C++ Builder, you'll first have to create a
Borland C++ Builder import library for it. This is done by using the
IMPLIB tool that comes with Borland C++ Builder. Simply execute this:

	IMPLIB BASSFLACBCB.LIB BASSFLAC.DLL

... and then use BASSFLACBCB.LIB in your projects to import BASSFLAC.

To use BASSFLAC with LCC-Win32, you'll first have to create a compatible
import library for it. This is done by using the PEDUMP and BUILDLIB
tools that come with LCC-Win32. Run these 2 commands:

	PEDUMP /EXP BASSFLAC.LIB > BASSFLACLCC.EXP
	BUILDLIB BASSFLACLCC.EXP BASSFLACLCC.LIB

... and then use BASSFLACLCC.LIB in your projects to import BASSFLAC.

For the BASS functions that return strings (char*), VB users should use
the VBStrFromAnsiPtr function to convert the returned pointer into a VB
string.

TIP: The BASSFLAC.CHM file should be put in the same directory as the BASS.CHM
     file, so that the BASSFLAC documentation can be accessed from within the
     BASS documentation.

MacOSX version
--------------
A separate "LIB" file is not required for OSX. Using XCode, you can simply
add the DYLIB file to the project. Or using a makefile, you can build your
programs like this, for example:

	gcc yoursource -L. -lbass -lbassflac -o yourprog

As with LIBBASS.DYLIB, the LIBBASSFLAC.DYLIB file must be put in the same
directory as the executable (it can't just be somewhere in the path).

LIBBASSFLAC.DYLIB is a universal binary, with support for both PowerPC and
Intel Macs. If you want PowerPC-only or Intel-only versions, the included
makefile can create them for you, by typing "make ppc" or "make i386".


Latest Version
==============
The latest versions of BASSFLAC & BASS can be found at the BASS website:

	www.un4seen.com


Licence
=======
BASSFLAC is free to use with BASS.

TO THE MAXIMUM EXTENT PERMITTED BY APPLICABLE LAW, BASSFLAC IS PROVIDED
"AS IS", WITHOUT WARRANTY OF ANY KIND, EITHER EXPRESSED OR IMPLIED,
INCLUDING BUT NOT LIMITED TO THE IMPLIED WARRANTIES OF MERCHANTABILITY
AND/OR FITNESS FOR A PARTICULAR PURPOSE. THE AUTHORS SHALL NOT BE HELD
LIABLE FOR ANY DAMAGE THAT MAY RESULT FROM THE USE OF BASSFLAC. YOU USE
BASSFLAC ENTIRELY AT YOUR OWN RISK.

Usage of BASSFLAC indicates that you agree to the above conditions.

All trademarks and other registered names contained in the BASSFLAC
package are the property of their respective owners.


History
=======
These are the major (and not so major) changes at each version stage.
There are of course bug fixes and other little improvements made along
the way too! To make upgrading simpler, all functions affected by a
change to the BASSFLAC interface are listed.

2.4.1 - 8/12/2009
-----------------
* Support for embedded images
	BASS_TAG_FLAC_PICTURE (BASS_ChannelGetTags type)
	TAG_FLAC_PICTURE structure
* Support for embedded cuesheets
	BASS_TAG_FLAC_CUE (BASS_ChannelGetTags type)
	TAG_FLAC_CUE/_TRACK/_INDEX structures

2.4 - 2/4/2008
--------------
* Support for updated user file stream system
	BASS_FLAC_StreamCreateFileUser
* 64-bit file positioning
	BASS_FLAC_StreamCreateFile
* Callback "user" parameters changed to pointers
	BASS_FLAC_StreamCreateURL/FileUser
* Updated to libFLAC 1.2.1

2.3.0.4 - 30/7/2007
------------------
* Updated to libFLAC 1.2.0

2.3.0.3 - 4/3/2007
------------------
* Updated to libFLAC 1.1.4

2.3.0.1 - 10/7/2006
-------------------
* Ogg FLAC support
	BASS_CTYPE_STREAM_FLAC_OGG (channel type)

2.3 - 21/5/2006
---------------
* No API changes

2.2 - 2/10/2005
---------------
* Support for new plugin system (BASS_PluginLoad)
* Support for internet streaming
	BASS_FLAC_StreamCreateURL
	BASS_StreamCreateURL (via plugin system)
* Support for buffered user file streaming
	BASS_FLAC_StreamCreateFileUser
	BASS_StreamCreateFileUser (via plugin system)
* Without the FLOAT flag, files above 16-bit will be decoded in 16-bit
	BASS_FLAC_StreamCreateFile/User/URL
	BASS_StreamCreateFile/User/URL (via plugin system)
* Vendor/encoder retrieval
	BASS_TAG_VENDOR (BASS_StreamGetTags type)
* MacOSX port introduced

2.1 - 28/11/2004
----------------
* User file stream support
	BASS_FLAC_StreamCreateFileUser
* Support for the improved "mixtime" sync system (allows custom looping)

2.0 - 2/10/2004
---------------
* First release


Credits
=======
FLAC decoding is based on libFLAC, Copyright (c) 2000-2007 Josh Coalson
Ogg FLAC support uses libogg, Copyright (c) 2002-2004 Xiph.org Foundation


Bug reports, Suggestions, Comments, Enquiries, etc...
=====================================================
If you have any of the aforementioned please visit the BASS forum at
the website.

