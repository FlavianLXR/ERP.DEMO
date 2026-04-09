// Zebra Printer SDK Support
// **********************************************************************************************************

using System;
using System.Text;
using System.Runtime.InteropServices;

namespace nsZBRPrinter
{
    public class ZBRPrinter
    {
        #region Private Variables

        private IntPtr  _handle;    // device context
        private int     _prnType;

        #endregion

        #region Constructor

        public ZBRPrinter()
		{
            _handle = IntPtr.Zero;
            _prnType = 0;
		}

		#endregion

		#region Printer DLLImports

        // SDK DLL Version

        [DllImport("ZBRPrinter.dll", EntryPoint = "ZBRPRNGetSDKVer", CharSet = CharSet.Auto, CallingConvention = CallingConvention.StdCall, 
                   SetLastError = true)]
        static extern void ZBRPRNGetSDKVer(out int major, out int minor, out int engLevel);

        // Handle

        [DllImport("ZBRPrinter.dll", EntryPoint = "ZBRGetHandle", CharSet = CharSet.Auto, CallingConvention = CallingConvention.StdCall,
                   SetLastError = true)]
        static extern int ZBRGetHandle(out IntPtr _handle, byte[] drvName, out int prn_type, out int err);

        [DllImport("ZBRPrinter.dll", EntryPoint = "ZBRGetHandle", CharSet = CharSet.Auto, CallingConvention = CallingConvention.StdCall, 
                   SetLastError = true)]
        static extern int ZBRGetHandle(out IntPtr _handle, [MarshalAs(UnmanagedType.LPStr)] string drvName, out int prn_type, out int err);

        [DllImport("ZBRPrinter.dll", EntryPoint = "ZBRCloseHandle", CharSet = CharSet.Auto, CallingConvention = CallingConvention.StdCall, 
                   SetLastError = true)]
        static extern int ZBRCloseHandle(IntPtr _handle, out int err);

        // Card Movement

        [DllImport("ZBRPrinter.dll", EntryPoint = "ZBRPRNEjectCard", CharSet = CharSet.Auto, CallingConvention = CallingConvention.StdCall,
                   SetLastError = true)]
        static extern int ZBRPRNEjectCard(IntPtr _handle, int prn_type, out int err);

        // Magnetic Encoding

        [DllImport("ZBRPrinter.dll", EntryPoint = "ZBRPRNReadMag", CharSet = CharSet.Auto,
                    CallingConvention = CallingConvention.StdCall, SetLastError = true)]
        static extern int ZBRPRNReadMag(IntPtr _handle, int prn_type, int trksToRead, byte[] trk1Buf,
                                        out int trk1BytesNeeded, byte[] trk2Buf, out int trk2BytesNeeded, byte[] trk3Buf,
                                        out int trk3BytesNeeded, out int err);

        [DllImport("ZBRPrinter.dll", EntryPoint = "ZBRPRNReadMag", CharSet = CharSet.Auto, CallingConvention = CallingConvention.StdCall, 
                    SetLastError = true)]
        static extern int ZBRPRNReadMag(IntPtr _handle, int prn_type, int trksToRead, char[] trk1Buf,
                                        out int trk1BytesNeeded, char[] trk2Buf, out int trk2BytesNeeded,
                                        char[] trk3Buf, out int trk3BytesNeeded, out int err);

        [DllImport("ZBRPrinter.dll", EntryPoint = "ZBRPRNReadMag", CharSet = CharSet.Auto, CallingConvention = CallingConvention.StdCall, 
                    SetLastError = true)]
        static extern int ZBRPRNReadMag(IntPtr _handle, int prn_type, int trksToRead, IntPtr trk1Buf,
                                        out int trk1BytesNeeded, IntPtr trk2Buf, out int trk2BytesNeeded,
                                        IntPtr trk3Buf, out int trk3BytesNeeded, out int err);

        [DllImport("ZBRPrinter.dll", EntryPoint = "ZBRPRNWriteMag", CharSet = CharSet.Auto, 
                    CallingConvention = CallingConvention.StdCall, SetLastError = true)]
        static extern int ZBRPRNWriteMag(IntPtr _handle, int prn_type, int trksToWrite,
                                         byte[] trk1Data, byte[] trk2Data, byte[] trk3Data, out int err);

