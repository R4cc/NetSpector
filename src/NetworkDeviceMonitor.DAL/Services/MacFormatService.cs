using System.Runtime.InteropServices;

namespace NetworkDeviceMonitor.DAL.Services;

public static class MacFormatService
{
    /// <summary>
    /// Convert a string into Int32  
    /// </summary>
    [DllImport("Ws2_32.dll")]
    private static extern Int32 inet_addr(string ip);

    /// <summary>
    /// The main function 
    /// </summary> 
    [DllImport("Iphlpapi.dll")]
    private static extern int SendARP(Int32 dest, Int32 host, ref Int64 mac, ref Int32 len);

    /// <summary>
    /// Returns the MACAddress by a string.
    /// </summary>
    public static string GetRemoteMac(string remoteIp, char separator)
    {   
        Int32 ldest = inet_addr(remoteIp);

        try
        {
            Int64 macinfo = 0;           
            Int32 len = 6;
            
            SendARP(ldest, 0, ref macinfo, ref len);
            
            return FormatMac(macinfo, separator);    
        }
        catch
        {
            return string.Empty;
        }
    }

    /// <summary>
    /// Format a long/Int64 into string.   
    /// </summary>
    private static string FormatMac(this Int64 mac, char separator)
    {
        if (mac <= 0)
        {
            return string.Empty;
        }

        char[] oldmac = Convert.ToString(mac, 16).PadLeft(12, '0').ToCharArray();

        System.Text.StringBuilder newMac = new System.Text.StringBuilder(17);

        if (oldmac.Length < 12)
        {
            return string.Empty;
        }

        newMac.Append(oldmac[10]);
        newMac.Append(oldmac[11]);
        newMac.Append(separator);
        newMac.Append(oldmac[8]);
        newMac.Append(oldmac[9]);
        newMac.Append(separator);
        newMac.Append(oldmac[6]);
        newMac.Append(oldmac[7]);
        newMac.Append(separator);
        newMac.Append(oldmac[4]);
        newMac.Append(oldmac[5]);
        newMac.Append(separator);
        newMac.Append(oldmac[2]);
        newMac.Append(oldmac[3]);
        newMac.Append(separator);
        newMac.Append(oldmac[0]);
        newMac.Append(oldmac[1]);

        return newMac.ToString();
    }
}