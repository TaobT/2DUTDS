using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using System;

public enum Team
{
    NoTeam,
    TeamA,
    TeamB
}

public class GameManager : MonoBehaviour
{
    public GameManager Singleton;

    Dictionary<ulong,Team> allPlayers = new Dictionary<ulong,Team>();

    List<ulong> playersTeamA = new List<ulong>();
    List<ulong> playersTeamB = new List<ulong>();

    private void Awake()
    {
        if(Singleton == null)
        {
            Singleton = this;
        }
    }

    public void AddPlayerBalancing()
    {

    }
}