        [DllImport("ZBRPrinter.dll", EntryPoint = "ZBRPRNWriteMag", CharSet = CharSet.Auto, CallingConvention = CallingConvention.StdCall, 
                    SetLastError = true)]
        static extern int ZBRPRNWriteMag(IntPtr _handle, int prn_type, int trksToWrite,
                                         char[] trk1Data, char[] trk2Data, char[] trk3Data, out int err);

        [DllImport("ZBRPrinter.dll", EntryPoint = "ZBRPRNWriteMag", CharSet = CharSet.Auto,
                    CallingConvention = CallingConvention.StdCall, SetLastError = true)]
        static extern int ZBRPRNWriteMag(IntPtr _handle, int prn_type, int trksToWrite,
                                         IntPtr trk1Data, IntPtr trk2Data, IntPtr trk3Data, out int err);

        [DllImport("ZBRPrinter.dll", EntryPoint = "ZBRPRNWriteMagPassThru", CharSet = CharSet.Auto, CallingConvention = CallingConvention.StdCall, 
                    SetLastError = true)]
        static extern int ZBRPRNWriteMagPassThru(IntPtr hDC, int prn_Type, int trksToWrite, 
                                                  byte[] trk1Data, byte[] trk2Data, byte[] trk3Data, 
                                                  out int err);

        [DllImport("ZBRPrinter.dll", EntryPoint = "ZBRPRNSetMagEncodingStd", CharSet = CharSet.Auto, CallingConvention = CallingConvention.StdCall,
                    SetLastError = true)]
        static extern int ZBRPRNSetMagEncodingStd(IntPtr hDC, int prn_Type, int std, out int err);

        [DllImport("ZBRPrinter.dll", EntryPoint = "ZBRPRNSetEncoderCoercivity", CharSet = CharSet.Auto, CallingConvention = CallingConvention.StdCall,
                    SetLastError = true)]
        static extern int ZBRPRNSetEncoderCoercivity(IntPtr hDC, int prn_Type, int coercivity, out int err);

        [DllImport("ZBRPrinter.dll", EntryPoint = "ZBRPRNGetPrinterStatus", CharSet = CharSet.Auto, CallingConvention = CallingConvention.StdCall, 
                    SetLastError = true)]
        static extern int ZBRPRNGetPrinterStatus(out int status);

        [DllImport("ZBRPrinter.dll", EntryPoint = "ZBRPRNGetPrinterOptions", CharSet = CharSet.Auto,
                   CallingConvention = CallingConvention.StdCall, SetLastError = true)]
        private static extern int ZBRPRNGetPrinterOptions(IntPtr handle, int prn_type, byte[] options, out int respSize, out int err);

        #endregion

        #region SDK Information

        // Get ZBRPrinter.dll Version -----------------------------------------------------------------------
        
        public void GetSDKVer(out int major, out int minor, out int engLevel)
        {
            ZBRPRNGetSDKVer(out major, out minor, out engLevel);
        }

        #endregion
        
        #region Handle

        // Opens a connection to a printer driver -----------------------------------------------------------
        
		public int Open(byte[] drvName, out int errValue)
		{
            return ZBRGetHandle(out _handle, drvName, out _prnType, out errValue);
		}

        // Closes the connection to a printer driver --------------------------------------------------------
        
        public int Close(out int errValue) 
		{
            _prnType = 0;
            return ZBRCloseHandle(_handle, out errValue);
		}

        #endregion

        #region Printer Configuration

        public int GetPrinterConfiguration(byte[] options, out int respSize, out int err, out string errMsg)
        {
            errMsg = string.Empty;
            err = -1;
            respSize = 0;
            int result = -1;
            try
            {
                result = ZBRPRNGetPrinterOptions(_handle, _prnType, options, out respSize, out err);
            }
            catch (Exception ex)
            {
                errMsg = ex.Message;
                result = -1;
            }
            return result;
        }

        #endregion Printer Configuration

        #region Printer Status

        public bool IsPrinterInErrorMode(out int errValue)
        {
            ZBRPRNGetPrinterStatus(out errValue);
            if (errValue == 0)
                return false;
            return true;
        }

        #endregion //Printer Status
        
        #region Card Movement

        // Ejects a card from a printer ---------------------------------------------------------------------

