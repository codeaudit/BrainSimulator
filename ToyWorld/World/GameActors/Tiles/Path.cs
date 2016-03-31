﻿using World.GameActions;

namespace World.GameActors.Tiles
{
    public class Path : World.GameActors.Tiles.StaticTile
    {
        public Path(int tileType) : base(tileType)
        {
        }

        public Path(TilesetTable tileset) : base(tileset)
        {
        }
    }
}
