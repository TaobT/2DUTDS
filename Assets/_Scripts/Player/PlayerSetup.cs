using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerSetup : MonoBehaviour
{
    private Team playerTeam;

    public Team PlayerTeam { get { return playerTeam; } }

    public void SetPlayerTeam(Team team)
    {
        playerTeam = team;
    }
}