        public int EjectCard (out int errValue)
		{
            return ZBRPRNEjectCard(_handle, _prnType, out errValue);
		}

        #endregion

        #region Magnetic Encoding

        // Reads all three magnetic strip tracks ------------------------------------------------------------

        public int ReadMag(int trksToRead, ref string track1, ref string track2, ref string track3, out string errMsg)
        {
            errMsg = string.Empty;

            byte[] trkBuf1 = null;
            byte[] trkBuf2 = null;
            byte[] trkBuf3 = null;

            int errValue = 0;
            try
            {
                trkBuf1 = new byte[50];
                trkBuf2 = new byte[50];
                trkBuf3 = new byte[50];

                int size1 = 0;
                int size2 = 0;
                int size3 = 0;
                
                int result = ZBRPRNReadMag(_handle, _prnType, trksToRead, trkBuf1, out size1, trkBuf2, out size2,
                                            trkBuf3, out size3, out errValue);

                if (result == 1 && errValue == 0)
                {
                    track1 = ASCIIEncoding.ASCII.GetString(trkBuf1, 0, size1);
                    track2 = ASCIIEncoding.ASCII.GetString(trkBuf2,0, size2);
                    track3 = ASCIIEncoding.ASCII.GetString(trkBuf3, 0, size3);
                }
                return result;
            }
            catch (Exception ex)
            {
                errMsg = ex.Message;
            }
            finally
            {
                trkBuf1 = null;
                trkBuf2 = null;
                trkBuf3 = null;
            }
            return 0;
        }

        // Writes to the magnetic strip tracks
        //     if data is null or "", the track is not written ----------------------------------------------

        public int WriteMag(int trksToWrite, string track1, string track2, string track3, out string errMsg)
        {
            errMsg = string.Empty;
            
            byte[] trkBuf1 = null;
            byte[] trkBuf2 = null;
            byte[] trkBuf3 = null;
            int errValue = 0;
            int IsoMag = 1;
            int HiCo = 1;
            try
            {
                //set to ISO mag encode standard:
                ZBRPRNSetMagEncodingStd(_handle, _prnType, IsoMag, out errValue);

                //set hico:
                ZBRPRNSetEncoderCoercivity(_handle, _prnType, HiCo, out errValue);

                trkBuf1 = ASCIIEncoding.ASCII.GetBytes(track1);
                trkBuf2 = ASCIIEncoding.ASCII.GetBytes(track2);
                trkBuf3 = ASCIIEncoding.ASCII.GetBytes(track3);

                int result = ZBRPRNWriteMag(_handle, _prnType, trksToWrite, trkBuf1, trkBuf2, trkBuf3, out errValue);
                if(result != 1)
                    errMsg = "WriteMag failed. Error = " + Convert.ToString(errValue);

                return result;
            }
            catch(Exception ex)
            {
                errMsg = ex.Message;
            }
            finally
            {
                trkBuf1 = null;
                trkBuf2 = null;
                trkBuf3 = null;
            }
            return 0;
        }

        private IntPtr AllocateUnmanagedArray(byte[] arr)
        {
            int size = Marshal.SizeOf(arr[0]) * arr.Length;
            IntPtr pointer = Marshal.AllocHGlobal(size);
            return pointer;
        }

        private void InitUnmanagedArray(ref IntPtr pointer, byte[] unmanagedArray)
        {
            int uidSize = Marshal.SizeOf(unmanagedArray[0]) * unmanagedArray.Length;
            pointer = Marshal.AllocHGlobal(uidSize);
            Marshal.Copy(unmanagedArray, 0, pointer, unmanagedArray.Length);
        }
        
        private void FreeUnmanagedMemory(IntPtr pointer)
        {
            Marshal.FreeHGlobal(pointer);
        }

        private void CopyUnmanagedArray(IntPtr pointer, ref byte[] ManagedArray, int paramLength)
        {
            Marshal.Copy(ManagedArray, 0, pointer, ManagedArray.Length);
        }

        private void CopyToUnmanagedArray(IntPtr pointer, ref byte[] ManagedArray, int paramLength)
        {
            Marshal.Copy(pointer, ManagedArray, 0, ManagedArray.Length);
        }
                    
        #endregion
    }
}
