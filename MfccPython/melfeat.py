import scipy.io.wavfile
import scipy.io as sio
import numpy as np
import math, cmath
import matplotlib.pyplot as plt
from scipy.fftpack import fft
from scipy.signal import lfilter
import pylab

class MelFeatures:
  """Mel-frequency cepstral coefficients

  Written by Mark Harvilla, Michael Garbus, and David Wozny
  """

  ###parameters for feature computation
  a         = 0.97
  t1        = 0.025
  t2        = 0.01
  numFilts  = 40
  minfrq    = 133.0
  maxfrq    = 6855.0
  width     = 1.0
  numcep    = 13
  del_w     = 2.0 #these should be EVEN
  dbl_del_w = 4.0

  def __init__(self):
    pass

  def preemph(self,x,a):
      y = lfilter(np.array([1,-a]),1,x)
      return y
  
  def hz2mel(self,frq):
      m   = 2595*math.log(1+frq/700,10)
      return m
  
  def mel2hz(self,m):
      frq = 700*(pow(10,m/2595)-1)
      return frq
  
  def hamming(self,L):
      M  = L-1
      n  = np.arange(0,L)
      hw = 0.54 - 0.46*np.cos(math.pi*2*n/M)
      return hw
  
  def stft(self,x,t1,t2,fs):
      L  = len(x)
  
      N1 = math.floor(t1*fs); nfft = math.pow(2,math.ceil(math.log(N1,2)))
      N2 = math.floor(t2*fs)
  
      numWindows = int(1 + math.floor((L-N1)/N2))
      #correct if all non-full-length windows are dropped
  
      n = range(0,L-1,int(N2))
      W = self.hamming(N1)
      X = np.zeros((1+nfft/2,numWindows))
      k = 0
      for i in n:
          if i+N1-1 > len(x):
              break
      
          x_seg  = x[i:i+N1]*W
          X_seg  = abs(fft(x_seg, nfft))
          X[:,k] = X_seg[0:1+nfft/2]
  
          k += 1
  
      return (X,nfft,numWindows)
  
  def filtbank(self,numFilts, minfrq, maxfrq, width, nfft):
      fftfrqs = np.arange(0,self.fs/2+self.fs/nfft,self.fs/nfft)
      #arange excludes stop point (like range), so we must use fs/2+fs/nfft instead of simply fs/2
  
      wts     = np.zeros((len(fftfrqs),numFilts));
      mb      = self.hz2mel(self.minfrq)
      mt      = self.hz2mel(self.maxfrq)
      melfrqs = np.linspace(mb,mt,self.numFilts+2);
      cntfrqs = np.zeros((self.numFilts+2));
  
      for k in range(0,numFilts+2):
          #note that range doesn't include the terminal point
          cntfrqs[k] = self.mel2hz(melfrqs[k])
  
      for k in range(0,self.numFilts):
          cfs = cntfrqs[k:k+3]; #doesn't take terminal point... weird
          cfs = cfs[1]+width*(cfs-cfs[1])
  
          loslope = (fftfrqs - cfs[0])/(cfs[1] - cfs[0])
          hislope = (cfs[2] - fftfrqs)/(cfs[2] - cfs[1])
  
          wts_temp = np.minimum(loslope,hislope)
          wts_temp = np.maximum(0,wts_temp)
  
          wts[:,k] = wts_temp
  
      return wts
  
  def dct(self,Q,numcep):
      #(the output of) this routine is essentially identical to MATLAB's
      S = Q.shape
      cos_arg = np.arange(1,2*S[0],2)
      dct_mat = np.zeros((S[0],S[0]))
      for k in np.arange(0,S[0]):
        dct_mat[k,:] = math.sqrt(2.0/S[0])*np.cos(math.pi*0.5*k*cos_arg/S[0])
  
      dct_mat[0,:] = dct_mat[0,:]/math.sqrt(2.0)
      
      C = np.dot(dct_mat,Q)
      C = C[0:self.numcep,:]
  
      return C

  def idct(self,Q,numlen):
    S = Q.shape
    if numlen > S[0]:
      newQ = np.zeros((numlen,S[1]))
      newQ[0:S[0],:] = Q
      Q = newQ

    S = Q.shape
    cos_arg = np.arange(0,S[0])
    dct_mat = np.zeros((S[0],S[0]))
    for n in np.arange(0,S[0]):
      dct_mat[n,:] = math.sqrt(2.0/S[0])*np.cos(math.pi*0.5*cos_arg*(2*n+1)/S[0])

    dct_mat[:,0] = dct_mat[:,0]/math.sqrt(2.0)

    R = np.dot(dct_mat,Q)

    return R
  
  def cmn(self,C):
      m = np.mean(C,1)
      for i in range(0,self.numcep):
          C[i,:] = C[i,:] - m[i]
  
      return C
  
  def deltas(self,c,w):
      S = c.shape
      d = np.zeros((S[0],S[1]))
      for n in range(0,S[1]):
          d[:,n] = c[:,(n+w/2) % S[1]]-c[:,n-w/2] #negative indices wrap around
      d = d/w
      return d

  def loadWAVfile(self, filename):
      w  = scipy.io.wavfile.read(filename)
      self.x  = w[1]
      self.fs = w[0]
      return self.x
    
  def calcMelFeatures(self, data):
      x = self.preemph(data,self.a)
      
      outTuple = self.stft(x,self.t1,self.t2,self.fs)
      
      X          = outTuple[0]
      nfft       = outTuple[1]
      numWindows = outTuple[2]
      
      wts = self.filtbank(self.numFilts, self.minfrq, self.maxfrq, 
            self.width, nfft)
      
      Xp  = pow(X,2)
      wts = pow(wts.transpose(),2)
      P   = np.dot(wts,Xp)
      
      Q = np.log(P);
      C = self.dct(Q,self.numcep)
      
      C_cmn = self.cmn(C);
      R_cmn = self.idct(C_cmn,128) #second parameter is length of iDCT
      
      d1 = self.deltas(C_cmn,self.del_w)
      d2 = self.deltas(d1,self.dbl_del_w)
      
      C_out = np.zeros((3*self.numcep,numWindows))
      
      C_out[0:self.numcep,:]             = C_cmn
      C_out[self.numcep:2*self.numcep]   = d1
      C_out[2*self.numcep:3*self.numcep] = d2
      
      return C_out

  def plotSpectrogram(self, data):
      plt.imshow(data, origin='lower')
      plt.show()

  def setNumFilts(self, num):
      self.numFilts = num

if __name__ == "__main__":
  MelFeat = MelFeatures()
  rawdata = MelFeat.loadWAVfile('testout.wav')
  MFCC    = MelFeat.calcMelFeatures(rawdata)
  MelFeat.plotSpectrogram(MFCC)
