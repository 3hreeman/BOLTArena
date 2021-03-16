using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public static class NetworkInfo {
    static string m_ipAddress;
    static int m_port = -1;
    static string m_sceneName = "Stage";
    static string m_hostName = "DEFAULT_HOST_NAME";
    private static int m_expireTime = 300;
    
    public static string ipAddress => m_ipAddress;
    public static int port => m_port;
    public static string sceneName => m_sceneName;
    public static string HostName => m_hostName;
    
    public static Dictionary<string, string> nameSkinDict = new Dictionary<string, string>() { {"Fighter", "Fighter"}, {"Halloween", "Halloween"}, {"Magician", "Magician"},
        {"Aloha", "Aloha"}, {"Blossom", "Blossom"}, {"Choir", "Choir"}, {"Mechagic", "Mechagic"}, {"Pirate", "Pirate"}, {"Pharaoh", "Pharaoh"}, {"Sulbim", "Sulbim"}};
    
    public static void SetHostName(string name) {
        m_hostName = name;
    }

    public static void SetPort(int port) {
        m_port = port;
    }
    public static string GetDefaultMatchName() {
        return m_hostName + "'s Game [" + m_ipAddress+"::"+m_port+ "]";
    }   
    
    public static IEnumerator GetIPAddress()
    {
        UnityWebRequest www = UnityWebRequest.Get("http://checkip.dyndns.org");
        yield return www.SendWebRequest();

        if (www.isNetworkError || www.isHttpError) {
            Debug.Log(www.error);
        }
        else {
            string result = www.downloadHandler.text;

            // This results in a string similar to this: <html><head><title>Current IP Check</title></head><body>Current IP Address: 123.123.123.123</body></html>
            // where 123.123.123.123 is your external IP Address.
            //  Debug.Log("" + result);

            string[] a = result.Split(':'); // Split into two substrings -> one before : and one after. 
            string a2 = a[1].Substring(1);  // Get the substring after the :
            string[] a3 = a2.Split('<');    // Now split to the first HTML tag after the IP address.
            string a4 = a3[0];              // Get the substring before the tag.


            Debug.Log("External IP Address = "+a4);
            m_ipAddress = a4;
        }
    }

    public static int GetLeftTime() {
        return m_expireTime - (int)BoltNetwork.ServerTime;
    }
    public static bool IsExpired() {
        return m_expireTime - BoltNetwork.ServerTime <= 0;
    } 
}
