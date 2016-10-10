# Class and NativeMethod

```
public class SerialPort : System.ComponentModel.Component
{
    private SerialStream internalSerialStream = null;
}

internal sealed class SerialStream : Stream
{
    UnsafeNativeMethods.ClearCommBreak()
    UnsafeNativeMethods.ClearCommError()
    UnsafeNativeMethods.CreateFile()
    UnsafeNativeMethods.EscapeCommFunction()
    UnsafeNativeMethods.FlushFileBuffers()
    UnsafeNativeMethods.GetCommModemStatus()
    UnsafeNativeMethods.GetCommProperties()
    UnsafeNativeMethods.GetCommState()
    UnsafeNativeMethods.GetFileType()
    UnsafeNativeMethods.GetOverlappedResult()
    UnsafeNativeMethods.PurgeComm()
    UnsafeNativeMethods.ReadFile()
    UnsafeNativeMethods.SetCommBreak()
    UnsafeNativeMethods.SetCommMask()
    UnsafeNativeMethods.SetCommState()
    UnsafeNativeMethods.SetCommTimeouts()
    UnsafeNativeMethods.SetupComm()
    UnsafeNativeMethods.WaitCommEvent()
    UnsafeNativeMethods.WriteFile()
}
```

# Call tree

```
internal SerialStream(string portName, int baudRate, Parity parity, int dataBits, StopBits stopBits, int readTimeout, int writeTimeout, Handshake handshake, bool dtrEnable, bool rtsEnable, bool discardNull, byte parityReplace)
	Thread eventLoopThread = new Thread(new ThreadStart(eventRunner.WaitForCommEvent));
		internal unsafe void WaitForCommEvent()
			while (!ShutdownLoop) {
				private void CallEvents(int nativeEvents)
					ThreadPool.QueueUserWorkItem(callErrorEvents, errors);
						private void CallErrorEvents(object state)
					ThreadPool.QueueUserWorkItem(callPinEvents, nativeEvents);
						private void CallPinEvents(object state)
					ThreadPool.QueueUserWorkItem(callReceiveEvents, nativeEvents);
						private void CallReceiveEvents(object state)
			}
```
