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

        [SerializeField]
        public static List<Color32> PLAYER_PRIMARY_COLORS = new List<Color32>(){
            new Color32(172, 55, 98, 255),
            new Color32(171, 129, 34, 255),
            new Color32(64, 171, 42, 255),
            new Color32(139, 100, 171, 255),
            new Color32(31, 164, 171, 255)
        };

        [SerializeField]
        public static List<Color32> PLAYER_SECONDARY_COLORS = new List<Color32>(){
            new Color32(254, 44, 135, 255),
            new Color32(255, 176, 6, 255),
            new Color32(70, 255, 20, 255),
            new Color32(193, 127, 255, 255),
            new Color32(0, 255, 255, 255)
        };

        [SerializeField]
        public static List<Color32> PLAYER_ACCENT_COLORS = new List<Color32>(){
            new Color32(205, 105, 142, 255),
            new Color32(208, 171, 87, 255),
            new Color32(112, 208, 94, 255),
            new Color32(180, 146, 208, 255),
            new Color32(84, 201, 208, 255)
        };
    }


    public static class GameMap
    {
        //Global Map. List of all the grid layers
        public static List<util.GridRepresentation.GridLayer> allGridLayers = new List<util.GridRepresentation.GridLayer>();
    }

}