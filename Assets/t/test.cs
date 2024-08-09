using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using FishNet.Managing;
using FishNet.Transporting;
using HeathenEngineering.SteamworksIntegration;
using HeathenEngineering.SteamworksIntegration.API;
using Steamworks;
using UnityEngine;

public class test : MonoBehaviour
{
    private static test instance;
    private void Awake() => instance = this;
    
    [SerializeField] private NetworkManager _networkManager;
    [SerializeField] private FishySteamworks.FishySteamworks _fishySteamworks;
    
    protected Callback<LobbyCreated_t> LobbyCreated;
    protected Callback<GameLobbyJoinRequested_t> JoinRequest;
    protected Callback<LobbyEnter_t> LobbyEntered;
    
    private HServerListRequest m_ServerListRequest;
    
    private ISteamMatchmakingServerListResponse m_ServerListResponse;
    
    private Steamworks.Callback<SteamNetConnectionStatusChangedCallback_t> _onRemoteConnectionStateCallback;
    
    public static ulong CurrentLobbyID;
    public SteamSettings settings;
    public void OnSteamInit()
    {
        var steamID = SteamGameServer.GetSteamID();
        
        Debug.Log("Steam Server Id" + steamID.m_SteamID);
    }
    
    private void Start()
    {
        // SteamAPI.Init();
        //
        // var serverConfiguration = settings.server.Configuration;
        // Steamworks.GameServer.Init(serverConfiguration.ip, serverConfiguration.gamePort, serverConfiguration.queryPort, EServerMode.eServerModeAuthentication, serverConfiguration.serverVersion);
        // SteamGameServer.LogOnAnonymous();
        
        LobbyCreated = Callback<LobbyCreated_t>.Create(OnLobbyCreated);
        JoinRequest = Callback<GameLobbyJoinRequested_t>.Create(OnJoinRequest);
        LobbyEntered = Callback<LobbyEnter_t>.Create(OnLobbyEntered);
        
        m_ServerListRequest = HServerListRequest.Invalid;
        
        m_ServerListResponse = new ISteamMatchmakingServerListResponse(OnServerResponded, OnServerFailedToRespond, OnRefreshComplete);
        
       

    }
    
    private void OnLobbyCreated(LobbyCreated_t callback)
    {
        //Debug.Log("Starting lobby creation: " + callback.m_eResult.ToString());
        if (callback.m_eResult != EResult.k_EResultOK)
            return;
 
        CurrentLobbyID = callback.m_ulSteamIDLobby;
        SteamMatchmaking.SetLobbyData(new CSteamID(CurrentLobbyID), "HostAddress", SteamUser.GetSteamID().ToString());
        SteamMatchmaking.SetLobbyData(new CSteamID(CurrentLobbyID), "name", SteamFriends.GetPersonaName().ToString() + "'s lobby");
        _fishySteamworks.SetClientAddress(SteamUser.GetSteamID().ToString());
        
        _fishySteamworks.StartConnection(true);
        Debug.Log("Lobby creation was successful");
        Debug.Log("Host Address" + SteamUser.GetSteamID().ToString());
        Debug.Log("Lobby Id" + CurrentLobbyID);
    }
 
    private void OnJoinRequest(GameLobbyJoinRequested_t callback)
    {
        SteamMatchmaking.JoinLobby(callback.m_steamIDLobby);
    }
 
    private void OnLobbyEntered(LobbyEnter_t callback)
    {
        CurrentLobbyID = callback.m_ulSteamIDLobby;
        
        Debug.Log("successfully lobby entered");
        
        Debug.Log("Host Address" + SteamUser.GetSteamID().ToString());
        Debug.Log("Lobby Id" + CurrentLobbyID);
        
        _fishySteamworks.SetClientAddress(SteamMatchmaking.GetLobbyData(new CSteamID(CurrentLobbyID), "HostAddress"));
        _fishySteamworks.StartConnection(false);
    }
    
