using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GameConstants
{
    public class GlobalConstants
    {
        // Example game constants

        public const int MIN_NUMBER_OF_PLAYERS = 2;
        public const int MAX_NUMBER_OF_PLAYERS = 4;

        public const int NUMBER_OF_ROOM_SLOTS_IN_LOBBYUI = 1;

        public const string TITLE_LEVEL = "Title";
        public const string LOBBY_LEVEL = "Lobby";
        public const string ROOM_LEVEL = "Room";
        public const string FIRST_SINGLEPLAYER_LEVEL = "Level1";
        public const string FIRST_MULTIPLAYER_LEVEL = "Level1";

        // https://stackoverflow.com/questions/64105721/get-build-path-of-local-file-unity
        //public string LAYOUT_JSON_FILE = Application.dataPath + "/Resources/CommunicationJson.txt";
    }

    public class Character
    {
        public static Dictionary<int, string> CHAR_LIST = new Dictionary<int, string>(){
            { 0, "Char1"},
            { 1, "Char2"},
            { 2, "Char3"},
            { 3, "Char4"}
        };
    }
}

