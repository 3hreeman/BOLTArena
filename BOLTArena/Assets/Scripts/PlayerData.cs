using System.Collections;
using System.Collections.Generic;
using Bolt;
using UdpKit;
using UnityEngine;

public class PlayerData : Bolt.IProtocolToken {
    public string m_playerName;
    public string m_resKey;
    
    public void Read(UdpPacket packet) {
        m_playerName = packet.ReadString();
        m_resKey = packet.ReadString();
    }

    public void Write(UdpPacket packet) {
        packet.WriteString(m_playerName);
        packet.WriteString(m_resKey);
    }
}
