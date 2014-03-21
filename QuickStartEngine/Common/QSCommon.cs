/*
 * QSCommon.cs
 * 
 * This file is part of the QuickStart Game Engine.  See http://www.codeplex.com/QuickStartEngine
 * for license details.
 */

using System;

namespace QuickStart
{
    public enum GraphicsLevel
    {
        Low = 0,
        Med = 1,
        High = 2,
        Highest = 3
    }

    /// <summary>
    /// A class to hold any global constants. Good for holding anything
    /// that is considered a "magic number". Magic numbers are what you notice
    /// if you look through someone's code and notice numbers defined for values,
    /// but you have no idea why that number was chosen. Also, having a single
    /// class for global constants creates an easy place to look when you know
    /// you're looking for a constant.
    /// </summary>
    public static class QSCommon
    {
        // Graphics constants ------------------------------------------------------
        // Change these two values to set the screen size
        // You should maintain a 4:3, 16:9, or 16:10 ratio in whatever
        // you choose. 4:3 for standard monitor or SDTV, 16:9 for HDTV,
        // or 16:10 for widescreen LCD monitors. 4:3 will be compatible
        // with the most devices.
        //
        // 1024 / 768 = 1.3333 or 4/3 (4:3)        
        public const int SCREEN_WIDTH = 1024;
        public const int SCREEN_HEIGHT = 768;

        public const GraphicsLevel QSDetail = GraphicsLevel.High;   // Set to lower value for slower computers or graphics cards

        public const bool IS_FULL_SCREEN = false;
        public const bool LOCK_FRAMERATE = true;

        // Multisampling should only be set to true for high-end graphics cards
        public const bool MULTISAMPLING_PREF = (QSDetail == GraphicsLevel.Highest) ? true : false;

        // World constants ---------------------------------------------------------

        // Terrain constants -------------------------------------------------------
        public const int MAX_TERRAIN_SCALE = 4;
        public const int DEF_TERRAIN_SMOOTHING = 10;
        public const float DEF_TERRAIN_ELEV_STR = 6.0f;
        public const int QUAD_TREE_WIDTH = 128;      

        // Weather system constants ------------------------------------------------
        public const float SNOW_INTENSITY = 150f;
        public const float RAIN_INTENSITY = 275f;

        // Input constants ---------------------------------------------------------
        public const float MIN_CONTROL_SENSITIVITY = 0.1f;
        public const float MAX_CONTROL_SENSITIVITY = 100f;

        // HUD constants -----------------------------------------------------------

    }
}