    public void CreateLobby()
    {
        //SteamMatchmaking.CreateLobby(ELobbyType.k_ELobbyTypeFriendsOnly, 4);
        //_fishySteamworks.SetClientAddress(SteamMatchmaking.GetLobbyData(new CSteamID(CurrentLobbyID), "HostAddress"));
        _fishySteamworks.StartConnection(true);
    }

    public void JoinByID(string id)
    {
        _fishySteamworks.SetClientAddress(id);
        _fishySteamworks.StartConnection(false);
        
        // CSteamID steamID = new CSteamID(Convert.ToUInt64(id));
        // JoinByID(steamID);
    }
    
    public void JoinByID(CSteamID steamID)
    {
        _fishySteamworks.SetClientAddress(steamID.m_SteamID.ToString());
        _fishySteamworks.StartConnection(false);
        
        // FishySteamworks.FishySteamworks.
        // FishySteamworks.FishySteamworks.StartConnection(false);
        // SteamNetworkingIdentity smi = new SteamNetworkingIdentity();
        // smi.SetSteamID(steamID);
        // SteamNetworkingConfigValue_t[] options = new SteamNetworkingConfigValue_t[] { };
        // SteamNetworkingSockets.ConnectP2P(ref smi, 0, options.Length, options);
        
        // Debug.Log("Attempting to join lobby with ID: " + steamID.m_SteamID);
        // if (SteamMatchmaking.RequestLobbyData(steamID))
        //     SteamMatchmaking.JoinLobby(steamID);
        // else
        //     Debug.Log("Failed to join lobby with ID: " + steamID.m_SteamID);
    }
 
    public static void LeaveLobby()
    {
        SteamMatchmaking.LeaveLobby(new CSteamID(CurrentLobbyID));
        CurrentLobbyID = 0;
 
        instance._fishySteamworks.StopConnection(false);
        if(instance._networkManager.IsServer)
            instance._fishySteamworks.StopConnection(true);
    }


    public void SearchServerList()
    {
        var sb = GetComponent<SteamworksBehaviour>();
       // MatchMakingKeyValuePair_t[] filters = new MatchMakingKeyValuePair_t[sb.settings.server.Configuration.rulePairs.Length];
        
        // var i = 0;
        // foreach (var pair in sb.settings.server.Configuration.rulePairs)
        // {
        //     
        //     
        //     filters[i] = new MatchMakingKeyValuePair_t
        //     {
        //         m_szKey = pair.key, m_szValue = pair.value
        //     };
        //
        //     ++i;
        // }\
        // sb.settings.server.Configuration
        
        //sb.settings.server.Configuration
        MatchMakingKeyValuePair_t[] filters = {
            new MatchMakingKeyValuePair_t { m_szKey = "spectatorServerName", m_szValue = sb.settings.server.Configuration.spectatorServerName },
            new MatchMakingKeyValuePair_t { m_szKey = "gamedir", m_szValue = "tf" },
            new MatchMakingKeyValuePair_t { m_szKey = "gametagsand", m_szValue = "beta" },
        };
  
        m_ServerListRequest = SteamMatchmakingServers.RequestSpectatorServerList(sb.settings.applicationId, null, 0, m_ServerListResponse);
    }

    private void Update()
    {
        Debug.Log(SteamGameServer.GetSteamID());
    }


    // ISteamMatchmakingServerListResponse
    private void OnServerResponded(HServerListRequest hRequest, int iServer) {
        Debug.Log("OnServerResponded: " + hRequest + " - " + iServer);
        var handler = SteamMatchmakingServers.GetServerDetails(hRequest, iServer);
        
    }

    private void OnServerFailedToRespond(HServerListRequest hRequest, int iServer) {
        Debug.Log("OnServerFailedToRespond: " + hRequest + " - " + iServer);
    }

    private void OnRefreshComplete(HServerListRequest hRequest, EMatchMakingServerResponse response) {
        Debug.Log("OnRefreshComplete: " + hRequest + " - " + response);
    }
}
